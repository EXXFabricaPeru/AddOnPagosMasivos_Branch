using System;
using System.Collections.Generic;
using SMC_APM.SAP;
using SMC_APM.dto;
using SMC_APM.dao;

namespace SMC_APM.Controladores
{
    class ctrFrmAutorizacion
    {
        #region Atributos
        public SAPbouiCOM.Application sboApplication;
        public SAPbobsCOM.Company sboCompany;

        
        SAPbouiCOM.EditText txtFInicio = null;
        SAPbouiCOM.EditText txtFFin = null;
        SAPbouiCOM.Matrix grdLista;

        SAPbouiCOM.Form oForm = null;
        SAPbouiCOM.Button btnBuscar = null;
        SAPbouiCOM.Matrix mt = null;
        SAPbouiCOM.DataTable dt = null;
        #endregion

        #region Constructor
        public ctrFrmAutorizacion()
        {
        }

        public ctrFrmAutorizacion(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
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
                                    case "Buscar":
                                        //consulta datos para autorizar
                                        this.consultar("frmSMC7");
                                        break;
                                    case "Aceptar":
                                        //actualiza en caso sean autorizados
                                        this.Aceptar("frmSMC7");
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


        public void cargarFormulario(string frmUID)
        {
            sapObjetos sapObj = new sapObjetos();
            var folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FrmAutorizacion.srf");
            var xml = System.IO.File.ReadAllText(folder);
            sapObj.sapCargarFormulario(sboApplication, frmUID, xml, "");

            iniciarFormulario(frmUID);
        }

        #region Metodos Privados
        private void iniciarFormulario(string FormUID)
        {
           
            //inicia formulario con sus componentes
            try
            {

                oForm = sboApplication.Forms.Item(FormUID);

                txtFInicio = (SAPbouiCOM.EditText)oForm.Items.Item("FInicio").Specific;
                txtFFin = (SAPbouiCOM.EditText)oForm.Items.Item("FFin").Specific;
                btnBuscar = (SAPbouiCOM.Button)oForm.Items.Item("Buscar").Specific;
                grdLista = (SAPbouiCOM.Matrix)oForm.Items.Item("grdLista").Specific;
                dt = oForm.DataSources.DataTables.Item("DT_0");

                dt.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dt.Columns.Add("Creador", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dt.Columns.Add("Fecha", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
                dt.Columns.Add("Escenario", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

                grdLista.Columns.Item("Col_Accion").ValOn = "Y";
                grdLista.Columns.Item("Col_Accion").ValOff = "N";

                grdLista.Columns.Item("Accion2").ValOn = "Y";
                grdLista.Columns.Item("Accion2").ValOff = "N";

                grdLista.Columns.Item("Col_0").DataBind.Bind("DT_0", "FILA");
                grdLista.Columns.Item("Col_UserC").DataBind.Bind("DT_0", "Creador");
                grdLista.Columns.Item("Col_Fecha").DataBind.Bind("DT_0", "Fecha");
                grdLista.Columns.Item("Col_Esc").DataBind.Bind("DT_0", "Escenario");

                oForm.Freeze(true);
                oForm.Left = 350;
                oForm.Top = 10;

                grdLista.AutoResizeColumns();

                oForm.PaneLevel = 1;

                oForm.Visible = true;
                oForm.Freeze(false);
            }
            catch(Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrFrmAutorizacon.cs > iniciarFormulario() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }




        //obtiene los escenarios no autorizados 
        private void consultar(string FormUID)
        {

            string consulta = "";

            try
            {


                var FechaInicio = txtFInicio.Value.ToString();
                var FechaFin = txtFFin.Value.ToString();
                var UsuarioConectado = sboCompany.UserName.ToString();



                btnBuscar.Item.Enabled = false;
                btnBuscar.Caption = "Cargando...";
                oForm.Freeze(true);

                dt.Rows.Clear();

                consulta = "CALL \"SMC_APM_PROCESO_AUTORIZAR\" ('"+ UsuarioConectado + "','"+ FechaInicio + "','"+ FechaFin + "') ";



                dt.ExecuteQuery(consulta);


                grdLista.Clear();
                grdLista.LoadFromDataSource();
                grdLista.AutoResizeColumns();


                

                oForm.Freeze(false);
                btnBuscar.Item.Enabled = true;
                btnBuscar.Caption = "Buscar";



            }
            catch(Exception ex)
            {

            }
            finally
            {

            }
        }

        //guarda la autorizacion
        private void Aceptar(string FormUID)
        {
          
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.DataTable dtPendientes = null;
            SAPbouiCOM.DataTable dtSelect = null;
            SAPbouiCOM.Button btnAgre = null;
            SAPbouiCOM.CheckBox ocheckBox = null;
            SAPbouiCOM.CheckBox ocheckBox2 = null;
            SAPbouiCOM.Matrix mtPendientes = null;
            SAPbouiCOM.Matrix mtSeleccionados = null;
            List<string> lstDocEntry;

            try
            {

                dtPendientes = oForm.DataSources.DataTables.Item("DT_0");

                oItem = oForm.Items.Item("Aceptar");
                btnAgre = (SAPbouiCOM.Button)oItem.Specific;

                oItem = oForm.Items.Item("grdLista");
                mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

                btnAgre.Item.Enabled = false;
                btnAgre.Caption = "CARGANDO...";
                oForm.Freeze(true);

                int rows = dtPendientes.Rows.Count;
                string consulta = "";
                string mensaje = "";

                //valida filas
                if (rows == 0)
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Sin datos. ",
                      SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                ConexionDAO conexion = new ConexionDAO();
                string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

                //recorre la tabla verificando los checks
                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("Col_Accion").Cells.Item(j).Specific;
                    ocheckBox2 = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("Accion2").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        var Escenario = dtPendientes.Columns.Item("Escenario").Cells.Item(i).Value.ToString();
                        daoEscenario a = new daoEscenario();
                        a.actualizarAccionAutorizacion(Escenario,"Y", NombreBaseDatos, ref mensaje); //actualiza autorizador 1
                        
                    }

                    if (ocheckBox2.Checked == true)
                    {
                        var Escenario = dtPendientes.Columns.Item("Escenario").Cells.Item(i).Value.ToString();
                        daoEscenario a = new daoEscenario();
                        a.actualizarAccionAutorizacion2(Escenario, "Y", NombreBaseDatos, ref mensaje); //actualiza autorizador 2

                    }

                }

                if (mensaje == "")
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " : Proceso Realizado Correctamente. ",
                       SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    this.consultar("frmSMC7");
                }
               

                oForm.Freeze(false);
                btnAgre.Item.Enabled = true;
                btnAgre.Caption = "Aceptar";

            }
            catch(Exception ex)
            {

            }
            finally
            {

            }
        }

        //private void quitarDoc(string FormUID)
        //{
        //    SAPbouiCOM.Form oForm = null;
        //    SAPbouiCOM.Item oItem = null;
        //    SAPbouiCOM.DataTable dtPendientes = null;
        //    SAPbouiCOM.DataTable dtSelect = null;
        //    SAPbouiCOM.Button btnQuitar = null;
        //    SAPbouiCOM.CheckBox ocheckBox = null;
        //    SAPbouiCOM.Matrix mtPendientes = null;
        //    SAPbouiCOM.Matrix mtSeleccionados = null;
        //    List<string> lstDocEntry;

        //    try
        //    {
        //        lstDocEntry = new List<string>();
        //        oForm = sboApplication.Forms.Item(FormUID);
        //        dtPendientes = oForm.DataSources.DataTables.Item("dtaFact");
        //        dtSelect = oForm.DataSources.DataTables.Item("dtaSelect");
        //        oItem = oForm.Items.Item("btnQuitar");
        //        btnQuitar = (SAPbouiCOM.Button)oItem.Specific;
        //        oItem = oForm.Items.Item("mtxFact");
        //        mtPendientes = (SAPbouiCOM.Matrix)oItem.Specific;
        //        oItem = oForm.Items.Item("mtxSelect");
        //        mtSeleccionados = (SAPbouiCOM.Matrix)oItem.Specific;

        //        btnQuitar.Item.Enabled = false;
        //        btnQuitar.Caption = "CARGANDO...";
        //        oForm.Freeze(true);

        //        int rows = dtSelect.Rows.Count;

        //        for (int j = 1; j <= rows; j++)
        //        {
        //            int i = j - 1;
        //            ocheckBox = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lSelect").Cells.Item(j).Specific;
        //            if (ocheckBox.Checked == true)
        //            {
        //                dtPendientes.Rows.Add();
        //                dtPendientes.SetValue("FILA", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FILA").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("DocEntry", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("DocNum", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocNum").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("FechaContable", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FechaContable").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("FechaVencimiento", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("FechaVencimiento").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("CardCode", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CardCode").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("CardName", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CardName").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("NumAtCard", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("NumAtCard").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("DocCur", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("DocCur").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("Total", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("Total").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("Cuenta", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("Cuenta").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("CuentaMoneda", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("CuentaMoneda").Cells.Item(i).Value.ToString());
        //                dtPendientes.SetValue("BankCode", dtPendientes.Rows.Count - 1, dtSelect.Columns.Item("BankCode").Cells.Item(i).Value.ToString());
        //            }
        //        }
        //        for (int j = 1; j <= rows; j++)
        //        {
        //            int i = j - 1;
        //            ocheckBox = (SAPbouiCOM.CheckBox)mtSeleccionados.Columns.Item("lSelect").Cells.Item(j).Specific;
        //            if (ocheckBox.Checked == true)
        //            {
        //                lstDocEntry.Add(dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString());
        //            }
        //        }
        //        for (int i = 0; i < dtSelect.Rows.Count; i++)
        //        {
        //            for (int j = 0; j < lstDocEntry.Count; j++)
        //            {
        //                if (dtSelect.Columns.Item("DocEntry").Cells.Item(i).Value.ToString().Equals(lstDocEntry[j]))
        //                {
        //                    dtSelect.Rows.Remove(i);
        //                    i = 0;
        //                }
        //            }
        //        }
                
        //        mtSeleccionados.Clear();
        //        mtPendientes.Clear();

        //        mtSeleccionados.Columns.Item("#").DataBind.Bind("dtaSelect", "FILA");
        //        mtSeleccionados.Columns.Item("fEntry").DataBind.Bind("dtaSelect", "DocEntry");
        //        mtSeleccionados.Columns.Item("fNumSap").DataBind.Bind("dtaSelect", "DocNum");
        //        mtSeleccionados.Columns.Item("fFecCon").DataBind.Bind("dtaSelect", "FechaContable");
        //        mtSeleccionados.Columns.Item("fFecVen").DataBind.Bind("dtaSelect", "FechaVencimiento");
        //        mtSeleccionados.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
        //        mtSeleccionados.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
        //        mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
        //        mtSeleccionados.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
        //        mtSeleccionados.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "Total");
        //        mtSeleccionados.LoadFromDataSource();
        //        mtSeleccionados.AutoResizeColumns();

        //        mtPendientes.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
        //        mtPendientes.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
        //        mtPendientes.Columns.Item("fNumSap").DataBind.Bind("dtaFact", "DocNum");
        //        mtPendientes.Columns.Item("fFecCon").DataBind.Bind("dtaFact", "FechaContable");
        //        mtPendientes.Columns.Item("fFecVen").DataBind.Bind("dtaFact", "FechaVencimiento");
        //        mtPendientes.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
        //        mtPendientes.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
        //        mtSeleccionados.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
        //        mtPendientes.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
        //        mtPendientes.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
        //        mtPendientes.LoadFromDataSource();
        //        mtPendientes.AutoResizeColumns();

        //        oForm.Freeze(false);
        //        btnQuitar.Item.Enabled = true;
        //        btnQuitar.Caption = "Quitar";

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //}
        #endregion
    }
}
