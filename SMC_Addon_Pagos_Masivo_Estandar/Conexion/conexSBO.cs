using System;
using System.Windows.Forms;
using SMC_APM.Util;

namespace SMC_APM.Conexion
{
    class conexSBO
    {
        #region Atributos
        private static SAPbouiCOM.Application _sboApplication;
        private static SAPbobsCOM.Company _sboCompany;
        #endregion

        #region Getter&Setter
        public SAPbouiCOM.Application sboApplication
        {
            get
            {
                return _sboApplication;
            }

            set
            {
                _sboApplication = value;
            }
        }

        public SAPbobsCOM.Company sboCompany
        {
            get
            {
                return _sboCompany;
            }

            set
            {
                _sboCompany = value;
            }
        }
        #endregion 

        #region Constructores
        public conexSBO()
        {
            try
            {
                
                ObtenerAplicacion();
                ConectarCompany();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.nombreAddon + ": ERRORCATCH > Conexion > ConexSBO.cs >  " + ex.Message, "Aceptar",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        #region Metodos
        private void ObtenerAplicacion()
        {
            try
            {
                //obtener app
                string strConexion = "";
                string[] strArgumentos = new string[4];
                SAPbouiCOM.SboGuiApi oSboGuiApi = null;

                oSboGuiApi = new SAPbouiCOM.SboGuiApi();
                //strArgumentos = System.Environment.GetCommandLineArgs();
                //strArgumentos[0] = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";
                strArgumentos[0] = System.Convert.ToString(Environment.GetCommandLineArgs().GetValue(1));

                if (strArgumentos.Length > 0)
                {
                    if (strArgumentos.Length > 1)
                    {
                        if (strArgumentos[0].LastIndexOf("\\") > 0) strConexion = strArgumentos[1];
                        else strConexion = strArgumentos[0];
                    }
                    else
                    {
                        if (strArgumentos[0].LastIndexOf("\\") > -1) strConexion = strArgumentos[0];
                        else
                        {
                            MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " Error en: Conexion_SBO.cs > ObtenerAplicacion(): SAP Business One no esta en ejecucion", "Aceptar",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " Error en: Conexion_SBO.cs > ObtenerAplicacion(): SAP Business One no esta en ejecucion", "Aceptar",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                oSboGuiApi.Connect(strConexion);
                sboApplication = oSboGuiApi.GetApplication(-1);
            }
            catch (Exception ex)
            {
                {
                    MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " Error en: Conexion_SBO.cs > ObtenerAplicacion(): " + ex.Message, "Aceptar",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        public void ConectarCompany()
        {
            //conexion company
            string sErrMsg = "";
            int iErrCode = 0;
            try
            {
                sboCompany = (SAPbobsCOM.Company)sboApplication.Company.GetDICompany();

            }
            catch (Exception ex)
            {
                sboCompany.GetLastError(out iErrCode, out sErrMsg);
                utilNET.liberarObjeto(sboCompany);
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: Conexion_SBO.cs > ConectarCompany() CONECTARCOMPANY:" + ex.Message,
                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void DesconectarCompany()
        {
            //desconexion
            try
            {
                sboCompany.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " Error en: Conexion_SBO.cs > DesconectarCompany(): " + ex.Message, "Aceptar",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
