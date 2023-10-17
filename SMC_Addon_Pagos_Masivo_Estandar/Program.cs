using System;
using System.Windows.Forms;
using SMC_APM.Conexion;
using SMC_APM.Controladores;
using SMC_APM.Util;
using EXX_MetaData;
using EXX_Metadata.BL;
using System.Reflection;

namespace SMC_Addon_Pagos_Masivo_Estandar
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            conexSBO conexSBO = null;
            ctrPrincipal ctrPrincipal = null;

            try
            {
                //realiza conexion 
                conexSBO = new conexSBO();
                if ((conexSBO != null) && (conexSBO.sboCompany.Connected))
                {
                    MDResources.Messages = (string m, MessageType t) =>
                    {
                        switch (t)
                        {
                            case MessageType.Info:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                break;
                            case MessageType.Success:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                break;
                            case MessageType.Error:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                break;
                            default:
                                break;
                        }
                    };

                    if (MDResources.loadMetaData(Assembly.GetExecutingAssembly().GetName().Version, conexSBO.sboApplication, "EXX", "PGOMSV"))
                    {
                        //inicia el addon
                        ctrPrincipal = new ctrPrincipal(conexSBO.sboApplication, conexSBO.sboCompany);
                        ctrPrincipal.iniciarAddon();
                        GC.KeepAlive(conexSBO);
                        GC.KeepAlive(ctrPrincipal);
                        Application.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " : MAIN > " + ex.Message, "Aceptar",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                conexSBO.DesconectarCompany();
            }
        }
    }
}
