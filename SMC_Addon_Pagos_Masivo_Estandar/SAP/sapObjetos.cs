using System;
using SMC_APM.Util;

namespace SMC_APM.SAP
{
    class sapObjetos
    {
        public void sapCrearSubMenu(SAPbouiCOM.Application application, string menuPrincipal, string subMenuCodigo, string subMenuNombre, SAPbouiCOM.BoMenuType tipoMenu)
        {
            SAPbouiCOM.Menus uiMenus = null;
            SAPbouiCOM.MenuItem uiMenuItem = null;
            SAPbouiCOM.MenuCreationParams uiMenuCreation = null;
            try
            {
                uiMenuCreation = ((SAPbouiCOM.MenuCreationParams)(application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
                uiMenuItem = application.Menus.Item(menuPrincipal);
                uiMenus = uiMenuItem.SubMenus;

                uiMenuCreation.Type = tipoMenu;
                uiMenuCreation.UniqueID = subMenuCodigo;
                uiMenuCreation.String = subMenuNombre;
                uiMenuCreation.Position = uiMenus.Count + 1;
                uiMenus.AddEx(uiMenuCreation);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("[66000-68]"))
                    application.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + ": SAP > sapCrearSubMenu() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                else
                    application.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + ": SAP > sapCrearSubMenu() > El menu "+
                    subMenuNombre +" ya ha sido creado.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            finally
            {
                utilNET.liberarObjeto(uiMenus);
                utilNET.liberarObjeto(uiMenuItem);
                utilNET.liberarObjeto(uiMenuCreation);
            }
        }

        public void sapCargarFormulario(SAPbouiCOM.Application application, string codFormulario, string archivoXml, string formType)
        {
            SAPbouiCOM.Form oForm = null;
            SAPbouiCOM.FormCreationParams oParams = null;
            SAPbouiCOM.ProgressBar oPB = null;

            try
            {
                oParams = (SAPbouiCOM.FormCreationParams)application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                oParams.UniqueID = codFormulario;
                if (formType.Equals(""))
                    oParams.XmlData = archivoXml;
                else
                {
                    oParams.FormType = formType;
                    //oParams.ObjectType = "54";
                }   
                oForm = application.Forms.AddEx(oParams);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("[66000-11]"))
                    application.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + ": Error: sapObjetos.cs > sapCargarFormulario() > " + 
                        ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                else
                    application.Forms.Item(codFormulario).Select();
            }
            finally
            {
                utilNET.liberarObjeto(oForm);
                utilNET.liberarObjeto(oParams);
                utilNET.liberarObjeto(oPB);
            }
        }
    }
}
