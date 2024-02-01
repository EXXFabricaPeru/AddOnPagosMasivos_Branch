using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Modelo
{
    class EPDocumento
    {
        public int CodSucursal { get; set; }
        public int FILA { get; set; }
        public int DocEntry { get; set; }
        public string DocNum { get; set; }
        public string FechaContable { get; set; }
        public string FechaVencimiento { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string NumAtCard { get; set; }
        public string CodBancoPago { get; set; }
        public string MonedaPago { get; set; }
        public string DocCur { get; set; }
        public double Total { get; set; }
        public string CodigoRetencion { get; set; }
        public double Retencion { get; set; }
        public double TotalPagar { get; set; }
        public string RUC { get; set; }
        public string Cuenta { get; set; }
        public string CuentaMoneda { get; set; }
        public string BankCode { get; set; }
        public string Atraso { get; set; }
        public string Marca { get; set; }
        public string Estado { get; set; }
        public string Documento { get; set; }
        public string NombreBanco { get; set; }
        public string Origen { get; set; }
        public string BloqueoPago { get; set; }
        public string DetraccionPend { get; set; }
        public int NroCuota { get; set; }
        public int LineaAsiento { get; set; }
        public string GlosaAsiento { get; set; }
        public string CardCodeFactoring { get; set; }
        public string CardNameFactoring { get; set; }
        public string EstadoExt { get; set; }
        public string NroCtaPago { get; set; }
        public string CodCtaPago { get; set; }
        public string CodPrioridad { get; set; }
    }
}
