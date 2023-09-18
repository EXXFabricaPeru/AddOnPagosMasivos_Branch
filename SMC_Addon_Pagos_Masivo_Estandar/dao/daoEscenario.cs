using System;
using System.Collections.Generic;
using SMC_APM.Conexion;
using SMC_APM.dto;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Configuration;

namespace SMC_APM.dao
{
    class daoEscenario
    {
        //registra cabecera de doc seleccionados en el en tabla de usuario
        public void registrarCabecera(string NombreBaseDatos, string UsuarioConectado, string codigo, string descripcion, string cta, string tipo, string fecha, string medioPago, string estado, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_REG_ESCENARIOCAB\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigo;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = descripcion;
                CommandEmpresa.Parameters.Add(param1);
                DbParameter param2;
                param2 = CommandEmpresa.CreateParameter();
                param2.DbType = DbType.String;
                param2.Direction = ParameterDirection.Input;
                param2.Value = cta;
                CommandEmpresa.Parameters.Add(param2);
                DbParameter param3;
                param3 = CommandEmpresa.CreateParameter();
                param3.DbType = DbType.String;
                param3.Direction = ParameterDirection.Input;
                param3.Value = tipo;
                CommandEmpresa.Parameters.Add(param3);
                DbParameter param4;
                param4 = CommandEmpresa.CreateParameter();
                param4.DbType = DbType.String;
                param4.Direction = ParameterDirection.Input;
                param4.Value = fecha;
                CommandEmpresa.Parameters.Add(param4);
                DbParameter param5;
                param5 = CommandEmpresa.CreateParameter();
                param5.DbType = DbType.String;
                param5.Direction = ParameterDirection.Input;
                param5.Value = UsuarioConectado;
                CommandEmpresa.Parameters.Add(param5);

                DbParameter param6;
                param6 = CommandEmpresa.CreateParameter();
                param6.DbType = DbType.String;
                param6.Direction = ParameterDirection.Input;
                param6.Value = medioPago;
                CommandEmpresa.Parameters.Add(param6);

                DbParameter param7;
                param7 = CommandEmpresa.CreateParameter();
                param7.DbType = DbType.String;
                param7.Direction = ParameterDirection.Input;
                param7.Value = estado;
                CommandEmpresa.Parameters.Add(param7);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //actualizar fecha de pago de escenario
        public void actualizarFechaPago(string codigo, SAPbouiCOM.Application sboApplication, string fecha, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_UPDATE_FECHAPAGO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigo;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = fecha;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
        }


        //actualizar accion de autorizacion
        public void actualizarAccionAutorizacion(string escenario, string accion, string NombreBaseDatos, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ACTUALIZAR_AUTORIZAR\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = accion;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
        }


        //actualizar accion de la segunda autorizacion 
        public void actualizarAccionAutorizacion2(string escenario, string accion, string NombreBaseDatos, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ACTUALIZAR_AUTORIZAR2\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = accion;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
        }

        //registra detalle de escenario documentos seleccionados 
        public void registrarDetalle(string NombreBD, string UsuarioConectado, string codigo, string docEntry, string total, string tipodocumento, int nroCuota, int lineaAsiento, string cuentaProveedor, string codigoBanco
            , string cardCodeFacto, string cardNameFacto, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";


            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBD + ".\"SMC_APM_REG_ESCENARIODET\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigo;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = docEntry;
                CommandEmpresa.Parameters.Add(param1);
                DbParameter param2;
                param2 = CommandEmpresa.CreateParameter();
                param2.DbType = DbType.String;
                param2.Direction = ParameterDirection.Input;
                param2.Value = total;
                CommandEmpresa.Parameters.Add(param2);
                DbParameter param3;
                param3 = CommandEmpresa.CreateParameter();
                param3.DbType = DbType.String;
                param3.Direction = ParameterDirection.Input;
                param3.Value = UsuarioConectado;
                CommandEmpresa.Parameters.Add(param3);
                DbParameter param4;
                param4 = CommandEmpresa.CreateParameter();
                param4.DbType = DbType.String;
                param4.Direction = ParameterDirection.Input;
                param4.Value = tipodocumento;
                CommandEmpresa.Parameters.Add(param4);
                DbParameter param5;
                param5 = CommandEmpresa.CreateParameter();
                param5.DbType = DbType.Int32;
                param5.Direction = ParameterDirection.Input;
                param5.Value = nroCuota;
                CommandEmpresa.Parameters.Add(param5);
                DbParameter param6;
                param6 = CommandEmpresa.CreateParameter();
                param6.DbType = DbType.Int32;
                param6.Direction = ParameterDirection.Input;
                param6.Value = lineaAsiento;
                CommandEmpresa.Parameters.Add(param6);

                DbParameter param7;
                param7 = CommandEmpresa.CreateParameter();
                param7.DbType = DbType.String;
                param7.Direction = ParameterDirection.Input;
                param7.Value = cuentaProveedor;
                CommandEmpresa.Parameters.Add(param7);

                DbParameter param8;
                param8 = CommandEmpresa.CreateParameter();
                param8.DbType = DbType.String;
                param8.Direction = ParameterDirection.Input;
                param8.Value = codigoBanco;
                CommandEmpresa.Parameters.Add(param8);

                DbParameter param9;
                param9 = CommandEmpresa.CreateParameter();
                param9.DbType = DbType.String;
                param9.Direction = ParameterDirection.Input;
                param9.Value = cardCodeFacto;
                CommandEmpresa.Parameters.Add(param9);

                DbParameter param10;
                param10 = CommandEmpresa.CreateParameter();
                param10.DbType = DbType.String;
                param10.Direction = ParameterDirection.Input;
                param10.Value = cardNameFacto;
                CommandEmpresa.Parameters.Add(param10);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //actualiza detalle check retencion
        public void actualizarDetalleRetenido(string NombreBaseDatos, string escenario, string docEntry, string documento, string retenido, string pagar, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_UPDATE_ESCENARIODET\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.Int32;
                param1.Direction = ParameterDirection.Input;
                param1.Value = docEntry;
                CommandEmpresa.Parameters.Add(param1);
                DbParameter param2;
                param2 = CommandEmpresa.CreateParameter();
                param2.DbType = DbType.String;
                param2.Direction = ParameterDirection.Input;
                param2.Value = retenido;
                CommandEmpresa.Parameters.Add(param2);
                DbParameter param3;
                param3 = CommandEmpresa.CreateParameter();
                param3.DbType = DbType.String;
                param3.Direction = ParameterDirection.Input;
                param3.Value = pagar;
                CommandEmpresa.Parameters.Add(param3);

                DbParameter param4;
                param4 = CommandEmpresa.CreateParameter();
                param4.DbType = DbType.String;
                param4.Direction = ParameterDirection.Input;
                param4.Value = documento;
                CommandEmpresa.Parameters.Add(param4);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //actualiza detalle liberacion 
        public void actualizarDetalleRetenidoLiberacion(string NombreBaseDatos, string escenario, string docEntry, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_UPDATE_ESCENARIODETLIBERACION\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.Int32;
                param1.Direction = ParameterDirection.Input;
                param1.Value = docEntry;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //actualiza el rc embargo del escenario
        public void actualizarRCEmbargo(string NombreBaseDatos, string escenario, string RCEmbargo, ref string mensaje, string ArchivoTxt, string fecha)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_UPDATE_RCEMBARGO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = RCEmbargo;
                CommandEmpresa.Parameters.Add(param1);

                DbParameter param2;
                param2 = CommandEmpresa.CreateParameter();
                param2.DbType = DbType.String;
                param2.Direction = ParameterDirection.Input;
                param2.Value = ArchivoTxt;
                CommandEmpresa.Parameters.Add(param2);

                DbParameter param4;
                param4 = CommandEmpresa.CreateParameter();
                param4.DbType = DbType.String;
                param4.Direction = ParameterDirection.Input;
                param4.Value = fecha;
                CommandEmpresa.Parameters.Add(param4);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //actualiza el dato rc liberacion 
        public void actualizarRCLiberacion(string NombreBaseDatos, string escenario, string RCLiberacion, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_UPDATE_RCLIBERACION\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = RCLiberacion;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //elimina documento del detalle de escenario
        public void eliminarDetalle(string NombreBaseDatos, string codigo, string docEntry, string tipodocumento, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetSqlStringCommand("SET SCHEMA " + NombreBaseDatos);
                db.ExecuteNonQuery(CommandEmpresa);

                CommandEmpresa = db.GetSqlStringCommand("DELETE FROM \"@SMC_APM_ESCDET\" WHERE \"U_SMC_DOCENTRY\" = '" + docEntry + "' AND \"U_SMC_ESCCAB\" = '" + codigo + "' AND \"U_SMC_TIPO_DOCUMENTO\" = '" + tipodocumento + "'");

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //no se utiliza
        public void marcarDetalle(SAPbouiCOM.Application sboApplication, string docentry, string marca, string escenario, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetSqlStringCommand("SET SCHEMA " + NombreBaseDatos);
                db.ExecuteNonQuery(CommandEmpresa);

                CommandEmpresa = db.GetSqlStringCommand("UPDATE \"@SMC_APM_ESCDET\" SET \"U_SMC_MARCA\" = '" + marca + "' WHERE \"U_SMC_DOCENTRY\" = '" + docentry + "' AND \"U_SMC_ESCCAB\" = '" + escenario + "'");

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //obtiene el correlativo del escenario
        public string obtenerCodigo(string NombreBaseDatos, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";
            string codigo = "";


            //ConexionDAO conexion = new ConexionDAO();
            //string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetSqlStringCommand("SET SCHEMA " + NombreBaseDatos);
                db.ExecuteNonQuery(CommandEmpresa);

                CommandEmpresa = db.GetSqlStringCommand("SELECT IFNULL((SELECT MAX(CAST(\"Code\" AS INT)) FROM \"@SMC_APM_ESCCAB\"),0)+1 AS \"Code\" FROM DUMMY");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        codigo = Dr["Code"].ToString();
                    }
                }

                return codigo;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }

            return "";
        }

        //registrar anexo de escenario dependiendo su tipo L (liberacion) E (embargo)
        public void registrarEscenarioAnexo(SAPbouiCOM.Application sboApplication, string escenario, string ruta, string tipo, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ADD_ESCENARIO ANEXOS\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = ruta;
                CommandEmpresa.Parameters.Add(param1);
                DbParameter param2;
                param2 = CommandEmpresa.CreateParameter();
                param2.DbType = DbType.String;
                param2.Direction = ParameterDirection.Input;
                param2.Value = tipo;
                CommandEmpresa.Parameters.Add(param2);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        //quitar anexo
        public void quitarEscenarioAnexo(SAPbouiCOM.Application sboApplication, string escenario, string idAnexo, ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_DELETE_ESCENARIO ANEXOS\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = escenario;
                CommandEmpresa.Parameters.Add(param);
                DbParameter param1;
                param1 = CommandEmpresa.CreateParameter();
                param1.DbType = DbType.String;
                param1.Direction = ParameterDirection.Input;
                param1.Value = idAnexo;
                CommandEmpresa.Parameters.Add(param1);

                db.ExecuteNonQuery(CommandEmpresa);
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }
        }

        internal void ActualizarEstadoAutorizacion(SAPbobsCOM.Company company, string codigoEscenario, string estado)
        {
            SAPbobsCOM.Recordset rs = null;
            try
            {
                rs = company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string query = $"UPDATE \"@SMC_APM_ESCCAB\" SET \"U_EXD_ESTE\" = '{estado}' WHERE \"Code\" = '{codigoEscenario}'";
                rs.DoQuery(query);

                query = $"update \"@SMC_APM_AUTORIZAR\" set \"U_SMC_ESTAT\" = 'PE' where \"Code\" = '{codigoEscenario}'";
                rs.DoQuery(query);
            }
            catch (Exception)
            {

                throw;
            }
            finally { Util.utilNET.liberarObjeto(rs); }
            ;
        }
    }
}
