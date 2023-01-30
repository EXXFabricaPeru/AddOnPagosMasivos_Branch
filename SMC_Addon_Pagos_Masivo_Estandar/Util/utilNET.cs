using System;
using System.Runtime.InteropServices;

namespace SMC_APM.Util
{
    public class utilNET
    {
        #region Atributos
        #endregion

        #region Metodos
        public static void liberarObjeto(Object objeto)
        {
            try
            {
                if (objeto != null)
                {
                    Marshal.ReleaseComObject(objeto);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " Error Liberando Objeto: " + ex.Message);
            }
        }
        #endregion
    }
}