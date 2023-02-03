using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbobsCOM;
using SAPbouiCOM;
using SMC_APM.Controller;
using SMC_APM.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SMC_APM.Modelo
{
    public class LiberarTerceroModelView
    {
        public DateTime FechaPago { get; set; }
        public string CuentaBanco { get; set; }
        public double TipoCambio { get; set; }
        public string NroOperacion { get; set; }
        public bool PagarRetenciones { get; set; }
        public bool PagarProveedores { get; set; }
        public Form Form { get; set; }
        public List<DatosSUNATRetencion> RptaSUNAT { get; set; }
        public List<LiberarTercero_Fila> Filas { get; set; }
        public string FilasAsXML { get; set; }

        public LiberarTerceroModelView(Form form)
        {
            Form = form;
            Filas = new List<LiberarTercero_Fila>();
        }

        public void BindData()
        {
            try
            {
                FechaPago = DateTime.ParseExact(Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_FPAG"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                CuentaBanco = Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_BNPA");
                TipoCambio = Convert.ToDouble(Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_TIPC"));
                NroOperacion = Form.GetDBDataSource("@EXD_OTRR").GetValueExt("U_EXD_NROP");
                PagarProveedores = Form.GetUserDataSource("UD_PP").Value == "Y";
                PagarRetenciones = Form.GetUserDataSource("UD_PR").Value == "Y";

                //EL DETALLE YA SE CARGÓ EN LA CONSULTA, ACÁ RECORREMOS PARA VALIDAR LAS SELECCIONES MASIVAS (CONTROL + CLICK)
                Matrix matrix = Form.GetMatrix("Item_18");
                CheckBox chk;

                for (int i = 1; i <= matrix.VisualRowCount; i++)
                {
                    chk = matrix.GetCellSpecific("Col_0", i);
                    Filas.Where(x => x.FilaMatrix == i).FirstOrDefault().Pagar = chk.Checked;
                }
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
            if (!PagarRetenciones && !PagarProveedores)
                throw new Exception("Debe seleccionar al menos una opción de pago. Pago Proveedores / Retenciones");

            if (string.IsNullOrEmpty(NroOperacion))
                throw new Exception("Debe ingresar un número de operación");

            if (RptaSUNAT == null)
                throw new Exception("Debe cargar una respuesta de SUNAT antes de procesar los pagos");

            if (Filas.Where(x => x.Pagar).Count() == 0)
                throw new Exception("Debe seleccionar al menos un documento para iniciar el proceso");

            if (PagarRetenciones)
            {
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
        }

        public void CrearPagos()
        {
            try
            {
                if (PagarRetenciones)
                    ProcesarRetenciones();

                if (PagarProveedores)
                    ProcesarProveedores();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ProcesarProveedores()
        {
            DBDataSource HEADER = Form.GetDBDataSource("@EXD_OTRR");
            DBDataSource DETAIL = Form.GetDBDataSource("@EXD_TRR1");
            DBDataSource PAGOMASIVO = Form.GetDBDataSource("@EXP_OPMP");
            Matrix matrix = Form.GetMatrix("Item_18");

            var pgoDS = DETAIL.GetAsXML();
            string docEntryPM = HEADER.GetValueExt("U_EXD_NRPM");

            Conditions conditions = new Conditions();
            Condition cnd = conditions.Add();
            cnd.Alias = "DocEntry";

            cnd.Operation = BoConditionOperation.co_EQUAL;
            cnd.CondVal = docEntryPM;

            PAGOMASIVO.Query(conditions);

            var lstPagos = ObtenerListaPagos(HEADER, PAGOMASIVO, pgoDS);
            var estado = string.Empty;
            var msjError = string.Empty;
            var nroPago = 0;

            try
            {
                Globales.Aplication.StatusBar.SetSystemMessage("Iniciando generacion de pagos proveedores...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                foreach (var pgo in lstPagos)
                {
                    msjError = string.Empty;
                    estado = "OK";
                    nroPago = 0;
                    try
                    {
                        nroPago = PagoMasivoController.GenerarPagoEfectuadoSBO(pgo);
                    }
                    catch (Exception ex)
                    {
                        msjError = ex.Message;
                        estado = "ER";
                    }

                    foreach (var linea in pgo.ExtLineasDS)
                    {
                        matrix.GetCellSpecific("Col_15", linea).Value = nroPago == 0 ? string.Empty : nroPago.ToString();
                        matrix.GetCellSpecific("Col_25", linea).Value = msjError;

                        ComboBox cb = matrix.GetCellSpecific("Col_26", linea);
                        cb.Select(estado == "OK" ? "Y" : "N", BoSearchKey.psk_ByValue);

                        //DETAIL.SetValue("U_EXD_IDPP", linea - 1, nroPago == 0 ? string.Empty : nroPago.ToString());
                        //DETAIL.SetValue("U_EXP_ESTADO", linea - 1, estado);
                        //DETAIL.SetValue("U_EXP_MSJERROR", linea - 1, msjError);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<SBOPago> ObtenerListaPagos(DBDataSource dbsCabLR, DBDataSource dbsCabPM, object obj)
        {
            XDocument xDoc = null;

            try
            {
                var rsltNroLinea = 0;
                var rsltNroCuota = 0;
                var fechaPago = DateTime.ParseExact(dbsCabLR.GetValue("U_EXD_FPAG", 0).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
                var refTransf = dbsCabLR.GetValue("U_EXD_NROP", 0).Trim();

                var codSeriePago = Convert.ToInt32(dbsCabPM.GetValue("U_EXP_SERIEPAGO", 0).Trim());
                var codSerieRtcn = Convert.ToInt32(dbsCabPM.GetValue("U_EXP_SERIERETENCION", 0).Trim());

                xDoc = XDocument.Parse((string)obj);

                var xElements = xDoc.XPathSelectElements("dbDataSources/rows/row").Where(w => w.Descendants("cell")
               .Any(a => a.Element("uid").Value.Equals("U_EXD_IPAG") && a.Element("value").Value.Equals("Y"))
               && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXD_GEPP") && a.Element("value").Value == "N"));

                return xElements.Descendants("cells").GroupBy(g => new
                {
                    CardCode = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_PROV")).FirstOrDefault().Element("value").Value,
                    MedioDePago = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_MEDP")).FirstOrDefault()?.Element("value").Value,
                    Moneda = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_MONE")).FirstOrDefault()?.Element("value").Value,
                    Banco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_BKCD")).FirstOrDefault()?.Element("value").Value,
                    CtaBanco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_CTAB")).FirstOrDefault()?.Element("value").Value,
                    AplSerieRetencion = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXD_INDR")).FirstOrDefault()?.Element("value").Value
                }).Select(s => new SBOPago
                {
                    CodSerieSBO = s.Key.AplSerieRetencion == "Y" ? codSerieRtcn : codSeriePago,
                    CodigoSN = s.Key.CardCode,
                    Moneda = s.Key.Moneda,
                    FechaContabilizacion = fechaPago,
                    FechaDocumento = fechaPago,
                    FechaVencimiento = fechaPago,
                    Monto = s.Sum(sm => Convert.ToDouble(sm.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_PPME")).FirstOrDefault()?.Element("value").Value)),
                    ExtLineasDS = s.Select(s1 => Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("LineId")).FirstOrDefault()?.Element("value").Value)),
                    MetodoPago = new SBOMetodoPago
                    {
                        Tipo = s.Key.MedioDePago,
                        Pais = "PE",
                        Banco = s.Key.Banco,
                        Cuenta = s.Key.CtaBanco,
                        Referencia = refTransf
                    },
                    Detalle = s.Select(s1 => new SBOPagoDetalle
                    {
                        TipoDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_TDOC")).FirstOrDefault()?.Element("value").Value),
                        IdDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_NUMI")).FirstOrDefault()?.Element("value").Value),
                        IdLinea = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_LIAS")).FirstOrDefault()?.Element("value").Value, out rsltNroLinea) ? rsltNroLinea : 0,
                        NroCuota = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_NCUO")).FirstOrDefault()?.Element("value").Value, out rsltNroCuota) ? rsltNroCuota : 0,
                        MontoPagado = Convert.ToDouble(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXD_PAGP")).FirstOrDefault()?.Element("value").Value)
                    })
                });
            }
            finally { }
        }

        private void ProcesarRetenciones()
        {
            Filas = Filas.Where(x => x.Pagar && x.DocEntryPagoRet == 0 && x.TotalRetencionML > 0).ToList(); //SOLO ENTRAN AL PROCESO LOS QUE NO TIENEN PAGO ASOCIADO

            //if ((Filas == null || Filas.Count == 0) && (Form.Mode == BoFormMode.fm_ADD_MODE))
            //    throw new Exception("Debe seleccionar al menos un documento");

            if(Filas != null && Filas.Count > 0) //HAY RETENCIONES POR PROCESAR
            {
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
                                        TipoCambio = TipoCambio,
                                        Documentos = _.Select(x => new DatoDocumento() { DocEntry = x.DocEntry, ObjType = x.TipoDocumento, Cuota = x.NroCuota, MontoAplicado = x.TotalRetencionML, Moneda = x.Moneda, Linea = x.LineaAsiento }).ToList()
                                    }).ToList();

                foreach (Terceros_Retencion proveedor in agrupados)
                {
                    int docEntryPago = proveedor.CrearPagoRetencion();
                    if (docEntryPago != -1)
                    {
                        Filas.Where(x => x.Proveedor == proveedor.Proveedor).All(c => { c.DocEntryPagoRet = docEntryPago; return true; });
                        Filas.Where(x => x.Proveedor == proveedor.Proveedor).All(c => { c.MensajeError = proveedor.MensajeError; return true; });
                    }
                }
            }
        }

        internal void GetDatosLiberacionTerceroRetendor(string docEntryPM, string fecha)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB ? $"CALL SP_EXD_GET_LIBERACION_SUNAT('{docEntryPM}', '{fecha}')" : $"EXEC SP_EXD_GET_LIBERACION_SUNAT('{docEntryPM}', '{fecha}')";
                recordset.DoQuery(query);

                if (recordset.RecordCount == 0)
                    throw new Exception($"No existen documentos retenidos para el pago masivo {docEntryPM}");

                string xml = recordset.GetAsXML();

                XDocument XDoc = XDocument.Parse(xml);

                Filas = (from q in XDoc.Descendants("row")
                         select new LiberarTercero_Fila
                         {
                             FilaMatrix = Convert.ToInt32(q.Element("LineId").Value),
                             Pagar = q.Element("U_EXD_IPAG").Value == "Y",
                             Escenario = Convert.ToInt32(q.Element("U_EXD_CODE").Value),
                             Banco = q.Element("U_EXD_BAES").Value,
                             BancoCode = q.Element("U_EXD_BKCD").Value,
                             GLCuentaBanco = q.Element("U_EXD_CTAB").Value,
                             MedioPago = q.Element("U_EXD_MEDP").Value,
                             Proveedor = q.Element("U_EXD_PROV").Value,
                             TipoDocumento = Convert.ToInt32(q.Element("U_EXD_TDOC").Value),
                             DocEntry = Convert.ToInt32(q.Element("U_EXD_NUMI").Value),
                             LineaAsiento = Convert.ToInt32(q.Element("U_EXD_LIAS").Value),
                             NroCuota = Convert.ToInt32(q.Element("U_EXD_NCUO").Value),
                             MedioPagoSAP = GetMedioPagoSAP(q.Element("U_EXD_MEDP").Value),
                             Moneda = q.Element("U_EXD_MONE").Value,
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

        internal void GetDatosLiberacionTerceroRetendor_Creado(int docEntry)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB ? $"CALL SP_EXD_GET_LIBERACION_SUNAT_CREADO({docEntry})" : $"EXEC SP_EXD_GET_LIBERACION_SUNAT_CREADO({docEntry})";
                recordset.DoQuery(query);

                if (recordset.RecordCount == 0)
                    throw new Exception($"No existen documentos retenidos para el registro {docEntry}");

                string xml = recordset.GetAsXML();

                XDocument XDoc = XDocument.Parse(xml);

                Filas = (from q in XDoc.Descendants("row")
                         select new LiberarTercero_Fila
                         {
                             FilaMatrix = Convert.ToInt32(q.Element("LineId").Value),
                             Pagar = q.Element("U_EXD_IPAG").Value == "Y",
                             Escenario = Convert.ToInt32(q.Element("U_EXD_CODE").Value),
                             Banco = q.Element("U_EXD_BAES").Value,
                             BancoCode = q.Element("U_EXD_BKCD").Value,
                             GLCuentaBanco = q.Element("U_EXD_CTAB").Value,
                             MedioPago = q.Element("U_EXD_MEDP").Value,
                             Proveedor = q.Element("U_EXD_PROV").Value,
                             TipoDocumento = Convert.ToInt32(q.Element("U_EXD_TDOC").Value),
                             DocEntry = Convert.ToInt32(q.Element("U_EXD_NUMI").Value),
                             LineaAsiento = Convert.ToInt32(q.Element("U_EXD_LIAS").Value),
                             NroCuota = Convert.ToInt32(q.Element("U_EXD_NCUO").Value),
                             MedioPagoSAP = GetMedioPagoSAP(q.Element("U_EXD_MEDP").Value),
                             Moneda = q.Element("U_EXD_MONE").Value,
                             TotalDocumento = Convert.ToDouble(q.Element("U_EXD_TOTD").Value),
                             TotalML = Convert.ToDouble(q.Element("U_EXD_TOTP").Value),
                             TotalProveedorML = Convert.ToDouble(q.Element("U_EXD_PAGP").Value),
                             TotalRetencionML = Convert.ToDouble(q.Element("U_EXD_PAGR").Value),
                             DocEntryPagoProv = GetElementValue(q, "U_EXD_IDPP") == string.Empty ? 0 : Convert.ToInt32(GetElementValue(q, "U_EXD_IDPP")),
                             DocEntryPagoRet = GetElementValue(q, "U_EXD_IDPR") == string.Empty ? 0 : Convert.ToInt32(GetElementValue(q, "U_EXD_IDPR")),
                             MensajeError = GetElementValue(q, "U_EXD_MSJE") == string.Empty ? "" : GetElementValue(q, "U_EXD_MSJE")
                         }).ToList();

                FilasAsXML = recordset.GetFixedXML(RecordsetXMLModeEnum.rxmData).Replace("<Table>Recordset</Table>", "").Replace("Rows", "rows").Replace("Row", "row").Replace("Fields", "cells").Replace("Field", "cell").Replace("Alias", "uid").Replace("Value", "value").Replace("<Recordset xmlns=\"http://www.sap.com/SBO/SDK/DI\">", "<dbDataSources Uid=\"@EXD_TRR1\">").Replace("</Recordset>", "</dbDataSources>");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetElementValue(XElement parentEl, string elementName, string defaultValue = "")
        {
            var foundEl = parentEl.Element(elementName);

            if (foundEl != null)
            {
                return foundEl.Value;
            }

            return defaultValue;
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
        public double TipoCambio { get; set; }
        public double MontoCheques { get; set; }
        public List<DatoDocumento> Documentos { get; set; }
        public string MensajeError { get; set; }

        internal int CrearPagoRetencion()
        {
            try
            {
                int decimales = utilNET.GetDecimalesConfigurado();

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

                    //oPagoEf.Checks.CheckAccount = CuentaCheque;
                    //oPagoEf.Checks.BankCode = GetBankCode(Banco);
                    //oPagoEf.Checks.CheckSum = MontoCheques;
                    //oPagoEf.Checks.DueDate = FechaTransferencia;

                    var sucursalBanco = PagoMasivoController.ObtenerSucursaCtaBanco(GetBankCode(Banco), CuentaCheque);
                    oPagoEf.Checks.AccounttNum = sucursalBanco.Item1;
                    oPagoEf.Checks.BankCode = GetBankCode(Banco);
                    oPagoEf.Checks.Branch = sucursalBanco.Item2;
                    oPagoEf.Checks.CheckAccount = CuentaCheque;

                    oPagoEf.Checks.CheckSum = MontoCheques;
                    oPagoEf.Checks.CountryCode = "PE";
                    oPagoEf.Checks.Trnsfrable = SAPbobsCOM.BoYesNoEnum.tNO;

                    oPagoEf.Checks.Add();
                }

                int indice = 0;

                foreach (DatoDocumento documento in Documentos)
                {
                    oPagoEf.Invoices.SetCurrentLine(indice);
                    oPagoEf.Invoices.DocEntry = documento.DocEntry;

                    switch (documento.ObjType)
                    {
                        case 18: oPagoEf.Invoices.InvoiceType = BoRcptInvTypes.it_PurchaseInvoice; oPagoEf.Invoices.InstallmentId = documento.Cuota; break;
                        case 30: oPagoEf.Invoices.InvoiceType = BoRcptInvTypes.it_JournalEntry; oPagoEf.Invoices.DocLine = documento.Linea; break;
                        default:
                            break;
                    }

                    if (documento.Moneda == "SOL")
                        oPagoEf.Invoices.SumApplied = documento.MontoAplicado;
                    else
                    {
                        oPagoEf.Invoices.SumApplied = documento.MontoAplicado;
                        oPagoEf.Invoices.AppliedFC = Math.Round(documento.MontoAplicado / TipoCambio, decimales);
                    }

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
                MensajeError = ex.Message;
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
            finally { utilNET.liberarObjeto(rs); }
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
        public string Moneda { get; set; }
        public double MontoAplicado { get; set; }
    }

    public class LiberarTercero_Fila
    {
        public int FilaMatrix { get; set; }
        public bool Pagar { get; set; }
        public int Escenario { get; set; }
        public string Banco { get; set; }
        public string GLCuentaBanco { get; set; }
        public string BancoCode { get; set; }
        public string MedioPago { get; set; }
        public string MedioPagoSAP { get; set; }
        public string Proveedor { get; set; }
        public int TipoDocumento { get; set; }
        public int DocEntry { get; set; }

        public int NroCuota { get; set; }
        public int LineaAsiento { get; set; }
        public string Moneda { get; set; }
        public double TotalDocumento { get; set; }
        public double TotalML { get; set; }
        public double TotalRetencionML { get; set; }
        public double TotalProveedorML { get; set; }
        public int DocEntryPagoRet { get; set; }
        public int DocEntryPagoProv { get; set; }
        public string MensajeError { get; set; }
    }
}