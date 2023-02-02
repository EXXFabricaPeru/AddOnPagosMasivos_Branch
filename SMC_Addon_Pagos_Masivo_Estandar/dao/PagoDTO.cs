using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dao
{
    class PagoDTO
    {
        public int DocEntry { get; set; }
        public string DocNumFactura { get; set; }
        public string CuentaContable { get; set; }
        public DateTime FechaFactura { get; set; }
        public string RucProveedor { get; set; }
        public string CardName { get; set; }
        public string MonedaFactura { get; set; }
        public string SerieFactura { get; set; }
        public string NumeroFactura { get; set; }
        public Decimal MontoFacturaSoles { get; set; }
        public Decimal MontoFactura { get; set; }
        public Decimal PagadoFact { get; set; }
        public Decimal PagadoDet { get; set; }
        public string TipoServicio { get; set; }
        public Decimal MontoPagoSoles { get; set; }
        public Decimal MontoPago { get; set; }
        public Decimal Cuota2Soles { get; set; }
        public Decimal Cuota2dolares { get; set; }
        public int MontoSinDecimal { get; set; }
        public Decimal ResiduoDecimal { get; set; }
        public Decimal tipoCambioDoc { get; set; }
        public Decimal tipoCambioPago { get; set; }
        public Decimal Cuota1TCPago { get; set; }
        public Decimal MontoRedondeo { get; set; }
        public DateTime FechaPago { get; set; }
        public string TipoDocumento { get; set; }
        public string TipoOperacion { get; set; }
        public int FlujoEfectivo { get; set; }
        public string FlujoEfectivoDescripcion { get; set; }
        public string NumAtCard { get; set; }
        public double TotalPagar { get; set; }
        public int InstallmentId { get; set; }
        public int SeriePago { get; set; }
        public int CorrelativoRetencion { get; set; }
        public double MontoRetencion { get; set; }
        public string BankCode { get; set; }

        public string CodigoRetencion { get; set; }
        public string Fila { get; set; }
    }

    public class TXT3Retenedor
    {
        public string RUC { get; set; }
        public Decimal monto { get; set; }
    }

    public class TXT3RetenedorRsp
    {
        public string RUC { get; set; }
        public int TipoDocumento { get; set; }
        public int IdDocumento { get; set; }
        public string Estado { get; set; }
    }

    public class PagoDatosDTO
    {
        public string RUC { get; set; }
        public string Comentario { get; set; }
        public string Operacion { get; set; }
        public string FlujoEfectivoDescripcion { get; set; }
    }

    public class dtoDocTerceroRetenedor
    {
        public string Code { get; set; }
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string NumAtCard { get; set; }
        public string DocCur { get; set; }
        public double TotalPagar { get; set; }
    }
}
