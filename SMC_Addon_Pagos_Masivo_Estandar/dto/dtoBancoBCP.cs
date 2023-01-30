using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    public class dtoBancoBCP
    {
        private string _tipoRegistro;
        private string _cantidadAbonos;
        private string _fechaProceso;
        private string _tipoCuenta;
        private string _moneda;
        private string _cuentaCargo;
        private string _montototal;
        private string _referencia;
        private string _validacion;
        private string _cadena;
        private string _tipoCuentaCargo;
        private string _validarIDC;

        public dtoBancoBCP()
        {

        }

        public string TipoRegistro { get => _tipoRegistro; set => _tipoRegistro = value; }
        public string CantidadAbonos { get => _cantidadAbonos; set => _cantidadAbonos = value; }
        public string FechaProceso { get => _fechaProceso; set => _fechaProceso = value; }
        public string TipoCuenta { get => _tipoCuenta; set => _tipoCuenta = value; }
        public string CuentaCargo { get => _cuentaCargo; set => _cuentaCargo = value; }
        public string Montototal { get => _montototal; set => _montototal = value; }
        public string Referencia { get => _referencia; set => _referencia = value; }
        public string Validacion { get => _validacion; set => _validacion = value; }
        public string Cadena { get => _cadena; set => _cadena = value; }
        public string Moneda { get => _moneda; set => _moneda = value; }

        public string TipoCuentaCargo { get => _tipoCuentaCargo; set => _tipoCuentaCargo = value; }
        public string ValidarIDC { get => _validarIDC; set => _validarIDC = value; }
    }

    public class dtoBancoBCPDetalle
    {
        private string _tipoRegistro;
        private string _tipoCuenta;
        private string _cuentaAbono;
        private string _caracter;
        private string _tipoDocumentoIdentidad;
        private string _numeroDocumentoIdentidad;
        private string _numeroDocumentoIdentidad2;
        private string _nombreProveedor;
        private string _referenciaBeneficiario;
        private string _referencia;
        private string _moneda;
        private string _ImporteParcial;
        private string _Importetotal;
        private string _validacionIDC;
        private string _tipoDocumentoPagar;
        private string _numeroDocumento;

        private string _fechaVencimiento;
        private string _tipoPersona;
        private string _formaPago;
        private string _cuentaAbonoCCI;
        public dtoBancoBCPDetalle()
        {

        }

        public string TipoRegistro { get => _tipoRegistro; set => _tipoRegistro = value; }
        public string TipoCuenta { get => _tipoCuenta; set => _tipoCuenta = value; }
        public string CuentaAbono { get => _cuentaAbono; set => _cuentaAbono = value; }
        public string TipoDocumentoIdentidad { get => _tipoDocumentoIdentidad; set => _tipoDocumentoIdentidad = value; }
        public string NumeroDocumentoIdentidad { get => _numeroDocumentoIdentidad; set => _numeroDocumentoIdentidad = value; }
        public string NumeroDocumentoIdentidad2 { get => _numeroDocumentoIdentidad2; set => _numeroDocumentoIdentidad2 = value; }
        public string NombreProveedor { get => _nombreProveedor; set => _nombreProveedor = value; }
        public string ReferenciaBeneficiario { get => _referenciaBeneficiario; set => _referenciaBeneficiario = value; }
        public string Referencia { get => _referencia; set => _referencia = value; }
        public string ImporteParcial { get => _ImporteParcial; set => _ImporteParcial = value; }
        public string Importetotal { get => _Importetotal; set => _Importetotal = value; }
        public string ValidacionIDC { get => _validacionIDC; set => _validacionIDC = value; }
        public string TipoDocumentoPagar { get => _tipoDocumentoPagar; set => _tipoDocumentoPagar = value; }
        public string NumeroDocumento { get => _numeroDocumento; set => _numeroDocumento = value; }
        public string Caracter { get => _caracter; set => _caracter = value; }
        public string Moneda { get => _moneda; set => _moneda = value; }

        public string FechaVencimiento { get => _fechaVencimiento; set => _fechaVencimiento = value; }
        public string TipoPersona { get => _tipoPersona; set => _tipoPersona = value; }
        public string FormaPago { get => _formaPago; set => _formaPago = value; }
        public string CuentaAbonoCCI { get => _cuentaAbonoCCI; set => _cuentaAbonoCCI = value; }

    }
}
