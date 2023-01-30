using System;
using System.Collections.Generic;
using SMC_APM.Util;

namespace SMC_APM.SAP
{
    class sapObjUser
    {
        #region Atributos
        private SAPbouiCOM.Application sboApplication;
        private SAPbobsCOM.Company sboCompany;
        private int m_iErrCode = 0;
        private string m_sErrMsg = "";
        #endregion

        #region Constructor
        public sapObjUser(SAPbouiCOM.Application _sboApplication, SAPbobsCOM.Company _sboCompany)
        {
            sboApplication = _sboApplication;
            sboCompany = _sboCompany;
        }
        #endregion

        #region Metodos
        public bool CreaTablaMD(string NombTabla, string DescTabla, SAPbobsCOM.BoUTBTableType tipoTabla)
        {
            SAPbobsCOM.UserTablesMD oUserTablesMD = null;//Objeto para mantener una instancia de las tablas contenidas en SBO
            try
            {

                oUserTablesMD = null;//Inicializamos en valor de la tabla en nulo para asegurarnos que no contenga datos
                oUserTablesMD = (SAPbobsCOM.UserTablesMD)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);//Creamos una instancia del tipo tabla de SBO
                if (!oUserTablesMD.GetByKey(NombTabla))//Verificamos si la tabla que deseamos crear ya se encuentra registrada en el sistema
                {
                    oUserTablesMD.TableName = NombTabla;//Asignamos el nombre de la tabla que deseamos crear
                    oUserTablesMD.TableDescription = DescTabla;//Asignamos la descripcion de la tabla que se desea crear
                    oUserTablesMD.TableType = tipoTabla;//Asignamos el tipo de tabla que se desea crear

                    m_iErrCode = oUserTablesMD.Add();//Capturamos el resultado arrojado al agregar la nueva tabla al modelo de datos de SBO
                    if (m_iErrCode != 0)//validamos si esxiste algun error
                    {
                        sboCompany.GetLastError(out m_iErrCode, out m_sErrMsg);//Obtenemos el mensaje de erro arrojado por SBO
                        sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon + ": Configuración verificada",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        return false;
                    }
                    else
                        sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon + ": Configuración verificada",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    utilNET.liberarObjeto(oUserTablesMD);//Liberamos dememoria el objeto utilizado
                    oUserTablesMD = null;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon + ": Configuración verificada",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                string error = ex.ToString();
                return false;
            }
            finally
            {
                utilNET.liberarObjeto(oUserTablesMD);
                oUserTablesMD = null;
            }
        }

        public void CreaCampoMD(string NombreTabla, string NombreCampo, string DescCampo, SAPbobsCOM.BoFieldTypes TipoCampo,
          SAPbobsCOM.BoFldSubTypes SubTipo, int Tamano, SAPbobsCOM.BoYesNoEnum Obligatorio, string[] validValues,
           string[] validDescription, string valorPorDef, string tablaVinculada)
        {
            SAPbobsCOM.UserFieldsMD oUserFieldsMD = null;//Objeto para la gestion de campos de usuarios de SBO
            try
            {
                if (NombreTabla == null) NombreTabla = "";
                if (NombreCampo == null) NombreCampo = "";
                if (Tamano == 0) Tamano = 10;
                if (validValues == null) validValues = new string[0];
                if (validDescription == null) validDescription = new string[0];
                if (valorPorDef == null) valorPorDef = "";
                if (tablaVinculada == null) tablaVinculada = "";

                oUserFieldsMD = (SAPbobsCOM.UserFieldsMD)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);//generamos una instacia del objeto para la gestion de campos SBO
                oUserFieldsMD.TableName = NombreTabla;//asignamos el nombre de la tabla en donde deseamos crear el campo
                oUserFieldsMD.Name = NombreCampo;//asignamos el nombre del campo a crear
                oUserFieldsMD.Description = DescCampo;//asignamos la descripcion del campo a crear
                oUserFieldsMD.Type = TipoCampo;//ajustamos el tipo del campo a crear
                if (TipoCampo != SAPbobsCOM.BoFieldTypes.db_Date) oUserFieldsMD.EditSize = Tamano;//verificamos que el tipo de campo no sea de tipo fecha para poder asignarle un tamaño
                oUserFieldsMD.SubType = SubTipo;//asignamos el subtipo del campo

                if (tablaVinculada != "") oUserFieldsMD.LinkedTable = tablaVinculada;//asignamos la tabla vinculada
                else
                {
                    if (validValues.Length > 0)//verificamos que el campo posea valores validos
                    {
                        for (int i = 0; i <= (validValues.Length - 1); i++)//Asignamos los valores validos para el campo
                        {
                            oUserFieldsMD.ValidValues.Value = validValues[i];
                            if (validDescription.Length > 0) oUserFieldsMD.ValidValues.Description = validDescription[i];
                            else oUserFieldsMD.ValidValues.Description = validValues[i];
                            oUserFieldsMD.ValidValues.Add();
                        }
                    }
                    oUserFieldsMD.Mandatory = Obligatorio;
                    if (valorPorDef != "") oUserFieldsMD.DefaultValue = valorPorDef;//Asignamos el valor por defecto para el campo
                }

                m_iErrCode = oUserFieldsMD.Add();//Obtenemos el valor de repuesta de SBO al crear el campo
                if (m_iErrCode != 0)//Validamos que no se hayan producido errores
                {
                    sboCompany.GetLastError(out m_iErrCode, out m_sErrMsg);//Obtenemos el mensaje de error ocurrido
                    if (m_iErrCode != -5002)
                        sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon + ": Configuración verificada",
                            SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
                else
                    sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon + " Se ha creado el campo de usuario: " + NombreCampo
                            + "en la tabla: " + NombreTabla, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(Properties.Resources.nombreAddon +
                    ": Configuración verificada",
                    SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                string error = ex.ToString();
            }
            finally
            {
                utilNET.liberarObjeto(oUserFieldsMD);//Liberamos el objeto para la creacion del campo
                oUserFieldsMD = null;
            }
        }
        #endregion
    }
}
