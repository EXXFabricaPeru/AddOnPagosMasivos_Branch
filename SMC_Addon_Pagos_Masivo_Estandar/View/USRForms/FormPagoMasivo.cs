using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbouiCOM;
using SMC_APM.Controller;
using SMC_APM.dao;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SMC_APM.View.USRForms
{
    public class FormPagoMasivo : IUSAP
    {
        public const string TYPE = "FrmPMP";
        public const string UNQID = "FrmPMP";
        public const string MENU = "MNUID_TRET";
        public const string PATH = "Resources/FrmPMP.srf";
        private SAPbouiCOM.DBDataSource dbsOPMP = null;
        private SAPbouiCOM.DBDataSource dbsPMP1 = null;
        private SAPbouiCOM.DBDataSource dbsPMP2 = null;
        private SAPbouiCOM.DBDataSource dbsPMP3 = null;
        private SAPbouiCOM.DBDataSource dbsPMP4 = null;
        private SAPbobsCOM.UserTable utblConf = null;
        private bool esAgenteRetenedor = true;
        private bool esTerceroRetenedor = true;
        private bool tieneAutorizaciones = false;
        private bool tieneSucursales = false;
        private bool actualizoNroOpe = false;
        private bool esHostToHost = false;
        private Dictionary<string, string> dcMetodoEnvPorBanco = null;

        private string codMonedaLocal = string.Empty;
        private string codMonedaUSD = string.Empty;

        public FormPagoMasivo(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
            dcMetodoEnvPorBanco = new Dictionary<string, string>();
        }

        protected override void CargarFormularioInicial()
        {
            try
            {
                dbsOPMP = Form.DataSources.DBDataSources.Item("@EXP_OPMP");
                dbsPMP1 = Form.DataSources.DBDataSources.Item("@EXP_PMP1");
                dbsPMP2 = Form.DataSources.DBDataSources.Item("@EXP_PMP2");
                dbsPMP3 = Form.DataSources.DBDataSources.Item("@EXP_PMP3");
                dbsPMP4 = Form.DataSources.DBDataSources.Item("@EXP_PMP4");
                utblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
                var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);

                codMonedaLocal = sboBOB.GetLocalCurrency().Fields.Item(0).Value;

                dbsPMP4.Clear();
                // Deshabilito la opcion de restablecer
                Form.EnableMenu("1285", false);
                // Deshabilito la opcion de cancelar
                Form.EnableMenu("1284", false);

                if (utblConf.GetByKey("2"))
                {
                    esAgenteRetenedor = utblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                }

                if (utblConf.GetByKey("3"))
                {
                    esTerceroRetenedor = utblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                }
                if (utblConf.GetByKey("10"))
                {
                    esHostToHost = false;
                    if (utblConf.UserFields.Fields.Item("U_VALOR").Value == "Y")
                    {
                        esHostToHost = true;
                        ((SAPbouiCOM.Button)Form.Items.Item("btnGenTXT").Specific).Caption = "Gen. TXT / Env. H2H";
                        ((SAPbouiCOM.Button)Form.Items.Item("btnGenTXT").Specific).Item.Description = "H2H";
                    }
                    else
                    {
                        ((SAPbouiCOM.Button)Form.Items.Item("btnGenTXT").Specific).Caption = "Generar TXT";
                        ((SAPbouiCOM.Button)Form.Items.Item("btnGenTXT").Specific).Item.Description = "TXT";
                    }
                }

                Matrix = Form.GetMatrix("Item_12");

                Matrix.SetColumnsVisible(false, "Col_3", "Col_15", "Col_19", "Col_24", "Col_37", "Col_38", "Col_39", "Col_40", "Col_41");
                Matrix.CommonSetting.FixedColumnsCount = 2;

                Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_3").Specific;
                while (Combo.ValidValues.Count > 0) Combo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                var recSet = PagoMasivoController.ObtenerSeriesDocumentoPago();
                while (!recSet.EoF)
                {
                    Combo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_5").Specific;
                if (esAgenteRetenedor)
                {
                    while (Combo.ValidValues.Count > 0) Combo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    recSet = PagoMasivoController.ObtenerSeriesRetencion();
                    while (!recSet.EoF)
                    {
                        Combo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                }
                else
                {
                    Combo.Item.Visible = false;
                    Form.Items.Item("Item_4").Visible = false;
                    Form.Items.Item("Item_23").Visible = false;
                }

                recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                recSet.DoQuery("select distinct \"GLAccount\",\"Account\" from DSC1 where coalesce(\"U_EXM_PMASIVO\",'') = 'Y'");
                while (!recSet.EoF)
                {
                    try
                    {
                        Matrix.Columns.Item("Col_12").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    }
                    catch (Exception ex) { }
                    recSet.MoveNext();
                }

                recSet.DoQuery("select \"CurrCode\" from OCRN where \"ISOCurrCod\" = 'USD'");
                codMonedaUSD = recSet.Fields.Item(0).Value.ToString();

                Form.Items.Item("btnTrcRtn").Visible = esTerceroRetenedor;
                Form.Items.Item("btnLibSNT").Visible = esTerceroRetenedor;
                Form.Items.Item("Item_19").Visible = esTerceroRetenedor;
                Form.Items.Item("Item_20").Visible = esTerceroRetenedor;
                Form.Items.Item("btnCrgRsp").Visible = esTerceroRetenedor;

                var cmbClmBncEsc = Matrix.Columns.Item("Col_5");
                var cmbClmBncPrv = Matrix.Columns.Item("Col_23");
                while (cmbClmBncEsc.ValidValues.Count > 0) cmbClmBncEsc.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                while (cmbClmBncPrv.ValidValues.Count > 0) cmbClmBncPrv.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                recSet = PagoMasivoController.ObtenerInfoBancos();
                while (!recSet.EoF)
                {
                    cmbClmBncEsc.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    cmbClmBncPrv.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                var cmbSucursales = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_22").Specific;
                recSet.DoQuery("select \"BPLId\",\"BPLName\",coalesce(U_EXX_RETPRO,'N') as \"RetPro\" from OBPL order by 1");
                //dbsPMP2.Clear();
                while (!recSet.EoF)
                {
                    cmbSucursales.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    Matrix.Columns.Item("Col_31").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }
                cmbSucursales.ValidValues.Add("-1", "Todas");

                var tblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
                tieneAutorizaciones = tblConf.GetByKey("5") && tblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                Form.GetButton("btnGrbEnv").Item.Visible = tieneAutorizaciones;

                var companyService = Globales.Company.GetCompanyService();
                var adminInfo = (SAPbobsCOM.AdminInfo)companyService.GetAdminInfo();

                tieneSucursales = (adminInfo.EnableBranches == SAPbobsCOM.BoYesNoEnum.tYES);

                var cmbPrioridad = Form.GetComboBox("Item_37");
                cmbPrioridad.ValidValues.Add(string.Empty, string.Empty);
                recSet.DoQuery("select \"Code\",\"Name\" from \"@EXX_PRIPAG\"");
                while (!recSet.EoF)
                {
                    cmbPrioridad.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    Matrix.Columns.Item("Col_42").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                var cmbFlujoDeCaja = Form.GetComboBox("Item_42");
                while (cmbFlujoDeCaja.ValidValues.Count > 0) cmbFlujoDeCaja.ValidValues.Remove(0, BoSearchKey.psk_Index);
                cmbFlujoDeCaja.ValidValues.Add(string.Empty, string.Empty);
                recSet.DoQuery("select \"CFWId\",\"CFWName\" from OCFW where \"Postable\" = 'Y'");
                while (!recSet.EoF)
                {
                    cmbFlujoDeCaja.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    Matrix.Columns.Item("Col_48").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                var sqlQry = "select U_VALOR from \"@SMC_APM_CONFIAPM\" where \"Code\" = '15' and coalesce(U_VALOR,'') <> '' ";
                recSet.DoQuery(sqlQry);
                if (!recSet.EoF)
                {
                    cmbFlujoDeCaja.Select(recSet.Fields.Item(0).Value.ToString(), BoSearchKey.psk_ByValue);
                }

                /*
                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Add, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Find, SAPbouiCOM.BoModeVisualBehavior.mvb_True);

                Form.Items.Item("Item_27").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("Item_27").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Add, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("Item_27").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Find, SAPbouiCOM.BoModeVisualBehavior.mvb_True);

                Form.Items.Item("btnLstDocs").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("btnLstDocs").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Add, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                */

                LoadDataOnFormAddMode();
            }
            catch (Exception ex)
            {
                Form.Close();
                Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void LoadDataOnFormAddMode()
        {
            var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
            Matrix = Form.GetMatrix("Item_12");
            Button = Form.GetButton("btnGrbEnv");
            Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_26").Specific;
            Combo.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (Combo.ValidValues.Count > 0) Combo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dbsOPMP.SetValue("U_EXP_ESTADO", 0, tieneAutorizaciones ? "P" : "A");
            dbsOPMP.SetValue("U_EXP_COD_SUCURSAL", 0, "-1");
            dbsOPMP.SetValue("U_EXP_FECHA", 0, DateTime.Today.ToString("yyyyMMdd"));
            dbsOPMP.SetValue("U_EXP_FECHAPAGO", 0, DateTime.Today.ToString("yyyyMMdd"));
            dbsOPMP.SetValue("U_EXP_ESTADOEJEC", 0, "0");
            dbsOPMP.SetValue("U_EXP_TIPODECAMBIO", 0, sboBOB.GetCurrencyRate(codMonedaUSD, DateTime.Today).Fields.Item(0).Value.ToString());
            dbsOPMP.SetValue("U_EXP_FORMA_SCOTIA", 0, "PV");
            dbsOPMP.SetValue("U_EXP_COD_PRIORIDAD", 0, "");
            dbsOPMP.SetValue("U_EXP_TIPO_DOCUMENTO", 0, "0");
            dbsOPMP.SetValue("DocNum", 0, Form.BusinessObject.GetNextSerialNumber(dbsOPMP.GetValue("Series", 0).Trim(), Form.BusinessObject.Type).ToString());
            Form.GetUserDataSource("UD_TOTAL").ValueEx = "0.00";
            Form.GetUserDataSource("UD_TOT_USD").ValueEx = "0.00";
            CargarSeriesDePago(DateTime.Today.Year);
            HabilitarControlesPorEstado("P");
            //Button.Caption = "Grabar";
            //Matrix.Columns.Item("Col_0").Editable = true;
            //Matrix.Columns.Item("Col_2").Editable = true;
            //Form.Items.Item("btnGenTXT").Enabled = false;
            //Form.Items.Item("Item_20").Enabled = false;
            //Form.Items.Item("Item_3").Enabled = true;
            //Form.Items.Item("Item_5").Enabled = true;
            //Form.Items.Item("btnCrgRsp").Enabled = false;
            //Form.Items.Item("btnTrcRtn").Enabled = false;
            //Form.Items.Item("btnGenPag").Enabled = false;
            //Form.Items.Item("btnLibSNT").Enabled = false;
            /*
            Form.Items.Item("btnGenTXT").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
            Form.Items.Item("btnGenPag").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
            Form.Items.Item("Item_20").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
            Form.Items.Item("btnCrgRsp").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
            Form.Items.Item("Item_3").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
            Form.Items.Item("Item_5").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
            */
        }

        private void CargarSeriesDePago(int indicador)
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = string.Empty;
            if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                sqlQry = $"CALL EXD_SP_PM_LISTAR_SERIES_X_SUCURSAL('{indicador}')";
            else
                sqlQry = $"EXEC EXD_SP_PM_LISTAR_SERIES_X_SUCURSAL '{indicador}'";
            var position = 0;
            recSet.DoQuery(sqlQry);
            dbsPMP2.Clear();
            while (!recSet.EoF)
            {
                dbsPMP2.InsertRecord(position);
                dbsPMP2.Offset = position;
                dbsPMP2.SetValue("U_COD_SUCURSAL", position, recSet.Fields.Item(0).Value);
                dbsPMP2.SetValue("U_NOM_SUCURSAL", position, recSet.Fields.Item(1).Value);
                dbsPMP2.SetValue("U_RETPRO", position, recSet.Fields.Item(2).Value);
                dbsPMP2.SetValue("U_COD_SERIE_PAGO", position, recSet.Fields.Item(3).Value);
                dbsPMP2.SetValue("U_COD_SERIE_RETEN", position, recSet.Fields.Item(5).Value);
                dbsPMP2.SetValue("U_NOM_SERIE_PAGO", position, recSet.Fields.Item(4).Value);
                dbsPMP2.SetValue("U_NOM_SERIE_RETEN", position, recSet.Fields.Item(6).Value);
                position++;
                recSet.MoveNext();
            }
        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLstDocs", e =>
            {
                if (!e.BeforeAction)
                {
                    var fecha = dbsOPMP.GetValue("U_EXP_FECHA", 0).Trim();
                    var codSucursal = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_COD_SUCURSAL").Trim());
                    var codPrioridad = dbsOPMP.GetValueExt("U_EXP_COD_PRIORIDAD");
                    var tipoDocumento = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_TIPO_DOCUMENTO"));
                    var lstDocumentos = PagoMasivoController.ListarDocumentosParaPagos(fecha, codSucursal, codPrioridad, tipoDocumento);
                    var lineNum = 0;

                    dbsPMP1.Clear();
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    foreach (var doc in lstDocumentos)
                    {
                        dbsPMP1.InsertRecord(lineNum);
                        dbsPMP1.Offset = lineNum;
                        dbsPMP1.SetValue("U_EXP_SLC_PAGO", lineNum, doc.SlcPago);
                        dbsPMP1.SetValue("U_EXP_SLC_RETENCION", lineNum, doc.SlcRetencion);
                        dbsPMP1.SetValue("U_EXP_COD_SUCURSAL", lineNum, doc.CodSucursal.ToString());
                        dbsPMP1.SetValue("U_EXP_COD_ESCENARIOPAGO", lineNum, doc.CodigoEscenarioPago);
                        dbsPMP1.SetValue("U_EXP_NUM_ESCENARIOPAGO", lineNum, doc.NumeroEscenarioPago);
                        dbsPMP1.SetValue("U_EXP_MEDIODEPAGO", lineNum, doc.MedioDePago);
                        dbsPMP1.SetValue("U_EXP_MONEDA_PAGO", lineNum, doc.MonedaDePago);
                        dbsPMP1.SetValue("U_EXP_CODBANCO", lineNum, doc.CodBanco);
                        dbsPMP1.SetValue("U_EXP_CODCTABANCO", lineNum, doc.CodCtaBanco);
                        dbsPMP1.SetValue("U_EXP_DOCENTRYDOC", lineNum, doc.DocEntryDocumento.ToString());
                        dbsPMP1.SetValue("U_EXP_TIPODOC", lineNum, doc.TipoDocumento);
                        dbsPMP1.SetValue("U_EXP_NROSUNAT", lineNum, doc.NroDocumentoSUNAT);
                        dbsPMP1.SetValue("U_EXP_MONEDA", lineNum, doc.Moneda);
                        dbsPMP1.SetValue("U_EXP_IMPORTE", lineNum, doc.Importe.ToString());
                        dbsPMP1.SetValue("U_EXP_CARDCODE", lineNum, doc.CardCode);
                        dbsPMP1.SetValue("U_EXP_CARDNAME", lineNum, doc.CardName);
                        dbsPMP1.SetValue("U_EXP_ASNROLINEA", lineNum, doc.NroLineaAsiento);
                        dbsPMP1.SetValue("U_EXP_NMROCUOTA", lineNum, doc.NroCuota);
                        dbsPMP1.SetValue("U_EXP_NRODOCUMENTOSN", lineNum, doc.NroDocumentoSN);
                        dbsPMP1.SetValue("U_EXP_APLSRERTN", lineNum, doc.AplSreRetencion);
                        dbsPMP1.SetValue("U_EXP_ESTADO", lineNum, " ");
                        dbsPMP1.SetValue("U_EXP_NROCTAPROV", lineNum, doc.NroCtaProveedor);
                        dbsPMP1.SetValue("U_EXP_CODBANCOPROV", lineNum, doc.CodBncProveedor.ToString());
                        dbsPMP1.SetValue("U_EXP_CODRETENCION", lineNum, doc.CodRetencion);
                        dbsPMP1.SetValue("U_EXP_IMPRETENCION", lineNum, doc.ImporteRetencion.ToString());
                        dbsPMP1.SetValue("U_EXP_TCDOCUMENTO", lineNum, doc.TCDocumento.ToString());
                        dbsPMP1.SetValue("U_EXP_GLOSAASIENTO", lineNum, doc.GlosaAsiento);
                        dbsPMP1.SetValue("U_EXP_CARDCODE_FACTO", lineNum, doc.CardCodeFactoring);
                        dbsPMP1.SetValue("U_EXP_CARDNAME_FACTO", lineNum, doc.CardNameFactoring);
                        dbsPMP1.SetValue("U_EXP_ESTADO2", lineNum, " ");
                        dbsPMP1.SetValue("U_EXP_AFECTO_RETENCION", lineNum, doc.AfectoRetencion);
                        dbsPMP1.SetValue("U_EXP_TIENE_RETENCION", lineNum, doc.TieneRetencion);
                        dbsPMP1.SetValue("U_EXP_APLICA_RETENCION", lineNum, doc.AplicaRetencion);
                        dbsPMP1.SetValue("U_EXP_IMPORTE_AUX", lineNum, doc.Importe.ToString());
                        dbsPMP1.SetValue("U_EXP_APL_RETENCION_AUX", lineNum, doc.AplicaRetencion);
                        dbsPMP1.SetValue("U_EXP_COD_PRIORIDAD", lineNum, doc.CodPrioridad);
                        dbsPMP1.SetValue("U_EXP_NROLINEA_EP", lineNum, doc.NroLineaEP.ToString());
                        dbsPMP1.SetValue("U_EXP_NROLINEA_EP", lineNum, doc.NroLineaEP.ToString());
                        dbsPMP1.SetValue("U_EXP_NROLINEA_EP", lineNum, doc.NroLineaEP.ToString());
                        dbsPMP1.SetValue("U_EXP_IMPORTE_MS", lineNum, doc.ImporteMS.ToString());
                        dbsPMP1.SetValue("U_EXP_IMP_RETENCION_MS", lineNum, doc.ImporteRetencionMS.ToString());
                        dbsPMP1.SetValue("U_EXP_COMENTARIOS", lineNum, doc.Comentarios.ToString());
                        dbsPMP1.SetValue("U_EXP_COD_FLUCAJ", lineNum, dbsOPMP.GetValueExt("U_EXP_COD_FLUCAJ"));
                    }
                    Matrix.LoadFromDataSource();
                    Matrix.AutoResizeColumns();
                    Form.GetUserDataSource("UD_TOTAL").ValueEx = lstDocumentos.Where(d => d.Moneda == codMonedaLocal).Sum(d => d.Importe).ToString();
                    Form.GetUserDataSource("UD_TOT_USD").ValueEx = lstDocumentos.Where(d => d.Moneda == codMonedaUSD).Sum(d => d.Importe).ToString();

                    //Agrego los bancos de la consulta
                    var nroLinea = 0;
                    dbsPMP4.Clear();
                    var lstBancos = lstDocumentos.Select(d => d.CodBanco).Distinct();
                    foreach (var bco in lstBancos)
                    {
                        dbsPMP4.InsertRecord(nroLinea);
                        dbsPMP4.Offset = nroLinea;
                        dbsPMP4.SetValue("U_COD_BANCO", nroLinea, bco);
                        dbsPMP4.SetValue("U_COD_METODO", nroLinea, "1");
                        nroLinea++;
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGrbEnv", e =>
            {
                var btnCrgEnv = (SAPbouiCOM.Button)Form.Items.Item("btnGrbEnv").Specific;
                var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();

                try
                {
                    if (!e.BeforeAction)
                    {
                        dbsOPMP.SetValue("U_EXP_ESTADO", 0, "E");

                        //Validaciones

                        /*if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && PagoMasivoController.ValidarRegistroUnicoPorFecha(dbsOPMP.GetValueExt("U_EXP_FECHA").Trim())) 
                            throw new InvalidOperationException("Ya existe un registro para la fecha seleccionada como filtro");*/
                        ((SAPbouiCOM.EditText)Form.GetItem("edtDocEnt").Specific).Active = true;
                        if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        HabilitarControlesPorEstado("E");
                    }
                }
                catch (Exception ex)
                {
                    Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnTrcRtn", e =>
            {
                if (!e.BeforeAction)
                {
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    var docEntryPMP = Convert.ToInt32(dbsOPMP.GetValue("DocEntry", 0));
                    var fechapago = dbsOPMP.GetValueExt("U_EXP_FECHAPAGO");
                    PagoMasivoController.generarTXT3Retenedor(docEntryPMP, fechapago);
                    dbsOPMP.SetValueExt("U_EXP_ESTADOEJEC", "1");
                    if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    Globales.Aplication.MessageBox("Archivo TXT de terceros generado con éxito");
                    Form.GetItem("btnGenTXT").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGenPag", e =>
            {
                if (!e.BeforeAction)
                {
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    var rslt = Globales.Aplication.MessageBox("Se procederá a generar el pago de los documentos seleccionados \n ¿Desea continuar con esta acción?"
                    , Btn1Caption: "SI", Btn2Caption: "NO");

                    if (rslt == 1)
                    {

                        Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                        Matrix.FlushToDataSource();
                        var pgoDS = dbsPMP1.GetAsXML();
                        var lstBancos = PagoMasivoController.ObtenerListaBancoPorPago(pgoDS).Distinct();
                        var lstPagos = PagoMasivoController.ObtenerListaPagos(dbsOPMP, pgoDS, dbsPMP2, esAgenteRetenedor, esHostToHost);
                        //var estado = string.Empty;
                        var msjError = string.Empty;
                        //var cntDocXPgo = lstPagos.Count();
                        //var progressBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, 1, false);
                        var docEntryForm = Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry"));
                        try
                        {
                            Form.GetItem("btnGenPag").Enabled = false;
                            GenerarPagosAsync(docEntryForm, null, lstBancos, lstPagos);

                            /*
                            Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(1000);
                                Globales.Aplication.Menus.Item("1304").Activate();
                                if (cntErrores == 0) HabilitarControlesPorEstado("C");
                            });
                            */

                            /*
                            if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                                Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                            Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                            */
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {

                            //pgrssBar.Stop();
                        }
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnCrgRsp", e =>
            {
                if (!e.BeforeAction)
                {
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    var fpRspSNT = dbsOPMP.GetValue("U_EXP_RUTARSPSUNAT", 0).Trim();
                    if (!string.IsNullOrWhiteSpace(fpRspSNT))
                    {
                        var docEntry = Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry"));
                        var lstRspSNT = PagoMasivoController.LeerTXT3RetenedorRsp(fpRspSNT, docEntry).ToList();
                        var nroRUC = string.Empty;
                        var tipoDocumento = 0;
                        var idDocumento = 0;
                        TXT3RetenedorRsp rspSNT = null;
                        for (int i = 0; i < dbsPMP1.Size; i++)
                        {
                            dbsPMP1.Offset = i;
                            nroRUC = dbsPMP1.GetValue("U_EXP_NRODOCUMENTOSN", i).Trim();
                            tipoDocumento = Convert.ToInt32(dbsPMP1.GetValue("U_EXP_TIPODOC", i).Trim());
                            idDocumento = Convert.ToInt32(dbsPMP1.GetValue("U_EXP_DOCENTRYDOC", i).Trim());
                            dbsPMP1.SetValue("U_EXP_SLC_RETENCION", i, "N");
                            rspSNT = lstRspSNT.Find(r => r.RUC == nroRUC && r.TipoDocumento == tipoDocumento && r.IdDocumento == idDocumento);
                            if (rspSNT != null && rspSNT.Estado.ToUpper().Substring(0, 6) == "EN REV")
                                dbsPMP1.SetValue("U_EXP_SLC_RETENCION", i, "Y");
                        }
                        Matrix.LoadFromDataSource();
                    }
                    else
                        throw new InvalidOperationException("No se ha seleccionado ningún archivo");
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    if (e.BeforeAction)
                    {
                        if (((SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific).VisualRowCount == 0)
                        {
                            Globales.Aplication.StatusBar.SetText("Debe registrar al menos un documento", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        var seriePago = dbsOPMP.GetValue("U_EXP_SERIEPAGO", 0).Trim();
                        var serieRetencion = dbsOPMP.GetValue("U_EXP_SERIERETENCION", 0).Trim();
                        var codSucursal = Convert.ToInt32(dbsOPMP.GetValue("U_EXP_COD_SUCURSAL", 0).Trim());
                        if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && (dbsPMP1.Size == 0 || (dbsPMP1.Size > 0 && string.IsNullOrWhiteSpace(dbsPMP1.GetValue("U_EXP_DOCENTRYDOC", 0)))))
                            throw new InvalidOperationException("Debe registrar al menos un documento para pagar");

                        if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && ((codSucursal != -1 && string.IsNullOrWhiteSpace(seriePago))
                        || (codSucursal == -1 && ValidarSelecSeriesPagoXSucursal()))) throw new InvalidOperationException("Seleccione una serie de pago");
                        //if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && string.IsNullOrWhiteSpace(serieRetencion) && esAgenteRetenedor) throw new InvalidOperationException("Seleccione una serie de retención");
                        QuitarFilasNoSeleccionadas();
                        //var btnCrgEnv = (SAPbouiCOM.Button)Form.Items.Item("btnGrbEnv").Specific;
                        //var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                        EstablecerCuentasParaNumerosDeOperacion("A");

                    }
                    else if (!e.BeforeAction && e.ActionSuccess)
                    {
                        LoadDataOnFormAddMode();
                    }
                }
                else if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {
                    if (actualizoNroOpe)
                    {
                        actualizoNroOpe = false;
                        var cntNrosOpe = 0;
                        for (int i = 0; i < dbsPMP3.Size; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(dbsPMP3.GetValue("U_NRO_OPERACION", i))) cntNrosOpe++;
                        }
                        if (cntNrosOpe == dbsPMP3.Size)
                        {
                            dbsOPMP.SetValueExt("U_EXP_ESTADOEJEC", "2");
                            var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                            HabilitarControlesPorEstado(estadoDoc);
                        }
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGenTXT", e =>
            {
                if (!e.BeforeAction && e.ActionSuccess)
                {
                    var cntErr = 0;
                    var tipoEnvio = Form.Items.Item("btnGenTXT").Description;
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    /*
                    if (tipoEnvio == "H2H" && dbsOPMP.GetValueExt("U_EXP_ENVIADO_H2H") == "Y")
                    {
                        Globales.Aplication.MessageBox("Estos documentos ya se enviaron a Host to Host, esperando respuesta de los bancos...");
                        return true;
                    }*/
                    Globales.Aplication.StatusBar.SetText("Iniciando generación de archivos para bancos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    Matrix.FlushToDataSource();
                    var pgoDS = dbsPMP1.GetAsXML();
                    var lstBancos = PagoMasivoController.ObtenerListaBancoPorPago(pgoDS);
                    //.Select(s => new { CodBanco = s.Banco, CodMoneda = s.Moneda, GLCuenta = s.MetodoPago.Cuenta }).Distinct().ToList();
                    var docEntry = Convert.ToInt32(dbsOPMP.GetValue("DocEntry", 0));
                    var pgrssBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, 1, false);
                    var formatoScotia = dbsOPMP.GetValueExt("U_EXP_FORMA_SCOTIA");

                    var EXP_PMP4 = Form.GetDBDataSource("@EXP_PMP4");
                    dcMetodoEnvPorBanco.Clear();
                    for (int i = 0; i < EXP_PMP4.Size; i++)
                    {
                        dcMetodoEnvPorBanco.Add(EXP_PMP4.GetValue("U_COD_BANCO", i), EXP_PMP4.GetValue("U_COD_METODO", i));
                    }

                    try
                    {
                        foreach (var banc in lstBancos)
                        {
                            try
                            {
                                var codPais = ObtenerPaisBanco(banc.Banco, banc.CtaBanco);
                                var metodoEnvio = "1";
                                if (dcMetodoEnvPorBanco.ContainsKey(banc.Banco)) metodoEnvio = dcMetodoEnvPorBanco[banc.Banco];
                                if (esHostToHost && metodoEnvio == "2")
                                {
                                    if (PagoMasivoController.ValidaArchivoH2HEstado(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, "EN")) continue;
                                    if (PagoMasivoController.ValidaArchivoH2HEstado(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, "DS")) continue;
                                    if (PagoMasivoController.ValidaArchivoH2HEstado(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, "PR")) continue;
                                    PagoMasivoController.GenerarTXTH2H(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, banc.CtaBanco, codPais);
                                    Globales.Aplication.StatusBar.SetText($"Archivos para banco {banc.Banco} generados correctamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                }
                                else
                                {
                                    PagoMasivoController.GenerarTXTBancos(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, banc.CtaBanco, banc.MedioDePago, codPais, formatoScotia);
                                    Globales.Aplication.StatusBar.SetText($"Archivos para banco {banc.Banco} generados correctamente en la ruta \n C:\\PagosMasivos\\", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                }
                            }
                            catch (Exception ex)
                            {
                                cntErr++;
                                Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            }
                        }
                        if (cntErr == 0)
                        {
                            dbsOPMP.SetValueExt("U_EXP_ENVIADO_H2H", "Y");
                            //Globales.Aplication.MessageBox("Archivos para bancos generados correctamente en la ruta \n C:\\PagosMasivos\\");
                            dbsOPMP.SetValueExt("U_EXP_ESTADOEJEC", esTerceroRetenedor ? "2" : "1");
                            if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                            Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                            var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                            HabilitarControlesPorEstado(estadoDoc);
                        }
                    }
                    finally
                    {

                        //Form.GetItem("btnGenPag").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                        pgrssBar.Stop();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLibSNT", e => AbrirLiberacionSUNAT(e)));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_24", e => AbrirFormAsignacionDeSeries(e, "P")));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_23", e => AbrirFormAsignacionDeSeries(e, "R")));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_34", e => AbrirFormRegistroDeNroDeOperacion(e)));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED, "Item_12", e =>
            {
                if (e.BeforeAction && e.ColUID == "Col_6")
                {
                    var objType = dbsPMP1.GetValue("U_EXP_TIPODOC", e.Row - 1).Trim();
                    objType = objType == "24" ? "30" : objType;
                    var linkedButton = (SAPbouiCOM.LinkedButton)Form.GetMatrix(e.ItemUID).Columns.Item("Col_6").ExtendedObject;
                    linkedButton.LinkedObjectType = objType;
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, "Item_22", e =>
            {
                if (!e.BeforeAction)
                {
                    var codSucursal = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_COD_SUCURSAL"));

                    Form.Items.Item("Item_3").Enabled = codSucursal != -1;
                    Form.Items.Item("Item_24").Enabled = codSucursal == -1;

                    Form.Items.Item("Item_5").Enabled = codSucursal != -1;
                    Form.Items.Item("Item_23").Enabled = codSucursal == -1;
                    dbsOPMP.SetValueExt("U_EXP_SERIEPAGO", null);
                    dbsOPMP.SetValueExt("U_EXP_SERIERETENCION", null);

                    if (codSucursal == -1) return true;
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'N' order by 1";
                    var cmbSeriePago = (SAPbouiCOM.ComboBox)Form.GetItem("Item_3").Specific;
                    recSet.DoQuery(sqlQry);
                    while (cmbSeriePago.ValidValues.Count > 0) cmbSeriePago.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSeriePago.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                    if (cmbSeriePago.ValidValues.Count > 0) cmbSeriePago.SelectExclusive(0, SAPbouiCOM.BoSearchKey.psk_Index);


                    sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'Y' order by 1";
                    var cmbSerieReten = (SAPbouiCOM.ComboBox)Form.GetItem("Item_5").Specific;
                    recSet.DoQuery(sqlQry);
                    while (cmbSerieReten.ValidValues.Count > 0) cmbSerieReten.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSerieReten.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                    if (cmbSerieReten.ValidValues.Count > 0) cmbSerieReten.SelectExclusive(0, SAPbouiCOM.BoSearchKey.psk_Index);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_12", e =>
            {
                var mtxDocs = Form.GetMatrix(e.ItemUID);

                // cambiar a Linq
                if (!e.BeforeAction && e.ColUID == "Col_0")
                {
                    var totSlc = 0.00;
                    var totSlcUSD = 0.00;

                    for (int i = 0; i < mtxDocs.RowCount; i++)
                    {
                        if (((SAPbouiCOM.CheckBox)mtxDocs.GetCellSpecific(e.ColUID, i + 1)).Checked)
                        {
                            if (((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_10", i + 1)).Value == codMonedaLocal)
                                totSlc += Convert.ToDouble(((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_11", i + 1)).Value);
                            if (((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_10", i + 1)).Value == codMonedaUSD)
                                totSlcUSD += Convert.ToDouble(((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_11", i + 1)).Value);

                        }
                    }
                    Form.GetUserDataSource("UD_TOTAL").ValueEx = totSlc.ToString();
                    Form.GetUserDataSource("UD_TOT_USD").ValueEx = totSlcUSD.ToString();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, "Item_12", e =>
            {
                if (e.BeforeAction && e.Row == 0 && e.ColUID == "Col_0")
                {
                    try
                    {
                        var mtxDocs = Form.GetMatrix(e.ItemUID);
                        Form.Freeze(true);
                        mtxDocs.Columns.Item("Col_0").Cells.Item(mtxDocs.RowCount).Click(SAPbouiCOM.BoCellClickType.ct_Regular, (int)SAPbouiCOM.BoModifiersEnum.mt_SHIFT);
                        mtxDocs.Columns.Item("Col_0").Cells.Item(1).Click(SAPbouiCOM.BoCellClickType.ct_Regular, (int)SAPbouiCOM.BoModifiersEnum.mt_SHIFT);
                    }
                    catch (Exception ex)
                    {
                        Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        Form.Freeze(false);
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_VALIDATE, "Item_31", e =>
            {
                if (e.BeforeAction && Form.Mode != BoFormMode.fm_FIND_MODE)
                {
                    try
                    {
                        var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                        var fchPago = DateTime.ParseExact(dbsOPMP.GetValueExt("U_EXP_FECHAPAGO"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        var rslt = sboBOB.GetCurrencyRate(codMonedaUSD, fchPago);
                        CargarSeriesDePago(fchPago.Year);
                        if (!rslt.EoF) dbsOPMP.SetValueExt("U_EXP_TIPODECAMBIO", (string)Convert.ToString(rslt.Fields.Item(0).Value));
                        if (Form.Mode == BoFormMode.fm_ADD_MODE)
                        {
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = 'EXP_OPMP' and \"Indicator\" = '{fchPago.Year}'";
                            recSet.DoQuery(sqlQry);
                            var cmbSeries = Form.GetComboBox("Item_26");
                            cmbSeries.LimpiarValoresValidos();
                            while (!recSet.EoF)
                            {
                                cmbSeries.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                                recSet.MoveNext();
                            }
                            if (cmbSeries.ValidValues.Count > 0)
                            {
                                dbsOPMP.SetValue("Series", 0, cmbSeries.ValidValues.Item(0).Value);
                                dbsOPMP.SetValue("DocNum", 0, Form.BusinessObject.GetNextSerialNumber(dbsOPMP.GetValueExt("Series"), "EXP_OPMP").ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Globales.Aplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "btnMetEnv", e =>
            {
                if (!e.BeforeAction)
                {
                    var formUID = string.Concat(FormMetodoEnvBanco.TYPE, new Random().Next(0, 1000));
                    if (!UIFormFactory.FormUIDExists(formUID))
                    {
                        actualizoNroOpe = true;
                        UIFormFactory.AddUSRForm(formUID, new FormMetodoEnvBanco(formUID, dbsPMP4, () =>
                        {
                            if (Form.Mode == BoFormMode.fm_OK_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        }));
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "Item_40", e =>
            {
                if (!e.BeforeAction)
                {
                    var nextSlcRow = Matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                    if (nextSlcRow == -1)
                    {
                        Globales.Aplication.StatusBar.SetText("Seleccione una fila...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        return false;
                    }

                    var rslt = Globales.Aplication.MessageBox("¿Esta seguro que desea proceder con esta acción?", 1, "SI", "NO");
                    if (rslt != 1) return false;

                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    var EXP_PMP4 = Form.GetDBDataSource("@EXP_PMP4");
                    dcMetodoEnvPorBanco.Clear();
                    for (int i = 0; i < EXP_PMP4.Size; i++)
                    {
                        dcMetodoEnvPorBanco.Add(EXP_PMP4.GetValue("U_COD_BANCO", i), EXP_PMP4.GetValue("U_COD_METODO", i));
                    }
                    var primeraFilaXSlc = 0;
                    do
                    {
                        nextSlcRow = Matrix.GetNextSelectedRow(primeraFilaXSlc, BoOrderType.ot_RowOrder);
                        if (nextSlcRow > -1)
                        {
                            var codBanco = ((SAPbouiCOM.ComboBox)Matrix.GetCellSpecific("Col_5", nextSlcRow)).Value;
                            if (dcMetodoEnvPorBanco.ContainsKey(codBanco) && dcMetodoEnvPorBanco[codBanco] == "2")
                            {
                                Globales.Aplication.StatusBar.SetText("No se puede excluir un documento de que pertenece a un banco que envia host to host...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                primeraFilaXSlc = nextSlcRow;
                                continue;
                                //return false;
                            }

                            var estadoConta = ((SAPbouiCOM.EditText)Matrix.GetCellSpecific("Col_17", nextSlcRow)).Value;
                            if (estadoConta == "OK")
                            {
                                Globales.Aplication.StatusBar.SetText("No se puede excluir un documento contabilizado...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                primeraFilaXSlc = nextSlcRow;
                                continue;
                                //return false;
                            }

                            var docEntryPM = dbsOPMP.GetValueExt("DocEntry");
                            var lineIdPM = ((SAPbouiCOM.EditText)Matrix.GetCellSpecific("Col_34", nextSlcRow)).Value;
                            var docEntryEP = ((SAPbouiCOM.EditText)Matrix.GetCellSpecific("Col_3", nextSlcRow)).Value;
                            var lineIdEP = ((SAPbouiCOM.EditText)Matrix.GetCellSpecific("Col_43", nextSlcRow)).Value;
                            //var sqlQry = $"update \"@EXP_PMP1\" set U_EXP_ESTADO = 'EX' where \"DocEntry\" = '{docEntry}' and \"LineId\" = '{lineId}'";
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var sqlQry = $"delete from \"@EXD_EPG1\" where \"DocEntry\" = '{docEntryEP}' and \"LineId\" = '{lineIdEP}'";
                            recSet.DoQuery(sqlQry);

                            sqlQry = $"delete from \"@EXP_PMP1\" where \"DocEntry\" = '{docEntryPM}' and \"LineId\" = '{lineIdPM}'";
                            recSet.DoQuery(sqlQry);
                        }
                        primeraFilaXSlc = nextSlcRow;
                    } while (primeraFilaXSlc > -1);

                    Globales.Aplication.ActivateMenuItem("1304");
                    Globales.Aplication.StatusBar.SetText("Documentos excluidos correctamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(BoEventTypes.et_COMBO_SELECT, "Item_42", e =>
             {
                 if (!e.BeforeAction)
                 {
                     var codFljCaj = dbsOPMP.GetValueExt("U_EXP_COD_FLUCAJ");
                     Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                     Matrix.FlushToDataSource();

                     for (int i = 0; i < Matrix.RowCount; i++)
                     {
                         Matrix.SetCellWithoutValidation(i + 1, "Col_48", codFljCaj);
                     }

                     Matrix.FlushToDataSource();

                 }
                 return true;
             }));

            //************## Data events ##*******************************************************************************************
            Eventos.Add(new EventoData(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD, TYPE, e =>
            {
                if (!e.BeforeAction)
                {
                    var cancelado = dbsOPMP.GetValueExt("Canceled") == "Y";
                    var cerrado = dbsOPMP.GetValueExt("Status") == "C";

                    dbsOPMP.SetValueExt("U_EXP_ESTADO", cerrado ? "C" : dbsOPMP.GetValueExt("U_EXP_ESTADO"));
                    dbsOPMP.SetValueExt("U_EXP_ESTADO", cancelado ? "N" : dbsOPMP.GetValueExt("U_EXP_ESTADO"));

                    var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                    var EXP_PMP1 = Form.GetDBDataSource("@EXP_PMP1");
                    var totPgoMsv = 0d;
                    var totPgoMsvUSD = 0d;
                    for (int i = 0; i < EXP_PMP1.Size; i++)
                    {
                        EXP_PMP1.Offset = i;
                        if (EXP_PMP1.GetValue("U_EXP_MONEDA", i) == codMonedaLocal)
                            totPgoMsv += Convert.ToDouble(EXP_PMP1.GetValue("U_EXP_IMPORTE", i));
                        if (EXP_PMP1.GetValue("U_EXP_MONEDA", i) == codMonedaUSD)
                            totPgoMsvUSD += Convert.ToDouble(EXP_PMP1.GetValue("U_EXP_IMPORTE", i));
                    }
                    Form.GetUserDataSource("UD_TOTAL").ValueEx = totPgoMsv.ToString();
                    Form.GetUserDataSource("UD_TOT_USD").ValueEx = totPgoMsvUSD.ToString();

                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = 'EXP_OPMP'";
                    recSet.DoQuery(sqlQry);
                    var cmbSeries = Form.GetComboBox("Item_26");
                    cmbSeries.LimpiarValoresValidos();
                    while (!recSet.EoF)
                    {
                        cmbSeries.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }

                    HabilitarControlesPorEstado(estadoDoc);
                }
                return true;
            }));

            Eventos.Add(new EventoData(BoEventTypes.et_FORM_DATA_ADD, TYPE, e =>
            {
                if (((SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific).VisualRowCount == 0)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(500);
                        Globales.Aplication.StatusBar.SetText("Debe registrar al menos un documento", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    });
                    return false;
                }
                if (!e.BeforeAction && e.ActionSuccess)
                {
                    var xmlElement = XElement.Parse(e.ObjectKey);
                    var docEntry = xmlElement.Element("DocEntry").Value;
                    var sqlQry = $"EXEC EXD_SP_PMP_CALCULAR_APLICA_RETENCION '{docEntry}'";
                    if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        sqlQry = $"CALL EXD_SP_PMP_CALCULAR_APLICA_RETENCION('{docEntry}')";
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    recSet.DoQuery(sqlQry);
                }
                return true;
            }));

            Eventos.Add(new EventoData(BoEventTypes.et_FORM_DATA_UPDATE, TYPE, e =>
            {
                if (!e.BeforeAction && e.ActionSuccess)
                {
                    var docEntry = dbsOPMP.GetValueExt("DocEntry");
                    var sqlQry = $"EXEC EXD_SP_PMP_CALCULAR_APLICA_RETENCION '{docEntry}'";
                    if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        sqlQry = $"CALL EXD_SP_PMP_CALCULAR_APLICA_RETENCION('{docEntry}')";
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    recSet.DoQuery(sqlQry);

                    var cnds = (SAPbouiCOM.Conditions)Globales.Aplication.CreateObject(BoCreatableObjectType.cot_Conditions);
                    var cnd = cnds.Add();
                    cnd.Alias = "DocEntry";
                    cnd.Operation = BoConditionOperation.co_EQUAL;
                    cnd.CondVal = docEntry;

                    dbsPMP1.Query(cnds);
                    Form.GetMatrix("Item_12").LoadFromDataSource();

                }
                return true;
            }));
        }

        private void EstablecerCuentasParaNumerosDeOperacion(string estado)
        {
            var pgoDS = dbsPMP1.GetAsXML();
            var lstBancos = PagoMasivoController.ObtenerListaBancoPorPago(pgoDS).Distinct().ToList();
            var nroFila = 0;

            if (estado == "A")
            {
                dbsPMP3.Clear();
                lstBancos.ForEach(b =>
                {
                    dbsPMP3.InsertRecord(nroFila);
                    dbsPMP3.Offset = nroFila;
                    dbsPMP3.SetValue("U_COD_SUCURSAL", nroFila, b.Sucursal);
                    dbsPMP3.SetValue("U_COD_BANCO", nroFila, b.Banco);
                    dbsPMP3.SetValue("U_COD_MONEDA", nroFila, b.Moneda);
                    dbsPMP3.SetValue("U_COD_CTAPAGO", nroFila, b.CtaBanco);
                    nroFila++;
                });
            }
            else if (estado == "U")
            {
                var codSucursal = string.Empty;
                var codBanco = string.Empty;
                var codMoneda = string.Empty;
                var codCtaPago = string.Empty;
                var registroEliminado = false;
                var actualizarUDO = false;

                do
                {
                    registroEliminado = false;
                    for (int i = 0; i < dbsPMP3.Size; i++)
                    {
                        codSucursal = dbsPMP3.GetValue("U_COD_SUCURSAL", i).Trim();
                        codBanco = dbsPMP3.GetValue("U_COD_BANCO", i).Trim();
                        codMoneda = dbsPMP3.GetValue("U_COD_MONEDA", i).Trim();
                        codCtaPago = dbsPMP3.GetValue("U_COD_CTAPAGO", i).Trim();
                        if (!lstBancos.Any(b => b.Sucursal.ToString() == codSucursal && b.Banco == codBanco && b.Moneda == codMoneda && b.CtaBanco == codCtaPago))
                        {
                            dbsPMP3.RemoveRecord(i);
                            actualizarUDO = true;
                            registroEliminado = true;
                            break;
                        }
                    }
                } while (registroEliminado);

                if (actualizarUDO)
                {
                    if (Form.Mode != BoFormMode.fm_UPDATE_MODE) Form.Mode = BoFormMode.fm_UPDATE_MODE;
                    Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                }
            }
        }

        private string ObtenerPaisBanco(string banco, string ctaBanco)
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"select max(\"Country\") as \"Pais\" from DSC1 where \"BankCode\" = '{banco}' and \"GLAccount\" = '{ctaBanco}'";

            recSet.DoQuery(sqlQry);
            if (!recSet.EoF) return recSet.Fields.Item(0).Value;
            throw new InvalidOperationException("No se pudo obtener código de pais del banco");
        }

        private bool AbrirLiberacionSUNAT(SAPbouiCOM.ItemEvent e)
        {
            if (!e.BeforeAction)
            {
                var formUID = string.Concat(FormLiberarTercero.TYPE, new Random().Next(0, 1000));
                string fechaPM = Form.GetItem("Item_1").Specific.Value;
                string docEntry = Form.GetDBDataSource("@EXP_OPMP").GetValueExt("DocEntry");

                if (!string.IsNullOrEmpty(docEntry))
                {
                    string relatiVePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "frmSMC_PM_LiberarTerceroRet.srf");

                    if (!UIFormFactory.FormUIDExists(formUID))
                        UIFormFactory.AddUSRForm(formUID, new FormLiberarTercero(formUID, relatiVePath, fechaPM, docEntry));
                }
                else
                {
                    Globales.Aplication.StatusBar.SetText("Solo puede abrir esta ventana desde un registro creado previamente", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            return true;
        }

        private bool AbrirFormAsignacionDeSeries(SAPbouiCOM.ItemEvent e, string tipoSerie)
        {
            if (!e.BeforeAction)
            {
                var formUID = string.Concat(FormSeriesPorSucursal.TYPE, new Random().Next(0, 1000));
                if (!UIFormFactory.FormUIDExists(formUID))
                    UIFormFactory.AddUSRForm(formUID, new FormSeriesPorSucursal(formUID, tipoSerie, dbsPMP2));
            }
            return true;
        }

        private bool AbrirFormRegistroDeNroDeOperacion(SAPbouiCOM.ItemEvent e)
        {
            if (!e.BeforeAction)
            {
                actualizoNroOpe = false;
                var formUID = string.Concat(FormNumeroDeOperacion.TYPE, new Random().Next(0, 1000));
                if (!UIFormFactory.FormUIDExists(formUID))
                {
                    actualizoNroOpe = true;
                    UIFormFactory.AddUSRForm(formUID, new FormNumeroDeOperacion(formUID, dbsPMP3, () =>
                    {
                        Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    }));
                }
            }
            return true;
        }

        private void HabilitarControlesPorEstado(string codEstado)
        {
            Form.Items.Item("Item_7").Enabled = false;
            Form.Items.Item("Item_27").Enabled = false;
            //codEstado = (codEstado == "" && tieneAutorizaciones) ? codEstado : (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE ? "P" : "A");
            Form.Items.Item("edtFocus").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
            Form.Items.Item("btnGrbEnv").Enabled = false;
            Form.Items.Item("Item_1").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_22").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_37").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_39").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_40").Enabled = false;
            Form.Items.Item("btnLstDocs").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_3").Enabled = false;
            Form.Items.Item("Item_5").Enabled = false;
            Form.Items.Item("Item_12").Enabled = false;
            //Form.Items.Item("Item_16").Enabled = false;
            //Form.Items.Item("Item_17").Enabled = false;
            Form.Items.Item("Item_18").Enabled = false;
            Form.Items.Item("Item_20").Enabled = false;
            Form.Items.Item("Item_26").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_31").Enabled = false;
            Form.Items.Item("Item_33").Enabled = false;
            Form.Items.Item("btnCrgRsp").Enabled = false;
            Form.Items.Item("btnGenTXT").Enabled = false;
            Form.Items.Item("btnGenPag").Enabled = false;
            Form.Items.Item("btnTrcRtn").Enabled = false;
            Form.Items.Item("btnLibSNT").Enabled = false;
            Form.Items.Item("Item_23").Enabled = false;
            Form.Items.Item("Item_24").Enabled = false;
            Form.Items.Item("Item_34").Enabled = false;
            Form.Items.Item("Item_42").Enabled = false;
            Form.GetMatrix("Item_12").Columns.Item("Col_2").Editable = false;
            if (codEstado == "P" || codEstado == "R")
            {
                Form.Items.Item("btnGrbEnv").Enabled = (Form.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE && true);
                //Form.Items.Item("Item_3").Enabled = true;
                //Form.Items.Item("Item_5").Enabled = true;
                Form.Items.Item("Item_12").Enabled = true;
                //Form.Items.Item("Item_16").Enabled = true;
                //Form.Items.Item("Item_17").Enabled = true;
                Form.Items.Item("Item_18").Enabled = true;
                //Form.Items.Item("Item_26").Enabled = true;
                Form.Items.Item("Item_31").Enabled = true;
                Form.Items.Item("Item_33").Enabled = true;
                Form.Items.Item("Item_23").Enabled = true;
                Form.Items.Item("Item_24").Enabled = true;
                Form.Items.Item("Item_40").Enabled = (Form.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE);
                Form.Items.Item("Item_42").Enabled = true;
                Form.GetMatrix("Item_12").Columns.Item("Col_2").Editable = false;
            }
            else if (codEstado == "A")
            {
                Form.Items.Item("Item_12").Enabled = true;
                //Form.Items.Item("Item_16").Enabled = true;
                //Form.Items.Item("Item_17").Enabled = true;
                Form.Items.Item("Item_20").Enabled = true;
                Form.Items.Item("Item_23").Enabled = false;
                Form.Items.Item("Item_24").Enabled = false;
                Form.Items.Item("btnTrcRtn").Enabled = true;
                Form.Items.Item("btnLibSNT").Enabled = true;
                Form.Items.Item("btnCrgRsp").Enabled = true;
                Form.Items.Item("Item_40").Enabled = true;
                Form.Items.Item("Item_42").Enabled = true;
                if (esTerceroRetenedor)
                {
                    switch (dbsOPMP.GetValueExt("U_EXP_ESTADOEJEC"))
                    {
                        case "1":
                            Form.Items.Item("Item_18").Enabled = true;
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            break;
                        case "2":
                            Form.Items.Item("Item_18").Enabled = true;
                            Form.Items.Item("Item_34").Enabled = false;
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            Form.Items.Item("btnGenPag").Enabled = true;
                            break;
                    }
                }
                else
                {
                    switch (dbsOPMP.GetValueExt("U_EXP_ESTADOEJEC"))
                    {
                        case "0":
                            Form.Items.Item("Item_18").Enabled = true;
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            break;
                        case "1":
                            Form.Items.Item("Item_18").Enabled = true;
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            //Form.Items.Item("btnGenPag").Enabled = true;
                            Form.Items.Item("Item_34").Enabled = true;
                            break;
                        case "2":
                            Form.Items.Item("Item_33").Enabled = true;
                            Form.Items.Item("Item_34").Enabled = true;
                            Form.Items.Item("Item_31").Enabled = true;
                            Form.Items.Item("Item_18").Enabled = true;
                            Form.Items.Item("btnGenPag").Enabled = true;
                            break;
                    }
                }
            }
        }

        private void ActualizarDatosCreacionPago(string nroPagoEfec, string estado, string msjError, int docEntry, int lineId)
        {

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"update \"@EXP_PMP1\" set \"U_EXP_NROPGOEFEC\" = '{nroPagoEfec}', \"U_EXP_ESTADO\" = '{estado}', " +
                $"\"U_EXP_MSJERROR\" = '{msjError.Replace("'", "")}' where \"DocEntry\" = '{docEntry}' and \"LineId\" = '{lineId}'";

            if (estado == "OK")
            {
                sqlQry = $"update \"@EXP_PMP1\" set \"U_EXP_NROPGOEFEC\" = '{nroPagoEfec}',U_EXP_DOCNUM_PAGO = (select \"DocNum\" from OVPM where \"DocEntry\" = '{nroPagoEfec}'), \"U_EXP_ESTADO\" = '{estado}', " +
                    $"\"U_EXP_MSJERROR\" = '{msjError.Replace("'", "")}' where \"DocEntry\" = '{docEntry}' and \"LineId\" = '{lineId}'";
            }

            recSet.DoQuery(sqlQry);

            /*
            var mtxDocs = Form.GetMatrix("Item_12");
            for (int i = 0; i < mtxDocs.RowCount; i++)
            {
                if (((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_34", i + 1)).Value == lineId.ToString())
                {
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_16", nroPagoEfec);
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_17", estado);
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_18", msjError);
                    break;
                }
            }
            */
        }

        private async void GenerarPagosAsync(int docEntryForm, SAPbouiCOM.ProgressBar progressBar, IEnumerable<dynamic> lstBancos, IEnumerable<SBOPago> lstPagos)
        {
            Globales.Aplication.StatusBar.SetText("Iniciando generación de pagos...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            var existenErrores = false;
            var pendienteRespuestaH2H = false;
            var cnds = (SAPbouiCOM.Conditions)Globales.Aplication.CreateObject(BoCreatableObjectType.cot_Conditions);
            var cnd = cnds.Add();
            cnd.Alias = "DocEntry";
            cnd.Operation = BoConditionOperation.co_EQUAL;
            cnd.CondVal = docEntryForm.ToString();

            var EXP_PMP4 = Form.GetDBDataSource("@EXP_PMP4");
            dcMetodoEnvPorBanco.Clear();
            for (int i = 0; i < EXP_PMP4.Size; i++)
            {
                if (string.IsNullOrWhiteSpace(EXP_PMP4.GetValue("U_COD_BANCO", i).Trim())) continue;
                dcMetodoEnvPorBanco.Add(EXP_PMP4.GetValue("U_COD_BANCO", i), EXP_PMP4.GetValue("U_COD_METODO", i));
            }

            foreach (var banc in lstBancos)
            {
                pendienteRespuestaH2H = false;
                //banc.Banco, banc.Sucursal, banc.Moneda, banc.CtaBanco
                var lstPagosAux = lstPagos.Where(p => p.CodSucursal == banc.Sucursal && p.MetodoPago.Banco == banc.Banco && p.MetodoPago.Cuenta == banc.CtaBanco);
                if (dcMetodoEnvPorBanco.ContainsKey(banc.Banco) && dcMetodoEnvPorBanco[banc.Banco] == "2")
                {
                    if (!PagoMasivoController.ValidaArchivoH2HEstado(docEntryForm, banc.Banco, banc.Sucursal, banc.Moneda, "PR"))
                    {
                        pendienteRespuestaH2H = true;
                        continue;
                    }
                    lstPagosAux = lstPagos.Where(p => p.CodSucursal == banc.Sucursal && p.MetodoPago.Banco == banc.Banco && p.MetodoPago.Cuenta == banc.CtaBanco && p.EstadoH2H == "OK");
                }
                //Globales.Company.StartTransaction();
                Globales.Aplication.StatusBar.SetText($"Iniciando generacion de pagos de la sucursal: { banc.Sucursal}, banco: {banc.Banco}, moneda: {banc.Moneda}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                var ejecucionOK = await Task.Run(() => GenerarPagosDocumentos(docEntryForm, lstPagosAux));
                if (ejecucionOK)
                {
                    dbsPMP1.Query(cnds);
                    Form.GetMatrix("Item_12").LoadFromDataSource();

                    //Globales.Aplication.Menus.Item("1304").Activate();
                    //Form.GetMatrix("Item_12").FlushToDataSource();
                    var rslt = await Task.Run(() => GenerarPagosCuentaBanco(docEntryForm, banc.Sucursal, banc.Banco, banc.CtaBanco));
                    dbsPMP1.Query(cnds);
                    Form.GetMatrix("Item_12").LoadFromDataSource();
                    //Form.GetMatrix("Item_12").LoadFromDataSource();
                    if (rslt)
                    {
                        try
                        {
                            /*Diferencia de tipo de cambio*/
                            rslt = await Task.Run(() => GenerarAsientoDiferenciaTipoDeCambio(docEntryForm, banc.Sucursal, banc.CtaBanco));
                            if (rslt)
                            {
                                //if (Globales.Company.InTransaction) Globales.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                                Globales.Aplication.StatusBar.SetText($"Pagos generados correctamente...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            }
                            else
                            {
                                //if (Globales.Company.InTransaction) Globales.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                existenErrores = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            existenErrores = true;
                            //if (Globales.Company.InTransaction) Globales.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                            Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            //Form.GetMatrix("Item_12").LoadFromDataSource();
                            //Form.GetMatrix("Item_12").LoadFromDataSourceEx();                           
                        }
                    }
                    else
                    {
                        existenErrores = true;
                        Globales.Aplication.StatusBar.SetText($"Se produjeron errores al ejecutar los pagos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
                else
                {
                    existenErrores = true;
                    //Globales.Aplication.Menus.Item("1304").Activate();
                    dbsPMP1.Query(cnds);
                    Form.GetMatrix("Item_12").LoadFromDataSource();
                    //if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    //Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    Globales.Aplication.StatusBar.SetText($"Se produjeron errores al ejecutar los pagos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

                }
                //Matrix.LoadFromDataSource();
            }
            Form.GetItem("btnGenPag").Enabled = true;
            //progressBar.Stop();
            if (!existenErrores)
            {
                if (pendienteRespuestaH2H)
                {
                    Globales.Aplication.StatusBar.SetText($"Pendiente la respuesta de los envios H2H...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                }
                else
                {
                    Globales.Aplication.StatusBar.SetText($"Proceso finalizado con éxito", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    dbsOPMP.SetValueExt("Status", "C");
                    dbsOPMP.SetValueExt("U_EXP_ESTADO", "C");
                    if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                }
            }
            else
            {
                Globales.Aplication.StatusBar.SetText($"Se produjeron errores al ejecutar los pagos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            Globales.Aplication.Menus.Item("1304").Activate();
        }

        private bool GenerarPagosDocumentos(int docEntryForm, IEnumerable<SBOPago> lstPagos)
        {
            var estado = string.Empty;
            var msjError = string.Empty;
            var nroPago = 0;
            var cntDocXPgo = lstPagos.Count();
            var cntPgoEjec = 0;
            var ejecucionOK = true;
            var tipoDeCambio = Convert.ToDouble(dbsOPMP.GetValueExt("U_EXP_TIPODECAMBIO"));
            var sboBob = ((SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge));
            var mndLoc = sboBob.GetLocalCurrency().Fields.Item(0).Value;

            foreach (var pgo in lstPagos)
            {
                cntPgoEjec++;
                Globales.Aplication.StatusBar.SetText($"Generando {cntPgoEjec} de {cntDocXPgo} pagos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                msjError = string.Empty;
                estado = "OK";
                nroPago = 0;
                var pgoDetAux = pgo.Detalle;
                pgo.Detalle = pgoDetAux.Where(d => d.TipoDocumento == 140);
                if (pgo.Detalle.Count() > 0)
                {
                    foreach (var pgoDet in pgo.Detalle)
                    {
                        try
                        {
                            nroPago = PagoMasivoController.GenerarPagoEfectuadoSBODesdeDraft(pgoDet.IdDocumento, pgo, tieneSucursales);
                            /*
                            dbsPMP1.SetValue("U_EXP_NROPGOEFEC", pgoDet.LineaPgoMsv - 1, nroPago.ToString());
                            dbsPMP1.SetValue("U_EXP_ESTADO", pgoDet.LineaPgoMsv - 1, "OK");
                            */
                            ActualizarDatosCreacionPago(nroPago.ToString(), "OK", msjError, docEntryForm, pgoDet.LineaPgoMsv);
                        }
                        catch (Exception ex)
                        {
                            ejecucionOK = false;
                            /*
                            dbsPMP1.SetValue("U_EXP_NROPGOEFEC", pgoDet.LineaPgoMsv - 1, nroPago == 0 ? string.Empty : nroPago.ToString());
                            dbsPMP1.SetValue("U_EXP_ESTADO", pgoDet.LineaPgoMsv - 1, "ER");
                            dbsPMP1.SetValue("U_EXP_MSJERROR", pgoDet.LineaPgoMsv - 1, ex.Message);
                            */
                            ActualizarDatosCreacionPago(nroPago == 0 ? string.Empty : nroPago.ToString(), "ER", ex.Message, docEntryForm, pgoDet.LineaPgoMsv);
                        }
                    };
                }

                pgo.Detalle = pgoDetAux.Where(d => d.TipoDocumento != 140);
                if (pgo.Detalle.Count() > 0)
                {
                    try
                    {

                        //pgo.Monto = pgo.Detalle.Sum(d => d.MontoPagado);
                        pgo.Monto = pgo.Detalle.Sum(d => d.MontoAPagar * ((d.MonedaDoc == mndLoc ? 1 : tipoDeCambio)
                        / (pgo.Moneda == mndLoc ? 1 : tipoDeCambio)));
                        PagoMasivoController.QuitarRetencionDocumento(pgo, codMonedaLocal);
                        pgo.Referencia = PagoMasivoController.ObtenerNroOperacion(docEntryForm, pgo.CodSucursal, pgo.MetodoPago.Banco, pgo.MetodoPago.Cuenta, pgo.Moneda);
                        nroPago = PagoMasivoController.GenerarPagoEfectuadoSBO(pgo, tieneSucursales);
                    }
                    catch (Exception ex)
                    {
                        ejecucionOK = false;
                        msjError = ex.Message;
                        estado = "ER";
                    }

                    foreach (var pgoDet in pgo.Detalle)
                    {
                        /*
                        dbsPMP1.SetValue("U_EXP_NROPGOEFEC", pgoDet.LineaPgoMsv - 1, nroPago == 0 ? string.Empty : nroPago.ToString());
                        dbsPMP1.SetValue("U_EXP_ESTADO", pgoDet.LineaPgoMsv - 1, estado);
                        dbsPMP1.SetValue("U_EXP_MSJERROR", pgoDet.LineaPgoMsv - 1, msjError);
                        */
                        ActualizarDatosCreacionPago(nroPago == 0 ? string.Empty : nroPago.ToString(), estado, msjError, docEntryForm, pgoDet.LineaPgoMsv);
                    }
                }
            }
            return ejecucionOK;
        }

        private bool GenerarPagosCuentaBanco(int docEntryForm, int sucursal, string banco, string codCtaBanco)
        {
            var estado = string.Empty;
            var msjError = string.Empty;
            var ejecucionOK = true;
            var obtSeriesPagoDesdeCBP = false;

            if (utblConf.GetByKey("13"))
            {
                string rslt13 = Convert.ToString(utblConf.UserFields.Fields.Item("U_VALOR").Value);
                obtSeriesPagoDesdeCBP = (rslt13 == "Y");
            }

            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDTDocs = dbsPMP1.GetAsXML();
            var _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
            var codSeriePago = int.TryParse(dbsOPMP.GetValue("U_EXP_SERIEPAGO", 0).Trim(), out var srePgoAux) ? srePgoAux : 0;
            var codSlcSucursal = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_COD_SUCURSAL"));
            var fechaPago = DateTime.ParseExact(dbsOPMP.GetValue("U_EXP_FECHAPAGO", 0).Trim(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            var tipoDeCambio = Convert.ToDouble(dbsOPMP.GetValueExt("U_EXP_TIPODECAMBIO"));

            var lstPagosPrev = _dsrXmlDBDataSource.Rows.Where(r =>
            r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_SLC_PAGO")).Value == "Y"
            && r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_ESTADO_H2H"))?.Value != "ER"
            && r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_COD_SUCURSAL")).Value == sucursal.ToString()
            && r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_CODBANCO")).Value == banco
            && r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_CODCTABANCO")).Value == codCtaBanco);

            if (lstPagosPrev.All(p => p.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_ESTADO")).Value == "OK"
              && p.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_ESTADO2")).Value != "OK"))
            {
                var pago = lstPagosPrev.GroupBy(g => new
                {
                    Sucursal = Convert.ToInt32(g.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_COD_SUCURSAL")).Value),
                    MedioDePago = g.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_MEDIODEPAGO")).Value,
                    MonedaDePago = g.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_MONEDA")).Value,
                    Banco = g.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_CODBANCO")).Value,
                    CtaBanco = g.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_CODCTABANCO")).Value
                }).Select(s => new SBOPago
                {
                    CodSerieSBO = codSlcSucursal != -1 ? codSeriePago : PagoMasivoController.ObtenerSeriePagoPorSucursal(s.Key.Sucursal, dbsPMP2, "N"),
                    CodSucursal = s.Key.Sucursal,
                    Moneda = s.Key.MonedaDePago,
                    FechaContabilizacion = fechaPago,
                    FechaDocumento = fechaPago,
                    FechaVencimiento = fechaPago,
                    TipoCambio = tipoDeCambio,
                    Monto = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
                    ExtLineasDS = s.Select(s1 => Convert.ToInt32(s1.Cells.FirstOrDefault(c => c.Uid == "LineId").Value)),
                    MetodoPago = new SBOMetodoPago
                    {
                        Tipo = s.Key.MedioDePago,
                        Pais = "PE",
                        Banco = s.Key.Banco,
                        Cuenta = s.Key.CtaBanco,
                        Referencia = "001",
                    },
                    Detalle = s.Select(s1 => new SBOPagoDetalle
                    {
                        CodigoCuenta = PagoMasivoController.ObtenerCodCuentaPuentePorSucursal(s.Key.Sucursal, tieneSucursales),
                        Monto = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
                        MontoAPagar = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
                    }).Take(1)
                }).FirstOrDefault();

                var nroPago = 0;
                try
                {
                    if (obtSeriesPagoDesdeCBP)
                    {
                        pago.CodSerieSBO = ObtenerCodSeriePagoPorCuentaMoneda(pago.MetodoPago.Banco, pago.MetodoPago.Cuenta, pago.Moneda);
                        nroPago = PagoMasivoController.GenerarPagoACuenta(docEntryForm, pago, tieneSucursales);
                    }
                    else
                        nroPago = PagoMasivoController.GenerarPagoACuenta(docEntryForm, pago, tieneSucursales);
                    foreach (var nroLinea in pago.ExtLineasDS)
                    {
                        ActualizarDatosCreacionPagoCuenta(nroPago.ToString(), "OK", string.Empty, Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry")), nroLinea);
                    }
                }
                catch (Exception ex)
                {
                    ejecucionOK = false;
                    foreach (var nroLinea in pago.ExtLineasDS)
                    {
                        ActualizarDatosCreacionPagoCuenta(string.Empty, "ER", ex.Message, Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry")), nroLinea);
                    }
                }

            }
            return ejecucionOK;
        }

        private bool GenerarAsientoDiferenciaTipoDeCambio(int docEntryForm, int sucursal, string codCtaBanco)
        {
            var rslt = true;

            try
            {
                PagoMasivoController.CrearAsientoAjusteRedondeo(docEntryForm, sucursal, codCtaBanco, tieneSucursales);
            }
            catch (Exception ex)
            {
                rslt = false;
                Globales.Aplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }

            return rslt;
        }

        private bool ValidarSelecSeriesPagoXSucursal()
        {
            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDSDocs = dbsPMP1.GetAsXML();
            var _dsrXmlDocs = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDSDocs));
            var strXMLDSSeries = dbsPMP2.GetAsXML();
            var _dsrXmlSeries = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDSSeries));

            var lstSucursalesDocs = _dsrXmlDocs.Rows.Select(r => r.Cells.FirstOrDefault(c => c.Uid == "U_EXP_COD_SUCURSAL").Value).Distinct();
            var lstSucursalesSrie = _dsrXmlSeries.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Cells.FirstOrDefault(c => c.Uid == "U_COD_SERIE_PAGO").Value)).Select(r => r.Cells.FirstOrDefault(c => c.Uid == "U_COD_SUCURSAL").Value).Distinct();
            return !lstSucursalesDocs.All(itm => lstSucursalesSrie.Contains(itm));
        }

        public void ValidarAnulacionPagos()
        {
            for (int i = 0; i < dbsPMP1.Size; i++)
            {
                if (dbsPMP1.GetValue("U_EXP_ESTADO", i) == "OK")
                    throw new InvalidOperationException("Existen pagos generados, no es posible la anulación");
            }
        }

        private void ActualizarDatosCreacionPagoCuenta(string nroPagoEfec, string estado, string msjError, int docEntry, int lineId)
        {

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"update \"@EXP_PMP1\" set \"U_EXP_NROPGOEFEC2\" = '{nroPagoEfec}', \"U_EXP_ESTADO2\" = '{estado}', " +
                $"\"U_EXP_MSJERROR\" = '{msjError.Replace("'", "")}' where \"DocEntry\" = '{docEntry}' and \"LineId\" = '{lineId}'";
            recSet.DoQuery(sqlQry);

            /*
            var mtxDocs = Form.GetMatrix("Item_12");
            for (int i = 0; i < mtxDocs.RowCount; i++)
            {
                if (((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_34", i + 1)).Value == lineId.ToString())
                {
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_32", nroPagoEfec);
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_33", estado);
                    mtxDocs.SetCellWithoutValidation(i + 1, "Col_18", msjError);
                    if (estado == "ER")
                    {
                        mtxDocs.SetCellWithoutValidation(i + 1, "Col_16", null);
                        mtxDocs.SetCellWithoutValidation(i + 1, "Col_17", null);
                    }
                    break;
                }
            }
            */
        }

        public void AnularPagosConError()
        {
            var pagoSAP = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var recSet2 = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            var docEntryForm = Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry"));
            var sqlQry = $"select \"U_EXP_NROPGOEFEC\" from \"@EXP_PMP1\" where \"U_EXP_ESTADO\" = 'OK' and coalesce(\"U_EXP_ESTADO2\",'') != 'OK' and \"DocEntry\" = '{docEntryForm}'";
            recSet.DoQuery(sqlQry);
            while (!recSet.EoF)
            {
                var idPago = Convert.ToInt32(recSet.Fields.Item(0).Value);
                pagoSAP.GetByKey(idPago);
                if (pagoSAP.Cancelled == SAPbobsCOM.BoYesNoEnum.tNO)
                {
                    var rsltCncel = pagoSAP.CancelbyCurrentSystemDate();
                    if (rsltCncel != 0)
                    {
                        Globales.Aplication.StatusBar.SetText(Globales.Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short);
                    }
                    else
                    {
                        sqlQry = $"update \"@EXP_PMP1\" set \"U_EXP_ESTADO\" = 'ER',\"U_EXP_MSJERROR\" = 'ANULADO' where \"DocEntry\" = '{docEntryForm}' and \"U_EXP_NROPGOEFEC\" = '{idPago}'";
                        recSet2.DoQuery(sqlQry);
                    }
                }
                /*
                else
                {
                    sqlQry = $"update \"@EXP_PMP1\" set \"U_EXP_ESTADO\" = 'ER',\"U_EXP_MSJERROR\" = 'ANULADO' where \"DocEntry\" = '{docEntryForm}' and \"U_EXP_NROPGOEFEC\" = '{idPago}'";
                    recSet2.DoQuery(sqlQry);
                }*/
                recSet.MoveNext();
            }
        }

        private int ObtenerCodSeriePagoPorCuentaMoneda(string codBanco, string codCuenta, string codMoneda)
        {
            var sqlQry = $"select U_EXD_PM_CODSERPGO from DSC1  where \"GLAccount\" = '{codCuenta}' and \"UsrNumber1\" = '{codMoneda}'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            recSet.DoQuery(sqlQry);
            if (recSet.EoF || (recSet.EoF == false && string.IsNullOrWhiteSpace(recSet.Fields.Item(0).Value)))
            {
                throw new Exception($"No se ha definido la serie de pago para el banco {codBanco} y moneda {codMoneda}");
            }
            return Convert.ToInt32(recSet.Fields.Item(0).Value);
        }

        public void HabilitarControlesEnModoBuscar()
        {
            Form.Items.Item("Item_7").Enabled = true;
            Form.Items.Item("Item_27").Enabled = true;
        }

        private void QuitarFilasNoSeleccionadas()
        {
            Form.GetMatrix("Item_12").FlushToDataSource();

            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDTDocs = dbsPMP1.GetAsXML();
            var xr = XmlReader.Create(new StringReader(strXMLDTDocs), new XmlReaderSettings { IgnoreWhitespace = false });

            var _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(xr);

            _dsrXmlDBDataSource.Rows = _dsrXmlDBDataSource.Rows.ToList().Where(r => r.Cells.FirstOrDefault(c => c.Uid == "U_EXP_SLC_PAGO").Value == "Y").ToArray();


            if (_dsrXmlDBDataSource.Rows.Length == 0) throw new Exception("Debe seleccionar al menos un documento para el pago masivo");

            _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDBDataSource);
                var verTmp = strWritter.ToString();
                dbsPMP1.LoadFromXML(strWritter.ToString());
                Form.GetMatrix("Item_12").LoadFromDataSource();
            }
        }

    }
}