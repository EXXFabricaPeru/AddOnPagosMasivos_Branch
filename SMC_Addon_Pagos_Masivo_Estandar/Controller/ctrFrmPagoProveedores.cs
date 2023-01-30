using System;
using System.Collections.Generic;
using SMC_APM.SAP;
using SMC_APM.dto;
using SMC_APM.dao;

namespace SMC_APM.Controladores
{
    class ctrFrmPagoProveedores
    {
        #region Atributos
        public SAPbouiCOM.Application sboApplication;
        public SAPbobsCOM.Company sboCompany;
        #endregion

        #region Constructor
        public ctrFrmPagoProveedores()
        {
        }

        public ctrFrmPagoProveedores(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
        }
        #endregion

        public void registrarItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.ComboBox oCombo = null;

            try
            {
                switch (pVal.BeforeAction)
                {
                    case false:
                        switch (pVal.EventType)
                        {
                            case SAPbouiCOM.BoEventTypes.et_CLICK:
                                switch (pVal.ItemUID)
                                {
                                    case "btnBuscar":
                                        this.consultarFact(Properties.Resources.nomFormulario1);
                                        break;
                                    case "btnAgregar":
                                        this.agregarDoc(Properties.Resources.nomFormulario1);
                                        break;
                                    case "btnQuitar":
                                        this.quitarDoc(Properties.Resources.nomFormulario1);
                                        break;
                                    case "btnNext":
                                        oForm = sboApplication.Forms.Item(FormUID);
                                        dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                                        oItem = oForm.Items.Item("txtMoneda");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;

                                        ctrFrmDatosCuenta ctrFrmDatos = new ctrFrmDatosCuenta(sboApplication, sboCompany);
                                        try
                                        {
                                            //sboApplication.Forms.Item("frmSMC2").Select();
                                            //ctrFrmDatos.iniciarFormulario("frmSMC2", dtSelect, oEdit.Value.ToString());
                                        }
                                        catch
                                        {
                                            //ctrFrmDatos.cargarFormulario(dtSelect, oEdit.Value.ToString());
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
                            case SAPbouiCOM.BoEventTypes.et_COMBO_SELECT:
                                switch (pVal.ItemUID)
                                {
                                    case "cmbBanco":
                                        oForm = null;
                                        oForm = sboApplication.Forms.Item(FormUID);
                                        oItem = oForm.Items.Item("cmbBanco");
                                        oCombo = (SAPbouiCOM.ComboBox)oItem.Specific;
                                        oItem = oForm.Items.Item("txtMoneda");
                                        oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                                        if (oCombo.Selected.Value.ToString().Contains("1041101"))
                                        {
                                            oEdit.Value = "SOL";
                                        }
                                        else
                                        {
                                            oEdit.Value = "USD";
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

        #region Metodos Privados
        private void iniciarFormulario(string FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.Item oNewItem = null;
            SAPbouiCOM.ComboBox cmbSap = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.Matrix mt = null;
            SAPbouiCOM.Folder oFolder = null;
            SAPbouiCOM.DataTable dtSelect = null;
            List<dtoBanco> lstBancos = null;
            daoBanco daoBanco = null;

            try
            {
                lstBancos = new List<dtoBanco>();
                daoBanco = new daoBanco();
                oForm = sboApplication.Forms.Item(FormUID);

                oItem = oForm.Items.Item("txtMoneda");
                oEdit = (SAPbouiCOM.EditText)oItem.Specific;
                oEdit.Item.Enabled = false;

                oItem = oForm.Items.Item("cmbBanco");
                cmbSap = (SAPbouiCOM.ComboBox)oItem.Specific;
                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");

                oForm.Freeze(true);
                oForm.Left = 350;
                oForm.Top = 10;

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

                oNewItem = oForm.Items.Add("tabSelec", SAPbouiCOM.BoFormItemTypes.it_FOLDER);
                oItem = oForm.Items.Item("tabDoc");
                oItem.Width = 120;
                oNewItem.Top = oItem.Top;
                oNewItem.Height = oItem.Height;
                oNewItem.Width = oItem.Width;
                oNewItem.Left = oItem.Width;

                oFolder = (SAPbouiCOM.Folder)oNewItem.Specific;
                oFolder.Caption = "Seleccionados";
                oFolder.GroupWith("tabDoc");

                dtSelect.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("DocEntry", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("DocNum", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("FechaContable", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("FechaVencimiento", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("CardCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("CardName", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("NumAtCard", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("DocCur", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("Cuenta", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("CuentaMoneda", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dtSelect.Columns.Add("BankCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

                oForm.PaneLevel = 1;

                oForm.Visible = true;
                oForm.Freeze(false);
            }
            catch(Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrFrmProveedores.cs > iniciarFormulario() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void consultarFact(string FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEdit0 = null;
            SAPbouiCOM.EditText oEdit1 = null;
            SAPbouiCOM.EditText oEdit2 = null;
            SAPbouiCOM.DataTable dt = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.Matrix mt = null;
            SAPbouiCOM.Matrix mtSelect = null;
            SAPbouiCOM.Button btnConsul = null;
            string consulta = "";

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);

                oItem = oForm.Items.Item("txtFecha");
                oEdit0 = (SAPbouiCOM.EditText)oItem.Specific;
                oItem = oForm.Items.Item("txtProve");
                oEdit1 = (SAPbouiCOM.EditText)oItem.Specific;
                oItem = oForm.Items.Item("txtMoneda");
                oEdit2 = (SAPbouiCOM.EditText)oItem.Specific;
                oItem = oForm.Items.Item("btnBuscar");
                btnConsul = (SAPbouiCOM.Button)oItem.Specific;
                dt = oForm.DataSources.DataTables.Item("dtaFact");
                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                oItem = oForm.Items.Item("mtxFact");
                mt = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSelect = (SAPbouiCOM.Matrix)oItem.Specific;

                btnConsul.Item.Enabled = false;
                btnConsul.Caption = "CARGANDO...";
                oForm.Freeze(true);
                dt.Clear();
                dtSelect.Clear();


                consulta = @"EXEC SMC_APM_LISTAR_FACPENDIENTES_PP '" + oEdit0.Value.ToString() + "','" + oEdit2.Value.ToString() + "','" + oEdit1.Value.ToString() + "' ";
                dt.ExecuteQuery(consulta);

                mt.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
                mt.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
                mt.Columns.Item("fNumSap").DataBind.Bind("dtaFact", "DocNum");
                mt.Columns.Item("fFecCon").DataBind.Bind("dtaFact", "FechaContable");
                mt.Columns.Item("fFecVen").DataBind.Bind("dtaFact", "FechaVencimiento");
                mt.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
                mt.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
                mt.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
                mt.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
                mt.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
                mt.LoadFromDataSource();
                mt.AutoResizeColumns();

                mt.Columns.Item("#").TitleObject.Sortable = false;
                mt.Columns.Item("#").Editable = false;

                oForm.Freeze(false);
                btnConsul.Item.Enabled = true;
                btnConsul.Caption = "BUSCAR";
            }
            catch(Exception ex)
            {

            }
            finally
            {

            }
        }

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
            List<string> lstDocEntry;

            try
            {
                lstDocEntry = new List<string>();
                oForm = sboApplication.Forms.Item(FormUID);
                dtPendientes = oForm.DataSources.DataTables.Item("dtaFact");
                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                oItem = oForm.Items.Item("btnAgregar");
                btnAgre = (SAPbouiCOM.Button)oItem.Specific;
                oItem = oForm.Items.Item("mtxFact");
                mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

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
                        dtSelect.SetValue("FILA",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("FILA").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("DocEntry",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("DocNum",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("DocNum").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("FechaContable",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("FechaContable").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("FechaVencimiento", dtSelect.Rows.Count-1, dtPendientes.Columns.Item("FechaVencimiento").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CardCode",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("CardCode").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CardName",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("CardName").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("NumAtCard", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("DocCur",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("DocCur").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("Total",dtSelect.Rows.Count-1, dtPendientes.Columns.Item("Total").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("Cuenta", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("Cuenta").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("CuentaMoneda", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("CuentaMoneda").Cells.Item(i).Value.ToString());
                        dtSelect.SetValue("BankCode", dtSelect.Rows.Count - 1, dtPendientes.Columns.Item("BankCode").Cells.Item(i).Value.ToString());
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
                    for(int j = 0; j < lstDocEntry.Count; j++)
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
                mtSeleccionados.Columns.Item("fNumSap").DataBind.Bind("dtaSelect", "DocNum");
                mtSeleccionados.Columns.Item("fFecCon").DataBind.Bind("dtaSelect", "FechaContable");
                mtSeleccionados.Columns.Item("fFecVen").DataBind.Bind("dtaSelect", "FechaVencimiento");
                mtSeleccionados.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
                mtSeleccionados.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
                mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
                mtSeleccionados.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
                mtSeleccionados.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "Total");
                mtSeleccionados.LoadFromDataSource();
                mtSeleccionados.AutoResizeColumns();

                mtPendientes.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
                mtPendientes.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
                mtPendientes.Columns.Item("fNumSap").DataBind.Bind("dtaFact", "DocNum");
                mtPendientes.Columns.Item("fFecCon").DataBind.Bind("dtaFact", "FechaContable");
                mtPendientes.Columns.Item("fFecVen").DataBind.Bind("dtaFact", "FechaVencimiento");
                mtPendientes.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
                mtPendientes.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
                mtPendientes.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
                mtPendientes.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
                mtPendientes.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
                mtPendientes.LoadFromDataSource();
                mtPendientes.AutoResizeColumns();

                oForm.Freeze(false);
                btnAgre.Item.Enabled = true;
                btnAgre.Caption = "Agregar";

            }
            catch(Exception ex)
            {

            }
            finally
            {

            }
        }

        private void quitarDoc(string FormUID)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.DataTable dtPendientes = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.Button btnQuitar = null;
            SAPbouiCOM.CheckBox ocheckBox = null;
            SAPbouiCOM.Matrix mtPendientes = null;
            SAPbouiCOM.Matrix mtSeleccionados = null;
            List<string> lstDocEntry;

            try
            {
                lstDocEntry = new List<string>();
                oForm = sboApplication.Forms.Item(FormUID);
                dtPendientes = oForm.DataSources.DataTables.Item("dtaFact");
                dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
                oItem = oForm.Items.Item("btnQuitar");
                btnQuitar = (SAPbouiCOM.Button)oItem.Specific;
                oItem = oForm.Items.Item("mtxFact");
                mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
                oItem = oForm.Items.Item("mtxSelect");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

                btnQuitar.Item.Enabled = false;
                btnQuitar.Caption = "CARGANDO...";

                daoBanco Banco = new daoBanco();

                //var ValidaAutorizacion = daoBanco.ObtenerAccion(codEscenario, sboApplication, ref mensaje);

                //if ((ValidaAutorizacion[0] == "N" && ValidaAutorizacion[1] == "N")
                //    || (ValidaAutorizacion[0] == "Y" && ValidaAutorizacion[1] == "N")
                //    || (ValidaAutorizacion[0] == "N" && ValidaAutorizacion[1] == "Y"))
                //{
                //    sboApplication.MessageBox("Requiere Autorizacion");
                //    return;
                //}



                oForm.Freeze(true);

                int rows = dtSelect.Rows.Count;

                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        dtPendientes.Rows.Add();
                        dtPendientes.SetValue("FILA", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FILA").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("DocEntry", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("DocNum", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocNum").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("FechaContable", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FechaContable").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("FechaVencimiento", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FechaVencimiento").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("CardCode", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CardCode").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("CardName", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CardName").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("NumAtCard", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("DocCur", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocCur").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("Total", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("Total").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("Cuenta", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("CuentaMoneda", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CuentaMoneda").Cells.Item(i).Value.ToString());
                        dtPendientes.SetValue("BankCode", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("BankCode").Cells.Item(i).Value.ToString());
                    }
                }
                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        lstDocEntry.Add(dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
                    }
                }
                for (int i = 0; i < dtSelect.Rows.Count; i++)
                {
                    for (int j = 0; j < lstDocEntry.Count; j++)
                    {
                        if (dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString().Equals(lstDocEntry[j]))
                        {
                            dtSelect.Rows.Remove(i);
                            i = 0;
                        }
                    }
                }
                
                mtSeleccionados.Clear();
                mtPendientes.Clear();

                mtSeleccionados.Columns.Item("#").DataBind.Bind("dtaSelect", "FILA");
                mtSeleccionados.Columns.Item("fEntry").DataBind.Bind("dtaSelect", "DocEntry");
                mtSeleccionados.Columns.Item("fNumSap").DataBind.Bind("dtaSelect", "DocNum");
                mtSeleccionados.Columns.Item("fFecCon").DataBind.Bind("dtaSelect", "FechaContable");
                mtSeleccionados.Columns.Item("fFecVen").DataBind.Bind("dtaSelect", "FechaVencimiento");
                mtSeleccionados.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
                mtSeleccionados.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
                mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
                mtSeleccionados.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
                mtSeleccionados.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "Total");
                mtSeleccionados.LoadFromDataSource();
                mtSeleccionados.AutoResizeColumns();

                mtPendientes.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
                mtPendientes.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
                mtPendientes.Columns.Item("fNumSap").DataBind.Bind("dtaFact", "DocNum");
                mtPendientes.Columns.Item("fFecCon").DataBind.Bind("dtaFact", "FechaContable");
                mtPendientes.Columns.Item("fFecVen").DataBind.Bind("dtaFact", "FechaVencimiento");
                mtPendientes.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
                mtPendientes.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
                mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
                mtPendientes.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
                mtPendientes.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
                mtPendientes.LoadFromDataSource();
                mtPendientes.AutoResizeColumns();

                oForm.Freeze(false);
                btnQuitar.Item.Enabled = true;
                btnQuitar.Caption = "Quitar";

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }
        #endregion
    }
}
