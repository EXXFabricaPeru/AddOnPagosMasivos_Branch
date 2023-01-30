using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Modelo
{
    public class PMPDocumento
    {
        public string SlcPago { get; set; }
        public string SlcRetencion { get; set; }
        public string CodigoEscenarioPago { get; set; }
        public string DescripcionEscenarioPago { get; set; }
        public string MedioDePago { get; set; }
        public string CodBanco { get; set; }
        public string CodCtaBanco { get; set; }
        public int DocEntryDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Moneda { get; set; }
        public double Importe { get; set; }
        public string NroCuota { get; set; }
        public string NroLineaAsiento { get; set; }
        public string NroDocumentoSN { get; set; }
        public string AplSreRetencion { get; set; }
    }
}



