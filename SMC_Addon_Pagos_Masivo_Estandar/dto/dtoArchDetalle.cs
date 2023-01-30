using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    public class dtoArchDetalle
    {
        private string tipoRegistro;
        private string docTipo;
        private string docNum;
        private string tipoAbono;
        private string numuenta;
        private string nombreBeneficiario;
        private string importe;
        private string tipoCompro;
        private string numDocumento;
        private string abonoAgru;
        private string referencia;
        private string aviso;
        private string medio;
        private string contacto;
        private string proceso;
        private string descripcion;
        private string filler;

        public string TipoRegistro { get => tipoRegistro; set => tipoRegistro = value; }
        public string DocTipo { get => docTipo; set => docTipo = value; }
        public string DocNum { get => docNum; set => docNum = value; }
        public string TipoAbono { get => tipoAbono; set => tipoAbono = value; }
        public string Numuenta { get => numuenta; set => numuenta = value; }
        public string NombreBeneficiario { get => nombreBeneficiario; set => nombreBeneficiario = value; }
        public string Importe { get => importe; set => importe = value; }
        public string TipoCompro { get => tipoCompro; set => tipoCompro = value; }
        public string NumDocumento { get => numDocumento; set => numDocumento = value; }
        public string AbonoAgru { get => abonoAgru; set => abonoAgru = value; }
        public string Referencia { get => referencia; set => referencia = value; }
        public string Aviso { get => aviso; set => aviso = value; }
        public string Medio { get => medio; set => medio = value; }
        public string Contacto { get => contacto; set => contacto = value; }
        public string Proceso { get => proceso; set => proceso = value; }
        public string Descripcion { get => descripcion; set => descripcion = value; }
        public string Filler { get => filler; set => filler = value; }
    }
}
