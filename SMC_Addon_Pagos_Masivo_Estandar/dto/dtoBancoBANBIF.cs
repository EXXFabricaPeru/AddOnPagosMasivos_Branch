using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    class dtoBancoBANBIF
    {
    }

    public class dtoBancoBANBIFDetalle
    {
        private string _tipoDocumentoProveedor;
        private string _numeroDocumentoProveedor;
        private string _nombreProveedor;
        private string _tipoDocumentoPago;
        private string _numeroDocumentoPago;
        private string _monedaPago;
        private string _importe;
        private DateTime _fechaPago;
        private string _formaPago;
        private string _codigoBanco;
        private string _monedaCuenta;
        private string _numeroCuenta;
        private string _documentoAplicaionNotaCredito;

        public dtoBancoBANBIFDetalle()
        {

        }

        public string TipoDocumentoProveedor { get => _tipoDocumentoProveedor; set => _tipoDocumentoProveedor = value; }
        public string NumeroDocumentoProveedor { get => _numeroDocumentoProveedor; set => _numeroDocumentoProveedor = value; }
        public string NombreProveedor { get => _nombreProveedor; set => _nombreProveedor = value; }
        public string TipoDocumentoPago { get => _tipoDocumentoPago; set => _tipoDocumentoPago = value; }
        public string NumeroDocumentoPago { get => _numeroDocumentoPago; set => _numeroDocumentoPago = value; }
        public string MonedaPago { get => _monedaPago; set => _monedaPago = value; }
        public string Importe { get => _importe; set => _importe = value; }
        public DateTime FechaPago { get => _fechaPago; set => _fechaPago = value; }
        public string FormaPago { get => _formaPago; set => _formaPago = value; }
        public string CodigoBanco { get => _codigoBanco; set => _codigoBanco = value; }
        public string MonedaCuenta { get => _monedaCuenta; set => _monedaCuenta = value; }
        public string NumeroCuenta { get => _numeroCuenta; set => _numeroCuenta = value; }
        public string DocumentoAplicaionNotaCredito { get => _documentoAplicaionNotaCredito; set => _documentoAplicaionNotaCredito = value; }
    }
}
