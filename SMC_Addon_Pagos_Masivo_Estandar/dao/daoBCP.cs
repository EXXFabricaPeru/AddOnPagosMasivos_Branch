using System;
using System.Collections.Generic;
using System.Linq;
using SMC_APM.dto;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Configuration;

namespace SMC_APM.dao
{
    class daoBCP
    {
        //obtiene cabecera para el txt del banco bcp
        public dtoBancoBCP getBCP(string codigoEscenario,string NombreBaseDatos, ref string mensaje)
        {
            dtoBancoBCP obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_BCP_CABECERA_NUEVO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigoEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBancoBCP();
                        obj.TipoRegistro = Dr["TipoRegistro"].ToString();
                        obj.CantidadAbonos = Dr["CantidadAbonos"].ToString();
                        obj.FechaProceso = Dr["FechaProceso"].ToString();
                        obj.TipoCuenta = Dr["TipoCuenta"].ToString();
                        obj.CuentaCargo = Dr["CuentaCargo"].ToString();
                        obj.Montototal = Dr["Montototal"].ToString();
                        obj.Referencia = Dr["Referencia"].ToString();
                        obj.Validacion = Dr["Validacion"].ToString();
                        obj.Cadena = Dr["Cadena"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();                
                    }
                }

                return obj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }

            return obj;
        }

        //obtiene detalle para el txt del banco bcp
        public List<dtoBancoBCPDetalle> getBCPDetalle(string codigoEscenario, string NombreBaseDatos, ref string mensaje)
        {
            List<dtoBancoBCPDetalle> lstobj = null;
            dtoBancoBCPDetalle obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBancoBCPDetalle>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_BCP_DETALLE_NUEVO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigoEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBancoBCPDetalle();
                        obj.TipoRegistro = Dr["TipoRegistro"].ToString();
                        obj.TipoCuenta = Dr["TipoCuenta"].ToString();
                        obj.CuentaAbono = Dr["CuentaAbono"].ToString();
                        obj.TipoDocumentoIdentidad = Dr["TipoDocumentoIdentidad"].ToString();
                        obj.NumeroDocumentoIdentidad = Dr["NumeroDocumentoIdentidad"].ToString();
                        obj.NombreProveedor = Dr["NombreProveedor"].ToString();
                        obj.ReferenciaBeneficiario = Dr["ReferenciaBeneficiario"].ToString();
                        obj.Referencia = Dr["Referencia"].ToString();
                        obj.ImporteParcial = Dr["ImporteParcial"].ToString();
                        obj.Importetotal = Dr["Importetotal"].ToString();
                        obj.ValidacionIDC = Dr["ValidarIDC"].ToString();
                        obj.TipoDocumentoPagar = Dr["TipoDocumentoPagar"].ToString();
                        obj.NumeroDocumento = Dr["NumeroDocumento"].ToString();
                        obj.Caracter = Dr["Caracter"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                       

                        lstobj.Add(obj);
                    }
                }

                if (lstobj.Count < 1)
                    mensaje = "Error => No hay registros.";

                return lstobj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
                return lstobj;
            }
        }







        //obtiene detalle para el txt del banco interbank
        public List<dtoBancoBCPDetalle> getINTERBANKDetalle(string codigoEscenario, string NombreBaseDatos, ref string mensaje)
        {
            List<dtoBancoBCPDetalle> lstobj = null;
            dtoBancoBCPDetalle obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBancoBCPDetalle>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_INTERBANK_DETALLE_NUEVO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigoEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBancoBCPDetalle();
                        obj.TipoRegistro = Dr["TipoRegistro"].ToString();
                        obj.TipoCuenta = Dr["TipoCuenta"].ToString();
                        obj.CuentaAbono = Dr["CuentaAbono"].ToString();
                        obj.TipoDocumentoIdentidad = Dr["TipoDocumentoIdentidad"].ToString();
                        obj.NumeroDocumentoIdentidad = Dr["NumeroDocumentoIdentidad"].ToString();
                        obj.NumeroDocumentoIdentidad2 = Dr["NumeroDocumentoIdentidad1"].ToString();
                        obj.NombreProveedor = Dr["NombreProveedor"].ToString();
                        obj.ReferenciaBeneficiario = Dr["ReferenciaBeneficiario"].ToString();
                        obj.Referencia = Dr["Referencia"].ToString();
                        obj.ImporteParcial = Dr["ImporteParcial"].ToString();
                        obj.Importetotal = Dr["Importetotal"].ToString();
                        obj.ValidacionIDC = Dr["ValidacionIDC"].ToString();
                        obj.TipoDocumentoPagar = Dr["TipoDocumentoPagar"].ToString();
                        obj.NumeroDocumento = Dr["NumeroDocumento"].ToString();
                        obj.Caracter = Dr["Caracter"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.FechaVencimiento = Dr["FechaVencimiento"].ToString();
                        obj.TipoPersona = Dr["TipoPersona"].ToString();

                        lstobj.Add(obj);
                    }
                }

                if (lstobj.Count < 1)
                    mensaje = "Error => No hay registros.";

                return lstobj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
                return lstobj;
            }
        }




        //obtiene detalle para el txt del banco scotiabank
        public List<dtoBancoBCPDetalle> getSCOTIABANKDetalle(string codigoEscenario, string NombreBaseDatos, ref string mensaje)
        {
            List<dtoBancoBCPDetalle> lstobj = null;
            dtoBancoBCPDetalle obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBancoBCPDetalle>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_SCOTIABANK_DETALLE\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigoEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBancoBCPDetalle();
                        obj.TipoRegistro = Dr["TipoRegistro"].ToString();
                        obj.TipoCuenta = Dr["TipoCuenta"].ToString();
                        obj.CuentaAbono = Dr["CuentaAbono"].ToString();
                        obj.TipoDocumentoIdentidad = Dr["TipoDocumentoIdentidad"].ToString();
                        obj.NumeroDocumentoIdentidad = Dr["NumeroDocumentoIdentidad"].ToString();
                        obj.NombreProveedor = Dr["NombreProveedor"].ToString();
                        obj.ReferenciaBeneficiario = Dr["ReferenciaBeneficiario"].ToString();
                        obj.Referencia = Dr["Referencia"].ToString();
                        obj.ImporteParcial = Dr["ImporteParcial"].ToString();
                        obj.Importetotal = Dr["Importetotal"].ToString();
                        obj.ValidacionIDC = Dr["ValidacionIDC"].ToString();
                        obj.TipoDocumentoPagar = Dr["TipoDocumentoPagar"].ToString();
                        obj.NumeroDocumento = Dr["NumeroDocumento"].ToString();
                        obj.Caracter = Dr["Caracter"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.FechaVencimiento = Dr["FechaVencimiento"].ToString();
                        obj.FormaPago = Dr["FormaPago"].ToString();
                        obj.CuentaAbonoCCI = Dr["CuentaAbonoCCI"].ToString();
                        lstobj.Add(obj);
                    }
                }

                if (lstobj.Count < 1)
                    mensaje = "Error => No hay registros.";

                return lstobj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
                return lstobj;
            }
        }


        //obtiene detalle para el txt del banco santander
        public List<dtoBancoBCPDetalle> getSANTANDERDetalle(string codigoEscenario, string NombreBaseDatos, ref string mensaje)
        {
            List<dtoBancoBCPDetalle> lstobj = null;
            dtoBancoBCPDetalle obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBancoBCPDetalle>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_SANTANDER_DETALLE\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigoEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBancoBCPDetalle();
                        obj.TipoRegistro = Dr["TipoRegistro"].ToString();
                        obj.TipoCuenta = Dr["TipoCuenta"].ToString();
                        obj.CuentaAbono = Dr["CuentaAbono"].ToString();
                        obj.TipoDocumentoIdentidad = Dr["TipoDocumentoIdentidad"].ToString();
                        obj.NumeroDocumentoIdentidad = Dr["NumeroDocumentoIdentidad"].ToString();
                        obj.NombreProveedor = Dr["NombreProveedor"].ToString();
                        obj.ReferenciaBeneficiario = Dr["ReferenciaBeneficiario"].ToString();
                        obj.Referencia = Dr["Referencia"].ToString();
                        obj.ImporteParcial = Dr["ImporteParcial"].ToString();
                        obj.Importetotal = Dr["Importetotal"].ToString();
                        obj.ValidacionIDC = Dr["ValidacionIDC"].ToString();
                        obj.TipoDocumentoPagar = Dr["TipoDocumentoPagar"].ToString();
                        obj.NumeroDocumento = Dr["NumeroDocumento"].ToString();
                        obj.Caracter = Dr["Caracter"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.FechaVencimiento = Dr["FechaVencimiento"].ToString();
                        obj.TipoPersona = Dr["TipoPersona"].ToString();
                        obj.CuentaAbonoCCI = Dr["CuentaAbonoCCI"].ToString();

                        lstobj.Add(obj);
                    }
                }

                if (lstobj.Count < 1)
                    mensaje = "Error => No hay registros.";

                return lstobj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
                return lstobj;
            }
        }


    }
}
