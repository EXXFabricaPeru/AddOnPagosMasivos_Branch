using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    class dtoReconciliacion
    {
        private string _cardcode;
        private string _pago;
        private string _asiento;
        private string _numFactura;

        public string cardcode { get => _cardcode; set => _cardcode = value; }
        public string pago { get => _pago; set => _pago = value; }
        public string asiento { get => _asiento; set => _asiento = value; }
        public string numFactura { get => _numFactura; set => _numFactura = value; }
    }
}
