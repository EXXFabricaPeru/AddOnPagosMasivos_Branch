using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    class dtoBanco
    {
        private string _CuentaCont;
        private string _AcctName;
        private string _BankCode;
        private string _BankName;
        private string _CuentaBank;
        private string _Moneda;
        private decimal _Saldo;
        private DateTime fechaPago;
        public string Estado { get; set; }
        public string MedioPago { get; set; }

        public string CuentaCont { get => _CuentaCont; set => _CuentaCont = value; }
        public string AcctName { get => _AcctName; set => _AcctName = value; }
        public string BankCode { get => _BankCode; set => _BankCode = value; }
        public string BankName { get => _BankName; set => _BankName = value; }
        public string CuentaBank { get => _CuentaBank; set => _CuentaBank = value; }
        public string Moneda { get => _Moneda; set => _Moneda = value; }
        public decimal Saldo { get => _Saldo; set => _Saldo = value; }
        public DateTime FechaPago { get => fechaPago; set => fechaPago = value; }
    }

    public class dtoSeriePago
    {
        private string _Series;
        private string _SeriesName;
        private string _SeriesDefecto;

        public string Series { get => _Series; set => _Series = value; }
        public string SeriesName { get => _SeriesName; set => _SeriesName = value; }
        public string SeriesDefecto { get => _SeriesDefecto; set => _SeriesDefecto = value; }
    }
}
