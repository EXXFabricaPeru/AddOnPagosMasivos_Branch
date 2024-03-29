﻿using SAP_AddonExtensions;
using SAP_AddonFramework;
using SMC_APM.Controller;
using SMC_APM.dao;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private SAPbobsCOM.UserTable utblConf = null;
        private bool esAgenteRetenedor = true;
        private bool esTerceroRetenedor = true;
        private bool tieneAutorizaciones = false;

        public FormPagoMasivo(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
        }

        protected override void CargarFormularioInicial()
        {
            try
            {
                dbsOPMP = Form.DataSources.DBDataSources.Item("@EXP_OPMP");
                dbsPMP1 = Form.DataSources.DBDataSources.Item("@EXP_PMP1");
                dbsPMP2 = Form.DataSources.DBDataSources.Item("@EXP_PMP2");
                utblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");

                if (utblConf.GetByKey("2"))
                {
                    esAgenteRetenedor = utblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                }

                if (utblConf.GetByKey("3"))
                {
                    esTerceroRetenedor = utblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                }

                Matrix = Form.GetMatrix("Item_12");

                Matrix.SetColumnsVisible(false, "Col_3", "Col_12", "Col_15", "Col_19", "Col_24");

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
                dbsPMP2.Clear();
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
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            Matrix = Form.GetMatrix("Item_12");
            Button = Form.GetButton("btnGrbEnv");
            Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_26").Specific;
            Combo.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (Combo.ValidValues.Count > 0) Combo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dbsOPMP.SetValue("U_EXP_ESTADO", 0, tieneAutorizaciones ? "P" : "A");
            dbsOPMP.SetValue("U_EXP_FECHAPAGO", 0, DateTime.Today.ToString("yyyyMMdd"));
            dbsOPMP.SetValue("U_EXP_ESTADOEJEC", 0, "0");
            dbsOPMP.SetValue("U_EXP_TIPODECAMBIO", 0, sboBOB.GetCurrencyRate("USD", DateTime.Today).Fields.Item(0).Value.ToString());
            dbsOPMP.SetValue("DocNum", 0, Form.BusinessObject.GetNextSerialNumber(dbsOPMP.GetValue("Series", 0).Trim(), Form.BusinessObject.Type).ToString());
            Form.GetUserDataSource("UD_TOTAL").Value = "0.00";

            var position = 0;
            recSet.DoQuery("select \"BPLId\",\"BPLName\",coalesce(U_EXX_RETPRO,'N') as \"RetPro\" from OBPL order by 1");
            dbsPMP2.Clear();
            while (!recSet.EoF)
            {           
                dbsPMP2.InsertRecord(position);
                dbsPMP2.Offset = position;
                dbsPMP2.SetValue("U_COD_SUCURSAL", position, recSet.Fields.Item(0).Value);
                dbsPMP2.SetValue("U_NOM_SUCURSAL", position, recSet.Fields.Item(1).Value);
                dbsPMP2.SetValue("U_RETPRO", position, recSet.Fields.Item(2).Value);
                dbsPMP2.SetValue("U_COD_SERIE_PAGO", position, string.Empty);
                dbsPMP2.SetValue("U_COD_SERIE_RETEN", position, string.Empty);
                dbsPMP2.SetValue("U_NOM_SERIE_PAGO", position, string.Empty);
                dbsPMP2.SetValue("U_NOM_SERIE_RETEN", position, string.Empty);
                position++;
                recSet.MoveNext();
            }

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

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLstDocs", e =>
            {
                if (!e.BeforeAction)
                {
                    var fecha = dbsOPMP.GetValue("U_EXP_FECHA", 0).Trim();
                    var codSucursal = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_COD_SUCURSAL").Trim());
                    var lstDocumentos = PagoMasivoController.ListarDocumentosParaPagos(fecha, codSucursal);
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
                        dbsPMP1.SetValue("U_EXP_ESTADO", lineNum, string.Empty);
                        dbsPMP1.SetValue("U_EXP_NROCTAPROV", lineNum, doc.NroCtaProveedor);
                        dbsPMP1.SetValue("U_EXP_CODBANCOPROV", lineNum, doc.CodBncProveedor.ToString());
                        dbsPMP1.SetValue("U_EXP_CODRETENCION", lineNum, doc.CodRetencion);
                        dbsPMP1.SetValue("U_EXP_IMPRETENCION", lineNum, doc.ImporteRetencion.ToString());
                        dbsPMP1.SetValue("U_EXP_TCDOCUMENTO", lineNum, doc.TCDocumento.ToString());
                        dbsPMP1.SetValue("U_EXP_GLOSAASIENTO", lineNum, doc.GlosaAsiento);
                        dbsPMP1.SetValue("U_EXP_CARDCODE_FACTO", lineNum, doc.CardCodeFactoring);
                        dbsPMP1.SetValue("U_EXP_CARDNAME_FACTO", lineNum, doc.CardNameFactoring);
                    }
                    Matrix.LoadFromDataSource();
                    Matrix.AutoResizeColumns();
                    Form.GetUserDataSource("UD_TOTAL").Value = lstDocumentos.Sum(d => d.Importe).ToString();
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
                        var lstPagos = PagoMasivoController.ObtenerListaPagos(dbsOPMP, pgoDS, dbsPMP2, esAgenteRetenedor);
                        var estado = string.Empty;
                        var msjError = string.Empty;
                        var nroPago = 0;
                        var cntDocXPgo = lstPagos.Count();
                        var cntPgoEjec = 0;
                        var cntErrores = 0;
                        //var pgrssBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, cntDocXPgo, false);
                        var docEntryForm = Convert.ToInt32(dbsOPMP.GetValueExt("DocEntry"));
                        try
                        {

                            GenerarPagosAsync(docEntryForm, lstPagos);

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

            Eventos.Add(new EventoData(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD, TYPE, e =>
            {
                if (!e.BeforeAction)
                {
                    var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                    var EXP_PMP1 = Form.GetDBDataSource("@EXP_PMP1");
                    var totPgoMsv = 0d;
                    for (int i = 0; i < EXP_PMP1.Size; i++)
                    {
                        EXP_PMP1.Offset = i;
                        totPgoMsv += Convert.ToDouble(EXP_PMP1.GetValue("U_EXP_IMPORTE", i));
                    }
                    Form.GetUserDataSource("UD_TOTAL").Value = totPgoMsv.ToString();
                    HabilitarControlesPorEstado(estadoDoc);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    var seriePago = dbsOPMP.GetValue("U_EXP_SERIEPAGO", 0).Trim();
                    var serieRetencion = dbsOPMP.GetValue("U_EXP_SERIERETENCION", 0).Trim();
                    var codSucursal = Convert.ToInt32(dbsOPMP.GetValue("U_EXP_COD_SUCURSAL", 0).Trim());
                    if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && ((codSucursal != -1 && string.IsNullOrWhiteSpace(seriePago))
                    || (codSucursal == -1 && ValidarSelecSeriesPagoXSucursal()))) throw new InvalidOperationException("Seleccione una serie de pago");
                    //if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && string.IsNullOrWhiteSpace(serieRetencion) && esAgenteRetenedor) throw new InvalidOperationException("Seleccione una serie de retención");
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && (dbsPMP1.Size == 0 || (dbsPMP1.Size > 0 && string.IsNullOrWhiteSpace(dbsPMP1.GetValue("U_EXP_DOCENTRYDOC", 0)))))
                        throw new InvalidOperationException("Debe registrar al menos un documento para pagar");
                    var btnCrgEnv = (SAPbouiCOM.Button)Form.Items.Item("btnGrbEnv").Specific;
                    var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGenTXT", e =>
            {
                if (!e.BeforeAction && e.ActionSuccess)
                {
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    Globales.Aplication.StatusBar.SetText("Iniciando generación de archivos para bancos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    Matrix.FlushToDataSource();
                    var pgoDS = dbsPMP1.GetAsXML();
                    var lstBancos = PagoMasivoController.ObtenerListaBancoPorPago(pgoDS);
                    //.Select(s => new { CodBanco = s.Banco, CodMoneda = s.Moneda, GLCuenta = s.MetodoPago.Cuenta }).Distinct().ToList();
                    var docEntry = Convert.ToInt32(dbsOPMP.GetValue("DocEntry", 0));
                    var pgrssBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, 1, false);
                    try
                    {
                        foreach (var banc in lstBancos)
                        {
                            try
                            {
                                var codPais = ObtenerPaisBanco(banc.Banco, banc.CtaBanco);
                                PagoMasivoController.GenerarTXTBancos(docEntry, banc.Banco, banc.Sucursal, banc.Moneda, banc.CtaBanco, codPais);
                                Globales.Aplication.StatusBar.SetText($"Archivos para banco {banc.Banco} generados correctamente en la ruta \n C:\\PagosMasivos\\", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                            }
                            catch (Exception ex)
                            {
                                Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            }
                        }
                        //Globales.Aplication.MessageBox("Archivos para bancos generados correctamente en la ruta \n C:\\PagosMasivos\\");
                    }
                    finally
                    {
                        dbsOPMP.SetValueExt("U_EXP_ESTADOEJEC", esTerceroRetenedor ? "2" : "1");
                        if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        Form.GetItem("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                        HabilitarControlesPorEstado(estadoDoc);
                        //Form.GetItem("btnGenPag").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                        pgrssBar.Stop();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLibSNT", e => AbrirLiberacionSUNAT(e)));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_24", e => AbrirFormAsignacionDeSeries(e, "P")));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_23", e => AbrirFormAsignacionDeSeries(e, "R")));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED, "Item_12", e =>
            {
                if (e.BeforeAction)
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
                    var sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'N'";
                    var cmbSeriePago = (SAPbouiCOM.ComboBox)Form.GetItem("Item_3").Specific;
                    recSet.DoQuery(sqlQry);
                    while (cmbSeriePago.ValidValues.Count > 0) cmbSeriePago.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSeriePago.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }

                    sqlQry = $"select \"Series\",\"SeriesName\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'Y'";
                    var cmbSerieReten = (SAPbouiCOM.ComboBox)Form.GetItem("Item_5").Specific;
                    recSet.DoQuery(sqlQry);
                    while (cmbSerieReten.ValidValues.Count > 0) cmbSerieReten.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSerieReten.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                }
                return true;
            }));
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

        private void HabilitarControlesPorEstado(string codEstado)
        {
            codEstado = tieneAutorizaciones ? codEstado : (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE ? "P" : "A");
            Form.Items.Item("edtFocus").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
            Form.Items.Item("btnGrbEnv").Enabled = false;
            Form.Items.Item("Item_1").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("btnLstDocs").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.Items.Item("Item_3").Enabled = false;
            Form.Items.Item("Item_5").Enabled = false;
            Form.Items.Item("Item_12").Enabled = false;
            Form.Items.Item("Item_17").Enabled = false;
            Form.Items.Item("Item_20").Enabled = false;
            Form.Items.Item("Item_26").Enabled = false;
            Form.Items.Item("Item_31").Enabled = false;
            Form.Items.Item("Item_33").Enabled = false;
            Form.Items.Item("btnCrgRsp").Enabled = false;
            Form.Items.Item("btnGenTXT").Enabled = false;
            Form.Items.Item("btnGenPag").Enabled = false;
            Form.Items.Item("btnTrcRtn").Enabled = false;
            Form.Items.Item("btnLibSNT").Enabled = false;
            Form.Items.Item("Item_23").Enabled = false;
            Form.Items.Item("Item_24").Enabled = false;
            if (codEstado == "P" || codEstado == "R")
            {
                Form.Items.Item("btnGrbEnv").Enabled = (Form.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE && true);
                //Form.Items.Item("Item_3").Enabled = true;
                //Form.Items.Item("Item_5").Enabled = true;
                Form.Items.Item("Item_12").Enabled = true;
                Form.Items.Item("Item_17").Enabled = true;
                Form.Items.Item("Item_26").Enabled = true;
                Form.Items.Item("Item_31").Enabled = true;
                Form.Items.Item("Item_33").Enabled = true;
            }
            else if (codEstado == "A")
            {
                Form.Items.Item("Item_12").Enabled = true;
                Form.Items.Item("Item_17").Enabled = true;
                Form.Items.Item("Item_20").Enabled = true;
                Form.Items.Item("btnTrcRtn").Enabled = true;
                Form.Items.Item("btnLibSNT").Enabled = true;
                Form.Items.Item("btnCrgRsp").Enabled = true;
                if (esTerceroRetenedor)
                {
                    switch (dbsOPMP.GetValueExt("U_EXP_ESTADOEJEC"))
                    {
                        case "1":
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            break;
                        case "2":
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
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            break;
                        case "1":
                            Form.Items.Item("btnGenTXT").Enabled = true;
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
            recSet.DoQuery(sqlQry);
        }

        private async void GenerarPagosAsync(int docEntryForm, IEnumerable<SBOPago> lstPagos)
        {
            Globales.Aplication.StatusBar.SetText("Iniciando generación de pagos...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning); ;
            var ejecucionOK = await Task.Run(() => GenerarPagosDocumentos(docEntryForm, lstPagos));
            if (ejecucionOK)
            {
                Globales.Aplication.Menus.Item("1304").Activate();
                GenerarPagosCuentaBanco(docEntryForm);
                Globales.Aplication.StatusBar.SetText($"Proceso finalizado con éxito", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            else
            {
                Globales.Aplication.StatusBar.SetText($"Proceso finalizado con errores", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                Globales.Aplication.Menus.Item("1304").Activate();
            }
            //Matrix.LoadFromDataSource();
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
                            nroPago = PagoMasivoController.GenerarPagoEfectuadoSBODesdeDraft(pgoDet.IdDocumento, pgo);
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
                        nroPago = PagoMasivoController.GenerarPagoEfectuadoSBO(pgo);
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

        private bool GenerarPagosCuentaBanco(int docEntryForm)
        {
            var estado = string.Empty;
            var msjError = string.Empty;
            var ejecucionOK = false;

            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDTDocs = dbsPMP1.GetAsXML();
            var _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
            var codSeriePago = int.TryParse(dbsOPMP.GetValue("U_EXP_SERIEPAGO", 0).Trim(), out var srePgoAux) ? srePgoAux : 0;
            var codSlcSucursal = Convert.ToInt32(dbsOPMP.GetValueExt("U_EXP_COD_SUCURSAL"));

            var lstPagos = _dsrXmlDBDataSource.Rows.Where(r => r.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_ESTADO")).Value == "OK").GroupBy(g => new
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
                FechaContabilizacion = DateTime.Today,
                FechaDocumento = DateTime.Today,
                FechaVencimiento = DateTime.Today,
                Monto = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
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
                    CodigoCuenta = PagoMasivoController.ObtenerCodCuentaPuentePorSucursal(s.Key.Sucursal),
                    Monto = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
                    MontoAPagar = s.Sum(sm => Convert.ToDouble(sm.Cells.FirstOrDefault(c => c.Uid.Equals("U_EXP_IMPORTE")).Value)),
                }).Take(1)
            });

            foreach (var pago in lstPagos)
            {
                PagoMasivoController.GenerarPagoACuenta(pago);
            }
            return ejecucionOK;
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
    }
}