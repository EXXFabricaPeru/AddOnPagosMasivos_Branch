using EXX_MetaData.BL;
using SAP_AddonFramework;
using SAPbouiCOM;
using SMC_APM.Controller;
using SMC_APM.SAP;
using SMC_APM.View;
using SMC_APM.View.USRForms;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SMC_APM.Controladores
{
    internal class ctrPrincipal
    {
        #region Atributos

        public SAPbouiCOM.Application sboApplication;
        public SAPbobsCOM.Company sboCompany;
        private frmPagoProveedores frmPagoPrvoveedores = null;
        private ctrFrmLiberarTerceroRet _ctrFrmLiberarTerceroRet = null;
        private ctrFrmReporteEmbargo _ctrFrmReporteEmbargo = null;
        private ctrFrmReporteLiberacion _ctrFrmReporteLiberacion = null;
        private ctrFrmReporteRCArchivo _ctrFrmReporteRCArchivo = null;
        private ctrFrmAutorizacion _ctrFrmAutorizacion = null;
        private FormPagoMasivo formPagoMasivo = null;


        #endregion Atributos

        #region Constructor

        public ctrPrincipal()
        {
        }

        public ctrPrincipal(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany)
        {
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;

            Globales.Aplication = sboApplication;
            Globales.Company = sboCompany;
        }

        #endregion Constructor

        #region Metodos Publicos

        public void iniciarAddon()
        {
            
            //CrearEstructuraDeDatos();

            //carga los menus
            cargarMenus();
            cargarObjetosUsuario();
            registrarEventos();

            setFiltersCustom();
        }

        private void CrearEstructuraDeDatos()
        {
            try
            {
                MDResources.Messages = (s, t) =>
                {
                    switch (t)
                    {
                        case MDResources.MessageType.Info:
                            sboApplication.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            break;

                        case MDResources.MessageType.Success:
                            sboApplication.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            break;

                        case MDResources.MessageType.Error:
                            sboApplication.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            break;
                    }
                };

                MDResources.loadMetaData(Assembly.GetExecutingAssembly().GetName().Version, sboApplication, "ADDPGMV");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void setFiltersCustom()
        {
            try
            {
                EventFilters eventFilters = new EventFilters();
                EventFilter eventFilter = null;
                eventFilter = eventFilters.Add(BoEventTypes.et_ALL_EVENTS);
                var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsClass
                        && (t.Namespace == "SMC_APM.View.USRForms")
                        && t.Name.StartsWith("Form")
                        select t;
                q.ToList().ForEach(c =>
                {
                    if (c.GetField("TYPE")?.GetValue(null).ToString() is string frmUID)
                    {
                        if (!existFormTypeInFilters(frmUID, eventFilter))
                            eventFilter.AddEx(frmUID);
                    }
                    else if (c.GetField("TYPES")?.GetValue(null) is string[] arrFrmUID)
                        arrFrmUID.ToList().ForEach(f => { if (!existFormTypeInFilters(f, eventFilter)) eventFilter.AddEx(f); });
                });

                //eventFilter = eventFilters.Add(BoEventTypes.et_ALL_EVENTS);
                eventFilter.AddEx("frmSMC");

                sboApplication.SetFilter(eventFilters);
            }
            catch { throw; }
        }

        private bool existFormTypeInFilters(string formType, EventFilter filter)
        {
            try
            {
                filter.Item(formType);
            }
            catch (Exception) { return false; }

            return true;
        }

        #endregion Metodos Publicos

        #region Metodos Privados

        private void cargarMenus()
        {
            sapObjetos sapObj = new sapObjetos();

            //creacion de menus y submenus
            sapObj.sapCrearSubMenu(sboApplication, SMC_APM.Properties.Resources.codMenuSAP1, SMC_APM.Properties.Resources.codMenu1, SMC_APM.Properties.Resources.nomMenu1, SAPbouiCOM.BoMenuType.mt_POPUP);
            sapObj.sapCrearSubMenu(sboApplication, SMC_APM.Properties.Resources.codMenu1, SMC_APM.Properties.Resources.codSubMenu1, SMC_APM.Properties.Resources.nomSubMenu1, SAPbouiCOM.BoMenuType.mt_STRING);
            sapObj.sapCrearSubMenu(sboApplication, SMC_APM.Properties.Resources.codMenu1, "SMC0007", "Autorizacion Pagos Masivos", SAPbouiCOM.BoMenuType.mt_STRING);
            sapObj.sapCrearSubMenu(sboApplication, SMC_APM.Properties.Resources.codMenu1, "SMC0008", "Pagos Masivos", SAPbouiCOM.BoMenuType.mt_STRING);

            sapObj.sapCrearSubMenu(sboApplication, "1536", "SMC0004", "Reporte Tercero Ret. Embargo", SAPbouiCOM.BoMenuType.mt_STRING);
            sapObj.sapCrearSubMenu(sboApplication, "1536", "SMC0005", "Reporte Tercero Ret. Liberación", SAPbouiCOM.BoMenuType.mt_STRING);
            sapObj.sapCrearSubMenu(sboApplication, "1536", "SMC0006", "Reporte cuentas por pagar informadas a SUNAT", SAPbouiCOM.BoMenuType.mt_STRING);
        }

        private void cargarObjetosUsuario()
        {
            //creacion de tablas de usuario y campos de usuario
            sapObjUser sapObj = new sapObjUser(sboApplication, sboCompany);
            string[] validValues = new string[2];
            string[] validDes = new string[2];

            validValues[0] = "N";
            validValues[1] = "Y";
            validDes[0] = "NO";
            validDes[1] = "SI";

            /*sapObj.CreaCampoMD("DSC1", "EXM_PMASIVO", "Pagos Masivos", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None,
                    1, SAPbobsCOM.BoYesNoEnum.tNO, validValues, validDes, null, null);

            sapObj.CreaCampoMD("DSC1", "EXM_SERIEP", "Serie Pago", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None,
                    10, SAPbobsCOM.BoYesNoEnum.tNO, null, null, null, null);

            sapObj.CreaCampoMD("OCRB", "EXM_INTERBANCARIA", "Número Cuenta Interbancaria", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None,
                    50, SAPbobsCOM.BoYesNoEnum.tNO, null, null, null, null);

            sapObj.CreaCampoMD("OPCH", "SMC_PFEFECTIVO", "Pos. Flujo de Efectivo", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None,
                    150, SAPbobsCOM.BoYesNoEnum.tNO, null, null, null, null);*/

            sapObj.CreaCampoMD("INV6", "EXX_CONFTIPODET", "Detracción", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None,
                    10, SAPbobsCOM.BoYesNoEnum.tNO, null, null, null, null);

            sapObj.CreaTablaMD("SMC_APM_CONFIAPM", "SMC: Conf. PagosMasivos", SAPbobsCOM.BoUTBTableType.bott_NoObject);
        }

        private void registrarEventos()
        {
            try
            {
                sboApplication.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(registrarAppEvent);
                sboApplication.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(registrarItemEvent);
                sboApplication.MenuEvent += new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(registrarMenuEvent);
                sboApplication.FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(registrarFormDataEvent);
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " ctrPrincipal.cs > registrarEventos() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void registrarFormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, out bool BubbleEvent)
        {
            BubbleEvent = true;
            IUSAP uiForm = null;
            try
            {
                if (!string.IsNullOrEmpty(BusinessObjectInfo.FormTypeEx)) uiForm = UIFormFactory.GetForm(BusinessObjectInfo);
                if (uiForm != null) BubbleEvent = uiForm.DataEvent(BusinessObjectInfo);
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }

        private void registrarAppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            try
            {
                switch (EventType)
                {
                    case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                        System.Windows.Forms.Application.Exit();
                        break;
                }
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrPrincipal.cs > registrarAppEvent() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void registrarMenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {
                if (pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case "SMCPAGMASPRO":
                            sboApplication.Forms.Item("frmSMC1").Select();
                            break;

                        case "SMC0004":
                            sboApplication.Forms.Item("frmSMC4").Select();
                            break;

                        case "SMC0005":
                            sboApplication.Forms.Item("frmSMC5").Select();
                            break;

                        case "SMC0006":
                            sboApplication.Forms.Item("frmSMC6").Select();
                            break;

                        case "SMC0007":
                            //sboApplication.Forms.Item("frmSMC7").Select();
                            _ctrFrmAutorizacion = new ctrFrmAutorizacion(sboApplication, sboCompany);
                            _ctrFrmAutorizacion.cargarFormulario("frmSMC7");
                            break;
                        case "SMC0008":
                            //sboApplication.Forms.Item("frmSMC7").Select();
                            formPagoMasivo = new FormPagoMasivo("FRMPMP" + DateTime.Now.ToString("hhmmss"));

                            break;
                            /*case "1287":
                                SAPbouiCOM.Form oFormFather = sboApplication.Forms.ActiveForm;
                                if (oFormFather.TypeEx.Equals("65306")
                                    || oFormFather.TypeEx.Equals("141")
                                    || oFormFather.TypeEx.Equals("65301")
                                    || oFormFather.TypeEx.Equals("181"))
                                {
                                    SAPbouiCOM.Form oForm = sboApplication.Forms.Item(oFormFather.UDFFormUID);
                                    SAPbouiCOM.Item oItem;
                                    SAPbouiCOM.EditText oEditText;

                                    oItem = oForm.Items.Item("U_SMC_FEFECTIVO");
                                    oEditText = (SAPbouiCOM.EditText)oItem.Specific;

                                    if (oEditText.Value.ToString().Equals(""))
                                    {
                                        oEditText.Value = "Proveedores de Bienes y Servicios";
                                    }

                                    oItem = oFormFather.Items.Item("4");
                                    oItem.Click();
                                }
                                break;*/
                    }
                }
                else
                {
                    switch (pVal.MenuUID)
                    {
                        case "1282":
                            ((FormPagoMasivo)UIFormFactory.GetFormByUID(Globales.Aplication.Forms.ActiveForm.UniqueID)).LoadDataOnFormAddMode();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case "SMCPAGMASPRO":
                            frmPagoPrvoveedores = new frmPagoProveedores(sboApplication, sboCompany, "frmSMC1", "PRO");
                            break;

                        case "SMC0004":
                            _ctrFrmReporteEmbargo = new ctrFrmReporteEmbargo(sboApplication, sboCompany);
                            _ctrFrmReporteEmbargo.cargarFormulario("frmSMC4");
                            break;

                        case "SMC0005":
                            _ctrFrmReporteLiberacion = new ctrFrmReporteLiberacion(sboApplication, sboCompany);
                            _ctrFrmReporteLiberacion.cargarFormulario("frmSMC5");
                            break;

                        case "SMC0006":
                            _ctrFrmReporteRCArchivo = new ctrFrmReporteRCArchivo(sboApplication, sboCompany);
                            _ctrFrmReporteRCArchivo.cargarFormulario("frmSMC6");
                            break;

                        case "SMC0007":
                            _ctrFrmAutorizacion = new ctrFrmAutorizacion(sboApplication, sboCompany);
                            _ctrFrmAutorizacion.cargarFormulario("frmSMC7");
                            break;
                    }
                }
            }
        }

        private void registrarItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {

                if (pVal.FormTypeEx == "FrmLPG" || pVal.FormTypeEx == "FrmPMP")
                {
                    IUSAP uiForm = null;

                    try
                    {
                        if (!string.IsNullOrEmpty(pVal.FormTypeEx)) uiForm = UIFormFactory.GetForm(pVal);
                        if (uiForm != null) BubbleEvent = uiForm.ItemEvent(pVal);
                    }
                    catch (Exception ex)
                    {
                        sboApplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        BubbleEvent = false;
                    }
                }

                switch (pVal.FormUID)
                {
                    case "frmSMC1":
                        if (frmPagoPrvoveedores != null)
                            frmPagoPrvoveedores.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC3":
                        if (frmPagoPrvoveedores != null)
                            frmPagoPrvoveedores.eventFrmLiberarTercero(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC2":

                        ctrFrmDatosCuenta ctrFrmDatosCuenta = new ctrFrmDatosCuenta(sboApplication, sboCompany);
                        ctrFrmDatosCuenta.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC4":
                        if (_ctrFrmReporteEmbargo != null)
                            _ctrFrmReporteEmbargo.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC5":
                        if (_ctrFrmReporteLiberacion != null)
                            _ctrFrmReporteLiberacion.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC6":
                        if (_ctrFrmReporteRCArchivo != null)
                            _ctrFrmReporteRCArchivo.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;

                    case "frmSMC7":
                        if (_ctrFrmAutorizacion != null)
                            _ctrFrmAutorizacion.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
                        break;
                }

            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrPrincipal.cs > registrarItemEvent() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion Metodos Privados
    }
}