using Microsoft.Practices.EnterpriseLibrary.Logging;
using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbobsCOM;
using SMC_APM.dao;
using SMC_APM.Modelo;
using SMC_APM.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace SMC_APM.Controller
{
    internal class PagoMasivoController
    {
        public static SAPbobsCOM.Recordset ObtenerSeriesDocumentoPago()
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and coalesce(\"U_EXC_CR\",'N') = 'N'";
            recordset.DoQuery(sqlQry);
            if (recordset.EoF)
                return null;
            else
                return recordset;
        }

        public static SAPbobsCOM.Recordset ObtenerSeriesRetencion()
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and coalesce(\"U_EXC_CR\",'') = 'Y'";
            recordset.DoQuery(sqlQry);
            if (recordset.RecordCount == 0)
                throw new Exception("No se han configurado series de retención, validar su configuración en el formulario numeración de documentos");
            else
                return recordset;
        }

        public static SAPbobsCOM.Recordset ObtenerInfoBancos()
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select \"BankCode\",\"BankName\" from ODSC";
            recordset.DoQuery(sqlQry);
            if (recordset.RecordCount == 0)
                throw new Exception("No se ha definido información de bancos");
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

        public static Tuple<string, string, string, string> ObtenerSucursalCtaBanco(string codBanco, string codCta)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            //var sqlQry = $"select top 1 \"Account\",\"Branch\" from DSC1 where \"BankCode\" = '{codBanco}' and \"GLAccount\" = '{codCta}';";
            var sqlQry = $"select top 1 \"Account\",\"Branch\",\"BankCode\",\"Country\" from DSC1 where \"GLAccount\" = '{codCta}';";
            recordset.DoQuery(sqlQry);
            if (recordset.EoF)
                return null;
            else
                return Tuple.Create(recordset.Fields.Item(0).Value, recordset.Fields.Item(1).Value
                    , recordset.Fields.Item(2).Value, recordset.Fields.Item(3).Value);
        }

        public static IEnumerable<PMPDocumento> ListarDocumentosParaPagos(string fecha, int codSucursal)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"EXEC EXP_SP_PMP_ListarDocumentosPorFecha '{fecha}','{codSucursal}'";
            if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                sqlQry = $"CALL EXP_SP_PMP_ListarDocumentosPorFecha('{fecha}','{codSucursal}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new PMPDocumento
                {
                    SlcPago = dc["SlcPago"],
                    SlcRetencion = dc["SlcRetencion"],
                    CodSucursal = Convert.ToInt32(dc["CodSucursal"]),
                    CodigoEscenarioPago = dc["CodEscenarioPago"],
                    NumeroEscenarioPago = dc["NumEscenarioPago"],
                    DescripcionEscenarioPago = dc["DscEscenarioPago"],
                    MedioDePago = dc["MedioDePago"],
                    MonedaDePago = dc["MonedaDePago"],
                    CodBanco = dc["CodBanco"],
                    CodCtaBanco = dc["CodCtaBanco"],
                    DocEntryDocumento = Convert.ToInt32(dc["DocEntryDocumento"]),
                    TipoDocumento = dc["TipoDocumento"],
                    NroDocumentoSUNAT = dc["NroDocSUNAT"],
                    CardCode = dc["CardCode"],
                    CardName = dc["CardName"],
                    Moneda = dc["Moneda"],
                    Importe = Convert.ToDouble(dc["Importe"]),
                    NroCuota = dc["NroCuota"].Trim(),
                    NroLineaAsiento = dc["NroLineaAsiento"].Trim(),
                    NroDocumentoSN = dc["NroDocumentoSN"].Trim(),
                    AplSreRetencion = dc["AplSerieRetencion"].Trim(),
                    NroCtaProveedor = dc["NroCtaProveedor"].Trim(),
                    CodBncProveedor = dc["CodBncProveedor"].Trim(),
                    CodRetencion = dc["CodRetencion"].Trim(),
                    ImporteRetencion = Convert.ToDouble(dc["MontoRetencion"]),
                    TCDocumento = Convert.ToDouble(dc["TCDocumento"]),
                    GlosaAsiento = dc["GlosaAsiento"],
                    CardCodeFactoring = dc["CardCodeFacto"],
                    CardNameFactoring = dc["CardNameFacto"]
                };
            });
            return rslt;
        }

        private static IEnumerable<TXT3Retenedor> ObtenerDatosTerceroRetenedor(int docEntryPMP)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"EXEC SMC_TERCERORETENEDOR '{docEntryPMP}'";
            if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                sqlQry = $"CALL SMC_TERCERORETENEDOR('{docEntryPMP}')";
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

        public static int ObtenerCorrelativoTXT3Retenedor(string fechaPago)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select count('A') as \"CNT\" from \"@EXP_OPMP\" where \"U_EXP_FECHAPAGO\" = '{fechaPago}'";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc => { return Convert.ToInt32(dc["CNT"]); }).FirstOrDefault();
            return rslt == 0 ? 1 : rslt;
        }

        public static void generarTXT3Retenedor(int docEntryPMP, string fechaPago)
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
                var correlativoTXT = ObtenerCorrelativoTXT3Retenedor(fechaPago);

                var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

                var qry = "select top 1 \"AttachPath\" from OADP";
                recordset.DoQuery(qry);
                var attachPath = recordset.Fields.Item(0).Value;
                if (string.IsNullOrWhiteSpace(attachPath)) throw new InvalidOperationException("No se ha definido ruta de anexos en SAP");

                //Generando el archivo
                //nombreArchivo = @"\\WIN-SOPORTESAP\DocSap\TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = attachPath + @"TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = nombreArchivo + $"RCP{fechaPago}{correlativoTXT.ToString().PadLeft(3, '0')}.txt";
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

        public static IEnumerable<TXT3RetenedorRsp> LeerTXT3RetenedorRsp(string filePath, int docEntry)
        {
            if (File.Exists(filePath))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var sqlQry = $"CALL SMC_TERCERORETENEDOR('{docEntry}')";
                var lstTerReten = QueryResultManager.executeQueryAsType(sqlQry, dc =>
                {
                    return new
                    {
                        RUC = dc["RUC"],
                        Documentos = dc["Documentos"]
                    };
                });
                if (lstTerReten.Count() == 0) throw new InvalidOperationException("No se obtuvieron datos BD");

                var fileLines = File.ReadAllLines(filePath).Select(s =>
                {
                    return new
                    {
                        RUC = s.Split('|')[0].Trim(),
                        Estado = s.Split('|')[3].Trim()
                    };
                });

                if (fileLines.Count() == 0) throw new InvalidOperationException("No se obtuvieron datos TXT");
                foreach (var terReten in lstTerReten)
                {
                    var lstDocumentos = terReten.Documentos.Split('|').ToList();
                    foreach (var doc in lstDocumentos)
                    {
                        yield return new TXT3RetenedorRsp
                        {
                            RUC = terReten.RUC,
                            TipoDocumento = Convert.ToInt32(doc.Split('-')[0]),
                            IdDocumento = Convert.ToInt32(doc.Split('-')[1]),
                            Estado = fileLines.FirstOrDefault(fl => fl.RUC == terReten.RUC).Estado
                        };
                    }
                }
            }
            else
                yield return null;
        }

        public static int GenerarPagoEfectuadoSBO(SBOPago pago, bool tieneSucursales)
        {
            var row = 0;
            var sboPayments = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
            var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(BoObjectTypes.BoBridge);
            var tblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
            var mndLoc = sboBOB.GetLocalCurrency().Fields.Item(0).Value;
            sboPayments.BPLID = pago.CodSucursal;
            sboPayments.Series = pago.CodSerieSBO;
            sboPayments.CardCode = pago.CodigoSN;
            sboPayments.DocDate = pago.FechaContabilizacion;
            sboPayments.DueDate = pago.FechaVencimiento;
            sboPayments.TaxDate = pago.FechaDocumento;
            sboPayments.DocCurrency = pago.Moneda;
            if (pago.Moneda != mndLoc) sboPayments.DocRate = pago.TipoCambio;
            sboPayments.DocType = pago.CodigoSN.StartsWith("C") ? BoRcptTypes.rCustomer : BoRcptTypes.rSupplier;
            //if (pago.ObjType == 24) sboPayments.CheckAccount = ((EL.Pago)documento).CuentaCheque;
            sboPayments.Remarks = "";
            sboPayments.JournalRemarks = "";
            if (!string.IsNullOrWhiteSpace(pago.CodigoProyecto)) sboPayments.ProjectCode = pago.CodigoProyecto;
            switch ("NN")//pago.MetodoPago.Tipo)
            {
                case "CB": //Pago con cheque
                    var sucursalBanco = ObtenerSucursalCtaBanco(pago.MetodoPago.Banco, pago.MetodoPago.Cuenta);
                    sboPayments.Checks.AccounttNum = sucursalBanco.Item1;
                    sboPayments.Checks.BankCode = pago.MetodoPago.Banco;
                    sboPayments.Checks.Branch = sucursalBanco.Item2;
                    sboPayments.Checks.CheckAccount = pago.MetodoPago.Cuenta;

                    sboPayments.Checks.CheckSum = pago.Monto;
                    sboPayments.Checks.CountryCode = sucursalBanco.Item4;
                    sboPayments.Checks.Trnsfrable = SAPbobsCOM.BoYesNoEnum.tNO;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPCHEQUE").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;
                case "CG":
                case "PV":
                case "TB":
                case "VV"://Pago con Transferencia
                    sboPayments.TransferAccount = pago.MetodoPago.Cuenta;
                    sboPayments.TransferDate = pago.FechaContabilizacion;
                    sboPayments.TransferReference = pago.Moneda == mndLoc ? pago.MetodoPago.Referencia : pago.MetodoPago.ReferenciaME;
                    sboPayments.TransferSum = pago.Monto;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;

                case "NN"://Pago en Efectivo
                    sboPayments.CashAccount = ObtenerCodCuentaPuentePorSucursal(pago.CodSucursal, tieneSucursales);
                    sboPayments.CashSum = pago.Monto;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPFONDEF").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
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
                sboPayments.Invoices.SumApplied = d.MonedaDoc.Equals(mndLoc) ? d.MontoAPagar : default(double);
                sboPayments.Invoices.AppliedFC = !d.MonedaDoc.Equals(mndLoc) ? d.MontoAPagar : default(double);
                row++;
            });

            if (sboPayments.Add() != 0) throw new InvalidOperationException($"{Globales.Company.GetLastErrorCode()}-{Globales.Company.GetLastErrorDescription()}");
            var rslt = 0;
            return int.TryParse(Globales.Company.GetNewObjectKey(), out rslt) ? rslt : 0;
        }

        public static int GenerarPagoEfectuadoSBODesdeDraft(int idDraft, SBOPago pago, bool tieneSucursales)
        {
            var tblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
            var sboPaymentDraft = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(BoObjectTypes.oPaymentsDrafts);
            var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(BoObjectTypes.BoBridge);
            var mndLoc = sboBOB.GetLocalCurrency().Fields.Item(0).Value;
            sboPaymentDraft.GetByKey(idDraft);
            sboPaymentDraft.DocDate = pago.FechaContabilizacion;
            sboPaymentDraft.TaxDate = pago.FechaDocumento;
            sboPaymentDraft.DueDate = pago.FechaVencimiento;
            if (sboPaymentDraft.DocCurrency != mndLoc) sboPaymentDraft.DocRate = pago.TipoCambio;
            switch (pago.MetodoPago.Tipo)
            {
                case "CB": //Pago con cheque
                    var sucursalBanco = ObtenerSucursalCtaBanco(pago.MetodoPago.Banco, pago.MetodoPago.Cuenta);
                    sboPaymentDraft.Checks.AccounttNum = sucursalBanco.Item1;
                    sboPaymentDraft.Checks.BankCode = pago.MetodoPago.Banco;
                    sboPaymentDraft.Checks.Branch = sucursalBanco.Item2;
                    sboPaymentDraft.Checks.CheckAccount = pago.MetodoPago.Cuenta;
                    sboPaymentDraft.Checks.Trnsfrable = SAPbobsCOM.BoYesNoEnum.tNO;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPaymentDraft.UserFields.Fields.Item("U_EXX_MPCHEQUE").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;
                case "CG":
                case "PV":
                case "TB"://Pago con Transferencia
                    var codCtaTrnsf = ObtenerCodCuentaPuentePorSucursal(pago.CodSucursal, tieneSucursales);
                    sboPaymentDraft.TransferAccount = codCtaTrnsf;
                    sboPaymentDraft.TransferReference = pago.MetodoPago.Referencia;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPaymentDraft.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;

                case "NN"://Pago en Efectivo
                    var codCtaEfect = ObtenerCodCuentaPuentePorSucursal(pago.CodSucursal, tieneSucursales);
                    sboPaymentDraft.CashAccount = codCtaEfect;
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPaymentDraft.UserFields.Fields.Item("U_EXX_MPFONDEF").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;
            }
            if (sboPaymentDraft.Update() != 0) throw new InvalidOperationException($"{Globales.Company.GetLastErrorCode()}-{Globales.Company.GetLastErrorDescription()}");
            if (sboPaymentDraft.SaveDraftToDocument() != 0) throw new InvalidOperationException($"{Globales.Company.GetLastErrorCode()}-{Globales.Company.GetLastErrorDescription()}");
            var rslt = 0;
            return int.TryParse(Globales.Company.GetNewObjectKey(), out rslt) ? rslt : 0;
        }

        public static IEnumerable<SBOPago> ObtenerListaPagos(SAPbouiCOM.DBDataSource dbsCab, object obj, SAPbouiCOM.DBDataSource dbsSre, bool esAgenteRetenedor, bool esHostToHost)
        {
            XDocument xDoc = null;

            try
            {
                var rsltNroLinea = 0;
                var rsltNroCuota = 0;
                var fechaPago = DateTime.ParseExact(dbsCab.GetValue("U_EXP_FECHAPAGO", 0).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
                var tipoDeCambio = dbsCab.GetValueExt("U_EXP_TIPODECAMBIO");
                var codSeriePago = int.TryParse(dbsCab.GetValue("U_EXP_SERIEPAGO", 0).Trim(), out var codSreAux) ? codSreAux : 0;
                var codSerieRtcn = esAgenteRetenedor ? (int.TryParse(dbsCab.GetValue("U_EXP_SERIERETENCION", 0).Trim(), out var codSreRtnAux) ? codSreRtnAux : 0) : 0;
                var codSlcSucursal = Convert.ToInt32(dbsCab.GetValue("U_EXP_COD_SUCURSAL", 0));
                var refTransf = dbsCab.GetValue("U_EXP_NMROREFTRANS", 0).Trim();
                var refTransfME = dbsCab.GetValue("U_EXP_NROREF_ME", 0).Trim();
                xDoc = XDocument.Parse((string)obj);
                IEnumerable<XElement> xElements = null;
                if (esHostToHost)
                {
                    xElements = xDoc.XPathSelectElements("dbDataSources/rows/row").Where(w => w.Descendants("cell")
                    .Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_PAGO") && a.Element("value").Value.Equals("Y"))
                    && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_ESTADO_H2H") && a.Element("value").Value.Equals("OK"))
                    && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_RETENCION") && a.Element("value").Value.Equals("N"))
                    && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_ESTADO") && a.Element("value").Value != "OK"));
                }
                else
                {
                    xElements = xDoc.XPathSelectElements("dbDataSources/rows/row").Where(w => w.Descendants("cell")
                    .Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_PAGO") && a.Element("value").Value.Equals("Y"))
                    && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_RETENCION") && a.Element("value").Value.Equals("N"))
                    && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_ESTADO") && a.Element("value").Value != "OK"));
                }

                return xElements.Descendants("cells").GroupBy(g => new
                {
                    CardCode = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("CARDCODE")).FirstOrDefault().Element("value").Value,
                    MedioDePago = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("MEDIODEPAGO")).FirstOrDefault()?.Element("value").Value,
                    MonedaPago = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_MONEDA_PAGO")).FirstOrDefault()?.Element("value").Value,
                    CodSucursal = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_COD_SUCURSAL")).FirstOrDefault()?.Element("value").Value,
                    Banco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODBANCO")).FirstOrDefault()?.Element("value").Value,
                    CtaBanco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODCTABANCO")).FirstOrDefault()?.Element("value").Value,
                    AplSerieRetencion = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_APLSRERTN")).FirstOrDefault()?.Element("value").Value,
                    CardCodeFactoring = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CARDCODE_FACTO")).FirstOrDefault()?.Element("value").Value,
                }).Select(s => new SBOPago
                {
                    CodSucursal = Convert.ToInt32(s.Key.CodSucursal),
                    CodSerieSBO = codSlcSucursal == -1 ? ObtenerSeriePagoPorSucursal(Convert.ToInt32(s.Key.CodSucursal), dbsSre, s.Key.AplSerieRetencion) : (s.Key.AplSerieRetencion == "Y" && esAgenteRetenedor ? codSerieRtcn : codSeriePago),
                    CodigoSN = s.Key.CardCode,
                    Moneda = s.Key.MonedaPago,
                    FechaContabilizacion = fechaPago,
                    FechaDocumento = fechaPago,
                    FechaVencimiento = fechaPago,
                    TipoCambio = Convert.ToDouble(tipoDeCambio),
                    Monto = s.Sum(sm => (Convert.ToDouble(sm.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPORTE")).FirstOrDefault()?.Element("value").Value)
                    /*- Convert.ToDouble(sm.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPRETENCION")).FirstOrDefault()?.Element("value").Value)*/)),
                    //ExtLineasDS = s.Select(s1 => Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("LineId")).FirstOrDefault()?.Element("value").Value)),
                    MetodoPago = new SBOMetodoPago
                    {
                        Tipo = s.Key.MedioDePago,
                        Pais = "PE",
                        Banco = s.Key.Banco,
                        Cuenta = s.Key.CtaBanco,
                        Referencia = refTransf,
                        ReferenciaME = refTransfME
                    },
                    Detalle = s.Select(s1 => new SBOPagoDetalle
                    {
                        TipoDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_TIPODOC")).FirstOrDefault()?.Element("value").Value),
                        IdDocumento = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_DOCENTRYDOC")).FirstOrDefault()?.Element("value").Value),
                        IdLinea = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_ASNROLINEA")).FirstOrDefault()?.Element("value").Value, out rsltNroLinea) ? rsltNroLinea : 0,
                        NroCuota = int.TryParse(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_NMROCUOTA")).FirstOrDefault()?.Element("value").Value, out rsltNroCuota) ? rsltNroCuota : 0,
                        MonedaDoc = s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_MONEDA")).FirstOrDefault()?.Element("value").Value,
                        MontoAPagar = Convert.ToDouble(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPORTE")).FirstOrDefault()?.Element("value").Value)
                        /*- Convert.ToDouble(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPRETENCION")).FirstOrDefault()?.Element("value").Value)*/,
                        LineaPgoMsv = Convert.ToInt32(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("LineId")).FirstOrDefault()?.Element("value").Value)
                        //MontoPagado = Convert.ToDouble(s1.Descendants("cell").Where(w => w.Element("uid").Value.Equals("U_EXP_IMPORTE")).FirstOrDefault()?.Element("value").Value)
                    })
                });
            }
            finally { }
        }

        public static void GenerarTXTBancos(int docEntry, string codBanco, int codSucursal, string codMoneda, string GLAccount, string codPais, string formatoScotia)
        {
            try
            {
                var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

                var qry = "select top 1 \"AttachPath\" from OADP";
                recordset.DoQuery(qry);
                var attachPath = recordset.Fields.Item(0).Value;
                if (string.IsNullOrWhiteSpace(attachPath)) throw new InvalidOperationException("No se ha definido ruta de anexos en SAP");
                qry = $"select max(\"Account\") from DSC1 where \"GLAccount\" = '{GLAccount}'";
                recordset.DoQuery(qry);
                var nroCuenta = recordset.Fields.Item(0).Value;
                var nombre = attachPath + @"PagosMasivos\";
                nombre = nombre + "ArchivoBanco-" + codBanco + "-" + codSucursal + "-" + codMoneda + "-" + nroCuenta + "-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";

                switch (codPais)
                {
                    case "PE":
                        switch (codBanco)
                        {
                            //PODRÍA SER UNA INTERFAZ
                            case "002":
                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL SBO_EXX_PM_BCP_DEV({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_BCP {docEntry},{codSucursal},'{GLAccount}'";
                                break;
                            case "003":
                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL SBO_EXX_PM_INTERBANK({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_INTERBANK {docEntry},{codSucursal},'{GLAccount}'";
                                break;
                            case "009":
                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL SBO_EXX_PM_SCOTIABANK({docEntry},{codSucursal},'{GLAccount}','{formatoScotia}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_SCOTIABANK {docEntry},{codSucursal},'{GLAccount}','{formatoScotia}'";
                                break;
                            case "011":
                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL SBO_EXX_PM_BBVA({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_BBVA {docEntry},{codSucursal},'{GLAccount}'";
                                break;
                            case "022":
                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL SBO_EXX_PM_SANTANDER({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_SANTANDER {docEntry},{codSucursal},'{GLAccount}'";
                                break;
                            default:
                                throw new Exception($"Código de banco {codBanco} no soportado");
                        }
                        break;
                    case "CL":
                        switch (codBanco)
                        {
                            //PODRÍA SER UNA INTERFAZ
                            case "001":
                                qry = $"CALL SBO_EXX_PM_BANCO_CHILE_CH({docEntry},'{GLAccount}')";
                                break;
                            case "014":
                                qry = $"CALL SBO_EXX_PM_SCOTIABANK_CH({docEntry},'{GLAccount}')";
                                break;
                            case "037":
                                qry = $"CALL SBO_EXX_PM_SANTANDER_CH({docEntry},'{GLAccount}')";
                                break;
                            default:
                                throw new Exception($"Código de banco {codBanco} no soportado");
                        }
                        break;
                    default:
                        throw new Exception($"Código de pais {codPais} no soportado");
                }

                string valorLinea = string.Empty;
                recordset.DoQuery(qry);
                using (StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252)))
                {
                    while (!recordset.EoF)
                    {
                        valorLinea = recordset.Fields.Item(0).Value;

                        archivo.WriteLine(valorLinea.Remove(valorLinea.Length - 1));
                        recordset.MoveNext();
                    }
                    archivo.Close();
                    archivo.Dispose();
                }
                Process.Start(nombre);
            }
            catch { throw; }
        }

        public static void GenerarTXTH2H(int docEntry, string codBanco, int codSucursal, string codMoneda, string GLAccount, string codPais)
        {
            try
            {
                var tblConfH2H = Globales.Company.UserTables.Item("EXD_PM_CONFH2H");
                var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

                var qry = "select top 1 \"AttachPath\" from OADP";
                recordset.DoQuery(qry);
                var attachPath = recordset.Fields.Item(0).Value;
                if (string.IsNullOrWhiteSpace(attachPath)) throw new InvalidOperationException("No se ha definido ruta de anexos en SAP");
                var rutaFisica = attachPath + @"PagosMasivos\";
                var ipFPT = string.Empty;
                var puertoFTP = default(int);
                var usuarioFTP = string.Empty;
                var passwordFTP = string.Empty;
                var rutaFldINFTP = string.Empty;
                var nombreArchivo = string.Empty;
                var rutaLlavePublica = string.Empty;
                var rutaLlavePrivada = string.Empty;

                switch (codPais)
                {
                    case "PE":
                        switch (codBanco)
                        {
                            case "002":
                                if (!tblConfH2H.GetByKey("001")) throw new InvalidOperationException("No se han definido los datos para el banco BCP");
                                ipFPT = tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value;
                                puertoFTP = Convert.ToInt32(tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value);
                                usuarioFTP = tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value;
                                passwordFTP = tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value;
                                rutaFldINFTP = tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value;

                                if (string.IsNullOrWhiteSpace(ipFPT)) throw new InvalidOperationException("No se ha definido la ip del banco BCP");

                                //nombreArchivo = $"P{DateTime.Today.ToString("yyyyMMdd")}{(string.Concat("000000", docEntry, codSucursal)).Substring(string.Concat("000000", docEntry, codSucursal).Length - 6)}P.txt";
                                //rutaFisica += nombreArchivo;
                                //rutaFldINFTP = Path.Combine(rutaFldINFTP, nombreArchivo);

                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL EXD_SP_PM_H2H_DATOS_TXT_BCP({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_BCP {docEntry},'{GLAccount}'";
                                break;
                            case "003":
                                if (!tblConfH2H.GetByKey("004")) throw new InvalidOperationException("No se han definido los datos para el banco Interbank");
                                ipFPT = tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value;
                                puertoFTP = Convert.ToInt32(tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value);
                                usuarioFTP = tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value;
                                passwordFTP = tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value;
                                rutaFldINFTP = tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value;
                                rutaLlavePrivada = tblConfH2H.UserFields.Fields.Item("U_RUTA_LLAVE_PRV").Value;

                                if (string.IsNullOrWhiteSpace(ipFPT)) throw new InvalidOperationException("No se ha definido la ip del banco Interbank");
                                var sqlQryExt = $"select U_COD_EMPRESA,U_USUARIO from \"@EXD_PM_CNFUSUH2H\" where U_COD_BANCO = '003' and U_COD_SUCURSAL = '{codSucursal}'";
                                recordset.DoQuery(sqlQryExt);
                                usuarioFTP = recordset.Fields.Item(1).Value;
                                var codEmpresa = recordset.Fields.Item(0).Value;

                                //nombreArchivo = $"H2HH00003AAED010000000000000000000000001_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                                //rutaFisica += nombreArchivo;
                                //rutaFldINFTP = Path.Combine(rutaFldINFTP, nombreArchivo);

                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL EXD_SP_PM_H2H_DATOS_TXT_INTERBANK({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_BCP {docEntry},'{GLAccount}'";
                                break;
                            case "009":
                                if (!tblConfH2H.GetByKey("003")) throw new InvalidOperationException("No se han definido los datos para el banco Scotia");
                                ipFPT = tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value;
                                puertoFTP = Convert.ToInt32(tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value);
                                usuarioFTP = tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value;
                                passwordFTP = tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value;
                                rutaFldINFTP = tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value;
                                rutaLlavePublica = tblConfH2H.UserFields.Fields.Item("U_RUTA_LLAVE_PUB").Value;

                                if (string.IsNullOrWhiteSpace(ipFPT)) throw new InvalidOperationException("No se ha definido la ip del banco Scotia");

                                //nombreArchivo = $"P{(string.Concat("000000000", docEntry, codSucursal)).Substring(string.Concat("000000000", docEntry, codSucursal).Length - 9)}.txt";
                                //rutaFisica += nombreArchivo;
                                //rutaFldINFTP = Path.Combine(rutaFldINFTP, nombreArchivo);

                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL EXD_SP_PM_H2H_DATOS_TXT_SCOTIABANK({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC SBO_EXX_PM_BCP {docEntry},'{GLAccount}'";
                                break;
                            case "011":
                                if (!tblConfH2H.GetByKey("002")) throw new InvalidOperationException("No se han definido los datos para el banco BBVA");
                                ipFPT = tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value;
                                puertoFTP = Convert.ToInt32(tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value);
                                usuarioFTP = tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value;
                                passwordFTP = tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value;
                                rutaFldINFTP = tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value;
                                rutaLlavePublica = tblConfH2H.UserFields.Fields.Item("U_RUTA_LLAVE_PUB").Value;

                                if (string.IsNullOrWhiteSpace(ipFPT)) throw new InvalidOperationException("No se ha definido la ip del banco BBVA");

                                //nombreArchivo = $"BUSI0000H2H_PE_{DateTime.Today.ToString("yyyyMMdd")}001.M03";
                                //rutaFisica += nombreArchivo;
                                //rutaFldINFTP = Path.Combine(rutaFldINFTP, nombreArchivo);

                                if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                                    qry = $"CALL EXD_SP_PM_H2H_DATOS_TXT_BBVA({docEntry},{codSucursal},'{GLAccount}')";
                                else
                                    qry = $"EXEC EXD_SP_PM_H2H_DATOS_TXT_BBVA {docEntry},{codSucursal},'{GLAccount}'";
                                break;
                            default:
                                throw new Exception($"Código de banco {codBanco} no soportado");
                        }
                        break;
                    case "CL":
                        switch (codBanco)
                        {
                            //PODRÍA SER UNA INTERFAZ
                            case "001":
                                qry = $"CALL SBO_EXX_PM_BANCO_CHILE_CH({docEntry},'{GLAccount}')";
                                break;
                            case "014":
                                qry = $"CALL SBO_EXX_PM_SCOTIABANK_CH({docEntry},'{GLAccount}')";
                                break;
                            case "037":
                                qry = $"CALL SBO_EXX_PM_SANTANDER_CH({docEntry},'{GLAccount}')";
                                break;
                            default:
                                throw new Exception($"Código de banco {codBanco} no soportado");
                        }
                        break;
                    default:
                        throw new Exception($"Código de pais {codPais} no soportado");
                }

                string valorLinea = string.Empty;
                recordset.DoQuery(qry);
                //Para BBVA obtengo el nombre desde la BD
                if (codBanco == "002" || codBanco == "003" || codBanco == "009")
                {
                    rutaFisica += $"{recordset.Fields.Item(1).Value.ToString()}.txt";
                    nombreArchivo = Path.GetFileName(rutaFisica);
                }
                else if (codBanco == "011")
                {
                    rutaFisica += $"{recordset.Fields.Item(1).Value.ToString()}.M03";
                    nombreArchivo = Path.GetFileName(rutaFisica);
                }
                //Inicio la generacion del txt
                using (StreamWriter archivo = new StreamWriter(rutaFisica, false, Encoding.GetEncoding(1252)))
                {
                    while (!recordset.EoF)
                    {
                        valorLinea = recordset.Fields.Item(0).Value;
                        archivo.WriteLine(valorLinea.Remove(valorLinea.Length - 1));
                        recordset.MoveNext();
                    }
                    archivo.Close();
                    archivo.Dispose();

                }

                var codeLogEnvio = GuardarEnvioH2HEnLog(docEntry, codBanco, codSucursal.ToString(), nombreArchivo);

                //Encriptacion
                if (codBanco == "011")
                {
                    var rutaDestinoAux = Path.Combine(Path.GetDirectoryName(rutaFisica), Path.GetFileName(rutaFisica) + ".pgp");
                    EncriptarArchivo(rutaLlavePublica, rutaFisica, rutaDestinoAux);
                    rutaFldINFTP = Path.Combine(rutaFldINFTP, Path.GetFileName(rutaDestinoAux));
                    HostToHostManager.SendToSFTP(ipFPT, puertoFTP, usuarioFTP, passwordFTP, rutaDestinoAux, rutaFldINFTP);
                }
                else if (codBanco == "009")
                {
                    var rutaDestinoAux = Path.Combine(Path.GetDirectoryName(rutaFisica), Path.GetFileNameWithoutExtension(rutaFisica) + ".gpg");
                    EncriptarArchivo(rutaLlavePublica, rutaFisica, rutaDestinoAux);
                    rutaFldINFTP = Path.Combine(rutaFldINFTP, Path.GetFileName(rutaDestinoAux));
                    HostToHostManager.SendToFTP(ipFPT, puertoFTP, usuarioFTP, passwordFTP, rutaDestinoAux, rutaFldINFTP);
                }
                else if (codBanco == "002")
                {
                    rutaFldINFTP = Path.Combine(rutaFldINFTP, Path.GetFileName(rutaFisica));
                    HostToHostManager.SendToSFTP(ipFPT, puertoFTP, usuarioFTP, passwordFTP, rutaFisica, rutaFldINFTP);
                }
                else if (codBanco == "003")
                {
                    rutaFldINFTP = rutaFldINFTP + "/" + Path.GetFileName(rutaFisica);
                    HostToHostManager.SendToSFTPWithKeySSH(ipFPT, puertoFTP, usuarioFTP, rutaLlavePrivada, rutaFisica, rutaFldINFTP);
                }
                ActualizarLogEstadoEnLogEnvio(codeLogEnvio, "EN");
            }
            catch { throw; }
        }

        public static IEnumerable<dynamic> ObtenerListaBancoPorPago(object obj)
        {
            XDocument xDoc = null;

            try
            {
                xDoc = XDocument.Parse((string)obj);
                var xElements = xDoc.XPathSelectElements("dbDataSources/rows/row").Where(w => w.Descendants("cell")
               .Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_PAGO") && a.Element("value").Value.Equals("Y"))
               && w.Descendants("cell").Any(a => a.Element("uid").Value.Equals("U_EXP_SLC_RETENCION") && a.Element("value").Value.Equals("N")));
                return xElements.Descendants("cells").GroupBy(g => new
                {
                    Moneda = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_MONEDA_PAGO")).FirstOrDefault()?.Element("value").Value,
                    Banco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODBANCO")).FirstOrDefault()?.Element("value").Value,
                    CodSucursal = Convert.ToInt32(g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_COD_SUCURSAL")).FirstOrDefault()?.Element("value").Value),
                    CtaBanco = g.Descendants("cell").Where(w => w.Element("uid").Value.Contains("U_EXP_CODCTABANCO")).FirstOrDefault()?.Element("value").Value
                }).Select(s => new { Banco = s.Key.Banco, Sucursal = s.Key.CodSucursal, CtaBanco = s.Key.CtaBanco, Moneda = s.Key.Moneda });
            }
            finally { }
        }

        public static void CrearAsientoAjusteRedondeo(int docEntryPM, int codSucursal, bool tieneSucursales)
        {
            var mntoRedondeo = 0.00;


            var sqlQry = $"EXEC EXD_SP_PM_OBTENER_DIFERENCIA_ME '{docEntryPM}','{codSucursal}'";
            if (Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB)
                sqlQry = $"CALL EXD_SP_PM_OBTENER_DIFERENCIA_ME('{docEntryPM}','{codSucursal}')";

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF) mntoRedondeo = Convert.ToDouble(recSet.Fields.Item(0).Value);

            if (mntoRedondeo != 0.00)
            {
                var asiento = (SAPbobsCOM.JournalEntries)Globales.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
                asiento.ReferenceDate = DateTime.Today;
                asiento.TaxDate = DateTime.Today;
                asiento.DueDate = DateTime.Today;
                asiento.Memo = "Asiento de ajuste de redondeo PM";

                var ctaPte = ObtenerCodCuentaPuentePorSucursal(codSucursal, tieneSucursales);
                var ctaRnd = ObtenerCodCuentaRedondeoPorSucursal(codSucursal, tieneSucursales);


                //Cuenta redondeo
                asiento.Lines.SetCurrentLine(0);
                asiento.Lines.BPLID = codSucursal;
                asiento.Lines.AccountCode = mntoRedondeo > 0 ? ctaRnd : ctaPte;
                asiento.Lines.Debit = Math.Abs(mntoRedondeo);

                //Cuenta puente
                asiento.Lines.Add();
                asiento.Lines.SetCurrentLine(1);
                asiento.Lines.BPLID = codSucursal;
                asiento.Lines.AccountCode = mntoRedondeo > 0 ? ctaPte : ctaRnd;
                asiento.Lines.Credit = Math.Abs(mntoRedondeo);

                var rslt = asiento.Add();
                if (rslt != 0) throw new Exception(Globales.Company.GetLastErrorDescription());
            }
        }

        public static int GenerarPagoACuenta(int docEntry, SBOPago pago, bool tieneSucursales)
        {
            var sboPayments = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
            var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(BoObjectTypes.BoBridge);
            var tblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
            var mndLoc = sboBOB.GetLocalCurrency().Fields.Item(0).Value;

            sboPayments.Series = pago.CodSerieSBO;
            sboPayments.BPLID = pago.CodSucursal;
            sboPayments.DocDate = pago.FechaContabilizacion;
            sboPayments.DueDate = pago.FechaVencimiento;
            sboPayments.TaxDate = pago.FechaDocumento;
            sboPayments.DocCurrency = pago.Moneda;
            sboPayments.DocRate = pago.TipoCambio;
            sboPayments.DocType = SAPbobsCOM.BoRcptTypes.rAccount;
            sboPayments.ProjectCode = pago.CodigoProyecto;
            switch (pago.MetodoPago.Tipo)
            {
                case "CB": //Pago con cheque
                    var sucursalBanco = ObtenerSucursalCtaBanco(pago.MetodoPago.Banco, pago.MetodoPago.Cuenta);
                    sboPayments.Checks.AccounttNum = sucursalBanco.Item1;
                    sboPayments.Checks.BankCode = pago.MetodoPago.Banco;
                    sboPayments.Checks.Branch = sucursalBanco.Item2;
                    sboPayments.Checks.CheckAccount = pago.MetodoPago.Cuenta;

                    sboPayments.Checks.CheckSum = pago.Monto;
                    sboPayments.Checks.CountryCode = sucursalBanco.Item4;
                    sboPayments.Checks.Trnsfrable = SAPbobsCOM.BoYesNoEnum.tNO;
                    if (EsRelevanteFlujoDeCaja(pago.MetodoPago.Cuenta))
                    {
                        sboPayments.PrimaryFormItems.CashFlowLineItemID = ObtenerIDFlujoDeCaja(pago.CodSucursal, tieneSucursales);
                        sboPayments.PrimaryFormItems.PaymentMeans = PaymentMeansTypeEnum.pmtChecks;
                        if (pago.Moneda == "SOL")
                            sboPayments.PrimaryFormItems.AmountLC = pago.Monto;
                        else
                            sboPayments.PrimaryFormItems.AmountFC = pago.Monto;
                    }
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPCHEQUE").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;
                case "CG":
                case "PV":
                case "TB":
                case "VV"://Pago con Transferencia
                    sboPayments.TransferAccount = pago.MetodoPago.Cuenta;
                    sboPayments.TransferDate = pago.FechaContabilizacion;
                    sboPayments.TransferReference = ObtenerNroOperacion(docEntry, sboPayments.BPLID, pago.MetodoPago.Banco, pago.MetodoPago.Cuenta, pago.Moneda);
                    sboPayments.TransferSum = pago.Monto;
                    sboPayments.CounterReference = sboPayments.TransferReference;
                    if (EsRelevanteFlujoDeCaja(pago.MetodoPago.Cuenta))
                    {
                        sboPayments.PrimaryFormItems.CashFlowLineItemID = ObtenerIDFlujoDeCaja(pago.CodSucursal, tieneSucursales);
                        sboPayments.PrimaryFormItems.PaymentMeans = PaymentMeansTypeEnum.pmtBankTransfer;
                        if (pago.Moneda == "SOL")
                            sboPayments.PrimaryFormItems.AmountLC = pago.Monto;
                        else
                            sboPayments.PrimaryFormItems.AmountFC = pago.Monto;
                    }
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;

                case "NN"://Pago en Efectivo
                    sboPayments.CashAccount = pago.MetodoPago.Cuenta;
                    sboPayments.CashSum = pago.Monto;
                    if (EsRelevanteFlujoDeCaja(pago.MetodoPago.Cuenta))
                    {
                        sboPayments.PrimaryFormItems.CashFlowLineItemID = ObtenerIDFlujoDeCaja(pago.CodSucursal, tieneSucursales);
                        sboPayments.PrimaryFormItems.PaymentMeans = PaymentMeansTypeEnum.pmtCash;
                        if (pago.Moneda == "SOL")
                            sboPayments.PrimaryFormItems.AmountLC = pago.Monto;
                        else
                            sboPayments.PrimaryFormItems.AmountFC = pago.Monto;
                    }
                    if (tblConf.GetByKey("7") && !string.IsNullOrWhiteSpace(tblConf.UserFields.Fields.Item("U_VALOR").Value))
                        sboPayments.UserFields.Fields.Item("U_EXX_MPFONDEF").Value = tblConf.UserFields.Fields.Item("U_VALOR").Value;
                    break;
            }
            pago.Detalle.All(d =>
            {
                sboPayments.AccountPayments.AccountCode = d.CodigoCuenta;
                sboPayments.AccountPayments.AccountName = d.NumeroCuenta;
                sboPayments.AccountPayments.GrossAmount = d.Monto;
                sboPayments.AccountPayments.SumPaid = d.MontoAPagar;
                sboPayments.AccountPayments.Decription = d.Comentarios;
                sboPayments.AccountPayments.ProjectCode = d.CodProyecto;
                sboPayments.AccountPayments.ProfitCenter = d.CentroCosto;
                sboPayments.AccountPayments.ProfitCenter2 = d.CentroCosto2;
                sboPayments.AccountPayments.ProfitCenter3 = d.CentroCosto3;
                sboPayments.AccountPayments.ProfitCenter4 = d.CentroCosto4;
                sboPayments.AccountPayments.ProfitCenter5 = d.CentroCosto5;
                //sboPayments.AccountPayments.UserFields.Fields.Item("U_CCH_SERIE").Value = d.CCHSerie;
                //sboPayments.AccountPayments.UserFields.Fields.Item("U_CCH_CORRLTV").Value = d.CCHCorrelativo;
                sboPayments.AccountPayments.Add();
                return true;
            });

            if (sboPayments.Add() != 0) throw new InvalidOperationException($"{Globales.Company.GetLastErrorCode()}-{Globales.Company.GetLastErrorDescription()}");
            var rslt = 0;
            return int.TryParse(Globales.Company.GetNewObjectKey(), out rslt) ? rslt : 0;
        }

        public static string ObtenerCodCuentaPuentePorSucursal(int codSucursal, bool tieneSucursales)
        {
            var sqlQry = "select U_VALOR from \"@SMC_APM_CONFIAPM\" where \"Code\" = '11'";
            if (tieneSucursales)
            {
                sqlQry = $"select TX0.\"AcctCode\" from OACT TX0 inner join OBPL TX1 on TX0.\"FormatCode\" = TX1.U_EXD_CTAPTEPGOMSV " +
                $"where TX1.\"BPLId\" = '{codSucursal}'";
            }

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF)
                return recSet.Fields.Item(0).Value;
            else
                throw new InvalidOperationException($"No se ha definido la cuenta puenta para las sucursal cod ID:{codSucursal}");
        }

        public static string ObtenerCodCuentaRedondeoPorSucursal(int codSucursal, bool tieneSucursales)
        {
            var sqlQry = "select U_VALOR from \"@SMC_APM_CONFIAPM\" where \"Code\" = '14'";
            if (tieneSucursales)
            {
                sqlQry = $"select TX0.\"AcctCode\" from OACT TX0 inner join OBPL TX1 on TX0.\"FormatCode\" = TX1.U_EXD_CTARNDPM " +
                $"where TX1.\"BPLId\" = '{codSucursal}'";
            }
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF)
                return recSet.Fields.Item(0).Value;
            else
                throw new InvalidOperationException($"No se ha definido la cuenta de redondeo para las sucursal cod ID:{codSucursal}");
        }

        public static int ObtenerSeriePagoPorSucursal(int codSucursal, SAPbouiCOM.DBDataSource dbsSource, string serieRetencion)
        {
            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDSDocs = dbsSource.GetAsXML();
            var _dsrXmlDocs = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDSDocs));

            var rslt = _dsrXmlDocs.Rows.FirstOrDefault(r => Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid ==
            "U_COD_SUCURSAL").Value) == codSucursal).Cells.FirstOrDefault(c => c.Uid == (serieRetencion == "Y"
            ? "U_COD_SERIE_RETEN" : "U_COD_SERIE_PAGO")).Value;
            return Convert.ToInt32(rslt);
        }

        private static string ObtenerNroOperacion(int docEntry, int codSucursal, string codBanco, string codCtaPago, string codMoneda)
        {

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"select U_NRO_OPERACION from \"@EXP_PMP3\" where  \"DocEntry\" = '{docEntry}' and U_COD_SUCURSAL = '{codSucursal}' and U_COD_BANCO = '{codBanco}' and U_COD_CTAPAGO = '{codCtaPago}' and U_COD_MONEDA = '{codMoneda}'";
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF) return recSet.Fields.Item(0).Value.ToString();
            throw new InvalidOperationException($"No se han definido los números de operación para la sucursal: {codSucursal}, banco: {codBanco}");
        }

        private static int ObtenerIDFlujoDeCaja(int codSucursal, bool tieneSucursales)
        {
            var sqlQry = "select U_VALOR from \"@SMC_APM_CONFIAPM\" where \"Code\" = '15' and coalesce(U_VALOR,'') <> '' ";
            if (tieneSucursales)
            {
                sqlQry = $"select TX1.\"U_EXD_CFWID\" from  OBPL TX1 where TX1.\"BPLId\" = '{codSucursal}' and coalesce(TX1.\"U_EXD_CFWID\",'-1') <> '-1'";
            }
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF)
                return Convert.ToInt32(recSet.Fields.Item(0).Value);
            else
                throw new InvalidOperationException($"No se ha definido el ID del flujo de caja para la sucursal cod ID:{codSucursal}");
        }

        private static bool EsRelevanteFlujoDeCaja(string codCuenta)
        {
            var sqlQry = $"select coalesce(\"CfwRlvnt\",'') from oact where\"AcctCode\" ='{codCuenta}'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            return recSet.Fields.Item(0).Value == "Y";
        }

        private static string GuardarEnvioH2HEnLog(int docEntryPM, string codBanco, string codSucursal, string idArchivo)
        {
            var sqlQry = "select 'E'||right('00000000' || ltrim(right(coalesce(max(\"Code\"),'0'),8)+1),8) as \"Codigo\" from \"@EXD_PM_LOGENVHTH\"";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            var utEXD_PM_LOGENVHTH = Globales.Company.UserTables.Item("EXD_PM_LOGENVHTH");
            utEXD_PM_LOGENVHTH.Code = recSet.Fields.Item(0).Value.ToString();
            utEXD_PM_LOGENVHTH.Name = recSet.Fields.Item(0).Value.ToString();
            utEXD_PM_LOGENVHTH.UserFields.Fields.Item("U_ID_PAGMSV").Value = docEntryPM.ToString();
            utEXD_PM_LOGENVHTH.UserFields.Fields.Item("U_COD_BANCO").Value = codBanco.ToString();
            utEXD_PM_LOGENVHTH.UserFields.Fields.Item("U_COD_SUCURSAL").Value = codSucursal.ToString();
            utEXD_PM_LOGENVHTH.UserFields.Fields.Item("U_ID_ARCHIVO").Value = idArchivo.ToString();
            utEXD_PM_LOGENVHTH.UserFields.Fields.Item("U_FECHA_ENVIO").Value = DateTime.Today;
            var rslt = utEXD_PM_LOGENVHTH.Add();

            if (rslt != 0) throw new Exception($"Error al guardar en log: {Globales.Company.GetLastErrorDescription()}");

            return utEXD_PM_LOGENVHTH.Code;
        }

        private static void EncriptarArchivo(string rutaLLavePublica, string rutaOrigen, string rutaDestino)
        {
            var ENCRYPT_KEY = File.ReadAllBytes(rutaLLavePublica);
            CryptoHelper.EncryptPgpFile(rutaOrigen, rutaDestino, ENCRYPT_KEY);
        }

        private static void ActualizarLogEstadoEnLogEnvio(string codeLog, string codEstado)
        {
            var sqlQry = $"update \"@EXD_PM_LOGENVHTH\" set \"U_ESTADO\" = '{codEstado}' where \"Code\" = '{codeLog}'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
        }

        #region Obsoleto
        private static void GenerarTXT_Santander(string nombre, int docEntry, string gLAccount)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_SANTANDER({docEntry},'{gLAccount}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private static void GenerarTXT_Scotiabank(string nombre, int docEntry, string gLAccount)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_SCOTIABANK({docEntry},'{gLAccount}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private static void GenerarTXT_Interbank(string nombre, int docEntry, string gLAccount)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_INTERBANK({docEntry},'{gLAccount}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private static void GenerarTXT_BCP(string nombre, int docEntry, string gLAccount)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_BCP({docEntry},'{gLAccount}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        #endregion
    }
}