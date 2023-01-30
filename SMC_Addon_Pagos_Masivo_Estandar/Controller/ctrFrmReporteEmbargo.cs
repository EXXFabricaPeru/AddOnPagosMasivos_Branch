﻿using System;
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
    class ctrFrmReporteEmbargo
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
        private SAPbouiCOM.Item oItemTxt4;
        private SAPbouiCOM.Item oItemMtx1;

        private SAPbouiCOM.EditText txt1;
        private SAPbouiCOM.EditText txt2;
        private SAPbouiCOM.EditText txt3;
        private SAPbouiCOM.EditText txt4;

        private SAPbouiCOM.Matrix mtx1;
        #endregion

        #region Constructor
        public ctrFrmReporteEmbargo()
        {
        }

        public ctrFrmReporteEmbargo(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
        }
        #endregion

        public void cargarFormulario(string frmUID)
        {
            sapObjetos sapObj = new sapObjetos();
            sapObj.sapCargarFormulario(sboApplication, frmUID, Properties.Resources.frmReporteRCEmbargo, "");
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

                oItemTxt4 = oForm.Items.Item("txt_esc");
                txt4 = (SAPbouiCOM.EditText)oItemTxt4.Specific;

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
                                        //obtiene documentos de reporte
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

        //Obtiene documentos para el reporte de embargo
        private void getdocumentos()
        {
            string consultaDB1 = "";
            try
            {
                consultaDB1 = "CALL \"SMC_APM_REPORTE_EMBARGO\" ('" + txt1.Value.ToString() + "','" + txt2.Value.ToString() + "','" + txt3.Value.ToString() + "','" + txt4.Value.ToString() + "') ";

                dta1.Rows.Clear();
                dta1.ExecuteQuery(consultaDB1);

                mtx1.Columns.Item("1").DataBind.Bind("dta_1_1", "Code");
                mtx1.Columns.Item("2").DataBind.Bind("dta_1_1", "U_SMC_RCEMB");
                mtx1.Columns.Item("3").DataBind.Bind("dta_1_1", "U_SMC_FECHA");
                mtx1.Columns.Item("4").DataBind.Bind("dta_1_1", "U_SMC_RUTA");

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
