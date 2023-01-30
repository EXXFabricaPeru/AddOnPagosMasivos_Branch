using System;
using System.Data.SqlClient;
using System.Data;

namespace SMC_APM.Conexion
{
    class conexSQL
    {
        #region Atributos
        private SqlConnection _conn;
        private String _server;
        private String _catalogo;
        private String _usuario;
        private String _contra;

        public string Server { get => _server; set => _server = value; }
        public string Catalogo { get => _catalogo; set => _catalogo = value; }
        public string Usuario { get => _usuario; set => _usuario = value; }
        public string Contra { get => _contra; set => _contra = value; }
        public SqlConnection Conn { get => _conn; set => _conn = value; }
        #endregion

        #region Constructor
        public conexSQL()
        {
            this.Server = @"10.45.1.124:30015";
            this.Catalogo = "";
            //this.Catalogo = "SBO_DEMO_1601";
            this.Usuario = "SAPINST";
            this.Contra = "[S@pinst22]";
            try
            {
                String conne = "Data Source = " + this.Server +
                                //"; Initial Catalog = " + this.Catalogo +
                                ";Persist Security Info=True; user ID = " + this.Usuario +
                                ";Password =" + this.Contra + ";Max Pool Size=1000;Connection Timeout=123600";

                Conn = new SqlConnection(conne);
                this.ConnectionOpen();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        #endregion

        #region Metodos Publicos
        public void ConnectionOpen()
        {
            try
            {
                if (Conn.State == ConnectionState.Closed)
                    Conn.Open();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }

        public void ConnectionClose()
        {
            try
            {
                if (Conn.State != ConnectionState.Closed) Conn.Close();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }

        public ConnectionState ConnectionEstate()
        {
            return Conn.State;
        }
        #endregion
    }
}
