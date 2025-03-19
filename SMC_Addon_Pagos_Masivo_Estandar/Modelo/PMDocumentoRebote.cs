using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.Modelo
{
    public class PMDocumentoRebote
    {
        public int DocEntry { get; set; }
        public int DocType { get; set; }
        public double Importe { get; set; }
        public int DocEntryPago { get; set; }
        public int DocEntryPago2 { get; set; }
        public int NroLineaPM { get; set; }
        public int NroEP { get; set; }
        public int NroLineaEP { get; set; }
    }
}
