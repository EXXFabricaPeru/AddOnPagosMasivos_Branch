using SAP_AddonExtensions;
using SAP_AddonFramework;
using SMC_APM.Controller;
using SMC_APM.dao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                Matrix = Form.GetMatrix("Item_12");

                Matrix.SetColumnsVisible(false, "Col_12", "Col_15", "Col_19");

                Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_3").Specific;
                while (Combo.ValidValues.Count > 0) Combo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                var recSet = PagoMasivoController.ObtenerSeriesDocumentoPago();
                while (!recSet.EoF)
                {
                    Combo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_5").Specific;
                while (Combo.ValidValues.Count > 0) Combo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                recSet = PagoMasivoController.ObtenerSeriesRetencion();
                while (!recSet.EoF)
                {
                    Combo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Add, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
                Form.Items.Item("Item_1").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Find, SAPbouiCOM.BoModeVisualBehavior.mvb_True);

                Form.Items.Item("btnLstDocs").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_All, SAPbouiCOM.BoModeVisualBehavior.mvb_False);
                Form.Items.Item("btnLstDocs").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, (int)SAPbouiCOM.BoAutoFormMode.afm_Add, SAPbouiCOM.BoModeVisualBehavior.mvb_True);
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
            dbsOPMP.SetValue("U_EXP_ESTADO", 0, "P");
            dbsOPMP.SetValue("U_EXP_FECHAPAGO", 0, DateTime.Today.ToString("yyyyMMdd"));
            dbsOPMP.SetValue("U_EXP_TIPODECAMBIO", 0, sboBOB.GetCurrencyRate("USD", DateTime.Today).Fields.Item(0).Value.ToString());
            dbsOPMP.SetValue("DocNum", 0, Form.BusinessObject.GetNextSerialNumber(dbsOPMP.GetValue("Series", 0).Trim(), Form.BusinessObject.Type).ToString());
            Button.Caption = "Grabar";
            //Matrix.Columns.Item("Col_0").Editable = true;
            Matrix.Columns.Item("Col_2").Editable = true;
            Form.Items.Item("btnGenTXT").Enabled = false;
            Form.Items.Item("Item_20").Enabled = false;
            Form.Items.Item("btnCrgRsp").Enabled = false;
            Form.Items.Item("btnTrcRtn").Enabled = false;
            Form.Items.Item("btnGenPag").Enabled = false;
            Form.Items.Item("btnLibSNT").Enabled = false;
        }
        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLstDocs", e =>
            {
                if (!e.BeforeAction)
                {
                    var fecha = dbsOPMP.GetValue("U_EXP_FECHA", 0).Trim();
                    var lstDocumentos = PagoMasivoController.ListarDocumentosParaPagos(fecha);
                    var lineNum = 0;

                    dbsPMP1.Clear();
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    foreach (var doc in lstDocumentos)
                    {
                        dbsPMP1.InsertRecord(lineNum);
                        dbsPMP1.Offset = lineNum;
                        dbsPMP1.SetValue("U_EXP_SLC_PAGO", lineNum, doc.SlcPago);
                        dbsPMP1.SetValue("U_EXP_SLC_RETENCION", lineNum, doc.SlcRetencion);
                        dbsPMP1.SetValue("U_EXP_COD_ESCENARIOPAGO", lineNum, doc.CodigoEscenarioPago);
                        dbsPMP1.SetValue("U_EXP_MEDIODEPAGO", lineNum, doc.MedioDePago);
                        dbsPMP1.SetValue("U_EXP_CODBANCO", lineNum, doc.CodBanco);
                        dbsPMP1.SetValue("U_EXP_CODCTABANCO", lineNum, doc.CodCtaBanco);
                        dbsPMP1.SetValue("U_EXP_DOCENTRYDOC", lineNum, doc.DocEntryDocumento.ToString());
                        dbsPMP1.SetValue("U_EXP_TIPODOC", lineNum, doc.TipoDocumento);
                        dbsPMP1.SetValue("U_EXP_MONEDA", lineNum, doc.Moneda);
                        dbsPMP1.SetValue("U_EXP_IMPORTE", lineNum, doc.Importe.ToString());
                        dbsPMP1.SetValue("U_EXP_CARDCODE", lineNum, doc.CardCode);
                        dbsPMP1.SetValue("U_EXP_CARDNAME", lineNum, doc.CardName);
                        dbsPMP1.SetValue("U_EXP_ASNROLINEA", lineNum, doc.NroLineaAsiento);
                        dbsPMP1.SetValue("U_EXP_NMROCUOTA", lineNum, doc.NroCuota);
                        dbsPMP1.SetValue("U_EXP_NRODOCUMENTOSN", lineNum, doc.NroDocumentoSN);
                        dbsPMP1.SetValue("U_EXP_APLSRERTN", lineNum, doc.AplSreRetencion);
                        dbsPMP1.SetValue("U_EXP_ESTADO", lineNum, string.Empty);
                    }
                    Matrix.LoadFromDataSource();
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
                        //Validaciones
                        var seriePago = dbsOPMP.GetValue("U_EXP_SERIEPAGO", 0).Trim();
                        var serieRetencion = dbsOPMP.GetValue("U_EXP_SERIERETENCION", 0).Trim();
                        if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && string.IsNullOrWhiteSpace(seriePago)) throw new InvalidOperationException("Seleccione una serie de pago");
                        if ((Form.Mode != SAPbouiCOM.BoFormMode.fm_FIND_MODE) && string.IsNullOrWhiteSpace(serieRetencion)) throw new InvalidOperationException("Seleccione una serie de retención");
                        if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && (dbsPMP1.Size == 0 || (dbsPMP1.Size > 0 && string.IsNullOrWhiteSpace(dbsPMP1.GetValue("U_EXP_DOCENTRYDOC", 0)))))
                            throw new InvalidOperationException("Debe registrar al menos un documento para pagar");
                        if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && PagoMasivoController.ValidarRegistroUnicoPorFecha(dbsOPMP.GetValueExt("U_EXP_FECHA").Trim())) throw new InvalidOperationException("Ya existe un registro para la fecha seleccionada como filtro");
                        Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
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
                    var docEntryPMP = Convert.ToInt32(dbsOPMP.GetValue("DocEntry", 0));
                    PagoMasivoController.generarTXT3Retenedor(docEntryPMP);
                    Globales.Aplication.MessageBox("Archivo TXT de terceros generado con éxito en la ruta\n D:\\Pagos_Masivos\\TerceroRetenedor\\ArchivoSunat\\");
                }
                return true;
            }));


            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGenPag", e =>
            {
                if (!e.BeforeAction)
                {
                    var rslt = Globales.Aplication.MessageBox("Se procederá a generar el pago de los documentos seleccionados \n ¿Desea continuar con esta accion?"
                    , Btn1Caption: "SI", Btn2Caption: "NO");
                    if (rslt == 1)
                    {
                        Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                        Matrix.FlushToDataSource();
                        var pgoDS = dbsPMP1.GetAsXML();
                        var lstPagos = PagoMasivoController.ObtenerListaPagos(dbsOPMP, pgoDS);
                        var estado = string.Empty;
                        var msjError = string.Empty;
                        var nroPago = 0;
                        var pgrssBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, 1, false);

                        try
                        {
                            Globales.Aplication.StatusBar.SetSystemMessage("Iniciando generacion de pagos...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning); ;
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
                                    dbsPMP1.SetValue("U_EXP_NROPGOEFEC", linea - 1, nroPago == 0 ? string.Empty : nroPago.ToString());
                                    dbsPMP1.SetValue("U_EXP_ESTADO", linea - 1, estado);
                                    dbsPMP1.SetValue("U_EXP_MSJERROR", linea - 1, msjError);
                                }
                            }
                            Matrix.LoadFromDataSource();
                            if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                                Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                            Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            pgrssBar.Stop();
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
                        var lstRspSNT = PagoMasivoController.LeerTXT3RetenedorRsp(fpRspSNT).ToList();
                        var nroRUC = string.Empty;
                        TXT3RetenedorRsp rspSNT = null;

                        for (int i = 0; i < dbsPMP1.Size; i++)
                        {
                            dbsPMP1.Offset = i;
                            nroRUC = dbsPMP1.GetValue("U_EXP_NRODOCUMENTOSN", i).Trim();
                            rspSNT = lstRspSNT.Find(f => f.RUC == nroRUC);
                            if (rspSNT != null && rspSNT.Estado == "E")
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
                    var btnCrgEnv = (SAPbouiCOM.Button)Form.Items.Item("btnGrbEnv").Specific;
                    var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                    Form.Items.Item("btnGenTXT").Enabled = false;
                    Form.Items.Item("Item_20").Enabled = false;
                    Form.Items.Item("btnCrgRsp").Enabled = false;
                    Form.Items.Item("btnTrcRtn").Enabled = false;
                    Form.Items.Item("btnGenPag").Enabled = false;
                    if (estadoDoc == "P" || estadoDoc == "R")
                    {
                        btnCrgEnv.Caption = "Enviar";
                        if (estadoDoc == "P")
                        {
                            Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                            Form.Items.Item("btnGenTXT").Enabled = true;
                            Form.Items.Item("Item_20").Enabled = true;
                            Form.Items.Item("btnCrgRsp").Enabled = true;
                            Form.Items.Item("btnTrcRtn").Enabled = true;
                        }
                        DesactivarItemsEnEstadoAprobado(estadoDoc);
                    }
                    else if (estadoDoc == "A")
                    {
                        Form.Items.Item("btnGenTXT").Enabled = true;
                        Form.Items.Item("btnGenPag").Enabled = true;
                        DesactivarItemsEnEstadoAprobado(estadoDoc);
                    }
                    else
                        btnCrgEnv.Caption = "OK";
                }
                return true;
            }));


            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {

                var btnCrgEnv = (SAPbouiCOM.Button)Form.Items.Item("btnGrbEnv").Specific;
                var estadoDoc = dbsOPMP.GetValue("U_EXP_ESTADO", 0).Trim();
                if (!e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE && e.InnerEvent == false)
                {
                    Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                }
                else if (e.BeforeAction && (Form.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE
                   || Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) && estadoDoc == "P")
                {
                    dbsOPMP.SetValue("U_EXP_ESTADO", 0, "A");
                    Form.Items.Item("btnGenPag").Enabled = true;
                    DesactivarItemsEnEstadoAprobado("A");
                }
                else if (!e.BeforeAction && (Form.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE
                        || Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) && estadoDoc == "E")
                {
                    btnCrgEnv.Caption = "OK";
                }
                else if (!e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {

                }

                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnGenTXT", e =>
            {
                if (!e.BeforeAction && e.ActionSuccess)
                {
                    Globales.Aplication.StatusBar.SetText("Iniciando generación de archivos para bancos", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    Matrix = (SAPbouiCOM.Matrix)Form.Items.Item("Item_12").Specific;
                    Matrix.FlushToDataSource();
                    var pgoDS = dbsPMP1.GetAsXML();
                    var lstBancos = PagoMasivoController.ObtenerListaPagos(dbsOPMP, pgoDS)
                    .Select(s => new { CodBanco = s.MetodoPago.Banco, CodMoneda = s.Moneda }).Distinct().ToList();
                    var docEntry = Convert.ToInt32(dbsOPMP.GetValue("DocEntry", 0));
                    var pgrssBar = (SAPbouiCOM.ProgressBar)Globales.Aplication.StatusBar.CreateProgressBar(null, 1, false);
                    try
                    {
                        foreach (var banc in lstBancos)
                        {
                            try
                            {
                                PagoMasivoController.GenerarTXTBancos(docEntry, banc.CodBanco, banc.CodMoneda);
                            }
                            catch (Exception ex)
                            {
                                Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            }
                        }
                        Globales.Aplication.MessageBox("Archivos para bancos generados correctamente en la ruta \n C:\\PagosMasivos\\");
                    }
                    finally
                    {
                        pgrssBar.Stop();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnLibSNT", e => AbrirLiberacionSUNAT(e)));

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

        private void DesactivarItemsEnEstadoAprobado(string codEstado)
        {
            Matrix = Form.GetMatrix("Item_12");
            Form.Items.Item("Item_3").Enabled = true;
            Form.Items.Item("Item_5").Enabled = true;
            Form.Items.Item("Item_31").Enabled = true;
            Form.Items.Item("Item_33").Enabled = true;
            Form.Items.Item("Item_20").Enabled = true;
            Form.Items.Item("Item_17").Enabled = true;
            Form.Items.Item("btnCrgRsp").Enabled = true;
            //Matrix.Columns.Item("Col_0").Editable = true;
            Matrix.Columns.Item("Col_2").Editable = true;
            if (codEstado == "A")
            {
                Form.GetItem("Item_10").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                Matrix = Form.GetMatrix("Item_12");
                Form.Items.Item("Item_3").Enabled = false;
                Form.Items.Item("Item_5").Enabled = false;
                Form.Items.Item("Item_31").Enabled = false;
                Form.Items.Item("Item_33").Enabled = false;
                Form.Items.Item("Item_20").Enabled = false;
                Form.Items.Item("Item_17").Enabled = false;
                Form.Items.Item("btnCrgRsp").Enabled = false;
                //Matrix.Columns.Item("Col_0").Editable = false;
                Matrix.Columns.Item("Col_2").Editable = false;
            }
        }
    }
}
