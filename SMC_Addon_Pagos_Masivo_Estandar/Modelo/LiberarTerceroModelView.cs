using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbobsCOM;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SMC_APM.Modelo
{
    public class LiberarTerceroModelView
    {
        public DateTime FechaPago { get; set; }
        public string CuentaBanco { get; set; }
        public double TipoCambio { get; set; }
        public string NroOperacion { get; set; }
        public Form Form { get; set; }
        public List<DatosSUNATRetencion> RptaSUNAT { get; set; }
        public List<LiberarTercero_Fila> Filas { get; set; }
        public string FilasAsXML { get; set; }

        public LiberarTerceroModelView(Form form)
        {
            Form = form;
            Filas = new List<LiberarTercero_Fila>();
        }

        public void BindDataCabecera()
        {
            try
            {
                FechaPago = DateTime.ParseExact(Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_FPAG"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                CuentaBanco = Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_BNPA");
                TipoCambio = Convert.ToDouble(Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_TIPC"));
                NroOperacion = Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_NROP");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetMedioPagoSAP(string medioPagoSUNAT)
        {
            switch (medioPagoSUNAT)
            {
                case "TB":
                case "CG":
                case "PV":

                    return "T";

                case "CB": return "C";
                default: return "T";

            }
        }

        internal void ValidarDatos()
        {
            if (string.IsNullOrEmpty(NroOperacion))
                throw new Exception("Debe ingresar un número de operación");

            if (RptaSUNAT == null)
                throw new Exception("Debe cargar una respuesta de SUNAT antes de procesar los pagos");

            foreach (DatosSUNATRetencion lineaArchivo in RptaSUNAT)
            {
                LiberarTercero_Fila fila = Filas.Where(x => x.Proveedor == ("P" + lineaArchivo.RUC)).FirstOrDefault();

                if (fila != null) //SI ESTÁ PRESENTE EN EL ARCHIVO PERO NO EN LA GRILLA, SE IGNORA
                {
                    double totalRetencionProveedor = Filas.Where(x => x.Proveedor == ("P" + lineaArchivo.RUC)).Sum(y => y.TotalRetencionML);
                    if (totalRetencionProveedor != lineaArchivo.MontoEmbargo)
                        throw new Exception($"El monto de retención para el proveedor P{lineaArchivo.RUC} no ha sido ingresado en su totalidad. Falta registrar {lineaArchivo.MontoEmbargo - totalRetencionProveedor} soles");
                }
            }
        }

        public void CrearPagos()
        {
            try
            {
                Filas = Filas.Where(x => x.Pagar && x.DocEntryPagoRet == 0).ToList(); //SOLO ENTRAN AL PROCESO LOS QUE NO TIENEN PAGO ASOCIADO ANTERIORMENTE

                if ((Filas == null || Filas.Count == 0) && Form.Mode == BoFormMode.fm_ADD_MODE)
                    throw new Exception("Debe seleccionar al menos un documento");

                List<Terceros_Retencion> agrupados = Filas
                                                    .Where(x => x.TotalRetencionML > 0)
                                                    .GroupBy(u => u.Proveedor)
                                                    .Select(_ => new Terceros_Retencion
                                                    {
                                                        Proveedor = _.Key,
                                                        MontoTransferencia = _.Where(x => x.MedioPagoSAP == "T").Sum(y => y.TotalRetencionML),
                                                        MontoCheques = _.Where(x => x.MedioPagoSAP == "C").Sum(y => y.TotalRetencionML),
                                                        Banco = _.Max(x => x.Banco),
                                                        CuentaTransferencia = CuentaBanco,
                                                        CuentaCheque = CuentaBanco,
                                                        FechaTransferencia = FechaPago,
                                                        NroOperacion = NroOperacion,
                                                        Documentos = _.Select(x => new DatoDocumento() { DocEntry = x.DocEntry, ObjType = x.TipoDocumento, Cuota = x.NroCuota, MontoAplicado = x.TotalRetencionML, Linea = x.LineaAsiento }).ToList()

                                                    }).ToList();

                foreach (Terceros_Retencion proveedor in agrupados)
                {
                    int docEntryPago = proveedor.CrearPagoRetencion();
                    if (docEntryPago != -1)
                    {
                        Filas.Where(x => x.Proveedor == proveedor.Proveedor).All(c => { c.DocEntryPagoRet = docEntryPago; return true; });
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void GetDatosLiberacionTerceroRetendor(string fecha)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? $"CALL SP_EXD_GET_LIBERACION_SUNAT('{fecha}')" : $"EXEC SP_EXD_GET_LIBERACION_SUNAT('{fecha}')";
                recordset.DoQuery(query);



                if (recordset.RecordCount == 0)
                    throw new Exception($"No existen documentos retenidos para la fecha {fecha}");

                string xml = recordset.GetAsXML();

                XDocument XDoc = XDocument.Parse(xml);

                Filas = (from q in XDoc.Descendants("row")
                         select new LiberarTercero_Fila
                         {
                             FilaMatrix = Convert.ToInt32(q.Element("LineId").Value),
                             Pagar = q.Element("U_EXD_IPAG").Value == "Y",
                             Escenario = Convert.ToInt32(q.Element("U_EXD_CODE").Value),
                             Banco = q.Element("U_EXD_IPAG").Value,
                             MedioPago = q.Element("U_EXD_MEDP").Value,
                             Proveedor = q.Element("U_EXD_PROV").Value,
                             TipoDocumento = Convert.ToInt32(q.Element("U_EXD_TDOC").Value),
                             DocEntry = Convert.ToInt32(q.Element("U_EXD_NUMI").Value),
                             LineaAsiento = Convert.ToInt32(q.Element("U_EXD_LIAS").Value),
                             NroCuota = Convert.ToInt32(q.Element("U_EXD_NCUO").Value),
                             MedioPagoSAP = GetMedioPagoSAP(q.Element("U_EXD_MEDP").Value),
                             TotalDocumento = Convert.ToDouble(q.Element("U_EXD_TOTD").Value),
                             TotalML = Convert.ToDouble(q.Element("U_EXD_TOTP").Value),
                             TotalProveedorML = 0.0,
                             TotalRetencionML = 0.0
                         }).ToList();

                FilasAsXML = recordset.GetFixedXML(RecordsetXMLModeEnum.rxmData).Replace("<Table>Recordset</Table>", "").Replace("Rows", "rows").Replace("Row", "row").Replace("Fields", "cells").Replace("Field", "cell").Replace("Alias", "uid").Replace("Value", "value").Replace("<Recordset xmlns=\"http://www.sap.com/SBO/SDK/DI\">", "<dbDataSources Uid=\"@EXD_TRR1\">").Replace("</Recordset>", "</dbDataSources>");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class Terceros_Retencion
    {
        public string Proveedor { get; set; }
        public string NroOperacion { get; set; }
        public string CuentaTransferencia { get; set; }
        public string CuentaCheque { get; set; }
        public string Banco { get; set; }
        public double MontoTransferencia { get; set; }
        public DateTime FechaTransferencia { get; set; }
        public double MontoCheques { get; set; }
        public List<DatoDocumento> Documentos { get; set; }

        internal int CrearPagoRetencion()
        {
            try
            {
                Payments oPagoEf = Globales.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
                oPagoEf.DocTypte = BoRcptTypes.rSupplier;
                oPagoEf.CardCode = Proveedor;
                oPagoEf.DocDate = FechaTransferencia;
                oPagoEf.TaxDate = FechaTransferencia;
                oPagoEf.DueDate = FechaTransferencia;
                oPagoEf.Remarks = "PAGO RETENCION " + NroOperacion;

                oPagoEf.DocCurrency = "SOL";
                oPagoEf.LocalCurrency = BoYesNoEnum.tYES;

                if (MontoTransferencia > 0)
                {
                    oPagoEf.TransferAccount = CuentaTransferencia;
                    oPagoEf.TransferSum = MontoTransferencia;
                    oPagoEf.TransferDate = FechaTransferencia;
                    oPagoEf.TransferReference = NroOperacion;
                }

                if (MontoCheques > 0) //CREAMOS EL PAGO CON UN CHEQUE
                {
                    oPagoEf.Checks.SetCurrentLine(0);

                    oPagoEf.Checks.CheckAccount = CuentaCheque;
                    oPagoEf.Checks.BankCode = GetBankCode(Banco);
                    oPagoEf.Checks.CheckSum = MontoCheques;
                    oPagoEf.Checks.DueDate = FechaTransferencia;

                    oPagoEf.Checks.Add();
                }


                int indice = 0;

                foreach (DatoDocumento documento in Documentos)
                {
                    oPagoEf.Invoices.SetCurrentLine(indice);
                    oPagoEf.Invoices.DocEntry = documento.DocEntry;

                    switch (documento.ObjType)
                    {
                        case 18: oPagoEf.Invoices.InvoiceType = BoRcptInvTypes.it_PurchaseInvoice; break;
                        case 30: oPagoEf.Invoices.InvoiceType = BoRcptInvTypes.it_JournalEntry; break;
                        default:
                            break;
                    }

                    oPagoEf.Invoices.InstallmentId = documento.Cuota;
                    oPagoEf.Invoices.SumApplied = documento.MontoAplicado;

                    oPagoEf.Invoices.Add();
                    indice++;
                }

                if (oPagoEf.Add() != 0)
                    throw new Exception(Globales.Company.GetLastErrorDescription());

                return Convert.ToInt32(Globales.Company.GetNewObjectKey());

            }
            catch (Exception ex)
            {
                Globales.Aplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return -1;
            }
        }

        private string GetBankCode(string banco)
        {
            Recordset rs = null;
            try
            {
                string query = $"select \"BankCode\" from odsc where \"BankName\" = '{banco}'";
                rs = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(query);
                if (rs.RecordCount == 0)
                    throw new Exception($"Error al obtener código de banco de la descripción {banco}");

                return rs.Fields.Item("BankCode").Value;
            }
            catch (Exception)
            {

                throw;
            }
            finally { Util.utilNET.liberarObjeto(rs); }

        }
    }

    public class DatosTransferencia
    {

    }

    public class DatoDocumento
    {
        public int DocEntry { get; set; }
        public int ObjType { get; set; }
        public int Linea { get; set; }
        public int Cuota { get; set; }
        public double MontoAplicado { get; set; }
    }

    public class LiberarTercero_Fila
    {
        public int FilaMatrix { get; set; }
        public bool Pagar { get; set; }
        public int Escenario { get; set; }
        public string Banco { get; set; }
        public string MedioPago { get; set; }
        public string MedioPagoSAP { get; set; }
        public string Proveedor { get; set; }
        public int TipoDocumento { get; set; }
        public int DocEntry { get; set; }
        public int NroCuota { get; set; }
        public int LineaAsiento { get; set; }
        public double TotalDocumento { get; set; }
        public double TotalML { get; set; }
        public double TotalRetencionML { get; set; }
        public double TotalProveedorML { get; set; }
        public int DocEntryPagoRet { get; set; }
        public int DocEntryPagoProv { get; set; }
    }
}