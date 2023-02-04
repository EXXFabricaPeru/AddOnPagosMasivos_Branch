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
using System.Diagnostics;
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
                        string mensajeError = modelo.Filas.Where(x => x.Proveedor == proveedor).Max(y => y.MensajeError);

                        matrix.GetCellSpecific("Col_14", i).Value = docEntryPagoRetencion.ToString();
                        matrix.GetCellSpecific("Col_25", i).Value = mensajeError;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public double GetTipoCambio(DateTime fecha)
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
                    else
                    {
                        if (string.IsNullOrEmpty(idPagoRet))
                        {
                            matrix.CommonSetting.SetCellEditable(i, 21, true);
                        }
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

        internal void GenerarTXTBancos(string codBanco, string GLCuentaBanco, int docEntry, string codMoneda)
        {
            try
            {
                var nombre = @"C:\PagosMasivos\";
                nombre = nombre + "ArchivoBancoLiberacion-" + codBanco + "-" + codMoneda + "-" + DateTime.Now.ToString("dd_MM_yyyyThh-mm") + ".txt";

                switch (codBanco)
                {
                    //PODRÍA SER UNA INTERFAZ
                    case "002": GenerarTXT_BCP(nombre, docEntry, GLCuentaBanco); break;
                    case "003": GenerarTXT_Interbank(nombre, docEntry, GLCuentaBanco); break;
                    case "009": GenerarTXT_Scotiabank(nombre, docEntry, GLCuentaBanco); break;
                    case "022": GenerarTXT_Santander(nombre, docEntry, GLCuentaBanco); break;

                    default:
                        throw new Exception($"Código de banco {codBanco} no soportado");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GenerarTXT_Santander(string nombre, int docEntry, string GLCuentaBanco)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_SANTANDER_RETENCION({docEntry},'{GLCuentaBanco}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private void GenerarTXT_Scotiabank(string nombre, int docEntry, string GLCuentaBanco)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_SCOTIABANK_RETENCION({docEntry},'{GLCuentaBanco}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private void GenerarTXT_Interbank(string nombre, int docEntry, string GLCuentaBanco)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_INTERBANK_RETENCION({docEntry},'{GLCuentaBanco}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }

        private void GenerarTXT_BCP(string nombre, int docEntry, string GLCuentaBanco)
        {
            StreamWriter archivo = new StreamWriter(nombre, false, Encoding.GetEncoding(1252));

            Recordset recordset = Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"CALL SBO_EXX_PM_BCP_RETENCION({docEntry},'{GLCuentaBanco}')";

            recordset.DoQuery(query);

            while (!recordset.EoF)
            {
                archivo.WriteLine(recordset.Fields.Item(0).Value);
                recordset.MoveNext();
            }

            archivo.Close();
            archivo.Dispose();

            Process.Start(nombre);
        }
    }
}