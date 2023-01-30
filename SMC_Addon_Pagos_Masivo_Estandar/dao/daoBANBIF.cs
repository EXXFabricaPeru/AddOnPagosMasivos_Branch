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
    class daoBANBIF
    {
        public List<dtoBancoBANBIFDetalle> getBANBIFDetalle(string codigoEscenario, SAPbouiCOM.Application sboApplication, ref string mensaje)
        {
            List<dtoBancoBANBIFDetalle> lstobj = null;
            dtoBancoBANBIFDetalle obj = null;
            mensaje = "";
            DbCommand CommandEmpresa = null;


            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            try
            {
                lstobj = new List<dtoBancoBANBIFDetalle>();
                Database db = DatabaseFactory.CreateDatabase();

                CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_ARCHIVOBANCO_BANBIF_DETALLE\"");
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
                        obj = new dtoBancoBANBIFDetalle();
                        obj.TipoDocumentoProveedor = Dr["TipoDocumentoProveedor"].ToString();
                        obj.NumeroDocumentoProveedor = Dr["NumeroDocumentoProveedor"].ToString();
                        obj.NombreProveedor = Dr["NombreProveedor"].ToString();
                        obj.TipoDocumentoPago = Dr["TipoDocumentoPago"].ToString();
                        obj.NumeroDocumentoPago = Dr["NumeroDocumentoPago"].ToString();
                        obj.MonedaPago = Dr["MonedaPago"].ToString();
                        obj.Importe = Dr["Importe"].ToString();
                        obj.FechaPago = Convert.ToDateTime(Dr["FechaPago"].ToString());
                        obj.FormaPago = Dr["FormaPago"].ToString();
                        obj.CodigoBanco = Dr["CodigoBanco"].ToString();
                        obj.MonedaCuenta = Dr["MonedaCuenta"].ToString();
                        obj.NumeroCuenta = Dr["NumeroCuenta"].ToString();
                        obj.DocumentoAplicaionNotaCredito = Dr["DocumentoAplicaionNotaCredito"].ToString();

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
