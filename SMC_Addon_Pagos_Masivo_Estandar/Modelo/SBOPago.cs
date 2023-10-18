using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Modelo
{
    public class SBOPago
    {
        public int CodSerieSBO { get; set; }
        public string CodigoSN { get; set; }
        public DateTime FechaDocumento { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaContabilizacion { get; set; }
        public string Moneda { get; set; }
        public int Clase { get; set; }
        public double TipoCambio { get; set; }
        public string MedioPagoSUNAT { get; set; }
        public string Referencia { get; set; }
        public double GastosBancarios { get; set; }
        public double Monto { get; set; }
        public string CuentaControl { get; set; }
        public string CodigoProyecto { get; set; }
        public string CorrelativoRendicion { get; set; }
        public string CuentaCheque { get; set; }
        public SBOMetodoPago MetodoPago { get; set; }
        public IEnumerable<SBOPagoDetalle> Detalle { get; set; }
        public IEnumerable<int> ExtLineasDS { get; set; }
    }

    public class SBOPagoDetalle
    {
        public int TipoDocumento { get; set; }
        public int IdDocumento { get; set; }
        public int IdLinea { get; set; }
        public int NroCuota { get; set; }
        public string MonedaDoc { get; set; }
        public string CodigoCuenta { get; set; }
        public string NumeroCuenta { get; set; }
        public double Monto { get; set; }
        public double MontoAPagar{ get; set; }
        public string Comentarios { get; set; }
        public string CodProyecto { get; set; }
        public string CentroCosto { get; set; }
        public string CentroCosto2 { get; set; }
        public string CentroCosto3 { get; set; }
        public string CentroCosto4 { get; set; }
        public string CentroCosto5 { get; set; }
        public int LineaPgoMsv { get; set; }

    }

    public class SBOMetodoPago
    {
        public string Tipo { get; set; }
        public string Cuenta { get; set; }
        public string Banco { get; set; }
        public string Sucursal { get; set; }
        public string CuentaBanco { get; set; }
        public string Pais { get; set; }
        public string Referencia { get; set; }
    }
}
