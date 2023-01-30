using System;
using System.Collections.Generic;
using SMC_APM.Conexion;
using SMC_APM.dto;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Configuration;
using SAP_AddonFramework;
using SAPbobsCOM;

namespace SMC_APM.dao
{
    class daoBanco
    {
        //listar bancos
        public List<dtoBanco> listarBancos(SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<dtoBanco> lstobj = null;
            dtoBanco obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBanco>();
                Database db = DatabaseFactory.CreateDatabase();
                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_BANCOS\"");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBanco();
                        obj.CuentaCont = Dr["CuentaCont"].ToString();
                        obj.AcctName = Dr["AcctName"].ToString();
                        obj.BankCode = Dr["BankCode"].ToString();
                        obj.BankName = Dr["BankName"].ToString();
                        obj.CuentaBank = Dr["CuentaBank"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.Saldo = Convert.ToDecimal(Dr["Saldo"].ToString());

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
            }
            finally
            {
            }

            return lstobj;
        }


        //lista filtro bancos
        public List<dtoBanco> listarFiltroBancos(SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<dtoBanco> lstobj = null;
            dtoBanco obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBanco>();
                Database db = DatabaseFactory.CreateDatabase();
                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_FILTRO_BANCO\"");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBanco();
                        obj.BankCode = Dr["BankCode"].ToString();
                        obj.BankName = Dr["BankName"].ToString();
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
            }
            finally
            {
            }

            return lstobj;
        }

        internal List<dtoBanco> listarBancos_Custom()
        {
            Recordset rs = null;
            try
            {
                List<dtoBanco> bancos = new List<dtoBanco>();
                dtoBanco banco;

                rs = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = Globales.Company.DbServerType == BoDataServerTypes.dst_HANADB ? "CALL SMC_APM_LISTAR_BANCOS_2()" : "EXEC SMC_APM_LISTAR_BANCOS_2";

                rs.DoQuery(query);

                if (rs.RecordCount == 0)
                    throw new Exception("No se encontraron cuentas de bancos configuradas en la sociedad");

                while (!rs.EoF)
                {
                    banco = new dtoBanco();


                    banco.CuentaCont = rs.Fields.Item("CuentaCont").Value.ToString();
                    banco.AcctName = rs.Fields.Item("AcctName").Value.ToString();
                    banco.BankCode = rs.Fields.Item("BankCode").Value.ToString();
                    banco.BankName = rs.Fields.Item("BankName").Value.ToString();
                    banco.CuentaBank = rs.Fields.Item("CuentaBank").Value.ToString();
                    banco.Moneda = rs.Fields.Item("Moneda").Value.ToString();
                    bancos.Add(banco);

                    rs.MoveNext();
                }

                return bancos;
            }
            catch (Exception)
            {

                throw;
            }
            finally { Util.utilNET.liberarObjeto(rs); }
        }


        //listar la series
        public List<dtoSeriePago> listarSeriesPago(string codEscenario, SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<dtoSeriePago> lstobj = null;
            dtoSeriePago obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;


            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoSeriePago>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_SERIEPAGO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codEscenario;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoSeriePago();
                        obj.Series = Dr["Series"].ToString();
                        obj.SeriesName = Dr["SeriesName"].ToString();
                        obj.SeriesDefecto = Dr["SeriesDefecto"].ToString();
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
            }
            finally
            {
            }

            return lstobj;
        }

        //listar bancos detalle
        public List<dtoBanco> listarBancosDet(SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<dtoBanco> lstobj = null;
            dtoBanco obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBanco>();

                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_BANCOS_DET\"");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBanco();
                        obj.CuentaCont = Dr["CuentaCont"].ToString();
                        obj.AcctName = Dr["AcctName"].ToString();
                        obj.BankCode = Dr["BankCode"].ToString();
                        obj.BankName = Dr["BankName"].ToString();
                        obj.CuentaBank = Dr["CuentaBank"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.Saldo = Convert.ToDecimal(Dr["Saldo"].ToString());

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
            }
            finally
            {
            }

            return lstobj;
        }

        //consulta datos de bancos
        public dtoBanco consultarBanco(string codigo,SAPbouiCOM.Application sboApplication,  ref string mensaje)
        {
            List<dtoBanco> lstobj = null;
            dtoBanco obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_CONSULTAR_BANCO\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigo;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        obj = new dtoBanco();
                        obj.CuentaCont = Dr["CuentaCont"].ToString();
                        obj.AcctName = Dr["AcctName"].ToString();
                        obj.BankCode = Dr["BankCode"].ToString();
                        obj.BankName = Dr["BankName"].ToString();
                        obj.CuentaBank = Dr["CuentaBank"].ToString();
                        obj.Moneda = Dr["Moneda"].ToString();
                        obj.Saldo = Convert.ToDecimal(Dr["Saldo"].ToString());
                        obj.FechaPago = Convert.ToDateTime(Dr["FechaPago"].ToString());
                        obj.MedioPago = Dr["MedioPago"].ToString();
                        obj.Estado = Dr["Estado"].ToString(); 
                    }
                }

                //if (lstobj.Count < 1)
                //    mensaje = "Error => No hay registros.";

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


        //consulta monto tope
        public List<decimal> MontosTopes(SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<decimal> lstobj = null;
            dtoBanco obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<decimal>();

                Database db = DatabaseFactory.CreateDatabase();
                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_MONTOTOPE\"");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        decimal TopeSoles = decimal.Parse(Dr["TopeSoles"].ToString());
                        decimal TopeDolares = decimal.Parse(Dr["TopeDolares"].ToString());

                        lstobj.Add(TopeSoles);
                        lstobj.Add(TopeDolares);
                    }
                }

                if (lstobj.Count < 1)
                    mensaje = "Error => No hay registros.";

                return lstobj;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }

            return lstobj;
        }



        //obtiene accion de autorizacion
        public List<string> ObtenerAccion(string codigo, SAPbouiCOM.Application sboApplication, ref string mensaje)
        {

            string Accion = "N";
            string Accion2 = "N";
            List<string> acciones = new List<string>();
            mensaje = "";
            DbCommand CommandEmpresa = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {


                Database db = DatabaseFactory.CreateDatabase();
                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ESCENARIOS_AUTORIZADOS\"");
                DbParameter param;
                param = CommandEmpresa.CreateParameter();
                param.DbType = DbType.String;
                param.Direction = ParameterDirection.Input;
                param.Value = codigo;
                CommandEmpresa.Parameters.Add(param);

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        Accion = Dr["Accion"].ToString();
                        Accion2 = Dr["Accion2"].ToString();
                        acciones.Add(Accion);
                        acciones.Add(Accion2);
                    }
                }

                return acciones;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }

            return acciones;
        }




    }
}
