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
    class pagoDAO
    {
        //no se utiliza
        public List<PagoDTO> GetListaPago(SAPbouiCOM.Application sboApplication, string codEscenario)
        {
            List<PagoDTO> olista = null;
            PagoDTO oPagoDTO = null;
            string[] seriennum = null;
            DbCommand CommandEmpresa = null;
            string mensaje = "";

            olista = new List<PagoDTO>();

            Database db = DatabaseFactory.CreateDatabase();


            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);



            CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_PAGO_CP1\"");
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
                    oPagoDTO = new PagoDTO();
                    seriennum = null;
                    seriennum = Dr["NumFactura"].ToString().Split('-');

                    //oPagoDTO.DocEntry = Dr["DocEntry"].ToString();
                    oPagoDTO.DocNumFactura = Dr["DocNum"].ToString();
                    oPagoDTO.CuentaContable = Dr["CuentaContable"].ToString();
                    oPagoDTO.FechaFactura = Convert.ToDateTime(Dr["FechaDoc"].ToString());
                    oPagoDTO.RucProveedor = Dr["CodCliente"].ToString();
                    oPagoDTO.CardName = Dr["NomCliente"].ToString();
                    oPagoDTO.MonedaFactura = Dr["TipoMoneda"].ToString();
                    oPagoDTO.SerieFactura = seriennum[0];
                    oPagoDTO.NumeroFactura = seriennum[1];
                    oPagoDTO.MontoFacturaSoles = Convert.ToDecimal(Dr["MontoFactura"].ToString());
                    oPagoDTO.MontoFactura = Convert.ToDecimal(Dr["MontoFacturaDolar"].ToString());
                    oPagoDTO.PagadoFact = Convert.ToDecimal(Dr["PAGADOFACT"].ToString());
                    oPagoDTO.PagadoDet = Convert.ToDecimal(Dr["PAGADODET"].ToString());
                    oPagoDTO.TipoServicio = Dr["TipoServicio"].ToString();
                    oPagoDTO.MontoPagoSoles = Convert.ToDecimal(Dr["Cuota1Soles"].ToString());
                    oPagoDTO.MontoPago = Convert.ToDecimal(Dr["Cuota1Dolar"].ToString());
                    oPagoDTO.Cuota2Soles = Convert.ToDecimal(Dr["Cuota2Soles"].ToString());
                    oPagoDTO.Cuota2dolares = Convert.ToDecimal(Dr["Cuota2Dolar"].ToString());
                    oPagoDTO.MontoSinDecimal = Convert.ToInt32(Convert.ToDecimal(Dr["DetracPagar"].ToString()));
                    oPagoDTO.ResiduoDecimal = Convert.ToDecimal(Dr["MontoDecimal"].ToString());
                    oPagoDTO.tipoCambioDoc = Convert.ToDecimal(Dr["TCDoc"].ToString());
                    oPagoDTO.tipoCambioPago = Convert.ToDecimal(Dr["TCPago"].ToString());
                    oPagoDTO.Cuota1TCPago = Convert.ToDecimal(Dr["Cuota1TCPago"].ToString());
                    oPagoDTO.MontoRedondeo = Convert.ToDecimal(Dr["MontoRedondeo"].ToString());
                    oPagoDTO.TipoDocumento = Dr["TipoDocumento"].ToString();
                    oPagoDTO.TipoOperacion = Dr["TipoOperacion"].ToString();
                    oPagoDTO.FechaPago = Convert.ToDateTime(Dr["FechaPago"].ToString());

                    olista.Add(oPagoDTO);
                }
            }
            return olista;
        }

        //lista los documentos que se van a pagar tanto interbancarios como del mismo banco
        public List<PagoDTO> GetListaPagoSAP(SAPbouiCOM.Application sboApplication, string codEscenario)
        {
            List<PagoDTO> olista = null;
            PagoDTO oPagoDTO = null;
            DbCommand CommandEmpresa = null;
            string mensaje = "";
            olista = new List<PagoDTO>();

            Database db = DatabaseFactory.CreateDatabase();

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_LISTAR_PAGO_SAP\"");
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
                    oPagoDTO = new PagoDTO();

                    oPagoDTO.DocEntry = Convert.ToInt32(Dr["DocEntry"].ToString());
                    oPagoDTO.RucProveedor = Dr["CardCode"].ToString();
                    oPagoDTO.CardName = Dr["CardName"].ToString();
                    oPagoDTO.NumAtCard = Dr["NumAtCard"].ToString();
                    oPagoDTO.FechaPago = Convert.ToDateTime(Dr["FechaPago"].ToString());
                    oPagoDTO.TotalPagar = Convert.ToDouble(Dr["TotalPagar"].ToString());
                    oPagoDTO.FlujoEfectivo = Convert.ToInt32(Dr["FlujoEfectivo"].ToString());
                    oPagoDTO.FlujoEfectivoDescripcion = Dr["FlujoEfectivoDescripcion"].ToString();
                    oPagoDTO.InstallmentId = Convert.ToInt32(Dr["InstlmntID"].ToString());
                    oPagoDTO.SeriePago = Convert.ToInt32(Dr["SeriePago"].ToString());
                    oPagoDTO.CorrelativoRetencion = Convert.ToInt32(Dr["CorrelativoRetencion"].ToString());
                    oPagoDTO.MontoRetencion = Convert.ToDouble(Dr["MontoRetencion"].ToString());
                    oPagoDTO.BankCode = Dr["BankCode"].ToString();

                    oPagoDTO.CodigoRetencion = Dr["CodigoRetencion"].ToString();

                    oPagoDTO.TipoDocumento = Dr["TipoDocumento"].ToString();
                    oPagoDTO.Fila = Dr["Fila"].ToString();

                    olista.Add(oPagoDTO);
                }
            }
            return olista;
        }

        //genera el txt de tercero retenerdor
        public List<TXT3Retenedor> getDetalleTXT3Retenedor(SAPbouiCOM.Application sboApplication, string codEscenario)
        {
            List<TXT3Retenedor> olista = null;
            TXT3Retenedor oPagoDTO = null;
            DbCommand CommandEmpresa = null;
            string mensaje = "";
            olista = new List<TXT3Retenedor>();

            Database db = DatabaseFactory.CreateDatabase();

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_TERCERORETENEDOR\"");
            DbParameter param;
            param = CommandEmpresa.CreateParameter();
            param.DbType = DbType.Int32;
            param.Direction = ParameterDirection.Input;
            param.Value = codEscenario;
            CommandEmpresa.Parameters.Add(param);

            using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
            {
                while (Dr.Read())
                {
                    oPagoDTO = new TXT3Retenedor();
                    oPagoDTO.RUC = Dr["RUC"].ToString();
                    oPagoDTO.monto = Convert.ToDecimal(Dr["Monto"].ToString());

                    olista.Add(oPagoDTO);
                }
            }
            return olista;
        }

        //obtiene los documentos retenidos para la liberacion
        public List<dtoDocTerceroRetenedor> getDocTerceroRetenedor(string fechaIni, string fechaFin)
        {
            List<dtoDocTerceroRetenedor> olista = null;
            dtoDocTerceroRetenedor oPagoDTO = null;
            DbCommand CommandEmpresa = null;

            olista = new List<dtoDocTerceroRetenedor>();

            Database db = DatabaseFactory.CreateDatabase();
            CommandEmpresa = db.GetStoredProcCommand(ConfigurationManager.AppSettings["Catalogo"] + ".\"SMC_APM_LISTAR_TERCERORETENEDOR\"");
            DbParameter param;
            param = CommandEmpresa.CreateParameter();
            param.DbType = DbType.String;
            param.Direction = ParameterDirection.Input;
            param.Value = fechaIni;
            CommandEmpresa.Parameters.Add(param);
            DbParameter param1;
            param1 = CommandEmpresa.CreateParameter();
            param1.DbType = DbType.String;
            param1.Direction = ParameterDirection.Input;
            param1.Value = fechaFin;
            CommandEmpresa.Parameters.Add(param1);

            using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
            {
                while (Dr.Read())
                {
                    oPagoDTO = new dtoDocTerceroRetenedor();
                    
                    oPagoDTO.Code = Dr["Code"].ToString();
                    oPagoDTO.DocEntry = Dr["DocEntry"].ToString();
                    oPagoDTO.CardCode = Dr["CardCode"].ToString();
                    oPagoDTO.CardName = Dr["CardName"].ToString();
                    oPagoDTO.NumAtCard = Dr["NumAtCard"].ToString();
                    oPagoDTO.DocCur = Dr["DocCur"].ToString();
                    oPagoDTO.TotalPagar = Convert.ToDouble(Dr["TotalPagar"].ToString()); ;

                    olista.Add(oPagoDTO);
                }
            }
            return olista;
        }


        //no se utiliza
        public string ValidarPagoPreliminar(SAPbouiCOM.Application sboApplication, string DocEntry)
        {

            DbCommand CommandEmpresa = null;
            string result = "";
            string mensaje = "";
            Database db = DatabaseFactory.CreateDatabase();

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


            CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_VALIDARPAGOPREELIMINAR\"");
            DbParameter param;
            param = CommandEmpresa.CreateParameter();
            param.DbType = DbType.String;
            param.Direction = ParameterDirection.Input;
            param.Value = DocEntry;
            CommandEmpresa.Parameters.Add(param);

            using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
            {
                while (Dr.Read())
                {
                    result = Dr["Canceled"].ToString();
                }
            }
            return result;
        }


        //obtiene serie de retencion colocada en una tabla de usuario (U_SMC_CONFIGRET)
        public int SerieRetencion(SAPbouiCOM.Application sboApplication)
        {

            DbCommand CommandEmpresa = null;
            int result = 0;
            string mensaje = "";
            Database db = DatabaseFactory.CreateDatabase();

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);


            CommandEmpresa = db.GetStoredProcCommand(NombreBaseDatos + ".\"SMC_APM_SERIE_RETENCION\"");


            using (IDataReader Dr = db.ExecuteReader(CommandEmpresa))
            {
                while (Dr.Read())
                {
                    result = Convert.ToInt32(Dr["Code"].ToString());
                }
            }
            return result;
        }
    }
}
