using SAP_AddonFramework;
using SAPbobsCOM;
using SMC_APM.dao;
using SMC_APM.dto;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SMC_APM.Controller
{
    internal class PagoMasivoController
    {
        public static SAPbobsCOM.Recordset ObtenerSeriesDocumentoPago()
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and ifnull(\"U_EXC_CR\",'') = 'N'";
            recordset.DoQuery(sqlQry);
            if (recordset.EoF)
                return null;
            else
                return recordset;
        }

        public static SAPbobsCOM.Recordset ObtenerSeriesRetencion()
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and ifnull(\"U_EXC_CR\",'') = 'Y'";
            recordset.DoQuery(sqlQry);
            if (recordset.RecordCount == 0)
                throw new Exception("No se han configurado series de retención, validar su configuración en el formulario numeración de documentos");
            else
                return recordset;
        }

        public static bool ValidarRegistroUnicoPorFecha(string fechaFiltro)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select 'A' from \"@EXP_OPMP\" where \"U_EXP_FECHA\" = '{fechaFiltro}'";
            recordset.DoQuery(sqlQry);
            return !recordset.EoF;
        }

        private static Tuple<string, string> ObtenerSucursaCtaBanco(string codBanco, string codCta)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select top 1 \"Account\",\"Branch\" from DSC1 where \"BankCode\" = '{codBanco}' and \"GLAccount\" = '{codCta}';";
            recordset.DoQuery(sqlQry);
            if (recordset.EoF)
                return null;
            else
                return Tuple.Create(recordset.Fields.Item(0).Value, recordset.Fields.Item(1).Value);
        }

        public static IEnumerable<PMPDocumento> ListarDocumentosParaPagos(string fecha)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL EXP_SP_PMP_ListarDocumentosPorFecha('{fecha}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new PMPDocumento
                {
                    SlcPago = dc["SlcPago"],
                    SlcRetencion = dc["SlcRetencion"],
                    CodigoEscenarioPago = dc["CodEscenarioPago"],
                    DescripcionEscenarioPago = dc["DscEscenarioPago"],
                    MedioDePago = dc["MedioDePago"],
                    CodBanco = dc["CodBanco"],
                    CodCtaBanco = dc["CodCtaBanco"],
                    DocEntryDocumento = Convert.ToInt32(dc["DocEntryDocumento"]),
                    TipoDocumento = dc["TipoDocumento"],
                    CardCode = dc["CardCode"],
                    CardName = dc["CardName"],
                    Moneda = dc["Moneda"],
                    Importe = Convert.ToDouble(dc["Importe"]),
                    NroCuota = dc["NroCuota"].Trim(),
                    NroLineaAsiento = dc["NroLineaAsiento"].Trim(),
                    NroDocumentoSN = dc["NroDocumentoSN"].Trim(),
                    AplSreRetencion = dc["AplSerieRetencion"].Trim()
                };
            });
            return rslt;
        }

        private static IEnumerable<TXT3Retenedor> ObtenerDatosTerceroRetenedor(int docEntryPMP)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_TERCERORETENEDOR('{docEntryPMP}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new TXT3Retenedor
                {
                    RUC = dc["RUC"],
                    monto = Convert.ToDecimal(dc["Monto"])
                };
            });
            return rslt;
        }

        public static void generarTXT3Retenedor(int docEntryPMP)
        {
            List<TXT3Retenedor> olista = null;
            pagoDAO _pagoDAO = null;
            string nombreArchivo = "";
            StreamWriter archivo = null;
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEditText = null;

            try
            {
                _pagoDAO = new pagoDAO();
                var lstDocs3Reten = ObtenerDatosTerceroRetenedor(docEntryPMP);

                //Generando el archivo
                //nombreArchivo = @"\\WIN-SOPORTESAP\DocSap\TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = @"D:\Pagos_Masivos\TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = nombreArchivo + "RCP20512857869_PM_" + docEntryPMP + ".txt";
                archivo = new System.IO.StreamWriter(nombreArchivo, false, Encoding.GetEncoding(1252));
                var lineaDetalle = string.Empty;
                foreach (var doc in lstDocs3Reten)
                {
                    lineaDetalle = doc.RUC + "|" + doc.monto.ToString();
                    archivo.WriteLine(lineaDetalle);
                }
                archivo.Close();

                Process.Start(nombreArchivo);

                //oForm = sboApplication.Forms.ActiveForm;
                //oItem = oForm.Items.Item("txt_3Ret");
                //oEditText = (SAPbouiCOM.EditText)oItem.Specific;

                //oEditText.Value = nombreArchivo;
            }
            catch
            {
                throw;
            }
        }

        public static IEnumerable<TXT3RetenedorRsp> LeerTXT3RetenedorRsp(string filePath)
        {
            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadAllLines(filePath))
                {
                    var segments = line.Split('|');
                    yield return new TXT3RetenedorRsp
                    {
                        RUC = segments[0].Trim(),
                        Estado = segments[3][0].ToString()
                    };
                }
            }
            else
                yield return null;
        }

        public static int GenerarPagoEfectuadoSBO(SBOPago pago)
        {
            var row = 0;
            var sboPayments = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
            sboPayments.Series = pago.CodSerieSBO;
            sboPayments.CardCode = pago.CodigoSN;
            sboPayments.DocDate = pago.FechaContabilizacion;
            sboPayments.DueDate = pago.FechaVencimiento;
            sboPayments.TaxDate = pago.FechaDocumento;
            sboPayments.DocCurrency = pago.Moneda;
            sboPayments.DocType = pago.CodigoSN.StartsWith("C") ? BoRcptTypes.rCustomer : BoRcptTypes.rSupplier;
            //if (pago.ObjType == 24) sboPayments.CheckAccount = ((EL.Pago)documento).CuentaCheque;
            sboPayments.Remarks = "";
            sboPayments.JournalRemarks = "";
            sboPayments.ProjectCode = pago.CodigoProyecto;
            //if (!string.IsNullOrWhiteSpace(pago.TipoRendicion)) sboPayments.UserFields.Fields.Item("U_BPP_TIPR").Value = pago.TipoRendicion;
            //if (!string.IsNullOrWhiteSpace(pago.SerieRendicion)) sboPayments.UserFields.Fields.Item("U_BPP_CCHI").Value = pago.SerieRendicion;
            //if (!string.IsNullOrWhiteSpace(pago.CorrelativoRendicion)) sboPayments.UserFields.Fields.Item("U_BPP_NUMC").Value = pago.CorrelativoRendicion;
            //lo_PgoEfc.UserFields.Fields.Item("U_BPP_PtFC").Value = "";
            //if (!string.IsNullOrWhiteSpace(pago.MedioPagoSUNAT)) sboPayments.UserFields.Fields.Item("U_BPP_MPPG").Value = pago.MedioPagoSUNAT;
            switch (pago.MetodoPago.Tipo)
            {
                case "CB": //Pago con cheque
                    var sucursalBanco = ObtenerSucursaCtaBanco(pago.MetodoPago.Banco, pago.MetodoPago.Cuenta);
                    sboPayments.Checks.AccounttNum = sucursalBanco.Item1;
                    sboPayments.Checks.BankCode = pago.MetodoPago.Banco;
                    sboPayments.Checks.Branch = sucursalBanco.Item2;
                    sboPayments.Checks.CheckAccount = pago.MetodoPago.Cuenta;
                    //if (lo_DBDSCCHAPR.GetValue(gs_UflChqMnl, 0).Trim() != "Y")
                    //{
                    //    lo_PgoEfc.Checks.ManualCheck = SAPbobsCOM.BoYesNoEnum.tNO;
                    //}
                    //else
                    //{
                    //    lo_PgoEfc.Checks.ManualCheck = SAPbobsCOM.BoYesNoEnum.tYES;
                    //    lo_PgoEfc.Checks.CheckNumber = Convert.ToInt32(lo_DBDSCCHAPR.GetValue(gs_UflChqNum, 0).Trim());
                    //}
                    sboPayments.Checks.CheckSum = pago.Monto;
                    sboPayments.Checks.CountryCode = "PE";
                    //lo_PgoEfc.Checks.DueDate = DateTime.ParseExact(lo_DBDSCCHAPR.GetValue(gs_UflChqFchVnc, 0).Trim(), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    sboPayments.Checks.Trnsfrable = SAPbobsCOM.BoYesNoEnum.tNO;
                    //Cash Flow
                    //if (lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim() != string.Empty)
                    //{
                    //    if (lo_DBDSCCHAPR.GetValue(gs_UflMndCaja, 0).Trim() == Cls_Global.sb_ObtenerMonedaLocal())
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountLC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    else
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountFC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    lo_PgoEfc.PrimaryFormItems.CheckNumber = "0";
                    //    lo_PgoEfc.PrimaryFormItems.CashFlowLineItemID = Convert.ToInt32(lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim());
                    //    lo_PgoEfc.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtChecks;
                    //    lo_PgoEfc.PrimaryFormItems.Add();
                    //}
                    break;

                case "CG":
                case "PV":
                case "TB"://Pago con Transferencia
                    sboPayments.TransferAccount = pago.MetodoPago.Cuenta;
                    sboPayments.TransferDate = pago.FechaContabilizacion;
                    sboPayments.TransferReference = pago.MetodoPago.Referencia;
                    sboPayments.TransferSum = pago.Monto;
                    //Cash Flow
                    //if (lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim() != string.Empty)
                    //{
                    //    if (lo_DBDSCCHAPR.GetValue(gs_UflMndCaja, 0).Trim() == Cls_Global.sb_ObtenerMonedaLocal())
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountLC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    else
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountFC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    lo_PgoEfc.PrimaryFormItems.CashFlowLineItemID = Convert.ToInt32(lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim());
                    //    lo_PgoEfc.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtBankTransfer;
                    //    lo_PgoEfc.PrimaryFormItems.Add();
                    //}
                    break;

                case "3"://Pago en Efectivo
                    sboPayments.CashAccount = pago.MetodoPago.Cuenta;
                    sboPayments.CashSum = pago.Monto;
                    //Para aperturas con caja chica
                    //lo_PgoEfc.UserFields.Fields.Item("U_BPP_CCHI").Value = lo_DBDSCCHAPR.GetValue("U_CC_CCHOC", 0).Trim();
                    //lo_PgoEfc.UserFields.Fields.Item("U_BPP_NUMC").Value = lo_DBDSCCHAPR.GetValue("U_CC_CCHON", 0).Trim();
                    //Cash Flow
                    //if (lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim() != string.Empty)
                    //{
                    //    if (lo_DBDSCCHAPR.GetValue(gs_UflMndCaja, 0).Trim() == Cls_Global.sb_ObtenerMonedaLocal())
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountLC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    else
                    //    {
                    //        lo_PgoEfc.PrimaryFormItems.AmountFC = Convert.ToDouble(lo_DBDSCCHAPR.GetValue(gs_UflMntTotApr, 0).Trim());
                    //    }
                    //    lo_PgoEfc.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtCash;
                    //    lo_PgoEfc.PrimaryFormItems.CashFlowLineItemID = Convert.ToInt32(lo_DBDSCCHAPR.GetValue(gs_UflCshFlw, 0).Trim());
                    //    lo_PgoEfc.PrimaryFormItems.Add();
                    //}
                    break;
            }

            pago.Detalle.ToList().ForEach(d =>
            {
                sboPayments.Invoices.Add();
                sboPayments.Invoices.SetCurrentLine(row);
                sboPayments.Invoices.InvoiceType = (SAPbobsCOM.BoRcptInvTypes)d.TipoDocumento;
                sboPayments.Invoices.DocEntry = d.IdDocumento;
                sboPayments.Invoices.DocLine = d.IdLinea;
                sboPayments.Invoices.InstallmentId = d.NroCuota;
                sboPayments.Invoices.SumApplied = pago.Moneda.Equals("SOL") ? d.MontoPagado : default(double);
                sboPayments.Invoices.AppliedFC = !pago.Moneda.Equals("SOL") ? d.MontoPagado : default(double);
                row++;
            });

            if (sboPayments.Add() != 0) throw new InvalidOperationException($"{ Globales.Company.GetLastErrorCode()}-{ Globales.Company.GetLastErrorDescription()}");
            var rslt = 0;
            return int.TryParse(Globales.Company.GetNewObjectKey(), out rslt) ? rslt : 0;
        }

        public static IEnumerable<SBOPago> ObtenerListaPagos(SAPbouiCOM.DBDataSource dbsCab, object obj)
        {
            XDocument xDoc = null;

            try
            {
                var rsltNroLinea = 0;
                var rsltNroCuota = 0;
                var fechaPago = DateTime.ParseExact(dbsCab.GetValue("U_EXP_FECHAPAGO", 0).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
                var codSeriePago = Convert.ToInt32(dbsCab.GetValue("U_EXP_SERIEPAGO", 0).Trim());
                var codSerieRtcn = Convert.ToInt32(dbsCab.GetValue("U_EXP_SERIERETENCION", 0).Trim());
                var refTransf = dbsCab.GetValue("U_EXP_NMROREFTRANS", 0).Trim();
                xDoc = XDocument.Parse((string)obj);
                var xElements = xDoc.XPathSelectElements("dbDataSources/rows/row").Where(w => w.Descendants("cell")
               .Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_PAGO") && a.Element("value").Value.Equals("Y"))
               && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_RETENCION") && a.Element("value").Value.Equals("N"))
               && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_ESTADO") && a.Element("value").Value != "OK"));
                return xElements.Descendants("cells").GroupBy(g => new
                {
                    CardCode = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("CARDCODE")).FirstOrDefault().Element("value").Value,
                    MedioDePago = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("MEDIODEPAGO")).FirstOrDefault()?.Element("value").Value,
                    Moneda = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("MONEDA")).FirstOrDefault()?.Element("value").Value,
                    Banco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODBANCO")).FirstOrDefault()?.Element("value").Value,
                    CtaBanco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODCTABANCO")).FirstOrDefault()?.Element("value").Value,
                    AplSerieRetencion = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_APLSRERTN")).FirstOrDefault()?.Element("value").Value
                }).Select(s => new SBOPago
                {
                    CodSerieSBO = s.Key.AplSerieRetencion == "Y" ? codSerieRtcn : codSeriePago,
                    CodigoSN = s.Key.CardCode,
                    Moneda = s.Key.Moneda,
                    FechaContabilizacion = fechaPago,
                    FechaDocumento = fechaPago,
                    FechaVencimiento = fechaPago,
                    Monto = s.Sum(sm => Convert.ToDouble(sm.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPORTE")).FirstOrDefault()?.Element("value").Value)),
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
                        TipoDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_TIPODOC")).FirstOrDefault()?.Element("value").Value),
                        IdDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_DOCENTRYDOC")).FirstOrDefault()?.Element("value").Value),
                        IdLinea = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_ASNROLINEA")).FirstOrDefault()?.Element("value").Value, out rsltNroLinea) ? rsltNroLinea : 0,
                        NroCuota = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_NMROCUOTA")).FirstOrDefault()?.Element("value").Value, out rsltNroCuota) ? rsltNroCuota : 0,
                        MontoPagado = Convert.ToDouble(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPORTE")).FirstOrDefault()?.Element("value").Value)
                    })
                });
            }
            finally { }
        }

        public static void GenerarTXTBancos(int docEntry, string codBanco, string codMoneda)
        {
            var nombre = @"C:\PagosMasivos\";
            nombre = nombre + "ArchivoBanco-" + codBanco + "-" + codMoneda + "-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";
            var archivo = new System.IO.StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            switch (codBanco)
            {
                case "002":

                    dtoBancoBCP _dtoBancoBCP = new dtoBancoBCP();
                    List<dtoBancoBCPDetalle> _listDetalleBCP = new List<dtoBancoBCPDetalle>();

                    string LineaCabeceraBCP = "";
                    string LineaDetalleBCP = "";

                    _dtoBancoBCP = ObtenerDatosCabArchivoTXTBCP(docEntry).FirstOrDefault();
                    _listDetalleBCP = ObtenerDatosDetArchivoTXTBCP(docEntry).ToList();

                    LineaCabeceraBCP = "#1P" + _dtoBancoBCP.TipoCuenta + _dtoBancoBCP.CuentaCargo +
                        _dtoBancoBCP.Moneda +
                        (_dtoBancoBCP.Montototal).Replace(".", "") +
                        _dtoBancoBCP.FechaProceso +
                        "                    " + _dtoBancoBCP.Cadena + (_listDetalleBCP.Count()).ToString("D6") +
                        "                0";

                    archivo.WriteLine(LineaCabeceraBCP);

                    for (int i = 0; i < _listDetalleBCP.Count; i++)
                    {
                        LineaDetalleBCP = " " + _listDetalleBCP[i].TipoRegistro +
                                _listDetalleBCP[i].TipoCuenta +
                                _listDetalleBCP[i].CuentaAbono +

                                _listDetalleBCP[i].NombreProveedor +

                                _listDetalleBCP[i].Moneda +
                                (_listDetalleBCP[i].Importetotal).Replace(".", "") +
                                _listDetalleBCP[i].TipoDocumentoIdentidad +
                                _listDetalleBCP[i].NumeroDocumentoIdentidad +
                                _listDetalleBCP[i].TipoDocumentoPagar +
                                _listDetalleBCP[i].NumeroDocumento +
                                "00" + _listDetalleBCP[i].ValidacionIDC;

                        archivo.WriteLine(LineaDetalleBCP);
                    }

                    archivo.Close();
                    break;

                case "003":

                    dtoBancoBCP _dtoBanco = new dtoBancoBCP();
                    List<dtoBancoBCPDetalle> _listDetalle = new List<dtoBancoBCPDetalle>();

                    string LineaCabecera = "";
                    string LineaDetalle = "";

                    _listDetalle = ObtenerDatosDetArchivoTXTInterBank(docEntry).ToList();

                    /*Detalles*/

                    for (int i = 0; i < _listDetalle.Count; i++)
                    {
                        string tipo = "";
                        if (_listDetalle[i].TipoCuenta == "I") //cuenta interbancara
                        {
                            tipo = "109";
                            LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2 +
                                        _listDetalle[i].TipoDocumentoPagar +
                                        _listDetalle[i].NumeroDocumento +
                                        "          " + _listDetalle[i].FechaVencimiento +
                                        _listDetalle[i].Moneda +
                                        (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                        " " + "99" +
                                        "   " + //espaco de tipo de cuenta
                                        "  " + //espacio de moneda cuando es interbancario
                                        "   " + //espacio de ofinia a la que pertenece
                                        _listDetalle[i].CuentaAbono +
                                        _listDetalle[i].TipoPersona + _listDetalle[i].TipoDocumentoIdentidad +
                                        _listDetalle[i].NumeroDocumentoIdentidad +
                                        _listDetalle[i].NombreProveedor + "000000000000000";
                            ;
                        }
                        else  //cuenta normla
                        {
                            tipo = "009";
                            LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2 +
                                        _listDetalle[i].TipoDocumentoPagar +
                                        _listDetalle[i].NumeroDocumento +
                                        "          " + _listDetalle[i].FechaVencimiento +
                                        _listDetalle[i].Moneda +
                                        (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                        " " + "09" +
                                        "001" +
                                        _listDetalle[i].Moneda + "200" +
                                        _listDetalle[i].CuentaAbono +
                                        _listDetalle[i].TipoPersona + _listDetalle[i].TipoDocumentoIdentidad + _listDetalle[i].NumeroDocumentoIdentidad +
                                        _listDetalle[i].NombreProveedor + "000000000000000";
                        }
                        archivo.WriteLine(LineaDetalle);
                    }
                    archivo.Close();

                    break;

                case "009":

                    dtoBancoBCP _dtoBancoSC = new dtoBancoBCP();
                    List<dtoBancoBCPDetalle> _listDetalleSC = new List<dtoBancoBCPDetalle>();

                    string LineaCabeceraSC = "";
                    string LineaDetalleSC = "";

                    //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                    _listDetalleSC = ObtenerDatosDetArchivoTXTScotiaBank(docEntry).ToList();

                    for (int i = 0; i < _listDetalleSC.Count; i++)
                    {
                        string FormaPago = "";
                        if (_listDetalleSC[i].TipoCuenta == "I")
                        {
                            FormaPago = "4";

                            LineaDetalleSC = _listDetalleSC[i].NumeroDocumentoIdentidad +
                                        _listDetalleSC[i].NombreProveedor +
                                        _listDetalleSC[i].NumeroDocumento +
                                        _listDetalleSC[i].FechaVencimiento +
                                        (_listDetalleSC[i].Importetotal).Replace(".", "") +
                                        FormaPago +
                                       "                                                             "
                                       + _listDetalleSC[i].CuentaAbonoCCI + _listDetalleSC[i].Moneda + "01";
                        }
                        else //nomral
                        {
                            FormaPago = "2";

                            LineaDetalleSC = _listDetalleSC[i].NumeroDocumentoIdentidad +
                                       _listDetalleSC[i].NombreProveedor +
                                       _listDetalleSC[i].NumeroDocumento +
                                       _listDetalleSC[i].FechaVencimiento +
                                       (_listDetalleSC[i].Importetotal).Replace(".", "") +
                                       FormaPago +
                                       _listDetalleSC[i].CuentaAbono + "                                                                       " + _listDetalleSC[i].Moneda + "01";
                        }

                        archivo.WriteLine(LineaDetalleSC);
                    }
                    archivo.Close();

                    break;

                case "022":

                    dtoBancoBCP _dtoBancoSA = new dtoBancoBCP();
                    List<dtoBancoBCPDetalle> _listDetalleSA = new List<dtoBancoBCPDetalle>();

                    string LineaCabeceraSA = "";
                    string LineaDetalleSA = "";

                    //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                    _listDetalleSA = ObtenerDatosDetArchivoTXTSantander(docEntry).ToList();

                    for (int i = 0; i < _listDetalleSA.Count; i++)
                    {
                        string FormaPago = "";
                        if (_listDetalleSA[i].TipoCuenta == "I")
                        {
                            FormaPago = "03";

                            LineaDetalleSA = _listDetalleSA[i].TipoDocumentoIdentidad + _listDetalleSA[i].NumeroDocumentoIdentidad +
                                   _listDetalleSA[i].TipoDocumentoPagar +
                                   _listDetalleSA[i].NumeroDocumento +
                                   _listDetalleSA[i].Moneda +
                                    _listDetalleSA[i].ImporteParcial +
                                   _listDetalleSA[i].FechaVencimiento + "Confirming" +
                                   "                    " + FormaPago + "          " + _listDetalleSA[i].CuentaAbonoCCI +
                                   _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor +
                                    "CONFIRMING";
                        }
                        else
                        { //normal
                            FormaPago = "01";

                            LineaDetalleSA = _listDetalleSA[i].TipoDocumentoIdentidad + _listDetalleSA[i].NumeroDocumentoIdentidad +
                                   _listDetalleSA[i].TipoDocumentoPagar +
                                   _listDetalleSA[i].NumeroDocumento +
                                   _listDetalleSA[i].Moneda +
                                    _listDetalleSA[i].ImporteParcial +
                                   _listDetalleSA[i].FechaVencimiento + "Confirming" +
                                   "                    " + FormaPago + _listDetalleSA[i].CuentaAbono +
                                    _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor +
                                    "CONFIRMING";
                        }
                        archivo.WriteLine(LineaDetalleSA);
                    }
                    archivo.Close();
                    /*
                    oForm.Freeze(false);
                    oButton.Item.Enabled = true;
                    oButton.Caption = "Generar TXT";

                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    Process.Start(nombre);
                    */

                    break;
            }
        }

        private static IEnumerable<dtoBancoBCP> ObtenerDatosCabArchivoTXTBCP(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_BCP_CABECERA_NUEVO('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCP
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    CantidadAbonos = dc["CantidadAbonos"].ToString(),
                    FechaProceso = dc["FechaProceso"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaCargo = dc["CuentaCargo"].ToString(),
                    Montototal = dc["Montototal"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    Validacion = dc["Validacion"].ToString(),
                    Cadena = dc["Cadena"].ToString(),
                    Moneda = dc["Moneda"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTBCP(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_BCP_DETALLE_NUEVO('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidarIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTInterBank(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_INTERBANK_DETALLE_NUEVO('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad2 = dc["NumeroDocumentoIdentidad1"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    TipoPersona = dc["TipoPersona"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTScotiaBank(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_SCOTIABANK_DETALLE('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    FormaPago = dc["FormaPago"].ToString(),
                    CuentaAbonoCCI = dc["CuentaAbonoCCI"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTSantander(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_SANTANDER_DETALLE('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    TipoPersona = dc["TipoPersona"].ToString(),
                    CuentaAbonoCCI = dc["CuentaAbonoCCI"].ToString()
                };
            });
            return rslt;
        }
    }
}