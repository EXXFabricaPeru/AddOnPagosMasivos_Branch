using Microsoft.VisualBasic.FileIO;
using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbobsCOM;
using SAPbouiCOM;
using SMC_APM.dao;
using SMC_APM.dto;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace SMC_APM.Controller
{
    public class LiberarTerceroController
    {
        internal void CargarComboSeries(ComboBox comboBox, string UDO_CODE, string modo)
        {
            try
            {
                comboBox.LimpiarValoresValidos();
                comboBox.ValidValues.LoadSeries(UDO_CODE, modo == "C" ? BoSeriesMode.sf_Add : BoSeriesMode.sf_View);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void SeleccionarSeriePorDefecto(ComboBox combo, string UDO_CODE)
        {
            string seriePorDefecto = GetSeriePorDefecto(UDO_CODE);

            if (seriePorDefecto != "0")
                combo.Select(seriePorDefecto, BoSearchKey.psk_ByValue);
            else
                combo.SelectExclusive(0, BoSearchKey.psk_Index);
        }

        private string GetSeriePorDefecto(string UDO_CODE)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $"select \"DfltSeries\" from ONNM WHERE \"ObjectCode\" = '{UDO_CODE}'";
                recordset.DoQuery(query);

                if (recordset.RecordCount > 0)
                    return recordset.Fields.Item("DfltSeries").Value.ToString();
                else
                    return "0";
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal bool ExisteRegistro(string docEntryPM)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $"select COUNT(\"DocEntry\") as \"Existe\" from \"@EXD_OTRR\" WHERE \"U_EXD_NRPM\" = '{docEntryPM}'";
                recordset.DoQuery(query);

                if (recordset.RecordCount > 0)
                    return recordset.Fields.Item("Existe").Value > 0;
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal int GetDocEntryFromPM(string docEntryPM)
        {
            try
            {
                Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $"select \"DocEntry\"  from \"@EXD_OTRR\" WHERE \"U_EXD_NRPM\" = '{docEntryPM}'";
                recordset.DoQuery(query);

                if (recordset.RecordCount > 0)
                    return recordset.Fields.Item("DocEntry").Value;
                else
                    throw new Exception($"No existe un registro con el número de Pago Masivo {docEntryPM}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CargarComboBancos(ComboBox combo)
        {
            daoBanco daoBanco = new daoBanco();
            List<dtoBanco> lstBancos = new List<dtoBanco>();
            string mensaje = string.Empty;
            lstBancos = daoBanco.listarBancos_Custom(); // consulta cuentas de los bancos

            combo.LimpiarValoresValidos();

            for (int i = 0; i < lstBancos.Count; i++)
            {
                combo.ValidValues.Add(lstBancos[i].CuentaCont, lstBancos[i].AcctName);
            }

            if (combo.ValidValues.Count > 0)
                combo.Select(0, BoSearchKey.psk_Index);
        }

        internal void RegistrarInformacionProceso(Matrix matrix, LiberarTerceroModelView modelo)
        {
            try
            {
                for (int i = 1; i <= matrix.VisualRowCount; i++)
                {
                    CheckBox chk = matrix.GetCellSpecific("Col_0", i);
                    string idPago = matrix.GetCellSpecific("Col_14", i).Value;
                    string proveedor = matrix.GetCellSpecific("Col_4", i).Value;
                    double montoRentencion = Convert.ToDouble(matrix.GetCellSpecific("Col_12", i).Value);

                    if (chk.Checked && string.IsNullOrEmpty(idPago) && montoRentencion > 0)
                    {
                        int docEntryPagoRetencion = modelo.Filas.Where(x => x.Proveedor == proveedor).Max(y => y.DocEntryPagoRet);
                        matrix.GetCellSpecific("Col_14", i).Value = docEntryPagoRetencion.ToString();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        internal double GetTipoCambio(DateTime fecha)
        {
            double tc = 0.0;

            try
            {
                SBObob bob = Globales.Company.GetBusinessObject(BoObjectTypes.BoBridge);
                Recordset rs = bob.GetCurrencyRate("USD", fecha);

                if (rs.RecordCount == 0)
                    throw new Exception($"No se ha encontrado tipo de cambio para la fecha {fecha.ToString("dd-MM-yyyy")}");

                tc = rs.Fields.Item(0).Value;

                return tc;
            }
            catch (Exception)
            {
                Globales.Aplication.MessageBox($"No se ha encontrado tipo de cambio para la fecha {fecha.ToString("dd-MM-yyyy")}");
                return tc;
                //Globales.Aplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }

        }

        internal void BloquearLineaConPago(Form form, Matrix matrix)
        {
            try
            {
                form.Freeze(true);

                for (int i = 1; i <= matrix.VisualRowCount; i++)
                {
                    string idPagoRet = matrix.GetCellSpecific("Col_14", i).Value;
                    string idPagoProv = matrix.GetCellSpecific("Col_15", i).Value;

                    if (!string.IsNullOrEmpty(idPagoRet) && !string.IsNullOrEmpty(idPagoProv))
                    {
                        matrix.CommonSetting.SetRowEditable(i, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally { form.Freeze(false); }
        }

        internal List<DatosSUNATRetencion> GetRespuetaSUNAT(string fileName)
        {
            List<DatosSUNATRetencion> rptaSUNAT = new List<DatosSUNATRetencion>();

            try
            {
                FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read);

                TextFieldParser parser = new TextFieldParser(fileStream)
                {
                    Delimiters = new[] { "," },
                    HasFieldsEnclosedInQuotes = true,
                    TextFieldType = FieldType.Delimited,
                    TrimWhiteSpace = true
                };

                int fila = 0;
                string[] campos;

                while (!parser.EndOfData)
                {
                    campos = parser.ReadFields() ?? throw new InvalidOperationException("formato de archivo incorrecto");

                    if (fila > 0) //SE IGNORA LA FILA DE CABECERA
                    {
                        try
                        {
                            rptaSUNAT.Add(new DatosSUNATRetencion()
                            {
                                NroResolucion = campos[0],
                                RUC = campos[1],
                                CodigoDep = campos[2],
                                DescDep = campos[3],
                                RazonSocial = campos[4],
                                MontoEmbargo = Convert.ToDouble(campos[5])
                            });
                        }
                        catch (Exception)
                        {
                            throw new Exception("El formato del archivo seleccionado no es válido");
                        }
                    }

                    fila++;
                }

                parser.Close();
                parser.Dispose();

                return rptaSUNAT;
            }
            catch (Exception ex)
            {
                Globales.Aplication.MessageBox(ex.Message);
                return null;
            }
        }

        internal void GenerarTXTBancos(int docEntry, string codBanco, string codMoneda)
        {
            try
            {
                var nombre = @"C:\PagosMasivos\";
                nombre = nombre + "ArchivoBancoLiberacion-" + codBanco + "-" + codMoneda + "-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";
                var archivo = new System.IO.StreamWriter(nombre, false, Encoding.GetEncoding(1252));

                switch (codBanco)
                {
                    case "002":

                        dtoBancoBCP _dtoBancoBCP = new dtoBancoBCP();
                        List<dtoBancoBCPDetalle> _listDetalleBCP = new List<dtoBancoBCPDetalle>();

                        string LineaCabeceraBCP = "";
                        string LineaDetalleBCP = "";


                        _dtoBancoBCP = ObtenerDatosCabArchivoTXTBCP_Retencion(docEntry, codMoneda).FirstOrDefault();
                        _listDetalleBCP = ObtenerDatosDetArchivoTXTBCP(docEntry).ToList();

                        LineaCabeceraBCP = "#1P" + _dtoBancoBCP.TipoCuenta + _dtoBancoBCP.CuentaCargo +
                            _dtoBancoBCP.Moneda +
                            (_dtoBancoBCP.Montototal).Replace(".", "") +
                            _dtoBancoBCP.FechaProceso +
                            "                    " + _dtoBancoBCP.Cadena + (_listDetalleBCP.Count()).ToString("D6") +
                            "                0";


                        archivo.WriteLine(LineaCabeceraBCP);


                        for (int i = 0; i < _listDetalleBCP.Count; i++)
                        {
                            LineaDetalleBCP = " " + _listDetalleBCP[i].TipoRegistro +
                                    _listDetalleBCP[i].TipoCuenta +
                                    _listDetalleBCP[i].CuentaAbono +

                                    _listDetalleBCP[i].NombreProveedor +

                                    _listDetalleBCP[i].Moneda +
                                    (_listDetalleBCP[i].Importetotal).Replace(".", "") +
                                    _listDetalleBCP[i].TipoDocumentoIdentidad +
                                    _listDetalleBCP[i].NumeroDocumentoIdentidad +
                                    _listDetalleBCP[i].TipoDocumentoPagar +
                                    _listDetalleBCP[i].NumeroDocumento +
                                    "00" + _listDetalleBCP[i].ValidacionIDC;

                            archivo.WriteLine(LineaDetalleBCP);
                        }

                        archivo.Close();
                        break;

                    case "003":

                        dtoBancoBCP _dtoBanco = new dtoBancoBCP();
                        List<dtoBancoBCPDetalle> _listDetalle = new List<dtoBancoBCPDetalle>();

                        string LineaCabecera = "";
                        string LineaDetalle = "";


                        _listDetalle = ObtenerDatosDetArchivoTXTInterBank(docEntry).ToList();


                        /*Detalles*/

                        for (int i = 0; i < _listDetalle.Count; i++)
                        {
                            string tipo = "";
                            if (_listDetalle[i].TipoCuenta == "I") //cuenta interbancara
                            {
                                tipo = "109";
                                LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2 +
                                            _listDetalle[i].TipoDocumentoPagar +
                                            _listDetalle[i].NumeroDocumento +
                                            "          " + _listDetalle[i].FechaVencimiento +
                                            _listDetalle[i].Moneda +
                                            (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                            " " + "99" +
                                            "   " + //espaco de tipo de cuenta
                                            "  " + //espacio de moneda cuando es interbancario
                                            "   " + //espacio de ofinia a la que pertenece
                                            _listDetalle[i].CuentaAbono +
                                            _listDetalle[i].TipoPersona + _listDetalle[i].TipoDocumentoIdentidad +
                                            _listDetalle[i].NumeroDocumentoIdentidad +
                                            _listDetalle[i].NombreProveedor + "000000000000000";
                                ;
                            }
                            else  //cuenta normla
                            {
                                tipo = "009";
                                LineaDetalle = "02" + _listDetalle[i].NumeroDocumentoIdentidad2 +
                                            _listDetalle[i].TipoDocumentoPagar +
                                            _listDetalle[i].NumeroDocumento +
                                            "          " + _listDetalle[i].FechaVencimiento +
                                            _listDetalle[i].Moneda +
                                            (_listDetalle[i].ImporteParcial).Replace(".", "") +
                                            " " + "09" +
                                            "001" +
                                            _listDetalle[i].Moneda + "200" +
                                            _listDetalle[i].CuentaAbono +
                                            _listDetalle[i].TipoPersona + _listDetalle[i].TipoDocumentoIdentidad + _listDetalle[i].NumeroDocumentoIdentidad +
                                            _listDetalle[i].NombreProveedor + "000000000000000";
                            }
                            archivo.WriteLine(LineaDetalle);
                        }
                        archivo.Close();

                        break;

                    case "009":



                        dtoBancoBCP _dtoBancoSC = new dtoBancoBCP();
                        List<dtoBancoBCPDetalle> _listDetalleSC = new List<dtoBancoBCPDetalle>();

                        string LineaCabeceraSC = "";
                        string LineaDetalleSC = "";

                        //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                        _listDetalleSC = ObtenerDatosDetArchivoTXTScotiaBank(docEntry).ToList();


                        for (int i = 0; i < _listDetalleSC.Count; i++)
                        {
                            string FormaPago = "";
                            if (_listDetalleSC[i].TipoCuenta == "I")
                            {
                                FormaPago = "4";

                                LineaDetalleSC = _listDetalleSC[i].NumeroDocumentoIdentidad +
                                            _listDetalleSC[i].NombreProveedor +
                                            _listDetalleSC[i].NumeroDocumento +
                                            _listDetalleSC[i].FechaVencimiento +
                                            (_listDetalleSC[i].Importetotal).Replace(".", "") +
                                            FormaPago +
                                           "                                                             "
                                           + _listDetalleSC[i].CuentaAbonoCCI + _listDetalleSC[i].Moneda + "01";

                            }
                            else //nomral
                            {
                                FormaPago = "2";

                                LineaDetalleSC = _listDetalleSC[i].NumeroDocumentoIdentidad +
                                           _listDetalleSC[i].NombreProveedor +
                                           _listDetalleSC[i].NumeroDocumento +
                                           _listDetalleSC[i].FechaVencimiento +
                                           (_listDetalleSC[i].Importetotal).Replace(".", "") +
                                           FormaPago +
                                           _listDetalleSC[i].CuentaAbono + "                                                                       " + _listDetalleSC[i].Moneda + "01";
                            }



                            archivo.WriteLine(LineaDetalleSC);


                        }
                        archivo.Close();



                        break;
                    case "022":


                        dtoBancoBCP _dtoBancoSA = new dtoBancoBCP();
                        List<dtoBancoBCPDetalle> _listDetalleSA = new List<dtoBancoBCPDetalle>();

                        string LineaCabeceraSA = "";
                        string LineaDetalleSA = "";

                        //_dtoBanco = _daoBCP.getBCP(escenario[1], sboApplication, ref mensajeErr);
                        _listDetalleSA = ObtenerDatosDetArchivoTXTSantander(docEntry).ToList();


                        for (int i = 0; i < _listDetalleSA.Count; i++)
                        {
                            string FormaPago = "";
                            if (_listDetalleSA[i].TipoCuenta == "I")
                            {
                                FormaPago = "03";

                                LineaDetalleSA = _listDetalleSA[i].TipoDocumentoIdentidad + _listDetalleSA[i].NumeroDocumentoIdentidad +
                                       _listDetalleSA[i].TipoDocumentoPagar +
                                       _listDetalleSA[i].NumeroDocumento +
                                       _listDetalleSA[i].Moneda +
                                        _listDetalleSA[i].ImporteParcial +
                                       _listDetalleSA[i].FechaVencimiento + "Confirming" +
                                       "                    " + FormaPago + "          " + _listDetalleSA[i].CuentaAbonoCCI +
                                       _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor +
                                        "CONFIRMING";

                            }
                            else
                            { //normal

                                FormaPago = "01";

                                LineaDetalleSA = _listDetalleSA[i].TipoDocumentoIdentidad + _listDetalleSA[i].NumeroDocumentoIdentidad +
                                       _listDetalleSA[i].TipoDocumentoPagar +
                                       _listDetalleSA[i].NumeroDocumento +
                                       _listDetalleSA[i].Moneda +
                                        _listDetalleSA[i].ImporteParcial +
                                       _listDetalleSA[i].FechaVencimiento + "Confirming" +
                                       "                    " + FormaPago + _listDetalleSA[i].CuentaAbono +
                                        _listDetalleSA[i].TipoPersona + _listDetalleSA[i].NombreProveedor +
                                        "CONFIRMING";

                            }
                            archivo.WriteLine(LineaDetalleSA);
                        }
                        archivo.Close();
                        break;

                }


            }
            catch (Exception)
            {
                throw;
            }
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTInterBank(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_INTERBANK_DETALLE_NUEVO_RETENCION('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad2 = dc["NumeroDocumentoIdentidad1"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    TipoPersona = dc["TipoPersona"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTScotiaBank(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_SCOTIABANK_DETALLE_RETENCION('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {

                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    FormaPago = dc["FormaPago"].ToString(),
                    CuentaAbonoCCI = dc["CuentaAbonoCCI"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTSantander(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_SANTANDER_DETALLE_RETENCION('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {

                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidacionIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString(),
                    FechaVencimiento = dc["FechaVencimiento"].ToString(),
                    TipoPersona = dc["TipoPersona"].ToString(),
                    CuentaAbonoCCI = dc["CuentaAbonoCCI"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCPDetalle> ObtenerDatosDetArchivoTXTBCP(int docEntry)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_BCP_DETALLE_NUEVO_RETENCION('{docEntry}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCPDetalle
                {

                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaAbono = dc["CuentaAbono"].ToString(),
                    TipoDocumentoIdentidad = dc["TipoDocumentoIdentidad"].ToString(),
                    NumeroDocumentoIdentidad = dc["NumeroDocumentoIdentidad"].ToString(),
                    NombreProveedor = dc["NombreProveedor"].ToString(),
                    ReferenciaBeneficiario = dc["ReferenciaBeneficiario"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    ImporteParcial = dc["ImporteParcial"].ToString(),
                    Importetotal = dc["Importetotal"].ToString(),
                    ValidacionIDC = dc["ValidarIDC"].ToString(),
                    TipoDocumentoPagar = dc["TipoDocumentoPagar"].ToString(),
                    NumeroDocumento = dc["NumeroDocumento"].ToString(),
                    Caracter = dc["Caracter"].ToString(),
                    Moneda = dc["Moneda"].ToString()
                };
            });
            return rslt;
        }

        private static IEnumerable<dtoBancoBCP> ObtenerDatosCabArchivoTXTBCP_Retencion(int docEntry, string moneda)
        {
            var recordset = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            var sqlQry = $"CALL SMC_APM_ARCHIVOBANCO_BCP_CABECERA_NUEVO_RETENCION({docEntry}, '{moneda}')";
            var rslt = QueryResultManager.executeQueryAsType(sqlQry, dc =>
            {
                return new dtoBancoBCP
                {
                    TipoRegistro = dc["TipoRegistro"].ToString(),
                    CantidadAbonos = dc["CantidadAbonos"].ToString(),
                    FechaProceso = dc["FechaProceso"].ToString(),
                    TipoCuenta = dc["TipoCuenta"].ToString(),
                    CuentaCargo = dc["CuentaCargo"].ToString(),
                    Montototal = dc["Montototal"].ToString(),
                    Referencia = dc["Referencia"].ToString(),
                    Validacion = dc["Validacion"].ToString(),
                    Cadena = dc["Cadena"].ToString(),
                    Moneda = dc["Moneda"].ToString()
                };
            });
            return rslt;
        }
    }
}