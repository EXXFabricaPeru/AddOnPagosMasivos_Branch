using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SMC_APM.SAP;
using SMC_APM.dto;
using SMC_APM.dao;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace SMC_APM.Controladores
{
    class ctrFrmDatosCuenta
    {
        #region Atributos
        public SAPbouiCOM.Application sboApplication;
        public SAPbobsCOM.Company sboCompany;
        SAPbouiCOM.Form oForm = null;
        SAPbouiCOM.Item oItem = null;
        SAPbouiCOM.DataTable dtFact = null;
        SAPbouiCOM.DataTable dtSelect = null;
        SAPbouiCOM.Matrix mtPendientes = null;
        SAPbouiCOM.Matrix mtSeleccionados = null;
        SAPbouiCOM.Column _column = null;
        string tipoEscenario;
        string codEscenario;
        string money;
        List<PagoDTO> lstPagos = null;
        string tipoBanco = "";
        #endregion

        #region Constructor
        public ctrFrmDatosCuenta()
        {
        }

        public ctrFrmDatosCuenta(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
        }
        #endregion

        public void cargarFormulario(SAPbouiCOM.DataTable dtSeleccinado, string moneda, string cuentaBank, string tipoEscenario, string frmUID, string codEscenario, string tipoBanco)
        {
            sapObjetos sapObj = new sapObjetos();
            sapObj.sapCargarFormulario(sboApplication, frmUID, Properties.Resources.frmSMC_PM_DatosCuenta, "");
            this.tipoEscenario = tipoEscenario;
            this.codEscenario = codEscenario;
            iniciarFormulario(frmUID, dtSeleccinado, moneda, cuentaBank, tipoEscenario, codEscenario, tipoBanco);
        }

        #region Metodos Privados
        public void iniciarFormulario(string FormUID, SAPbouiCOM.DataTable dtSeleccinado, string moneda, string cuentaBank, string tipoEscenario, string codEscenario, string tipoBanco)
        {
            SAPbouiCOM.Item oNewItem = null;
            SAPbouiCOM.CheckBox oCheck = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.ComboBox oCombo = null;
            SAPbouiCOM.Matrix mt = null;
            SAPbouiCOM.Folder oFolder = null;
            SAPbouiCOM.LinkedButton lnkPurchaseFac = null;
            SAPbouiCOM.LinkedButton lnkPurchaseSelect = null;
            SAPbobsCOM.Recordset oRecord = null;
            daoFacturaProveedor daoFacturaProveedor = new daoFacturaProveedor();
            List<dtoSeriePago> listSeriesPago = new List<dtoSeriePago>();
            daoBanco _daoBanco = new daoBanco();
            string lote = "";
            string mensaje = "";

            try
            {
                this.tipoEscenario = tipoEscenario;
                this.tipoBanco = tipoBanco;
                this.codEscenario = codEscenario;
                this.money = moneda;
                oForm = sboApplication.Forms.Item(FormUID);

                //consulta series de pago
                listSeriesPago = _daoBanco.listarSeriesPago(codEscenario, sboApplication, ref mensaje);

                try
                {
                    oItem = oForm.Items.Item("cmb_1_1");
                    oCombo = (SAPbouiCOM.ComboBox)oItem.Specific;
                    oCombo.ValidValues.Add("0", "");
                    //llena combo series
                    for (int i = 0; i < listSeriesPago.Count; i++)
                    {
                        oCombo.ValidValues.Add(listSeriesPago[i].Series, listSeriesPago[i].SeriesName);
                    }
                    oCombo.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
                    oCombo.Item.DisplayDesc = true;
                    oCombo.Select(listSeriesPago[0].SeriesDefecto, SAPbouiCOM.BoSearchKey.psk_ByDescription);
                }
                catch(Exception e)
                {
                    string err = e.ToString();
                }

                oItem = oForm.Items.Item("txtEsc");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = tipoEscenario + "-" + codEscenario; //asigna valor

                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                dtFact = oForm.DataSources.DataTables.Item("dtaFact");
                oItem = oForm.Items.Item("mtxFact");
                mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;
                oForm.Left = 350;
                oForm.Top = 10;

   

                oItem = oForm.Items.Item("txtMoneda");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = moneda; //asigna valor 

                oItem = oForm.Items.Item("txtBanco");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = cuentaBank;//asigna valor


                oItem = oForm.Items.Item("codBanco");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = tipoBanco; //asigna valor

                oItem = oForm.Items.Item("txtLote");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
   

                oItem = oForm.Items.Item("mtxFact");
                mt = (SAPbouiCOM.Matrix)oItem.Specific;
                mt.Columns.Item("lSelect").ValOn = "Y";
                mt.Columns.Item("lSelect").ValOff = "N";
                oItem.ToPane = 1;
                oItem.FromPane = 1;

                oItem = oForm.Items.Item("mtxSelect");
                mt = (SAPbouiCOM.Matrix)oItem.Specific;
                mt.Columns.Item("lSelect").ValOn = "Y";
                mt.Columns.Item("lSelect").ValOff = "N";
                oItem.ToPane = 2;
                oItem.FromPane = 2;

                oItem = oForm.Items.Item("btnAgregar");
                oItem.ToPane = 1;
                oItem.FromPane = 1;
                oItem = oForm.Items.Item("btnQuitar");
                oItem.ToPane = 2;
                oItem.FromPane = 2;
                oItem.Visible = false;
                oItem.Enabled = false;


                //oItem = oForm.Items.Item("btnTxt");
                //oItem.Visible = false;

                //oItem = oForm.Items.Item("btnPagos");
                //oItem.Visible = false;

                if (tipoEscenario.Equals("DET"))
                {
                    oItem = oForm.Items.Item("lblLote");
                    oItem.Visible = true;
                    oItem = oForm.Items.Item("lblArch");
                    oItem.Visible = true;
                    //oItem = oForm.Items.Item("lblDepo");
                    //oItem.Visible = true;
                    oItem = oForm.Items.Item("txtLote");
                    oItem.Visible = true;
                    oItem = oForm.Items.Item("txtArch");
                    oItem.Visible = true;
                    //oItem = oForm.Items.Item("txtDepo");
                    //oItem.Visible = true;
                    oItem = oForm.Items.Item("lblOpe");
                    oItem.Visible = true;
                    oItem = oForm.Items.Item("txtOpe");
                    oItem.Visible = true;
                }
                else
                {
                    oItem = oForm.Items.Item("lblOpe");
                    oItem.Visible = true;
                    oItem = oForm.Items.Item("txtOpe");
                    oItem.Visible = true;
                }

                try
                {
                    oNewItem = oForm.Items.Add("tabSelec", SAPbouiCOM.BoFormItemTypes.it_FOLDER);
                    oItem = oForm.Items.Item("tabDoc");
                    oItem.Width = 120;
                    oNewItem.Top = oItem.Top;
                    oNewItem.Height = oItem.Height;
                    oNewItem.Width = oItem.Width;
                    oNewItem.Left = oItem.Width;

                    oFolder = (SAPbouiCOM.Folder)oNewItem.Specific;
                    oFolder.Caption = "Mismo Banco";
                    oFolder.GroupWith("tabDoc");
                    oFolder.Select();
                }
                catch (Exception ex)
                {

                }

                try
                {
                    //inicializa datatble
                    dtFact.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("DocEntry", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("CardCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("CardName", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("NumAtCard", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("DocCur", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("Cuenta", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("CuentaMoneda", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("BankCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("Retenido", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("Liberado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("Registrado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("RCEmbargo", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtFact.Columns.Add("FechaSUNAT", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

                    dtFact.Columns.Add("Documento", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                }
                catch
                {

                }

                try
                {
                    //inicializa datatble
                    dtSelect.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("DocEntry", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("CardCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("CardName", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("NumAtCard", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("DocCur", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("Cuenta", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("CuentaMoneda", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("BankCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("Retenido", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("Liberado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("Registrado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("RCEmbargo", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                    dtSelect.Columns.Add("FechaSUNAT", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

                    dtSelect.Columns.Add("Documento", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                }
                catch
                {

                }

                //limpiar datatables
                dtFact.Rows.Clear();
                dtSelect.Rows.Clear();

                //consulta proc
                string consultaMismoBanco = "CALL \"SMC_APM_LISTAR_PAGOS_MISMOBANCO\" ('" + codEscenario + "','" + tipoBanco + "','" + moneda + "') ";
                string consultaIterbancario = "CALL \"SMC_APM_LISTAR_PAGOS_INTERBANCARIO\" ('" + codEscenario + "','" + tipoBanco + "','" + moneda + "') ";

                //ejecuta proc
                dtFact.ExecuteQuery(consultaIterbancario);
                dtSelect.ExecuteQuery(consultaMismoBanco);

                oItem = oForm.Items.Item("txtRC");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = dtFact.Columns.Item("RCEmbargo").Cells.Item(0).Value.ToString();
                if(oEdit.Value.ToString().Equals("")) oEdit.Value = dtSelect.Columns.Item("RCEmbargo").Cells.Item(0).Value.ToString();

                oItem = oForm.Items.Item("txtFechaS");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Value = dtFact.Columns.Item("FechaSUNAT").Cells.Item(0).Value.ToString();
                if (oEdit.Value.ToString().Equals("")) oEdit.Value = dtSelect.Columns.Item("FechaSUNAT").Cells.Item(0).Value.ToString();
                
                //limpia matrix
                mtSeleccionados.Clear();
                mtPendientes.Clear();

                //relaciona columna datable con matrix
                mtSeleccionados.Columns.Item("#").DataBind.Bind("dtaSelect", "FILA");
                mtSeleccionados.Columns.Item("fEntry").DataBind.Bind("dtaSelect", "DocEntry");
                mtSeleccionados.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
                mtSeleccionados.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
                mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
                mtSeleccionados.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
                mtSeleccionados.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "TotalPagar");
                mtSeleccionados.Columns.Item("lblBank").DataBind.Bind("dtaSelect", "BankCode");
                mtSeleccionados.Columns.Item("lblAcct").DataBind.Bind("dtaSelect", "Cuenta");
                mtSeleccionados.Columns.Item("lSelect").DataBind.Bind("dtaSelect", "ParaPago");
                mtSeleccionados.Columns.Item("lRet").DataBind.Bind("dtaSelect", "Retenido");
                mtSeleccionados.Columns.Item("FE").DataBind.Bind("dtaSelect", "FlujoEfectivo");

                mtSeleccionados.Columns.Item("fDoc").DataBind.Bind("dtaSelect", "Documento");

                //carga datos
                mtSeleccionados.LoadFromDataSource();
                //acomoda columnas
                mtSeleccionados.AutoResizeColumns();

                SAPbouiCOM.Column _column1 = mtSeleccionados.Columns.Item("fDetrac");
                SAPbouiCOM.Column _column2 = mtSeleccionados.Columns.Item("fTC");
                _column1.Visible = false;
                _column2.Visible = false;


                //relaciona columna datable con matrix
                mtPendientes.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
                mtPendientes.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
                mtPendientes.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
                mtPendientes.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
                mtPendientes.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
                mtPendientes.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
                mtPendientes.Columns.Item("fTotal").DataBind.Bind("dtaFact", "TotalPagar");
                mtPendientes.Columns.Item("lblBank").DataBind.Bind("dtaFact", "BankCode");
                mtPendientes.Columns.Item("lblAcct").DataBind.Bind("dtaFact", "Cuenta");
                mtPendientes.Columns.Item("lSelect").DataBind.Bind("dtaFact", "ParaPago");
                mtPendientes.Columns.Item("lRet").DataBind.Bind("dtaFact", "Retenido");
                mtPendientes.Columns.Item("FE").DataBind.Bind("dtaFact", "FlujoEfectivo");

                mtPendientes.Columns.Item("fDoc").DataBind.Bind("dtaFact", "Documento");

                //carga datos
                mtPendientes.LoadFromDataSource();
                //acomoda columnas
                mtPendientes.AutoResizeColumns();

                SAPbouiCOM.Column _column3 = mtPendientes.Columns.Item("fDetrac");
                SAPbouiCOM.Column _column4 = mtPendientes.Columns.Item("fTC");
                _column3.Visible = false;
                _column4.Visible = false;


                oForm.PaneLevel = 2;
          

                oForm.Visible = true;

                generarTXT3Retenedor(); //genera txt 

                oItem = oForm.Items.Item("tabSelec");
                oFolder = (SAPbouiCOM.Folder)oItem.Specific;
                oFolder.Select();

                actualizarMatrixAnexos(FormUID); //actualiza matrix anexos
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrFrmProveedores.cs > iniciarFormulario() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {

            }
        }


        //hilo de validacion y generacion de txt
        public void Generatxt(object FormID,object serie,object escenario,object moneda,object banco)
        {
            //verifica los doc que van a pasar por 3ro retenedor
            if (!this.Verificar(FormID.ToString()))
            {
                return;
            }


            if (serie.Equals("PRO"))
            {
                //genera txt
                generarArchivo(FormID);

            }
            else
            {
                //no se utiliza
                this.generarArchivoSUNAT(FormID.ToString(), escenario.ToString());
            }

            //regresca grilla
            this.RefrescarGrilla(FormID.ToString(), escenario.ToString(), banco.ToString(), moneda.ToString());

        }




        public void GeneraPago(object FormID, object serie, object escenario, object moneda, object banco)
        {
            //verifica los doc que van a pasar por 3ro retenedor
            if (!this.Verificar(FormID.ToString()))
            {
                return;
            }



            if (serie.Equals("PRO"))
                //genera pago
                generarPagosSAP(FormID.ToString(), escenario.ToString());
            else
                generarPagosDET(FormID.ToString(), escenario.ToString()); //no se utiliza

            //regresca grilla
            this.RefrescarGrilla(FormID.ToString(), escenario.ToString(), banco.ToString(), moneda.ToString());

        }



        public void registrarItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            SAPbouiCOM.Form oForm = null;

            try
            {
                switch (pVal.BeforeAction)
                {
                    case false:
                        switch (pVal.EventType)
                        {
                            case SAPbouiCOM.BoEventTypes.et_FORM_RESIZE:
                                    oForm = sboApplication.Forms.Item(FormUID);
                                    SAPbouiCOM.Item oItemMtxAnex = oForm.Items.Item("mtxAnex");
                                    oItemMtxAnex.Height = 104;
                                break;
                        case SAPbouiCOM.BoEventTypes.et_CLICK:
                                switch (pVal.ItemUID)
                                {
                                    case "btn_rsp":

                                        //cargar los anexos en pagos (+)
                                        Thread hilo = new Thread(new ParameterizedThreadStart(cargarArchivoRespuesta));
                                        hilo.SetApartmentState(ApartmentState.STA);
                                        hilo.Start(FormUID);
                                        break;
                                    case "btn_rspd":
                                        //quitar anexos en pagos (-)
                                        quitarAnexo(FormUID); 
                                        break;
                                    case "btnAgregar":

                                        this.agregarDoc(FormUID); //no se utiliza
                                        break;
                                    case "btnTxt":

                                        //genera el txt
                                        oForm = sboApplication.Forms.Item(FormUID);

                                        SAPbouiCOM.Item oItem = oForm.Items.Item("txtEsc");
                                        SAPbouiCOM.EditText oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        string[] datos = oEdit.Value.ToString().Split('-');


                                        oItem = oForm.Items.Item("txtMoneda");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        string moneda = null;
                                        moneda = oEdit.Value.ToString();


                                        oItem = oForm.Items.Item("codBanco");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        string banco = null;
                                        banco = oEdit.Value.ToString();


                                        //crea hilo de generacion del txt
                                        Thread th = new Thread(() => Generatxt(FormUID, datos[0], datos[1], moneda, banco));
                                        th.Start();

                                        break;


                                    case "btnPagos":

   
                                        //boton creacion de pagos
                                        oForm = sboApplication.Forms.Item(FormUID);
                                        oItem = oForm.Items.Item("txtEsc");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        datos = null;
                                        datos = oEdit.Value.ToString().Split('-');

                                        oItem = oForm.Items.Item("txtMoneda");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        string moneda1 = null;
                                        moneda1 = oEdit.Value.ToString();

                                        oItem = oForm.Items.Item("codBanco");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        string banco1 = null;
                                        banco1 = oEdit.Value.ToString();


                                        //inica hilo para generacion de pagos
                                        Thread th1 = new Thread(() => GeneraPago(FormUID, datos[0], datos[1], moneda1, banco1));
                                        th1.Start();




                                        break;
                                    case "btn_3Ret":
                                        openArchivo3Retenedor(FormUID); //abre el txt de tercero retenedor
                                        break;
                                    case "bver":



                                        break;
                                    case "mtxFact":
                                        switch (pVal.ColUID)
                                        {
                                            case "lRet":

                                                break;
                                        }
                                        break;
                                    case "mtxSelect":
                                        switch (pVal.ColUID)
                                        {
                                            case "lRet":

                                                break;
                                        }
                                        break;
                                }
                                break;
                            case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED:
                                switch (pVal.ItemUID)
                                {
                                    case "tabSelec":
                                        oForm = null;
                                        oForm = sboApplication.Forms.Item(FormUID);
                                        oForm.PaneLevel = 2;
                                        break;
                                    case "tabDoc":
                                        oForm = sboApplication.Forms.Item(FormUID);
                                        oForm.PaneLevel = 1;
                                        break;
                                }
                                break;


                            case SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED:
                                switch (pVal.ItemUID)
                                {
                                    case "mtxSelect": //valida documentos por tipo para abrir su formulario
                                        SAPbouiCOM.EditText campoDocEntry = null;

                                        int fila = pVal.Row;

                                        oForm = sboApplication.Forms.Item(pVal.FormUID);
                                        oItem = oForm.Items.Item("mtxSelect");
                                        mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

                                        campoDocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fDoc").Cells.Item(fila).Specific;

                                        if (campoDocEntry.Value.ToString() == "NC-C")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_InvoiceCreditMemo, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry.Value.ToString() == "FT-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_PurchaseInvoice, "", DocEntry.Value.ToString());

                                        }
                                        else if (campoDocEntry.Value.ToString() == "FA-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry.Value.ToString() == "SA-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry.Value.ToString() == "AS")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry.Value.ToString() == "SP")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)140, "", DocEntry.Value.ToString());
                                        }else if(campoDocEntry.Value.ToString() == "PR")
                                        {

                                            var DocEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                        }

                                        break;
                                    case "mtxFact": //valida documentos por tipo para abrir su formulario

                                        SAPbouiCOM.EditText campoDocEntry1 = null;

                                        int fila1 = pVal.Row;
                                        oForm = sboApplication.Forms.Item(pVal.FormUID);
                                        oItem = oForm.Items.Item("mtxFact");
                                        mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                                        campoDocEntry1 = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fDoc").Cells.Item(fila1).Specific;

                                        if (campoDocEntry1.Value.ToString() == "NC-C")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_InvoiceCreditMemo, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry1.Value.ToString() == "FT-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_PurchaseInvoice, "", DocEntry.Value.ToString());

                                        }
                                        else if (campoDocEntry1.Value.ToString() == "FA-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry1.Value.ToString() == "SA-P")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry1.Value.ToString() == "AS")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                        }
                                        else if (campoDocEntry1.Value.ToString() == "SP")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)140, "", DocEntry.Value.ToString());
                                        }else if (campoDocEntry1.Value.ToString() == "PR")
                                        {
                                            var DocEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                            sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                     
                                        }
                                        break;

                                }
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrFrmProveedores.cs > registrarItemEvent(() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        //carga anexo
        private void cargarArchivoRespuesta(object frm)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEditText = null;
            string nombreArchivo = "";
            string mensaje="";
            pagoDAO _pagoDAO = null;
            _pagoDAO = new pagoDAO();
            daoEscenario _daoEscenario = new daoEscenario();

            oForm = sboApplication.Forms.Item(frm.ToString());
            oItem = oForm.Items.Item("txtEsc");
            oEditText = (SAPbouiCOM.EditText)oItem.Specific;
            string[] datos1 = oEditText.Value.ToString().Split('-');

            try
            {
                OpenFileDialog choofdlog = new OpenFileDialog();
                choofdlog.Filter = "All Files (*.*)|*.*";
                choofdlog.FilterIndex = 1;
                choofdlog.Multiselect = false;

                if (choofdlog.ShowDialog() == DialogResult.OK)
                {
                    string sFileRuta = choofdlog.FileName;
                    string sFileName = choofdlog.SafeFileName;

                    string[] ext = sFileName.Split('.');
                    nombreArchivo = "ESC" + datos1[1] + "_" + ext[0] + "." + ext[1];
               
                    File.Copy(sFileRuta, @"D:\Pagos_Masivos\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, true);

                  
                    _daoEscenario.registrarEscenarioAnexo(sboApplication,datos1[1], @"D:\Pagos_Masivos\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, "E", ref mensaje);
                    actualizarMatrixAnexos(frm.ToString());

                    sboApplication.MessageBox("Se ha subido el anexo correctamente.");
                }
            }
            catch(Exception ex)
            {
                string err = ex.ToString();
            }
        }

        //generar txt tercero retenedor
        private void generarTXT3Retenedor()
        {
            List<TXT3Retenedor> olista = null;
            pagoDAO _pagoDAO = null;
            string nombreArchivo = "";
            string lineaDetalle = "";
            StreamWriter archivo = null;
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEditText = null;

            try
            {
                _pagoDAO = new pagoDAO();
                olista = _pagoDAO.getDetalleTXT3Retenedor(sboApplication,codEscenario);

                //Generando el archivo
                //nombreArchivo = @"\\WIN-SOPORTESAP\DocSap\TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = @"D:\Pagos_Masivos\TerceroRetenedor\ArchivoSunat\";
                nombreArchivo = nombreArchivo + "RCP20512857869_ESC"+ codEscenario + ".txt";
                archivo = new System.IO.StreamWriter(nombreArchivo, false, Encoding.GetEncoding(1252));

                for (int i = 0; i < olista.Count; i++)
                {
                    lineaDetalle = "";
                    lineaDetalle = olista[i].RUC + "|" + olista[i].monto.ToString();
                    archivo.WriteLine(lineaDetalle);
                }

                archivo.Close();

                Process.Start(nombreArchivo);

                oForm = sboApplication.Forms.ActiveForm;
                oItem = oForm.Items.Item("txt_3Ret");
                oEditText = (SAPbouiCOM.EditText)oItem.Specific;

                oEditText.Value = nombreArchivo;
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }
        }

        //abre archivo de tercero retenedor
        private void openArchivo3Retenedor(string FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEditText = null;

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("txt_3Ret");
                oEditText = (SAPbouiCOM.EditText)oItem.Specific;

                Process.Start(oEditText.Value);
            }
            catch(Exception ex)
            {
                string err = ex.ToString();
            }
        }

        //no se utiliza
        private void agregarDoc(string FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.DataTable dtPendientes = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.Button btnAgre = null;
            SAPbouiCOM.CheckBox ocheckBox = null;
            SAPbouiCOM.Matrix mtPendientes = null;
            SAPbouiCOM.Matrix mtSeleccionados = null;
            SAPbouiCOM.EditText oEdit = null;
            List<string> lstDocEntry;
            daoEscenario daoEscenario = null;
            string mensaje = "";
            string[] datos = null;

            try
            {
                lstDocEntry = new List<string>();
                daoEscenario = new daoEscenario();
                oForm = sboApplication.Forms.Item(FormUID);
                dtPendientes = oForm.DataSources.DataTables.Item("dtaFact");
                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                oItem = oForm.Items.Item("btnAgregar");
                btnAgre = (SAPbouiCOM.Button)oItem.Specific;
                oItem = oForm.Items.Item("mtxFact");
                mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("txtEsc");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                datos = oEdit.Value.ToString().Split('-');

                btnAgre.Item.Enabled = false;
                btnAgre.Caption = "CARGANDO...";
                oForm.Freeze(true);

                int rows = dtPendientes.Rows.Count;

                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtPendientes.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        dtSelect.Rows.Add();
                        dtSelect.SetValue("FILA", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("FILA").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("DocEntry", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CardCode", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("CardCode").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CardName", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("CardName").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("NumAtCard", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("DocCur", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("DocCur").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("Total", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("Total").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("Cuenta", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("Cuenta").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CuentaMoneda", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("CuentaMoneda").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("BankCode", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("BankCode").Cells.Item(i).Value.ToString());
                        daoEscenario.marcarDetalle(sboApplication,dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString(),"Y", datos[1], ref mensaje);
                    }
                }

                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtPendientes.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        lstDocEntry.Add(dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
                    }
                }

                for (int i = 0; i < dtPendientes.Rows.Count; i++)
                {
                    for (int j = 0; j < lstDocEntry.Count; j++)
                    {
                        if (dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString().Equals(lstDocEntry[j]))
                        {
                            dtPendientes.Rows.Remove(i);
                            i = 0;
                        }
                    }
                }

                mtSeleccionados.Clear();
                mtPendientes.Clear();

                mtSeleccionados.Columns.Item("#").DataBind.Bind("dtaSelect", "FILA");
                mtSeleccionados.Columns.Item("fEntry").DataBind.Bind("dtaSelect", "DocEntry");
                mtSeleccionados.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
                mtSeleccionados.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
                mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
                mtSeleccionados.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
                mtSeleccionados.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "Total");
                mtSeleccionados.Columns.Item("lblBank").DataBind.Bind("dtaSelect", "BankCode");
                mtSeleccionados.Columns.Item("lblAcct").DataBind.Bind("dtaSelect", "Cuenta");
                mtSeleccionados.LoadFromDataSource();
                mtSeleccionados.AutoResizeColumns();

                mtPendientes.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
                mtPendientes.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
                mtPendientes.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
                mtPendientes.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
                mtPendientes.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
                mtPendientes.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
                mtPendientes.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
                mtPendientes.Columns.Item("lblBank").DataBind.Bind("dtaFact", "BankCode");
                mtPendientes.Columns.Item("lblAcct").DataBind.Bind("dtaFact", "Cuenta");
                mtPendientes.LoadFromDataSource();
                mtPendientes.AutoResizeColumns();

                oForm.Freeze(false);
                btnAgre.Item.Enabled = true;
                btnAgre.Caption = "Agregar";

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }

        //genera txt de banco
        private void generarArchivo(object FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.Button oButton = null;
            SAPbouiCOM.EditText oEdit= null;
            SAPbouiCOM.EditText oEdit4 = null;
            SAPbouiCOM.EditText txtFechaSunat = null;
            SAPbouiCOM.CheckBox oEdit1 = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.DataTable dtPendientes = null;
            StreamWriter archivo = null;
            List<dtoArchDetalle> lstDetalle = null;
            dtoArchDetalle detalle = null;
            daoEscenario _daoEscenario = null;
            daoBCP _daoBCP = null;
            daoBANBIF _daoBANBIF = null;
            string cuenta = "";
            string moneda = "";
            string nombre = "";
            double total = 0;
            string totalDoc = "";
            string totalRegDoc = "";
            string[] lstCuenta; 
            string lineacab = "";
            string lineadet = "";
            string ruc;
            string importeDet;
            string rz;
            string correlativo;
            string err = "";
            string[] escenario;
            pagoDAO pagoDAO = new pagoDAO();
            _daoBCP = new daoBCP();
            _daoBANBIF = new daoBANBIF();
            string mensajeErr = "";
            string ArchivoTXT = "";
            string ArchivoRSP = "";

            try
            {
                oForm = sboApplication.Forms.Item(FormUID.ToString());
                _daoEscenario = new daoEscenario();

                oItem = oForm.Items.Item("txt_3Ret");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                ArchivoTXT = oEdit.Value.ToString();

                oItem = oForm.Items.Item("txtEsc");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                string[] datos1 = oEdit.Value.ToString().Split('-');
                lstPagos = pagoDAO.GetListaPagoSAP(sboApplication,datos1[1]);
                lstDetalle = new List<dtoArchDetalle>();

                escenario = null;
                SAPbouiCOM.Item oItem1 = oForm.Items.Item("txtEsc");
                SAPbouiCOM.EditText oEdit2 = (SAPbouiCOM.EditText)oItem1.Specific;
                escenario = oEdit2.Value.ToString().Split('-');
                oItem1 = oForm.Items.Item("txtRC");
                oEdit4 = (SAPbouiCOM.EditText)oItem1.Specific;

                oItem = oForm.Items.Item("btnTxt");
                oButton = (SAPbouiCOM.Button)oItem.Specific;
                oButton.Item.Enabled = false;
                oButton.Caption = "CARGANDO...";
                oForm.Freeze(true);

                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                dtPendientes = oForm.DataSources.DataTables.Item("dtaFact");
                oItem = oForm.Items.Item("txtBanco");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                cuenta = oEdit.Value.ToString();
                oItem = oForm.Items.Item("txtMoneda");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                moneda = oEdit.Value.ToString();
                oItem = oForm.Items.Item("txtFechaS");
                txtFechaSunat = (SAPbouiCOM.EditText)oItem.Specific;

                oItem = oForm.Items.Item("mtxFact");
                mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

                string mensaje = "";

                ConexionDAO conexion = new ConexionDAO();
                string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


                //Interbancarias
                if (mtPendientes.RowCount > 0)
                    for (int i = 0; i < dtPendientes.Rows.Count; i++)
                    {
                        oEdit1 = (SAPbouiCOM.CheckBox)mtPendientes.Columns.Item("lRet").Cells.Item(i + 1).Specific;
                        if (oEdit1.Checked)
                        {

                            _daoEscenario.actualizarRCEmbargo(NombreBaseDatos, escenario[1], oEdit4.Value, ref err, ArchivoTXT, txtFechaSunat.Value.ToString());
                            _daoEscenario.actualizarDetalleRetenido(NombreBaseDatos, escenario[1], dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString(), dtPendientes.Columns.Item("Documento").Cells.Item(i).Value.ToString(), "Y", "N", ref err);

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Actualizo Fila Retenida" + i + " de: " + dtPendientes.Rows.Count
                  , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                            continue;
                        }
                        else
                        {
                            _daoEscenario.actualizarRCEmbargo(NombreBaseDatos, escenario[1], oEdit4.Value, ref err, ArchivoTXT, txtFechaSunat.Value.ToString());
                            _daoEscenario.actualizarDetalleRetenido(NombreBaseDatos, escenario[1], dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString(), dtPendientes.Columns.Item("Documento").Cells.Item(i).Value.ToString(), "N", "Y", ref err);

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Actualizo Fila " + i + " de: " + dtPendientes.Rows.Count
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        }


                        /*
                        detalle = new dtoArchDetalle();
                        detalle.TipoRegistro = "002";
                        detalle.DocTipo = "R";
                        ruc = dtPendientes.Columns.Item("CardCode").Cells.Item(i).Value.ToString();
                        detalle.DocNum = (ruc.Substring(1, ruc.Length - 1) + "                ").Substring(0, 12);

                        if (dtPendientes.Columns.Item("BankCode").Cells.Item(i).Value.ToString().Equals("011"))
                            detalle.TipoAbono = "P";
                        else
                            detalle.TipoAbono = "I";

                        if (dtPendientes.Columns.Item("BankCode").Cells.Item(i).Value.ToString().Equals("011"))
                        {
                            lstCuenta = dtPendientes.Columns.Item("Cuenta").Cells.Item(i).Value.ToString().Split('-');
                            if (lstCuenta.Length == 3)
                                detalle.Numuenta = lstCuenta[0] + lstCuenta[1] + lstCuenta[2];
                            else
                                detalle.Numuenta = "00000000000000000000";
                        }
                        else
                            detalle.Numuenta = dtPendientes.Columns.Item("Cuenta").Cells.Item(i).Value.ToString().Replace("-", "");


                        rz = Regex.Replace(dtPendientes.Columns.Item("CardName").Cells.Item(i).Value.ToString(), @"[^\w\s!@$%^&*()\-\/]+", "");
                        if (rz.Length > 40)
                            rz = rz.Substring(0, 40);
                        detalle.NombreBeneficiario = rz.PadRight(40, ' ');

                        importeDet = Convert.ToDecimal(dtPendientes.Columns.Item("TotalPagar").Cells.Item(i).Value.ToString()).ToString("F");
                        importeDet = "00000000000000000000000" + importeDet;
                        importeDet = importeDet.Replace(".", "");
                        detalle.Importe = importeDet.Substring(importeDet.Length - 15, 15);

                        detalle.TipoCompro = "F";

                        correlativo = dtPendientes.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString();
                        detalle.NumDocumento = correlativo.PadRight(12, ' ');
                        detalle.AbonoAgru = "N";
                        detalle.Referencia = "PROVEEDOR                               ";
                        detalle.Aviso = " ";
                        detalle.Medio = "                                                  ";
                        detalle.Contacto = "                              ";
                        detalle.Proceso = "00";
                        detalle.Descripcion = "000000000000000000000000000000";
                        detalle.Filler = "                  ";
                        total += Convert.ToDouble(dtPendientes.Columns.Item("TotalPagar").Cells.Item(i).Value.ToString());
                        lstDetalle.Add(detalle);
                        */

                    }

                //Bancarias
                if (mtSeleccionados.RowCount > 0)
                    for (int i = 0; i < dtSelect.Rows.Count; i++)
                    {
                        oEdit1 = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lRet").Cells.Item(i + 1).Specific;
                        if (oEdit1.Checked)
                        {
                            _daoEscenario.actualizarRCEmbargo(NombreBaseDatos, escenario[1], oEdit4.Value, ref err, ArchivoTXT, txtFechaSunat.Value.ToString());
                            _daoEscenario.actualizarDetalleRetenido(NombreBaseDatos, escenario[1], dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString(), dtSelect.Columns.Item("Documento").Cells.Item(i).Value.ToString(), "Y", "N", ref err);

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Actualizo Fila Retenida" + i + " de: " + dtSelect.Rows.Count
                  , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                            continue;
                        }
                        else
                        {
                            _daoEscenario.actualizarRCEmbargo(NombreBaseDatos, escenario[1], oEdit4.Value, ref err, ArchivoTXT, txtFechaSunat.Value.ToString());
                            _daoEscenario.actualizarDetalleRetenido(NombreBaseDatos, escenario[1], dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString(), dtSelect.Columns.Item("Documento").Cells.Item(i).Value.ToString(), "N", "Y", ref err);

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Actualizo Fila " + i + " de: " + dtPendientes.Rows.Count
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        }

                        /*
                        detalle = new dtoArchDetalle();
                        detalle.TipoRegistro = "002";
                        detalle.DocTipo = "R";
                        ruc = dtSelect.Columns.Item("CardCode").Cells.Item(i).Value.ToString();
                        detalle.DocNum = (ruc.Substring(1, ruc.Length - 1) + "                ").Substring(0, 12);

                        if (dtSelect.Columns.Item("BankCode").Cells.Item(i).Value.ToString().Equals("011"))
                            detalle.TipoAbono = "P";
                        else
                            detalle.TipoAbono = "I";

                        if (dtSelect.Columns.Item("BankCode").Cells.Item(i).Value.ToString().Equals("011"))
                        {
                            

                            detalle.Numuenta = dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString().Substring(0, 8) +
                                                "00" +
                                                dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString()
                                                    .Substring(8, dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString().Length - 8);
                        }
                        else
                            detalle.Numuenta = dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString().Replace("-", "");


                        rz = Regex.Replace(dtSelect.Columns.Item("CardName").Cells.Item(i).Value.ToString(), @"[^\w\s!@$%^&*()\-\/]+", "");
                        if (rz.Length > 40)
                            rz = rz.Substring(0, 40);
                        detalle.NombreBeneficiario = rz.PadRight(40, ' ');

                        importeDet = Convert.ToDecimal(dtSelect.Columns.Item("TotalPagar").Cells.Item(i).Value.ToString()).ToString("F");
                        importeDet = "00000000000000000000000" + importeDet;
                        importeDet = importeDet.Replace(".", "");
                        detalle.Importe = importeDet.Substring(importeDet.Length - 15, 15);

                        detalle.TipoCompro = "F";

                        correlativo = dtSelect.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString();
                        detalle.NumDocumento = correlativo.PadRight(12, ' ');
                        detalle.AbonoAgru = "N";
                        detalle.Referencia = "PROVEEDOR                               ";
                        detalle.Aviso = " ";
                        detalle.Medio = "                                                  ";
                        detalle.Contacto = "                              ";
                        detalle.Proceso = "00";
                        detalle.Descripcion = "000000000000000000000000000000";
                        detalle.Filler = "                  ";
                        total += Convert.ToDouble(dtSelect.Columns.Item("TotalPagar").Cells.Item(i).Value.ToString());
                        lstDetalle.Add(detalle);
                        */
                    }


                lstPagos = pagoDAO.GetListaPagoSAP(sboApplication,datos1[1]);

                //string mensaje = "";

                //ConexionDAO conexion = new ConexionDAO();
                //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

                if (lstPagos.Count > 0)
                {
                    nombre = @"C:\PagosMasivos\";
                    nombre = nombre + "ArchivoBanco-" + lstPagos[0].BankCode + "-" + moneda + "-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";
                    archivo = new System.IO.StreamWriter(nombre, false, Encoding.GetEncoding(1252));

                


                switch (lstPagos[0].BankCode)
                {
                    case "002":

                        dtoBancoBCP _dtoBancoBCP = new dtoBancoBCP();
                        List<dtoBancoBCPDetalle> _listDetalleBCP = new List<dtoBancoBCPDetalle>();
                        List<dtoBancoBCPDetalle> _listDetalleBCPxProveedor = null;
                        List<dtoBancoBCPDetalle> _listDetalleBCPDocumentos = null;

                        string LineaCabeceraBCP = "";
                        string LineaDetalleBCP = "";


                        _dtoBancoBCP = _daoBCP.getBCP(escenario[1], NombreBaseDatos, ref mensajeErr);
                        _listDetalleBCP = _daoBCP.getBCPDetalle(escenario[1], NombreBaseDatos, ref mensajeErr);

                            LineaCabeceraBCP = "#1P"+ _dtoBancoBCP.TipoCuenta+ _dtoBancoBCP.CuentaCargo+ 
                                _dtoBancoBCP.Moneda +
                                (_dtoBancoBCP.Montototal).Replace(".","")+
                                _dtoBancoBCP.FechaProceso+
                                "                    "+ _dtoBancoBCP.Cadena+ (_listDetalleBCP.Count()).ToString("D6")+
                                "                0";
                      

                        archivo.WriteLine(LineaCabeceraBCP);

                         
                            for (int i = 0; i < _listDetalleBCP.Count; i++)
                            {
                            //_listDetalleBCPDocumentos = _listDetalleBCP.Where(x => x.NumeroDocumentoIdentidad.Equals(_listDetalleBCPxProveedor[i].NumeroDocumentoIdentidad)).ToList();

                            LineaDetalleBCP = " "+_listDetalleBCP[i].TipoRegistro +
                                    _listDetalleBCP[i].TipoCuenta +
                                    _listDetalleBCP[i].CuentaAbono +
                                    
                                    _listDetalleBCP[i].NombreProveedor +

                                    _listDetalleBCP[i].Moneda +
                                    (_listDetalleBCP[i].Importetotal).Replace(".","") +
                                    _listDetalleBCP[i].TipoDocumentoIdentidad +
                                    _listDetalleBCP[i].NumeroDocumentoIdentidad+
                                    _listDetalleBCP[i].TipoDocumentoPagar+
                                    _listDetalleBCP[i].NumeroDocumento+
                                    "00"+ _listDetalleBCP[i].ValidacionIDC;

                            archivo.WriteLine(LineaDetalleBCP);



                                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Read Fila " + i + " de: " + _listDetalleBCP.Count
                   , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                               
                            }
                        /*Fin: Detalles*/

                        archivo.Close();

                        oForm.Freeze(false);
                        oButton.Item.Enabled = true;
                        oButton.Caption = "Generar TXT";

                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                        SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        Process.Start(nombre);

                        return;
                    case "003":

                            dtoBancoBCP _dtoBanco = new dtoBancoBCP();
                            List<dtoBancoBCPDetalle> _listDetalle = new List<dtoBancoBCPDetalle>();

                            string LineaCabecera = "";
                            string LineaDetalle = "";

                           
                            _listDetalle = _daoBCP.getINTERBANKDetalle(escenario[1], NombreBaseDatos, ref mensajeErr);

                            
                            /*Detalles*/

                            for (int i = 0; i < _listDetalle.Count; i++)
                            {
                                string tipo = "";
                                if (_listDetalle[i].TipoCuenta == "I") //cuenta interbancara
                                {
                                    tipo = "109";
                                    LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2+
                                                _listDetalle[i].TipoDocumentoPagar +
                                                _listDetalle[i].NumeroDocumento +
                                                "          " + _listDetalle[i].FechaVencimiento +
                                                _listDetalle[i].Moneda +
                                                (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                                " " + "99"+
                                                "   " + //espaco de tipo de cuenta
                                                "  " + //espacio de moneda cuando es interbancario
                                                "   " + //espacio de ofinia a la que pertenece
                                                _listDetalle[i].CuentaAbono +
                                                _listDetalle[i].TipoPersona + _listDetalle[i].TipoDocumentoIdentidad +
                                                _listDetalle[i].NumeroDocumentoIdentidad +
                                                _listDetalle[i].NombreProveedor+"000000000000000";
                                        ;
                                }
                                else  //cuenta normla
                                {
                                    tipo = "009";
                                    LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2+
                                                _listDetalle[i].TipoDocumentoPagar +
                                                _listDetalle[i].NumeroDocumento +
                                                "          " + _listDetalle[i].FechaVencimiento +
                                                _listDetalle[i].Moneda +
                                                (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                                " " + "09" +
                                                "001" +
                                                _listDetalle[i].Moneda + "200"+
                                                _listDetalle[i].CuentaAbono+
                                                _listDetalle[i].TipoPersona+ _listDetalle[i].TipoDocumentoIdentidad + _listDetalle[i].NumeroDocumentoIdentidad+
                                                _listDetalle[i].NombreProveedor + "000000000000000";
                                }

                                archivo.WriteLine(LineaDetalle);

             
                                }


                            archivo.Close();

                        oForm.Freeze(false);
                        oButton.Item.Enabled = true;
                        oButton.Caption = "Generar TXT";

                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                        SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        Process.Start(nombre);

                        return;

                 case "009":



                            dtoBancoBCP _dtoBancoSC = new dtoBancoBCP();
                            List<dtoBancoBCPDetalle> _listDetalleSC = new List<dtoBancoBCPDetalle>();

                            string LineaCabeceraSC = "";
                            string LineaDetalleSC = "";

                            //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                            _listDetalleSC = _daoBCP.getSCOTIABANKDetalle(escenario[1], NombreBaseDatos, ref mensajeErr);


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
                                               + _listDetalleSC[i].CuentaAbonoCCI + _listDetalleSC[i].Moneda +"01";
                                    
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
                                               _listDetalleSC[i].CuentaAbono+ "                                                                       " + _listDetalleSC[i].Moneda+"01";
                                }
                                    
                               

                                archivo.WriteLine(LineaDetalleSC);


                            }


                            archivo.Close();

                            oForm.Freeze(false);
                            oButton.Item.Enabled = true;
                            oButton.Caption = "Generar TXT";

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                            Process.Start(nombre);




                            return;
                 case "022":


                            dtoBancoBCP _dtoBancoSA = new dtoBancoBCP();
                            List<dtoBancoBCPDetalle> _listDetalleSA = new List<dtoBancoBCPDetalle>();

                            string LineaCabeceraSA = "";
                            string LineaDetalleSA = "";

                            //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                            _listDetalleSA = _daoBCP.getSANTANDERDetalle(escenario[1], NombreBaseDatos, ref mensajeErr);


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
                                           "                    " + FormaPago + "          " + _listDetalleSA[i].CuentaAbonoCCI+
                                           _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor+
                                            "CONFIRMING";

                                }
                                else{ //normal

                                    FormaPago = "01";

                                    LineaDetalleSA = _listDetalleSA[i].TipoDocumentoIdentidad + _listDetalleSA[i].NumeroDocumentoIdentidad +
                                           _listDetalleSA[i].TipoDocumentoPagar +
                                           _listDetalleSA[i].NumeroDocumento +
                                           _listDetalleSA[i].Moneda +
                                            _listDetalleSA[i].ImporteParcial +
                                           _listDetalleSA[i].FechaVencimiento + "Confirming" +
                                           "                    " + FormaPago + _listDetalleSA[i].CuentaAbono +
                                            _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor+
                                            "CONFIRMING";

                                }

                               

                                      


                                archivo.WriteLine(LineaDetalleSA);


                            }


                            archivo.Close();

                            oForm.Freeze(false);
                            oButton.Item.Enabled = true;
                            oButton.Caption = "Generar TXT";

                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                            Process.Start(nombre);


                            return;

                }


               
                //Formatenado Campos
                cuenta = cuenta.Replace("-", "");
                if (moneda.Equals("SOL"))
                    moneda = "PEN";
                //Formatenado Campos

                totalDoc = "000000000000000000" + total.ToString("F");
                totalDoc = totalDoc.Replace(".", "");
                totalRegDoc = "000000" + (mtPendientes.RowCount + mtSeleccionados.RowCount).ToString();
                lineacab = "750" + cuenta + moneda;
                lineacab = lineacab + totalDoc.Substring(totalDoc.Length - 15, 15) + "A";
                lineacab = lineacab + lstPagos[0].FechaPago.ToString("yyyyMMdd");
                lineacab = lineacab + " PROVEEDORES              ";
                lineacab = lineacab + totalRegDoc.Substring(totalRegDoc.Length - 6, 6) + "S" + "000000000000000000                                                  ";
                archivo.WriteLine(lineacab);

                for (int i = 0; i < lstDetalle.Count; i++)
                {
                    lineadet = "";
                    lineadet =
                    /*lineacab = lineacab + */lstDetalle[i].TipoRegistro + lstDetalle[i].DocTipo + lstDetalle[i].DocNum + lstDetalle[i].TipoAbono
                         + lstDetalle[i].Numuenta + lstDetalle[i].NombreBeneficiario + lstDetalle[i].Importe + lstDetalle[i].TipoCompro
                          + lstDetalle[i].NumDocumento + lstDetalle[i].AbonoAgru + lstDetalle[i].Referencia + lstDetalle[i].Aviso
                           + lstDetalle[i].Medio + lstDetalle[i].Contacto + lstDetalle[i].Proceso + lstDetalle[i].Descripcion + lstDetalle[i].Filler;
                    archivo.WriteLine(lineadet);
                }
                //archivo.WriteLine(lineacab);
                archivo.Close();

                oForm.Freeze(false);

                

                oButton.Item.Enabled = true;
                oButton.Caption = "Generar TXT";

                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Archivo generado corrrectamente. ",
                SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                Process.Start(nombre);
                }
                else
                {
                    oForm.Freeze(false);
                    oButton.Item.Enabled = true;
                    oButton.Caption = "Generar TXT";

                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Todos los documentos estan retenidos. ",
                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + ex.Message.ToString(),
                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {

            }
        }

        //genera pago
        private void generarPagosSAP(string FormUID,string tipoEsc)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.Matrix oMatrixSelect= null;
            SAPbouiCOM.Matrix oMatrixFact = null;
            SAPbobsCOM.Payments payments1 = null;
           
            
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.EditText oEdit1 = null;
            SAPbouiCOM.CheckBox oCheck = null;
            SAPbouiCOM.ComboBox oCombo = null;
            bool validacionComentario = false;
            bool validacionFlujoEfectivo = false;
            string comentario = "";
            string moneda = "";
            string numOperI = "";
            string numOperM = "";
            string proveedor = ""; 
            double total = 0;
            int correlativoRetencion = 0;
            List<string> lstProve = new List<string>();
            daoFacturaProveedor daoFac = new daoFacturaProveedor();
            pagoDAO pagoDAO = new pagoDAO();
            List<PagoDTO> ListAuxProveedor = null;
            List<PagoDTO> ListAuxFlujoEfectivo = null;
            List<PagoDTO> ListAuxPago = null;
            List<PagoDTO> ListAuxPagoSP = null;
            List<PagoDatosDTO> ListPagoDatos = new List<PagoDatosDTO>();
            List<PagoDatosDTO> ListPagoDatosAux = null;
            PagoDatosDTO objPagoDatos = null;


            try
            {
                oForm = sboApplication.Forms.Item(FormUID);

                oItem = oForm.Items.Item("cmb_1_1");
                oCombo = (SAPbouiCOM.ComboBox)oItem.Specific;

                oItem = oForm.Items.Item("mtxSelect");
                oMatrixSelect = (SAPbouiCOM.Matrix)oItem.Specific;

                oItem = oForm.Items.Item("mtxFact");
                oMatrixFact = (SAPbouiCOM.Matrix)oItem.Specific;

                oItem = oForm.Items.Item("txtMoneda");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                moneda = oEdit.Value.ToString();

                oItem = oForm.Items.Item("txtOpe");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                numOperI = oEdit.Value.ToString();

                oItem = oForm.Items.Item("txtOpeM");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                numOperM = oEdit.Value.ToString();

                oItem = oForm.Items.Item("txtEsc");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                string[] datos1 = oEdit.Value.ToString().Split('-');

                //consulta la lista que se va a pagar
                lstPagos = pagoDAO.GetListaPagoSAP(sboApplication,datos1[1]);

                //agrupa por proveedor
                ListAuxProveedor = lstPagos.GroupBy(x => new { x.RucProveedor, x.FlujoEfectivoDescripcion }).
                                        Select(g => new PagoDTO { RucProveedor = g.Key.RucProveedor, FlujoEfectivoDescripcion = g.Key.FlujoEfectivoDescripcion}).ToList();


                //Validación Flujo de Efectivo
                for (int i = 0; i < lstPagos.Count; i++)
                {
                    if(lstPagos[i].FlujoEfectivo == 0)
                    {
                        validacionFlujoEfectivo = true;
                    }
                }

                if (validacionFlujoEfectivo)
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se han detectado comprobantes sin Flujo de Efectivo definido, favor corregir.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                //Validacion Comentario
                for (int i = 0; i < ListAuxProveedor.Count; i++)
                {
                    comentario = "";
                    for (int k = 1; k <= oMatrixFact.RowCount; k++)
                    {
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Agregando variables de flujo efectivo " + k,
                                           SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        oEdit = (SAPbouiCOM.EditText)oMatrixFact.Columns.Item("fCard").Cells.Item(k).Specific;
                        oEdit1 = (SAPbouiCOM.EditText)oMatrixFact.Columns.Item("FE").Cells.Item(k).Specific;
                        oCheck = (SAPbouiCOM.CheckBox)oMatrixFact.Columns.Item("lSelect").Cells.Item(k).Specific;
                        if (oEdit.Value.ToString().Equals(ListAuxProveedor[i].RucProveedor) && oCheck.Checked && oEdit1.Value.ToString().Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion))
                        {
                            oEdit = (SAPbouiCOM.EditText)oMatrixFact.Columns.Item("fComen").Cells.Item(k).Specific;
                            objPagoDatos = new PagoDatosDTO();
                            objPagoDatos.RUC = ListAuxProveedor[i].RucProveedor;
                            objPagoDatos.Comentario = oEdit.Value.ToString();
                            objPagoDatos.Operacion = numOperI;
                            objPagoDatos.FlujoEfectivoDescripcion = ListAuxProveedor[i].FlujoEfectivoDescripcion;
                            ListPagoDatos.Add(objPagoDatos);
                            if (comentario.Equals("")) comentario = oEdit.Value.ToString();
                            if (!comentario.Equals(oEdit.Value.ToString()) || oEdit.Value.ToString().Equals(""))
                            {
                                validacionComentario = true;
                                proveedor = ListAuxProveedor[i].RucProveedor;
                            }
                        }
                    }

                    comentario = "";
                    for (int l = 1; l <= oMatrixSelect.RowCount; l++)
                    {
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Agregando variables de flujo efectivo "+l,
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                        oEdit = (SAPbouiCOM.EditText)oMatrixSelect.Columns.Item("fCard").Cells.Item(l).Specific;
                        oEdit1 = (SAPbouiCOM.EditText)oMatrixSelect.Columns.Item("FE").Cells.Item(l).Specific;
                        oCheck = (SAPbouiCOM.CheckBox)oMatrixSelect.Columns.Item("lSelect").Cells.Item(l).Specific;
                        if (oEdit.Value.ToString().Equals(ListAuxProveedor[i].RucProveedor) && oCheck.Checked && oEdit1.Value.ToString().Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion))
                        {
                            oEdit = (SAPbouiCOM.EditText)oMatrixSelect.Columns.Item("fComen").Cells.Item(l).Specific;
                            objPagoDatos = new PagoDatosDTO();
                            objPagoDatos.RUC = ListAuxProveedor[i].RucProveedor;
                            objPagoDatos.Comentario = oEdit.Value.ToString();
                            objPagoDatos.Operacion = numOperM;
                            objPagoDatos.FlujoEfectivoDescripcion = ListAuxProveedor[i].FlujoEfectivoDescripcion;
                            ListPagoDatos.Add(objPagoDatos);
                            if (comentario.Equals("")) comentario = oEdit.Value.ToString();
                            if (!comentario.Equals(oEdit.Value.ToString()) || oEdit.Value.ToString().Equals(""))
                            {
                                validacionComentario = true;
                                proveedor = ListAuxProveedor[i].RucProveedor;
                            }
                        }
                    }
                }

                //if (validacionComentario)
                //{
                //    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se han detectado dos comprobantes con distintos Comentarios o sin Comentario"
                //                            + " para el proveedor " + proveedor + ", favor corregir.",
                //                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                //    return;
                //}




                correlativoRetencion = lstPagos[0].CorrelativoRetencion;

                
                //pago documentos preliminares
                for (int i = 0; i < ListAuxProveedor.Count; i++)
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Creando Pago Preliminar: " + ListAuxProveedor[i].RucProveedor,
                                   SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    ListAuxPagoSP = lstPagos.Where(z => z.RucProveedor.Equals(ListAuxProveedor[i].RucProveedor)
                                       && z.FlujoEfectivoDescripcion.Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion) && z.TipoDocumento == "SP").ToList();
                    

                    ListPagoDatosAux = ListPagoDatos.Where(z => z.RUC.Equals(ListAuxProveedor[i].RucProveedor)
                                  && z.FlujoEfectivoDescripcion.Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion)).ToList();


                    for (int j = 0; j < ListAuxPagoSP.Count; j++)
                    {
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "(Pago Preliminar) Agregando linea " + j,
                                  SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        if (ListAuxPagoSP[j].TipoDocumento == "SP")
                        {
                            var oDrf = (SAPbobsCOM.Payments)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                            oDrf.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;

                        
                                oDrf.Series = Convert.ToInt32(oCombo.Selected.Value.ToString());
                                oDrf.CardCode = ListAuxPagoSP[j].RucProveedor;
                                oDrf.CardName = ListAuxPagoSP[j].CardName;
                                oDrf.DocDate = ListAuxPagoSP[j].FechaPago;
                                oDrf.TaxDate = ListAuxPagoSP[j].FechaPago;
                                oDrf.DueDate = ListAuxPagoSP[j].FechaPago;

                                if (ListAuxPagoSP[j].NumAtCard == "C")
                                {
                                oDrf.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                                }
                                else
                                {
                                oDrf.DocType = SAPbobsCOM.BoRcptTypes.rSupplier;
                                }
                                

                                oDrf.Remarks = ListPagoDatosAux[0].Comentario;
                                oDrf.JournalRemarks = ListPagoDatosAux[0].Comentario;
                                oDrf.TransferReference = ListPagoDatosAux[0].Operacion;
                                oDrf.CounterReference = ListPagoDatosAux[0].Operacion;

                                oDrf.TransferDate = ListAuxPagoSP[j].FechaPago;


                                oDrf.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtBankTransfer;
                                oDrf.PrimaryFormItems.CashFlowLineItemID = ListAuxPagoSP[j].FlujoEfectivo;
                                
                               


                                oDrf.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = "003";


                                oDrf.TransferSum = ListAuxPagoSP[j].TotalPagar;

                                //asigna cuenta por moneda
                                if (moneda.Equals("SOL"))
                                    switch (lstPagos[0].BankCode)
                                    {
                                        case "011":
                                            oDrf.TransferAccount = "_SYS00000000163";
                                            break;
                                        case "002":
                                            oDrf.TransferAccount = "_SYS00000013882";
                                            break;
                                        case "038":
                                            oDrf.TransferAccount = "_SYS00000000165";
                                            break;
                                        case "003":
                                            oDrf.TransferAccount = "_SYS00000013883";
                                            break;
                                        case "009":
                                            oDrf.TransferAccount = "_SYS00000013885";
                                            break;
                                        case "022":
                                            oDrf.TransferAccount = "_SYS00000013887";
                                            break;
                                    }

                                else
                                    switch (lstPagos[0].BankCode)
                                    {
                                        case "011":
                                            oDrf.TransferAccount = "_SYS00000000164";
                                            break;
                                        case "002":
                                            oDrf.TransferAccount = "_SYS00000013890";
                                            break;
                                        case "038":
                                            oDrf.TransferAccount = "_SYS00000000166";
                                            break;
                                        case "003":
                                            oDrf.TransferAccount = "_SYS00000013892";
                                            break;
                                        case "009":
                                            oDrf.TransferAccount = "_SYS00000013894";
                                            break;
                                        case "022":
                                            oDrf.TransferAccount = "_SYS00000013895";
                                            break;
                                    }

                        

                                if (oDrf.Add() != 0)
                                {
                                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al generar los comprobantes SP => DocEntry:" + ListAuxPagoSP[j].DocEntry + "||" + sboCompany.GetLastErrorDescription(),
                                                SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return;
                                }
                                else
                                {
                                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se han generado los Pagos Correctamente.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                                
                                var oDraft1 = (SAPbobsCOM.Payments)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPaymentsDrafts);
                                oDraft1.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                                oDraft1.GetByKey(ListAuxPagoSP[j].DocEntry);
                                var resCmpTo = oDraft1.Remove();
                                if (resCmpTo != 0)
                                {
                                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al eliminar el pago preliminar => DocEntry:" + ListAuxPagoSP[j].DocEntry + "||" + sboCompany.GetLastErrorDescription(),
                                               SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                }

                            }




                            
                        }
                    }
                }



                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Creando objeto de pago...",
                                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                //crea pago de documentos
                for (int i = 0; i < ListAuxProveedor.Count; i++)
                {

                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Creando cabecera de pago " + ListAuxProveedor[i].RucProveedor ,
                                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                    ListAuxPago = lstPagos.Where(z => z.RucProveedor.Equals(ListAuxProveedor[i].RucProveedor)
                                    && z.FlujoEfectivoDescripcion.Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion) && z.TipoDocumento != "SP").ToList();
                    



                    if (ListAuxPago.Count > 0)
                    {

                    
                    ListPagoDatosAux = ListPagoDatos.Where(z => z.RUC.Equals(ListAuxProveedor[i].RucProveedor)
                                    && z.FlujoEfectivoDescripcion.Equals(ListAuxProveedor[i].FlujoEfectivoDescripcion)).ToList();

                    payments1 = (SAPbobsCOM.Payments)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);

                    //Cabecera
                    payments1.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                    payments1.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                    payments1.Series = Convert.ToInt32(oCombo.Selected.Value.ToString());
                    payments1.CardCode = ListAuxPago[0].RucProveedor;
                    payments1.CardName = ListAuxPago[0].CardName;
                    payments1.DocDate = ListAuxPago[0].FechaPago;
                    payments1.TaxDate = ListAuxPago[0].FechaPago;
                    payments1.DueDate = ListAuxPago[0].FechaPago;
                        
                        //asigna moneda a tipo de documento
                        switch (ListAuxPago[0].TipoDocumento)
                        {
                            case "AS":
                                payments1.DocCurrency = moneda;
                                break;
                            case "PR":
                                payments1.DocCurrency = moneda;
                                break;
                            case "FT-P":
                                payments1.DocCurrency = moneda;
                                break;
                            case "SA-P":
                                payments1.DocCurrency = moneda;
                                break;
                            case "FA-P":
                                payments1.DocCurrency = moneda;
                                break;
                            case "NC-C":
                                payments1.DocCurrency = moneda;
                                break;
                            case "SP":
                                payments1.DocCurrency = moneda;
                                break;

                        }




                    payments1.Remarks = (ListPagoDatosAux[0].Comentario==null)?"": ListPagoDatosAux[0].Comentario;
                    payments1.JournalRemarks = (ListPagoDatosAux[0].Comentario == null) ? "" : ListPagoDatosAux[0].Comentario;
                    payments1.TransferReference = ListPagoDatosAux[0].Operacion;
                    payments1.CounterReference = ListPagoDatosAux[0].Operacion;



                    payments1.TransferDate = ListAuxPago[0].FechaPago;
                    payments1.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtBankTransfer;
                    payments1.PrimaryFormItems.CashFlowLineItemID = ListAuxPago[0].FlujoEfectivo;
                    payments1.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = "003";
                    payments1.UserFields.Fields.Item("U_SMC_ESCPAG").Value = datos1[1];

                    //valida la serie de retencion
                    if (ListAuxPago[0].MontoRetencion > 0 && ListAuxPago[0].CodigoRetencion == "RIGV")
                    {
                        int serie = pagoDAO.SerieRetencion(sboApplication);
                        payments1.Series = serie;

                        payments1.UserFields.Fields.Item("U_EXX_SERIECER").Value = "R001";
                        payments1.UserFields.Fields.Item("U_EXX_CORRECER").Value = correlativoRetencion.ToString();
                        correlativoRetencion++;
                    }

                    //asigna numero de cuenta por moneda
                    if (moneda.Equals("SOL"))
                        switch (lstPagos[0].BankCode)
                        {
                            case "011":
                                payments1.TransferAccount = "_SYS00000000163";
                                break;
                            case "002":
                                payments1.TransferAccount = "_SYS00000013882";
                                break;
                            case "038":
                                payments1.TransferAccount = "_SYS00000000165";
                                break;
                            case "003":
                                payments1.TransferAccount = "_SYS00000013883";
                                break;
                            case "009":
                                payments1.TransferAccount = "_SYS00000013885";
                                break;
                            case "022":
                                payments1.TransferAccount = "_SYS00000013887";
                                break;
                        }

                    else
                        switch (lstPagos[0].BankCode)
                        {
                            case "011":
                                payments1.TransferAccount = "_SYS00000000164";
                                break;
                            case "002":
                                payments1.TransferAccount = "_SYS00000013890";
                                break;
                            case "038":
                                payments1.TransferAccount = "_SYS00000000166";
                                break;
                            case "003":
                                payments1.TransferAccount = "_SYS00000013892";
                                break;
                            case "009":
                                payments1.TransferAccount = "_SYS00000013894";
                                break;
                            case "022":
                                payments1.TransferAccount = "_SYS00000013895";
                                break;
                        }


                    total = 0.0;


                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Creacion de cabeceras de pago terminadas",
                                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                        //llena detalle
                    for (int j = 0; j < ListAuxPago.Count; j++)
                    {
                            
                            if (ListAuxPago[j].TipoDocumento == "NC-C") //carga nota de credito
                        {
                            payments1.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;

                            payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_CredItnote;
                            payments1.Invoices.DocEntry = ListAuxPago[j].DocEntry;
                            payments1.Invoices.SumApplied = ListAuxPago[j].TotalPagar;

                            payments1.Invoices.InstallmentId = ListAuxPago[j].InstallmentId;



                        }
                        else if (ListAuxPago[j].TipoDocumento == "FA-P")//carga facturas
                        {
                            payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseDownPayment;
                            payments1.Invoices.DocEntry = ListAuxPago[j].DocEntry;
                            payments1.Invoices.SumApplied = ListAuxPago[j].TotalPagar;

                            payments1.Invoices.InstallmentId = ListAuxPago[j].InstallmentId;
                        }
                        else if (ListAuxPago[j].TipoDocumento == "SA-P")//carga solicitud de anticipo
                        {
                            payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseDownPayment;
                            payments1.Invoices.DocEntry = ListAuxPago[j].DocEntry;
                            payments1.Invoices.SumApplied = ListAuxPago[j].TotalPagar;

                            payments1.Invoices.InstallmentId = ListAuxPago[j].InstallmentId;
                        }

                        else if (ListAuxPago[j].TipoDocumento == "AS" || ListAuxPago[j].TipoDocumento == "PR") //carga asiento y pagos recibidos
                        {



                           if (ListAuxPago[j].TipoDocumento == "PR")
                           {
                              payments1.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                           }
                            payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_JournalEntry;
                            
                            payments1.Invoices.DocEntry = ListAuxPago[j].DocEntry;
                            payments1.Invoices.SumApplied = ListAuxPago[j].TotalPagar;
                            

                            payments1.Invoices.DocLine = int.Parse(ListAuxPago[0].Fila);

                               
                               
                        }

                        else
                        {
                            payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseInvoice;
                            payments1.Invoices.DocEntry = ListAuxPago[j].DocEntry;
                            payments1.Invoices.SumApplied = ListAuxPago[j].TotalPagar;


                            payments1.Invoices.InstallmentId = ListAuxPago[j].InstallmentId;
                        }



                        payments1.Invoices.Add();


                        total += (ListAuxPago[j].TotalPagar);


                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se agrego la linea numero: " + j,
                                     SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                        }

                    payments1.TransferSum = total;

                      sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Creando pago en SAP...",
                                     SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    //crea pago
                     int a = payments1.Add();
                    int b = sboCompany.GetLastErrorCode();
                    string b1 = sboCompany.GetLastErrorDescription();
                    if (a != 0)
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al generar los comprobantes => " + b1,
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    else
                    {
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se han generado los Pagos Correctamente.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                        int errASiento = 0;
                        string desErrAsiento = "";

                        SAPbobsCOM.Payments payments2 = (SAPbobsCOM.Payments)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                        payments2.GetByKey(Convert.ToInt32(sboCompany.GetNewObjectKey()));

                        SAPbobsCOM.Recordset oREcord = null;
                        oREcord = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                        oREcord.DoQuery("SELECT \"TransId\" FROM OVPM WHERE \"DocEntry\" = '" + payments2.DocEntry + "'");

                        SAPbobsCOM.JournalEntries oAsiento = (SAPbobsCOM.JournalEntries)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
                        oAsiento.GetByKey(Convert.ToInt32(oREcord.Fields.Item(0).Value.ToString()));

                        //agrega referencia a asiento
                        oAsiento.Reference2 = ListPagoDatosAux[0].Operacion;
                        oAsiento.Reference3 = ListPagoDatosAux[0].Operacion;

                        for (int j = 0; j < oAsiento.Lines.Count; j++)
                        {
                            oAsiento.Lines.SetCurrentLine(j);
                            oAsiento.Lines.Reference2 = ListPagoDatosAux[0].Operacion;
                            oAsiento.Lines.AdditionalReference = ListPagoDatosAux[0].Operacion;
                        }

                        errASiento = oAsiento.Update();
                        desErrAsiento = sboCompany.GetLastErrorDescription();

                        if (a != 0)
                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al actualizar referencias del asiento diario => " + desErrAsiento,
                                                SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        else
                            sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "Pago creado -> se han actulizado las referencias correctamente.",
                                                SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    }
                }
                }
            }
            catch(Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al generar los comprobantes.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

            
        }

        //no se utiliza
        private void generarArchivoSUNAT(string FormUID, string codEsce)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.Button oButton = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.DataTable dtSelect = null;
            pagoDAO pagoDAO = new pagoDAO();
            string moneda;
            string lote;
            int sum = 0;

            oForm = sboApplication.Forms.Item(FormUID);
            oItem = oForm.Items.Item("btnTxt");
            oButton = (SAPbouiCOM.Button)oItem.Specific;
            oButton.Item.Enabled = false;
            oButton.Caption = "Cargando...";
            oForm.Freeze(true);

            dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
            oItem = oForm.Items.Item("txtMoneda");
            oEdit = (SAPbouiCOM.EditText)oItem.Specific;
            moneda = oEdit.Value.ToString();

            oItem = oForm.Items.Item("txtLote");
            oEdit = (SAPbouiCOM.EditText)oItem.Specific;
            lote = oEdit.Value.ToString();

            lstPagos = pagoDAO.GetListaPago(sboApplication,codEsce);

            String dlDir = @"C:\PagosMasivos\";
            string DIR = dlDir;
            String
            path = (DIR + "D20554039708" + lote + ".txt");
            File.Create(path).Dispose();
            //Directory.CreateDirectory(path);
            StreamWriter arch = new StreamWriter(path, true);
            string first_Line = "*20554039708CNTA SAC";
            first_Line = first_Line.PadRight(47, ' ');

            foreach (PagoDTO auxPagoEfectuad in lstPagos)
            {
                sum += auxPagoEfectuad.MontoSinDecimal;
            }
            int total = sum;
            string total_Formateado = total.ToString().PadLeft(13, '0');
            string numberFieldSerie = "1";
            string numberFieldCorrelativo = "1000";
            arch.WriteLine(first_Line + lote + total_Formateado.ToString() + "00");

            if (lstPagos.Count > 0)
                lstPagos = lstPagos.OrderBy(x => x.RucProveedor).ToList();

            foreach (PagoDTO auxPagoEfectuad in lstPagos)
            {
                //DTOUsuario usuario = new BOCUsuario().BOC_ObtenetDatosBancarios_xUsuario("E000" + auxPagoEfectuad.DNI);

                string importe_Formateado = decimal.Round(auxPagoEfectuad.MontoFactura, 2).ToString();
                importe_Formateado = importe_Formateado.Replace(".", "").PadLeft(11, '0');
                string factura_Formateado = auxPagoEfectuad.NumeroFactura;
                factura_Formateado = factura_Formateado.PadLeft(8, '0');
                string serie_Formateado = auxPagoEfectuad.SerieFactura;
                serie_Formateado = serie_Formateado.PadLeft(4, '0');
                string fecha_Month_Formateado = auxPagoEfectuad.FechaFactura.Month.ToString().PadLeft(2, '0');
                string fecha_Year_Formateado = auxPagoEfectuad.FechaFactura.Year.ToString();
                string montoSinDecimal_Formateado = auxPagoEfectuad.MontoSinDecimal.ToString().PadLeft(13, '0');
                string cuentaContable_Formateado = auxPagoEfectuad.CuentaContable.PadLeft(11, '0');
                string rucProveedor_Formateado = auxPagoEfectuad.RucProveedor.PadRight(46, ' ');

                string texto = "6" + rucProveedor_Formateado + auxPagoEfectuad.TipoServicio.PadLeft(12, '0')
                // "000000000025" 
                       + cuentaContable_Formateado + montoSinDecimal_Formateado + "00" + auxPagoEfectuad.TipoOperacion
                       + fecha_Year_Formateado + fecha_Month_Formateado +
                       auxPagoEfectuad.TipoDocumento + serie_Formateado + factura_Formateado;
                arch.WriteLine(texto);

            }

            arch.Close();

            oForm.Freeze(false);
            oButton.Item.Enabled = true;
            oButton.Caption = "Generar TXT";
        }
        //no se utiliza
        private void generarPagosDET(string FormUID, string codEsce)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbobsCOM.Payments payments1 = null;
            SAPbouiCOM.EditText oEdit = null;
            pagoDAO pagoDAO = new pagoDAO();
            List<dtoConstancia> lstDtoConstancia = null;
            string moneda = "";
            string numOper = "";
            List<dtoReconciliacion> lstReconciliacion = new List<dtoReconciliacion>();
            dtoReconciliacion recon = new dtoReconciliacion();
            string facturas = "";
            string remark = "";
            string fecha = "";
            int fila = 0;
            string constancia = "";
            string proveedor = "";
            bool flagProvee = false;
            List<string> lstProve = new List<string>();

            try
            {
                //return;
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("txtArch");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;

                lstDtoConstancia = getConstancia(ref fecha, oEdit.Value);
                lstPagos = pagoDAO.GetListaPago(sboApplication,codEsce);

                proveedor = lstPagos[0].RucProveedor;
                lstProve.Add(proveedor);
                for (int i = 0; i < lstPagos.Count; i++)
                {
                    flagProvee = false;
                    for (int j = 0; j < lstProve.Count; j++)
                    {
                        if (lstPagos[i].RucProveedor.Equals(lstProve[j]))
                        {
                            flagProvee = true;
                        }
                    }
                    if (!flagProvee)
                        lstProve.Add(lstPagos[i].RucProveedor);
                }

                moneda = lstPagos[0].MonedaFactura;
                
                oItem = oForm.Items.Item("txtOpe");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                numOper = oEdit.Value.ToString();

                //dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");

                for (int i = 0; i < lstPagos.Count; i++)
                {
                    facturas = "";
                    payments1 = (SAPbobsCOM.Payments)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                    payments1.Series = 241;

                    facturas += lstPagos[i].NumeroFactura.ToString() + " ";

                    payments1.Invoices.InstallmentId = 1;
                    payments1.Invoices.DocEntry = Convert.ToInt32(lstPagos[i].DocEntry);
                    payments1.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_PurchaseInvoice;
                    payments1.Invoices.SumApplied = Convert.ToDouble(lstPagos[i].MontoSinDecimal);
                    payments1.Invoices.AppliedFC = Convert.ToDouble(lstPagos[i].MontoSinDecimal);
                    payments1.PaymentPriority = SAPbobsCOM.BoPaymentPriorities.bopp_Priority_2;
                    payments1.Invoices.Add();

                    /**/
                    for(int z = 0; z < lstDtoConstancia.Count; z++)
                    {
                        if (lstDtoConstancia[z].rucProvedor.Equals(lstPagos[i].RucProveedor)
                            && lstDtoConstancia[z].numComprobante.Equals(lstPagos[i].SerieFactura + "-" + lstPagos[i].NumeroFactura.PadLeft(8,'0')))
                        {
                            constancia = lstDtoConstancia[z].numConstancia;
                        }
                    }

                    payments1.UserFields.Fields.Item("U_EXX_NRODEPDE").Value = constancia;
                    payments1.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = "003";
                    payments1.UserFields.Fields.Item("U_EXX_FEDEPDET").Value = fecha;

                    oItem = oForm.Items.Item("txtLote");
                    oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                    payments1.UserFields.Fields.Item("U_EXX_DETLOT").Value = oEdit.Value.ToString().Trim();
                    /**/

                    payments1.DocObjectCode = SAPbobsCOM.BoPaymentsObjectType.bopot_OutgoingPayments;
                    payments1.CardCode = "P" + lstPagos[i].RucProveedor;
                    payments1.CardName = lstPagos[i].CardName;
                    payments1.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                    payments1.DocDate = lstPagos[i].FechaPago;
                    payments1.TaxDate = lstPagos[i].FechaPago;
                    payments1.DueDate = lstPagos[i].FechaPago;
                    payments1.DocCurrency = "SOL";
                    remark = "DETRAC " + lstPagos[i].CardName.PadRight(10, ' ');
                    payments1.Remarks = remark.Substring(0, 17) + " " + facturas;
                    payments1.JournalRemarks = remark.Substring(0, 17) + facturas;
                    payments1.TransferReference = numOper;
                    payments1.CounterReference = numOper;
                    payments1.TransferDate = lstPagos[i].FechaPago;
                    payments1.PrimaryFormItems.PaymentMeans = SAPbobsCOM.PaymentMeansTypeEnum.pmtBankTransfer;
                    payments1.PrimaryFormItems.CashFlowLineItemID = 81;

                    payments1.TransferAccount = "_SYS00000001986";
                    payments1.TransferSum = Convert.ToDouble(lstPagos[i].MontoSinDecimal);

                    if(Convert.ToDouble(lstPagos[i].MontoRedondeo) > 0.5)
                    {
                        payments1.CashSum = Convert.ToDouble(lstPagos[i].MontoRedondeo);
                        payments1.CashAccount = "_SYS00000003963";
                    }

                    int a = payments1.Add();

                    if(a == 0)
                    {
                        string docEntry;
                        sboCompany.GetNewObjectCode(out docEntry);

                        if (Convert.ToDouble(lstPagos[i].MontoRedondeo) < -0.5)
                        {
                            //CREAR ASIENTO CONTABLE
                            recon = new dtoReconciliacion();

                            recon.cardcode = lstPagos[i].RucProveedor;
                            recon.numFactura = lstPagos[i].SerieFactura + "-" + lstPagos[i].NumeroFactura;
                            recon.pago = docEntry;

                            SAPbobsCOM.JournalEntries documents = null;
                            documents = (SAPbobsCOM.JournalEntries)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

                            documents.ReferenceDate = lstPagos[i].FechaPago;
                            documents.DueDate = lstPagos[i].FechaPago;
                            documents.TaxDate = lstPagos[i].FechaPago;
                            documents.Indicator = "";
                            documents.Reference = "";
                            documents.Reference2 = "";
                            documents.TransactionCode = "AJU";
                            documents.Memo = "AJT REDON. DETRAC REF. " + lstPagos[i].NumeroFactura + " " + lstPagos[i].CardName.Substring(0,10);

                            documents.Lines.AccountCode = "_SYS00000002896";
                            documents.Lines.Reference1 = "";
                            documents.Lines.Reference2 = "";
                            documents.Lines.LineMemo = "AJT REDON. DETRAC REF. " + lstPagos[i].NumeroFactura + " " + lstPagos[i].CardName.Substring(0, 10);
                            documents.Lines.ShortName = "P" + lstPagos[i].RucProveedor;
                            documents.Lines.ContraAccount = "_SYS00000003560";
                            documents.Lines.Credit = Convert.ToDouble(lstPagos[i].MontoRedondeo) * -1;
                            documents.Lines.Add();

                            documents.Lines.AccountCode = "_SYS00000003560";
                            documents.Lines.Reference1 = "";
                            documents.Lines.Reference2 = "";
                            documents.Lines.LineMemo = "AJT REDON. DETRAC REF. " + lstPagos[i].NumeroFactura + " " + lstPagos[i].CardName.Substring(0, 10);
                            documents.Lines.ShortName = "_SYS00000003560";
                            documents.Lines.ContraAccount = "";
                            documents.Lines.Debit = Convert.ToDouble(lstPagos[i].MontoRedondeo) * -1;
                            documents.Lines.Add();

                            if (documents.Add() == 0 )
                            {
                                docEntry = "";
                                sboCompany.GetNewObjectCode(out docEntry);

                                documents.GetByKey(Convert.ToInt32(docEntry));
                                recon.asiento = documents.BaseReference;
                                lstReconciliacion.Add(recon);
                            }

                        }
                    }

                    int b = sboCompany.GetLastErrorCode();
                    string b1 = sboCompany.GetLastErrorDescription();
                    if (a != 0)
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al generar los comprobantes => " + b1,
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    else
                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Se han generado los Pagos Correctamente.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                if (lstReconciliacion.Count > 0)
                {
                    SAPbouiCOM.Form oForm1 = null;
                    SAPbouiCOM.MenuItem uiMenuItem = null;
                    SAPbouiCOM.Matrix mtxRecon = null;
                    SAPbouiCOM.EditText editText = null;
                    SAPbouiCOM.CheckBox chkBox = null;
                    SAPbouiCOM.OptionBtn oOption = null;

                    uiMenuItem = sboApplication.Menus.Item("9459");
                    uiMenuItem.Activate();
                    oForm1 = sboApplication.Forms.GetForm("120060803", 1);

                    oItem = oForm1.Items.Item("120000005");
                    oOption = (SAPbouiCOM.OptionBtn)oItem.Specific;
                    oOption.Selected = true;

                    oItem = oForm1.Items.Item("10000084");
                    chkBox = (SAPbouiCOM.CheckBox)oItem.Specific;
                    chkBox.Checked = true;

                    oItem = oForm1.Items.Item("540000092");
                    chkBox = (SAPbouiCOM.CheckBox)oItem.Specific;
                    chkBox.Checked = true;

                    oItem = oForm1.Items.Item("540000095");
                    editText = (SAPbouiCOM.EditText)oItem.Specific;
                    editText.Value = lstPagos[0].FechaPago.ToString("yyyyMMdd");
                    oItem = oForm1.Items.Item("540000097");
                    editText = (SAPbouiCOM.EditText)oItem.Specific;
                    editText.Value = lstPagos[0].FechaPago.ToString("yyyyMMdd");

                    oItem = oForm1.Items.Item("10000085");
                    mtxRecon = (SAPbouiCOM.Matrix)oItem.Specific;
                    //mtxRecon.AddRow();

                    editText = (SAPbouiCOM.EditText)mtxRecon.Columns.Item("10000003").Cells.Item(1).Specific;
                    editText.Value = "P" + lstProve[0];

                    for (int i = 1; i < lstProve.Count; i++)
                    {
                        mtxRecon.AddRow();
                        editText = (SAPbouiCOM.EditText)mtxRecon.Columns.Item("10000003").Cells.Item(i + 1).Specific;
                        editText.Value = "P" + lstProve[i];
                    }

                    oItem = oForm1.Items.Item("120000001");
                    oItem.Click();

                    oForm1 = sboApplication.Forms.GetForm("120060805", 1);
                    oItem = oForm1.Items.Item("120000020");
                    editText = (SAPbouiCOM.EditText)oItem.Specific;
                    editText.Value = lstPagos[0].FechaPago.ToString("yyyyMMdd");

                    oItem = oForm1.Items.Item("120000039");
                    mtxRecon = (SAPbouiCOM.Matrix)oItem.Specific;

                    for (int i = 1; i <= mtxRecon.RowCount; i++)
                    {
                        editText = (SAPbouiCOM.EditText)mtxRecon.Columns.Item("10000045").Cells.Item(i).Specific;
                        for (int j = 0; j < lstReconciliacion.Count ; j++)
                        {
                            if(editText.Value.Equals(lstReconciliacion[j].pago) || editText.Value.Equals(lstReconciliacion[j].asiento))
                            {
                                chkBox = (SAPbouiCOM.CheckBox)mtxRecon.Columns.Item("120000002").Cells.Item(i).Specific;
                                chkBox.Checked = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error al generar los comprobantes.",
                                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }


        }
        //no se utiliza
        private List<dtoConstancia> getConstancia(ref string fecha, string arch)
        {
            List <dtoConstancia> lstDtoConstancia = null;
            dtoConstancia dtoConstancia = null;
            List<string> lines = null;
            string path = @"C:\PagosMasivos\";
            string nomArchivo = arch + ".txt";

            try
            {
                lstDtoConstancia = new List<dtoConstancia>();
                lines = System.IO.File.ReadAllLines(path + nomArchivo).ToList();
                for(int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("hora de pago"))
                    {
                        string a = lines[i];
                        a = a.Substring(32, 10);
                        fecha = a;
                    }

                    if (lines[i].Contains("Numero de constancia"))
                    {
                        dtoConstancia = new dtoConstancia();
                        string a = lines[i];
                        try
                        {
                            a = a.Substring(37, 11); //Con 9 caracteres
                        }
                        catch
                        {
                            a = a.Substring(37, 10); //Con 8 caracteres
                        }
                        a = a.Replace("\t", "");
                        dtoConstancia.numConstancia = a;
                    }

                    if (lines[i].Contains("ro Documento del Proveedor"))
                    {
                        string a = lines[i];
                        a = a.Substring(37, 11);
                        a = a.Replace("\t", "");
                        dtoConstancia.rucProvedor = a;
                    }

                    if (lines[i].Contains("mero de Comprobante"))
                    {
                        string a = lines[i];
                        a = a.Substring(39, 13);
                        a = a.Replace("\t", "");
                        a = a.Replace(" ", "-");
                        dtoConstancia.numComprobante = a;
                        lstDtoConstancia.Add(dtoConstancia);
                    }
                }
            }
            catch(Exception ex)
            {
                lstDtoConstancia = null;
            }

            return lstDtoConstancia;
        }

        //actualiza los anexos de la tabla de usuario
        private void actualizarMatrixAnexos(string FormUID)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.DataTable dtaAnexos;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Matrix mtxAnexos;
            SAPbouiCOM.EditText oEditText;

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("mtxAnex");
                mtxAnexos = (SAPbouiCOM.Matrix)oItem.Specific;
                dtaAnexos = oForm.DataSources.DataTables.Item("dtaAnex");
                oItem = oForm.Items.Item("txtEsc");
                oEditText = (SAPbouiCOM.EditText)oItem.Specific;
                string[] datos1 = oEditText.Value.ToString().Split('-');

                dtaAnexos.Rows.Clear();
                dtaAnexos.ExecuteQuery("CALL \"SMC_APM_GET_ESCENARIO_ANEXOS\" ('"+datos1[1]+"','E')");

                mtxAnexos.Clear();
                mtxAnexos.Columns.Item("#").DataBind.Bind("dtaAnex", "Id");
                mtxAnexos.Columns.Item("1").DataBind.Bind("dtaAnex", "Code");
                mtxAnexos.Columns.Item("2").DataBind.Bind("dtaAnex", "Ruta");
                mtxAnexos.LoadFromDataSource();
                mtxAnexos.AutoResizeColumns();
            }
            catch(Exception e)
            {
                string err = e.ToString();
            }
        }

        //quita los anexos de la tabla de usuario
        private void quitarAnexo(string FormUID)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Matrix mtxAnexos;
            SAPbouiCOM.EditText oEditText;
            daoEscenario _daoEscenario = new daoEscenario();
            string mensaje = "";

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("mtxAnex");
                mtxAnexos = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("txtEsc");
                oEditText = (SAPbouiCOM.EditText)oItem.Specific;
                string[] datos1 = oEditText.Value.ToString().Split('-');

                //valida filas
                if (mtxAnexos.GetNextSelectedRow() < 1)
                {
                    sboApplication.MessageBox("Debe selecionar un anexo de la grilla.");
                    return;
                }

                oEditText = (SAPbouiCOM.EditText)mtxAnexos.Columns.Item("1").Cells.Item(mtxAnexos.GetNextSelectedRow()).Specific;
                _daoEscenario.quitarEscenarioAnexo(sboApplication,datos1[1], oEditText.Value,ref mensaje); //quita anexos

                oEditText = (SAPbouiCOM.EditText)mtxAnexos.Columns.Item("2").Cells.Item(mtxAnexos.GetNextSelectedRow()).Specific;
                File.Delete(oEditText.Value);

                actualizarMatrixAnexos(FormUID); //actualiza matrix
                sboApplication.MessageBox("Se ha retirado el anexo correctamente.");
            }
            catch (Exception e)
            {
                string err = e.ToString();
            }
        }

        #endregion

        //verifica los documentos que van a pasar por 3ro retenedor
        public bool Verificar(string FormUID)
        {

            oForm = sboApplication.Forms.Item(FormUID);
            oItem = oForm.Items.Item("mtxSelect");
            mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;
            


            oForm = sboApplication.Forms.Item(FormUID);
            oItem = oForm.Items.Item("mtxFact");
            mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
            

            SAPbouiCOM.CheckBox ocheckBox0 = null;
            SAPbouiCOM.CheckBox ocheckBox1 = null;
            SAPbouiCOM.EditText _tipoDocumento = null;
            SAPbouiCOM.EditText _docEntry = null;

            SAPbouiCOM.Item oItembtnTXT;
            SAPbouiCOM.Button btnTXT;
            oItembtnTXT = oForm.Items.Item("btnTxt");
            btnTXT = (SAPbouiCOM.Button)oItembtnTXT.Specific;


            StreamWriter archivo = null;
            string nombre = "";

            var flag = 0;
            string error = "";


            btnTXT.Item.Enabled = false;
            btnTXT.Caption = "Cargando...";


            try
            {
                //recorre matrix (mismo banco)
                for (int i = 1; i <= mtSeleccionados.RowCount; i++)
                {
                    ocheckBox0 = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lRet").Cells.Item(i).Specific;
                    ocheckBox1 = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lSelect").Cells.Item(i).Specific;
                    _tipoDocumento = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fDoc").Cells.Item(i).Specific;
                    _docEntry = (SAPbouiCOM.EditText)mtSeleccionados.Columns.Item("fEntry").Cells.Item(i).Specific;


                    if (ocheckBox0.Checked)
                    {
                        //valida por tipo de documento los que pasaran a tercero retenedor
                        switch (_tipoDocumento.Value.ToString())
                        {
                            case "AS":

                                int resultado = 0;
                                SAPbobsCOM.Recordset oRecord = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecord.DoQuery("select count(*) from OJDT T0 where T0.\"TransType\" = 30 and T0.\"TransCode\" in ('LET', 'FAC') and T0.\"TransId\" = '" + _docEntry.Value.ToString() + "'");
                                resultado = Convert.ToInt32(oRecord.Fields.Item(0).Value.ToString());

                                if (resultado <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }


                                break;
                            case "PR":
                                flag++;
                                error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                break;
                            case "NC-C":

                                int resultadonc = 0;
                                SAPbobsCOM.Recordset oRecordnc = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordnc.DoQuery("select count(*) from ORIN where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' and \"Indicator\" in ('07')");
                                resultadonc = Convert.ToInt32(oRecordnc.Fields.Item(0).Value.ToString());

                                if (resultadonc <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }
                                //flag++;
                                //error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                
                                
                                break;
                            case "SP":
                                flag++;
                                error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                break;
                            case "FT-P": //facturas

                                int resultadoft = 0;
                                SAPbobsCOM.Recordset oRecordft = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordft.DoQuery("select  count(*) from OPCH where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordft.Fields.Item(0).Value.ToString());

                                if (resultadoft <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;
                            case "FA-P": //factura anticipo

                                int resultadofa = 0;
                                SAPbobsCOM.Recordset oRecordfa = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordfa.DoQuery("select count(*) from ODPO where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' AND \"CreateTran\" = 'Y' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordfa.Fields.Item(0).Value.ToString());

                                if (resultadofa <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;
                            case "SA-P": //solicitud anticipo

                                int resultadosa = 0;
                                SAPbobsCOM.Recordset oRecordsa = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordsa.DoQuery("select count(*) from ODPO where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' AND \"CreateTran\" = 'N' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordsa.Fields.Item(0).Value.ToString());

                                if (resultadosa <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;
                        }

                    }
                    else
                    {
                        ocheckBox0.Checked = false;
                        ocheckBox1.Checked = true;

                    }


                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "(Mismo Banco) Validando fila " + i 
                   , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                }

                //recorre matrix (interbancario)
                for (int i = 1; i <= mtPendientes.RowCount; i++)
                {
                    ocheckBox0 = (SAPbouiCOM.CheckBox)mtPendientes.Columns.Item("lRet").Cells.Item(i).Specific;
                    ocheckBox1 = (SAPbouiCOM.CheckBox)mtPendientes.Columns.Item("lSelect").Cells.Item(i).Specific;
                    _tipoDocumento = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fDoc").Cells.Item(i).Specific;
                    _docEntry = (SAPbouiCOM.EditText)mtPendientes.Columns.Item("fEntry").Cells.Item(i).Specific;


                    if (ocheckBox0.Checked)
                    {
                        //valida documentod que pasaran a 3ro retenedor
                        switch (_tipoDocumento.Value.ToString())
                        {
                            case "AS":

                                int resultado = 0;
                                SAPbobsCOM.Recordset oRecord = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecord.DoQuery("select count(*) from OJDT T0 where T0.\"TransType\" = 30 and T0.\"TransCode\" in ('LET', 'FAC') and T0.\"TransId\" = '" + _docEntry.Value.ToString() + "'");
                                resultado = Convert.ToInt32(oRecord.Fields.Item(0).Value.ToString());

                                if (resultado <= 0)
                                {
                                    flag++;
                                    error += "(INTERBANCARIO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }


                                break;
                            case "PR":
                                flag++;
                                error += "(INTERBANCARIO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                break;
                            case "NC-C":

                                int resultadonc = 0;
                                SAPbobsCOM.Recordset oRecordnc = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordnc.DoQuery("select count(*) from ORIN where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' and \"Indicator\" in ('07')");
                                resultadonc = Convert.ToInt32(oRecordnc.Fields.Item(0).Value.ToString());

                                if (resultadonc <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }
                                break;

                            case "SP":
                                flag++;
                                error += "(INTERBANCARIO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                break;

                            case "FT-P": //facturas

                                int resultadoft = 0;
                                SAPbobsCOM.Recordset oRecordft = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordft.DoQuery("select  count(*) from OPCH where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordft.Fields.Item(0).Value.ToString());

                                if (resultadoft <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;

                            case "FA-P": //factura anticipo

                                int resultadofa = 0;
                                SAPbobsCOM.Recordset oRecordfa = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordfa.DoQuery("select count(*) from ODPO where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' AND \"CreateTran\" = 'Y' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordfa.Fields.Item(0).Value.ToString());

                                if (resultadofa <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;

                            case "SA-P": //solicitud anticipo

                                int resultadosa = 0;
                                SAPbobsCOM.Recordset oRecordsa = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                oRecordsa.DoQuery("select count(*) from ODPO where \"DocEntry\" = '" + _docEntry.Value.ToString() + "' AND \"CreateTran\" = 'N' and \"Indicator\" in ('01','02','03','08','DT','DO')");
                                resultadoft = Convert.ToInt32(oRecordsa.Fields.Item(0).Value.ToString());

                                if (resultadosa <= 0)
                                {
                                    flag++;
                                    error += "(MISMO BANCO)--- Documento no valido fila: " + Convert.ToString(i) + Environment.NewLine;
                                }
                                else
                                {
                                    ocheckBox0.Checked = true;
                                    ocheckBox1.Checked = false;

                                }

                                break;

                        }

                    }
                    else
                    {
                        ocheckBox0.Checked = false;
                        ocheckBox1.Checked = true;

                    }


                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + "(Interbancario) Validando fila " + i
              , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                }



                btnTXT.Item.Enabled = true;
                btnTXT.Caption = "Generar TXT";

                //genera txt de errores
                if (flag > 0)
                {

                    nombre = @"C:\PagosMasivos\";
                    nombre = nombre + "Excepcion-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";
                    archivo = new System.IO.StreamWriter(nombre, false, Encoding.GetEncoding(1252));

                    archivo.WriteLine(error);
                    archivo.Close();

                    Process.Start(nombre); //lanza txt


                    return false;
                }
                else
                {
                    
                    return true;



                }

            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        //refresca la grilla
        public void RefrescarGrilla(string FormUID,string escenario,string banco,string moneda)
        {

            oForm = sboApplication.Forms.Item(FormUID);
            dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
            dtFact = oForm.DataSources.DataTables.Item("dtaFact");
            oItem = oForm.Items.Item("mtxFact");
            mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
            oItem = oForm.Items.Item("mtxSelect");
            mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

            dtFact.Rows.Clear();
            dtSelect.Rows.Clear();

            //consutla los proc
            string consultaMismoBanco = "CALL \"SMC_APM_LISTAR_PAGOS_MISMOBANCO\" ('" + escenario + "','" + banco + "','" + moneda + "') ";
            string consultaIterbancario = "CALL \"SMC_APM_LISTAR_PAGOS_INTERBANCARIO\" ('" + escenario + "','" + banco + "','" + moneda + "') ";

            //ejectura los proc
            dtFact.ExecuteQuery(consultaIterbancario);
            dtSelect.ExecuteQuery(consultaMismoBanco);

            //carga datos en las matrix
            mtSeleccionados.LoadFromDataSource();
            mtSeleccionados.AutoResizeColumns();

            mtPendientes.LoadFromDataSource();
            mtPendientes.AutoResizeColumns();

        }


    }
}
