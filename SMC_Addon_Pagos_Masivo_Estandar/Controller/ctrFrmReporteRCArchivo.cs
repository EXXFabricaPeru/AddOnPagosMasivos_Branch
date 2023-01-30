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
    class ctrFrmReporteRCArchivo
    {
        #region Atributos
        string SQLerr = "";
        private SAPbouiCOM.Application sboApplication;
        private SAPbobsCOM.Company sboCompany;
        private SAPbouiCOM.Form oForm = null;

        private SAPbouiCOM.DataTable dta1;

        private SAPbouiCOM.Item oItemTxt1;
        private SAPbouiCOM.Item oItemTxt2;
        private SAPbouiCOM.Item oItemMtx1;

        private SAPbouiCOM.EditText txt1;
        private SAPbouiCOM.EditText txt2;

        private SAPbouiCOM.Matrix mtx1;
        #endregion

        #region Constructor
        public ctrFrmReporteRCArchivo()
        {
        }

        public ctrFrmReporteRCArchivo(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
        }
        #endregion

        public void cargarFormulario(string frmUID)
        {
            sapObjetos sapObj = new sapObjetos();
            sapObj.sapCargarFormulario(sboApplication, frmUID, Properties.Resources.frmReporteRCArchivo, "");
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

                oItemMtx1 = oForm.Items.Item("mtx_1_1");
                mtx1 = (SAPbouiCOM.Matrix)oItemMtx1.Specific;

                dta1 = oForm.DataSources.DataTables.Item("dta_1_1");

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
                                        //cargar documentos
                                        getdocumentos();
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


        private void getdocumentos()
        {
            string consultaDB1 = "";
            try
            {
                consultaDB1 = "CALL \"SMC_APM_ARCHIVOS_EMBARGO\" ('" + txt1.Value.ToString() + "','" + txt2.Value.ToString() + "') ";

                dta1.Rows.Clear();
                dta1.ExecuteQuery(consultaDB1);

                mtx1.Columns.Item("1").DataBind.Bind("dta_1_1", "Escenario");
                mtx1.Columns.Item("2").DataBind.Bind("dta_1_1", "U_SMC_RCEMB");
                mtx1.Columns.Item("3").DataBind.Bind("dta_1_1", "Monto");
                mtx1.Columns.Item("4").DataBind.Bind("dta_1_1", "U_SMC_FECHAS");
                mtx1.Columns.Item("5").DataBind.Bind("dta_1_1", "Archivo");

                mtx1.LoadFromDataSource();
                mtx1.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }
        }
    }
}
