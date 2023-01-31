using SAP_AddonFramework;
using SAPbobsCOM;
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

        internal static int GetDecimalesConfigurado()
        {
            Recordset rs = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = "SELECT \"SumDec\"  FROM OADM;";
            rs.DoQuery(query);

            if(rs.RecordCount == 0)
                throw new Exception("No se ha configurado los decimales");

            return rs.Fields.Item("SumDec").Value;
        }
        #endregion
    }
}