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

namespace SMC_APM.Controller
{
    public class ctrFrmLiberarTerceroRet
    {
        #region Atributos
        pagoDAO _pagoDAO = null;
        daoEscenario _daoEscenario = null;
        string SQLerr = "";
        private SAPbouiCOM.Application sboApplication;
        private SAPbobsCOM.Company sboCompany;
        private SAPbouiCOM.Form oForm = null;

        private SAPbouiCOM.DataTable dta1;

        private SAPbouiCOM.Item oItemTxt1;
        private SAPbouiCOM.Item oItemTxt2;
        private SAPbouiCOM.Item oItemTxt3;
        private SAPbouiCOM.Item oItemMtx1;
        private SAPbouiCOM.Item oItembtn1;

        private SAPbouiCOM.Button btnCargar;

        private SAPbouiCOM.EditText txt1;
        private SAPbouiCOM.EditText txt2;
        private SAPbouiCOM.EditText txt3;

        private SAPbouiCOM.Matrix mtx1;
        #endregion

        #region Constructor
        public ctrFrmLiberarTerceroRet()
        {
        }

        public ctrFrmLiberarTerceroRet(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
            _pagoDAO = new pagoDAO();
            _daoEscenario = new daoEscenario();
        }
        #endregion

        public void cargarFormulario(string frmUID)
        {
            sapObjetos sapObj = new sapObjetos();
            sapObj.sapCargarFormulario(sboApplication, frmUID, Properties.Resources.frmSMC_PM_LiberarTerceroRet, "");
            iniciarFormulario(frmUID);
        }

        public void iniciarFormulario(string FormUID)
        {
            try
            {
                oForm = sboApplication.Forms.Item(FormUID);

                oItemTxt1 = oForm.Items.Item("txt_1_1");
                txt1 = (SAPbouiCOM.EditText)oItemTxt1.Specific;
                oItemTxt2 = oForm.Items.Item("txt_1_2");
                txt2 = (SAPbouiCOM.EditText)oItemTxt2.Specific;
                oItemTxt3 = oForm.Items.Item("txt_1_3");
                txt3 = (SAPbouiCOM.EditText)oItemTxt3.Specific;

                oItemMtx1 = oForm.Items.Item("mtx_1_1");
                mtx1 = (SAPbouiCOM.Matrix)oItemMtx1.Specific;

                dta1 = oForm.DataSources.DataTables.Item("dta_1_1");

                oItembtn1 = oForm.Items.Item("btn_1_5");
                btnCargar = (SAPbouiCOM.Button)oItembtn1.Specific;

                //oItembtn1.Visible = false;

                oForm.Left = 350;
                oForm.Top = 10;
                oForm.Visible = true;
            }
            catch (Exception ex)
            {
                string err = ex.ToString(); 
            }
        }

        public void registrarItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

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
                                    case "btn_1_1":
                                        //obtiene documentos de tercero retnedor
                                        getDocTerceroRetenedor();
                                        break;
                                    case "btn_1_2":
                                        //guarda y acutliza el estado de documento (quita check retencion)
                                        updateTerceroRetenedor(FormUID);
                                        break;
                                    case "btn_1_3":
                                        //carga anexos
                                        Thread hilo = new Thread(new ParameterizedThreadStart(cargarArchivoRespuesta));
                                        hilo.SetApartmentState(ApartmentState.STA);
                                        hilo.Start(FormUID);
                                        break;
                                    case "btn_1_4":
                                        //quita anexos
                                        quitarAnexo(FormUID);
                                        break;
                                    case "btn_1_5":
                                        //cara datos de la liberacion
                                        cargarDataRCLiberacion(FormUID);
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

        private void cargarDataRCLiberacion(string FormUID)
        {
            string consultaDB1 = "";
            SAPbouiCOM.EditText _editText0;
            SAPbouiCOM.CheckBox _checkBox;

            try
            {
                if (txt3.Value.ToString().Equals(""))
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Debe ingresar un número de RC de Liberación."
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                consultaDB1 = "CALL \"SMC_APM_LISTAR_PORRCLIBERACION\" ('" + txt3.Value.ToString() + "') ";

                dta1.Rows.Clear();
                dta1.ExecuteQuery(consultaDB1);

                mtx1.Columns.Item("2").DataBind.Bind("dta_1_1", "Code");
                mtx1.Columns.Item("3").DataBind.Bind("dta_1_1", "DocEntry");
                mtx1.Columns.Item("4").DataBind.Bind("dta_1_1", "CardCode");
                mtx1.Columns.Item("5").DataBind.Bind("dta_1_1", "CardName");
                mtx1.Columns.Item("6").DataBind.Bind("dta_1_1", "NumAtCard");
                mtx1.Columns.Item("7").DataBind.Bind("dta_1_1", "DocCur");
                mtx1.Columns.Item("8").DataBind.Bind("dta_1_1", "TotalPagar");

                mtx1.LoadFromDataSource();
                mtx1.AutoResizeColumns();

                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    _checkBox.Checked = true;
                }

                _editText0 = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(1).Specific;

                actualizarMatrixAnexos(FormUID, _editText0.Value.ToString());
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }
        }

        private void getDocTerceroRetenedor()
        {
            string consultaDB1 = "";
            try
            {
                consultaDB1 = "CALL \"SMC_APM_LISTAR_TERCERORETENEDOR\" ('" + txt1.Value.ToString() + "','" + txt2.Value.ToString() + "') ";

                dta1.Rows.Clear();
                dta1.ExecuteQuery(consultaDB1);
                
                mtx1.Columns.Item("2").DataBind.Bind("dta_1_1", "Code");
                mtx1.Columns.Item("3").DataBind.Bind("dta_1_1", "DocEntry");
                mtx1.Columns.Item("fDoc").DataBind.Bind("dta_1_1", "Documento");
                mtx1.Columns.Item("4").DataBind.Bind("dta_1_1", "CardCode");
                mtx1.Columns.Item("5").DataBind.Bind("dta_1_1", "CardName");
                mtx1.Columns.Item("6").DataBind.Bind("dta_1_1", "NumAtCard");
                mtx1.Columns.Item("7").DataBind.Bind("dta_1_1", "DocCur");
                mtx1.Columns.Item("8").DataBind.Bind("dta_1_1", "TotalPagar");

                mtx1.LoadFromDataSource();
                mtx1.AutoResizeColumns();
            }
            catch(Exception ex)
            {
                string err = ex.ToString();
            }
        }

        private void updateTerceroRetenedor(string FormUID)
        {
            SAPbouiCOM.CheckBox _checkBox;
            SAPbouiCOM.EditText _editText0;
            SAPbouiCOM.EditText _editText1;
            int contadorfilasSeleccionadas = 0;
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Matrix mtxAnexos;

            try
            {
                string mensaje = "";
                ConexionDAO conexion = new ConexionDAO();
                string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


                //VALIDACION DE CAMPOS OBLIGATORIOS
                if (txt3.Value.ToString().Equals("") || txt3.Value.ToString().Length != 13)
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " El campo RC Liberación es obligatorio y debe ser de 13 caracteres."
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                for(int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if(_checkBox.Checked) contadorfilasSeleccionadas++;
                }

                if (contadorfilasSeleccionadas == 0)
                {
                    sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Debe seleccionar al menos un documento."
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }

                //ACTUALIZAR CABECERA Y DETALLE
                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if (_checkBox.Checked)
                    {
                        _editText0 = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(i + 1).Specific;
                        _editText1 = (SAPbouiCOM.EditText)mtx1.Columns.Item("3").Cells.Item(i + 1).Specific;
                        _daoEscenario.actualizarDetalleRetenidoLiberacion(NombreBaseDatos, _editText0.Value.ToString(), _editText1.Value.ToString(), ref SQLerr);
                        _daoEscenario.actualizarRCLiberacion(NombreBaseDatos, _editText0.Value.ToString(), txt3.Value.ToString(), ref SQLerr);
                    }
                }

                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Los registros se actualizaron correctamente."
                       , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                getDocTerceroRetenedor();
                txt3.Value = "";

                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("mtx_1_2");
                mtxAnexos = (SAPbouiCOM.Matrix)oItem.Specific;

                mtxAnexos.Clear();
            }
            catch (Exception ex)
            {
                string err = ex.ToString();

            }
        }

        private void cargarArchivoRespuesta(object frm)
        {
            SAPbouiCOM.CheckBox _checkBox;
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.Item oItem = null;
            SAPbouiCOM.EditText oEditText = null;
            string nombreArchivo = "";
            string mensaje = "";
            pagoDAO _pagoDAO = null;
            _pagoDAO = new pagoDAO();
            daoEscenario _daoEscenario = new daoEscenario();
            string escenario = "";
            bool primerEscenario = false;

            oForm = sboApplication.Forms.Item(frm.ToString());

            try
            {
                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if (_checkBox.Checked && !primerEscenario)
                    {
                        oEditText = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(i + 1).Specific;
                        escenario = oEditText.Value;
                        primerEscenario = true;
                    }
                }

                if (!primerEscenario)
                {
                    sboApplication.MessageBox("No ha seleccionado ningún comprobante.");
                    return;
                }

                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if (_checkBox.Checked)
                    {
                        oEditText = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(i + 1).Specific;
                        if (!oEditText.Value.Equals(escenario))
                        {
                            sboApplication.MessageBox("Solo puede liberar comprobantes de un mismo escenario a la vez.");
                            return;
                        }
                    }
                }

                OpenFileDialog choofdlog = new OpenFileDialog();
                choofdlog.Filter = "All Files (*.*)|*.*";
                choofdlog.FilterIndex = 1;
                choofdlog.Multiselect = false;

                if (choofdlog.ShowDialog() == DialogResult.OK)
                {
                    string sFileRuta = choofdlog.FileName;
                    string sFileName = choofdlog.SafeFileName;

                    string[] ext = sFileName.Split('.');
                    nombreArchivo = "ESC" + escenario + "_" + ext[0] + "." + ext[1];
                    //File.Copy(sFileRuta, @"\\WIN-SOPORTESAP\DocSap\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, true);
                    File.Copy(sFileRuta, @"D:\Pagos_Masivos\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, true);

                    //_daoEscenario.registrarEscenarioAnexo(escenario, @"\\WIN-SOPORTESAP\DocSap\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, "L", ref mensaje);
                    _daoEscenario.registrarEscenarioAnexo(sboApplication,escenario, @"D:\Pagos_Masivos\TerceroRetenedor\RespuestaSunat\" + nombreArchivo, "L", ref mensaje);
                    actualizarMatrixAnexos(frm.ToString(), escenario);

                    sboApplication.MessageBox("Se ha subido el anexo correctamente.");
                }
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }
        }

        private void actualizarMatrixAnexos(string FormUID,string escenario)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.DataTable dtaAnexos;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Matrix mtxAnexos;
            SAPbouiCOM.EditText oEditText;

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("mtx_1_2");
                mtxAnexos = (SAPbouiCOM.Matrix)oItem.Specific;
                dtaAnexos = oForm.DataSources.DataTables.Item("dta_1_2");

                dtaAnexos.Rows.Clear();
                dtaAnexos.ExecuteQuery("CALL \"SMC_APM_GET_ESCENARIO_ANEXOS\" ('" +escenario + "','L')");

                mtxAnexos.Clear();
                mtxAnexos.Columns.Item("#").DataBind.Bind("dta_1_2", "Id");
                mtxAnexos.Columns.Item("1").DataBind.Bind("dta_1_2", "Code");
                mtxAnexos.Columns.Item("2").DataBind.Bind("dta_1_2", "Ruta");
                mtxAnexos.LoadFromDataSource();
                mtxAnexos.AutoResizeColumns();
            }
            catch (Exception e)
            {
                string err = e.ToString();
            }
        }

        private void quitarAnexo(string FormUID)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Matrix mtxAnexos;
            SAPbouiCOM.EditText oEditText;
            daoEscenario _daoEscenario = new daoEscenario();
            string mensaje = "";
            SAPbouiCOM.CheckBox _checkBox;
            string escenario = "";
            bool primerEscenario = false;

            try
            {
                oForm = sboApplication.Forms.Item(FormUID);
                oItem = oForm.Items.Item("mtx_1_2");
                mtxAnexos = (SAPbouiCOM.Matrix)oItem.Specific;

                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if (_checkBox.Checked && !primerEscenario)
                    {
                        oEditText = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(i + 1).Specific;
                        escenario = oEditText.Value;
                        primerEscenario = true;
                    }
                }

                if (!primerEscenario)
                {
                    sboApplication.MessageBox("No ha seleccionado ningún comprobante.");
                    return;
                }

                for (int i = 0; i < mtx1.RowCount; i++)
                {
                    _checkBox = (SAPbouiCOM.CheckBox)mtx1.Columns.Item("1").Cells.Item(i + 1).Specific;
                    if (_checkBox.Checked)
                    {
                        oEditText = (SAPbouiCOM.EditText)mtx1.Columns.Item("2").Cells.Item(i + 1).Specific;
                        if (!oEditText.Value.Equals(escenario))
                        {
                            sboApplication.MessageBox("Solo puede liberar comprobantes de un mismo escenario a la vez.");
                            return;
                        }
                    }
                }

                if (mtxAnexos.GetNextSelectedRow() < 1)
                {
                    sboApplication.MessageBox("Debe selecionar un anexo de la grilla.");
                    return;
                }

                oEditText = (SAPbouiCOM.EditText)mtxAnexos.Columns.Item("1").Cells.Item(mtxAnexos.GetNextSelectedRow()).Specific;
                _daoEscenario.quitarEscenarioAnexo(sboApplication,escenario, oEditText.Value, ref mensaje);

                oEditText = (SAPbouiCOM.EditText)mtxAnexos.Columns.Item("2").Cells.Item(mtxAnexos.GetNextSelectedRow()).Specific;
                File.Delete(oEditText.Value);

                actualizarMatrixAnexos(FormUID,escenario);
                sboApplication.MessageBox("Se ha retirado el anexo correctamente.");
            }
            catch (Exception e)
            {
                string err = e.ToString();
            }
        }
    }
}
