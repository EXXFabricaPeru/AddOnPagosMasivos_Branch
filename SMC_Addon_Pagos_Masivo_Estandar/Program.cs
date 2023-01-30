using System;
using System.Windows.Forms;
using SMC_APM.Conexion;
using SMC_APM.Controladores;
using SMC_APM.Util;

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
                    //inicia el addon
                    ctrPrincipal = new ctrPrincipal(conexSBO.sboApplication, conexSBO.sboCompany);
                    ctrPrincipal.iniciarAddon();
                    GC.KeepAlive(conexSBO);
                    GC.KeepAlive(ctrPrincipal);
                    Application.Run();
                }
            }
            catch(Exception ex)
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
