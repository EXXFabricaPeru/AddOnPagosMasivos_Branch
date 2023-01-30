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
    class daoFacturaProveedor
    {
        //no se utiliza
        public string getCondicionPago(string docEntry,ref string mensaje)
        {
            DbCommand CommandEmpresa = null;
            mensaje = "";
            string mpago = "";

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetSqlStringCommand("SET SCHEMA " + ConfigurationManager.AppSettings["Catalogo"]);
                db.ExecuteNonQuery(CommandEmpresa);

                CommandEmpresa = db.GetSqlStringCommand("SELECT R1.\"PymntGroup\" FROM OPCH R0 LEFT JOIN OCTG R1 ON R1.\"GroupNum\" = R0.\"GroupNum\" WHERE R0.\"DocEntry\" = '" + docEntry + "'");

                using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
                {
                    while (Dr.Read())
                    {
                        mpago = Dr["PymntGroup"].ToString();
                    }
                }

                return mpago;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
            }

            return mpago;
        }

        //no se utiliza
        public string getLoteDet(ref string mensaje)
        {
            //conexSQL con = null;
            //SqlDataAdapter da = null;
            //DataSet pDataSet = null;
            //mensaje = "";
            string mpago = "";
            /*
            try
            {
                con = new conexSQL();
                da = new SqlDataAdapter();
                pDataSet = new DataSet();

                da.SelectCommand = new SqlCommand("select " +
                                                  "ISNULL(SUBSTRING(CONVERT(VARCHAR, YEAR(GETDATE())), 3, 2) +" +
                                                  "'00' +"+
                                                  "CONVERT(VARCHAR, CONVERT(INT, SUBSTRING(MAX(U_EXX_DETLOT), 5, 2)) + 1),SUBSTRING(CONVERT(VARCHAR, YEAR(GETDATE())),0,3) + '0001') AS 'LOTE' " +
                                                  "from OVPM where "+
                                                  "U_EXX_DETLOT like SUBSTRING(CONVERT(VARCHAR, YEAR(GETDATE())), 3, 2) + '00%' " +
                                                  "AND Canceled <> 'Y'", con.Conn);
                da.Fill(pDataSet);

                mpago = pDataSet.Tables[0].Rows[0]["LOTE"].ToString();

                return mpago;
            }
            catch (Exception e)
            {
                mensaje = "Catch => " + e.Message.ToString();
            }
            finally
            {
                con.ConnectionClose();
            }
            */
            return mpago;
        }
    }
}
