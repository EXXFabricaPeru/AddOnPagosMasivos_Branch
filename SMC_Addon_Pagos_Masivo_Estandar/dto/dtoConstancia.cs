using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    class dtoConstancia
    {
        private string _numConstancia;
        private string _rucProvedor;
        private string _numComprobante;

        public string numConstancia { get => _numConstancia; set => _numConstancia = value; }
        public string rucProvedor { get => _rucProvedor; set => _rucProvedor = value; }
        public string numComprobante { get => _numComprobante; set => _numComprobante = value; }
    }
}
