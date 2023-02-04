using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbouiCOM;
using SMC_APM.Controller;
using SMC_APM.Modelo;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SMC_APM.View.USRForms
{
    public class FormLiberarTercero : IUSAP
    {
        public const string TYPE = "FrmLPG";
        public const string UNQID = "FrmLPG";
        public const string MENU = "MNUID_TRET";

        //public const string PATH = "Resources/frmSMC_PM_LiberarTerceroRet.srf";
        private readonly string HEADER = "@EXD_OTRR";

        private readonly string DETAIL = "@EXD_TRR1";
        private readonly string UDO_CODE = "EXD_OTRR";
        //public bool PagosRetenciones { get; set; }
        //public bool PagosProveedores { get; set; }
        public string FechaPM { get; set; }
        public string DocEntryPM { get; set; }
        public LiberarTerceroController Controller { get; set; }
        public LiberarTerceroModelView Modelo { get; set; }

        public FormLiberarTercero(string id, string path, string fechaPM, string docEntry) : base(TYPE, MENU, id, path)
        {
            //Modelo = new LiberarTerceroModelView();
            DocEntryPM = docEntry;
            FechaPM = fechaPM;
            Controller = new LiberarTerceroController();
            FormLoadAdd(true);
        }

        private void FormLoadAdd(bool isExternal = false)
        {
            try
            {
                Form.Freeze(true);
                Modelo = new LiberarTerceroModelView(Form);
                Matrix = Form.GetMatrix("Item_18");

                CargarDatosPorDefecto();
                ConfigurarMatrix();
                Automanage();

                if (!Controller.ExisteRegistro(DocEntryPM) || !isExternal)
                {
                    ConsultarPagosRetenidos(DocEntryPM);
                }
                else
                {
                    int docEntry = Controller.GetDocEntryFromPM(DocEntryPM);
                    
                    //int docEntry = Controller.GetDocEntryFromPM(DocEntryPM);

                    Conditions conditions = new Conditions();
                    Condition cnd = conditions.Add();
                    cnd.Alias = "DocEntry";

                    cnd.Operation = BoConditionOperation.co_EQUAL;
                    cnd.CondVal = docEntry.ToString();

                    Form.GetDBDataSource(HEADER).Query(conditions);
                    Form.GetDBDataSource(DETAIL).Query(conditions);

                    ConsultarLiberacionSUNAT(docEntry);
                    ConfigurarMatrix();

                    Matrix.LoadFromDataSource();
                    

                    Form.Mode = BoFormMode.fm_OK_MODE;
                    DataLoad();

                    Matrix.AutoResizeColumns();
                }

                Form.Visible = true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Form.Freeze(false);
            }
        }

        private void ConsultarLiberacionSUNAT(int docEntry)
        {
            //CUANDO YA FUE CREADO CONSULTAMOS LA TABLA DEL UDO
            Modelo.GetDatosLiberacionTerceroRetendor_Creado(docEntry);
            string filepath = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_ARRE");
            Modelo.RptaSUNAT = Controller.GetRespuetaSUNAT(filepath);
        }

        private void Automanage()
        {
            SetAutomanageAttribute("Item_5", BoAutoManagedAttr.ama_Editable, new bool[4] { false, true, true, false }); //FECHA DE PAGO
            SetAutomanageAttribute("Item_14", BoAutoManagedAttr.ama_Editable, new bool[4] { false, true, true, false }); //BANCO
            //SetAutomanageAttribute("Item_16", BoAutoManagedAttr.ama_Editable, new bool[4] { false, true, false, false }); //RESPUESTA SUNAT
            SetAutomanageAttribute("Item_17", BoAutoManagedAttr.ama_Visible, new bool[4] { false, true, false, false }); //BOTÓN RESPUESTA
            SetAutomanageAttribute("Item_7", BoAutoManagedAttr.ama_Editable, new bool[4] { false, false, true, false }); //COMBO SERIE
            SetAutomanageAttribute("Item_8", BoAutoManagedAttr.ama_Editable, new bool[4] { false, false, true, false }); //DOCNUM
            SetAutomanageAttribute("Item_8", BoAutoManagedAttr.ama_Editable, new bool[4] { false, false, true, false }); //fecha registro
            SetAutomanageAttribute("Item_4", BoAutoManagedAttr.ama_Editable, new bool[4] { false, false, true, false }); //fecha PM
            SetAutomanageAttribute("Item_5", BoAutoManagedAttr.ama_Editable, new bool[4] { false, true, true, false }); //fecha PAGO

            SetAutomanageAttribute("Item_12", BoAutoManagedAttr.ama_Editable, new bool[4] { false, false, true, false }); //# pm
        }

        private void SetAutomanageAttribute(string itemId, BoAutoManagedAttr attribute, bool[] valores)
        {
            //1 = OK
            //2 = CREACION
            //4 = BUSQUEDA
            //8 = VISTA

            for (int i = 0; i < 4; i++)
            {
                Form.Items.Item(itemId).SetAutoManagedAttribute(attribute, Convert.ToInt32(Math.Pow(2, i)), valores[i] == true ? BoModeVisualBehavior.mvb_True : BoModeVisualBehavior.mvb_False);
            }
        }

        internal void CrearPagos()
        {
            //YA SE HIZO EL BINDING
            try
            {
                Modelo.BindData();

                Globales.Aplication.StatusBar.SetText("Creando pagos, espere por favor...", BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Warning);

                Form.Freeze(true);

                Modelo.ValidarDatos();
                //Modelo = new LiberarTerceroModelView(Form);

                //Modelo.BindData(); //SE HACE UN BINDING INTERNO
                Modelo.CrearPagos();

                Controller.RegistrarInformacionProceso(Matrix, Modelo);
                Matrix.FlushToDataSource();
                Matrix.LoadFromDataSource();
            }
            catch (Exception)
            {
                throw;
            }
            finally { Form.Freeze(false); }
        }

        private void ConsultarPagosRetenidos(string docEntryPM)
        {
            try
            {
                string fecha = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_FEPM");
                string tipoCambio = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_TIPC");

                if (string.IsNullOrEmpty(fecha))
                    throw new Exception("Debe ingresar la fecha");

                if (string.IsNullOrEmpty(tipoCambio) || tipoCambio == "0.0")
                    throw new Exception("Debe ingresar un tipo valor de tipo de cambio válido");

                Globales.Aplication.StatusBar.SetText("Consultando pagos retenidos, espere por favor...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                Modelo.GetDatosLiberacionTerceroRetendor(docEntryPM, fecha);
                Form.GetDBDataSource(DETAIL).LoadFromXML(Modelo.FilasAsXML);

                Matrix.LoadFromDataSource();

                ConfigurarMatrix();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ConfigurarMatrix()
        {
            Matrix.Columns.Item("Col_22").Visible = false;

            Matrix.Columns.Item("Col_14").Visible = !(Form.Mode == BoFormMode.fm_ADD_MODE);
            Matrix.Columns.Item("Col_15").Visible = !(Form.Mode == BoFormMode.fm_ADD_MODE);

            Matrix.Columns.Item("Col_17").ColumnSetting.SumType = BoColumnSumType.bst_Auto;
            Matrix.Columns.Item("Col_12").ColumnSetting.SumType = BoColumnSumType.bst_Auto;
            Matrix.Columns.Item("Col_13").ColumnSetting.SumType = BoColumnSumType.bst_Auto;

            
            Matrix.AutoResizeColumns();
        }

        private void CargarDatosPorDefecto()
        {
            Form.EnableMenu("1282", false);
            Combo = Form.GetComboBox("Item_7"); //  COMBO SERIES
            Combo.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
            Controller.CargarComboSeries(Combo, UDO_CODE, "C");

            Controller.SeleccionarSeriePorDefecto(Combo, UDO_CODE);

            DateTime fechaActual = Globales.Company.GetDBServerDate();

            Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_NRPM", DocEntryPM);
            Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_FEPM", FechaPM);
            Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_FREG", fechaActual.ToString("yyyyMMdd"));
            Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_FPAG", fechaActual.ToString("yyyyMMdd"));
            Form.GetDBDataSource(HEADER).SetValueExt("DocNum", Form.BusinessObject.GetNextSerialNumber(Combo.Value, UDO_CODE).ToString());

            Form.GetUserDataSource("UD_PR").Value = "Y";
            Form.GetUserDataSource("UD_PP").Value = "Y";

            //BANCOS
            Combo = Form.GetComboBox("Item_14");
            Combo.ExpandType = BoExpandType.et_DescriptionOnly;
            Controller.CargarComboBancos(Combo);

            //TIPO DE CAMBIO
            Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_TIPC", Controller.GetTipoCambio(fechaActual).ToString());
        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(BoEventTypes.et_VALIDATE, "Item_5", e => ActualizarTipoCambio(e)));
            //Eventos.Add(new EventoItem(BoEventTypes.et_VALIDATE, "Item_10", e => ValidateTipoCambio(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_DOUBLE_CLICK, "Item_18", e => { if (e.BeforeAction && e.Row > 0) return false;  return true; })); //DESHABILITAMOS PARA NO PERMITIR LOS CLICKS RAPIDOS SE CONFUNDAN CON DOBLE CLICK
            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "Item_18", e => SeleccionarDocumento(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "Item_19", e => GenerarTXT(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "1", e => BotonCrear(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "Item_17", e => SeleccionarArchivoSUNAT(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_VALIDATE, "Item_18", e => ValidarMontos(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_MATRIX_LINK_PRESSED, "Item_18", e => LinkButtonAction(e)));
            Eventos.Add(new EventoItem(BoEventTypes.et_DOUBLE_CLICK, "Item_16", e => AbrirRespuestaSUNAT(e)));

            Eventos.Add(new EventoData(BoEventTypes.et_FORM_DATA_LOAD, TYPE, e => DataLoad(e)));
        }

        private bool AbrirRespuestaSUNAT(ItemEvent e)
        {
            if (!e.BeforeAction)
            {
                string rutaArchivo = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_ARRE");
                if (!string.IsNullOrEmpty(rutaArchivo))
                    Process.Start(rutaArchivo);
            }

            return true;
        }

        private bool LinkButtonAction(ItemEvent e)
        {
            if (e.BeforeAction && e.ColUID == "Col_5")
            {
                string objType = Matrix.GetCellSpecific("Col_6", e.Row).Value;
                LinkedButton link = Matrix.Columns.Item("Col_5").ExtendedObject;
                link.LinkedObjectType = objType;
            }

            return true;
        }

        private bool ValidarMontos(ItemEvent e)
        {
            if (e.BeforeAction && e.Row > 0  && e.ItemChanged)
            {
                if (e.ColUID == "Col_12")
                {
                    //VALIDAMOS LOS MONTOS DE RETENCIÓN
                    string proveedor = Matrix.GetCellSpecific("Col_4", e.Row).Value;
                    double montoRetencion = Convert.ToDouble(Matrix.GetCellSpecific("Col_12", e.Row).Value);
                    double totalPagar = Convert.ToDouble(Matrix.GetCellSpecific("Col_17", e.Row).Value);
                    double totalProveedor = Convert.ToDouble(Matrix.GetCellSpecific("Col_17", e.Row).Value);

                    double montoRetencionRegistrado = 0;
                    double montoActual = Modelo.Filas.Where(y => y.FilaMatrix == e.Row).FirstOrDefault().TotalRetencionML;

                    montoRetencionRegistrado = Modelo.Filas.Where(x => x.Proveedor == proveedor).Sum(y => y.TotalRetencionML) - montoActual + montoRetencion;

                    double montoRetencionSUNAT = Modelo.RptaSUNAT.Where(x => ("P" + x.RUC) == proveedor).FirstOrDefault() == null ? 0 : Modelo.RptaSUNAT.Where(x => ("P" + x.RUC) == proveedor).FirstOrDefault().MontoEmbargo;

                    if (montoRetencion > totalPagar)
                    {
                        Globales.Aplication.StatusBar.SetText("No puede ingresar un monto de retención mayor al saldo del documento");
                        return false;
                    }

                    if (montoRetencionRegistrado > montoRetencionSUNAT)
                    {
                        Globales.Aplication.StatusBar.SetText($"El monto máximo de retención que puede registrar es {montoRetencionSUNAT}. Hay una diferencia de {montoRetencionSUNAT - montoRetencionRegistrado} ", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }
            }

            if (!e.BeforeAction && e.Row > 0  && e.ItemChanged)
            {
                try
                {
                    Form.Freeze(true);

                    if (e.ColUID == "Col_12")
                    {
                        int decimales = Util.utilNET.GetDecimalesConfigurado();

                        string moneda = Matrix.GetCellSpecific("Col_10", e.Row).Value;
                        double totalPagar = Convert.ToDouble(Matrix.GetCellSpecific("Col_17", e.Row).Value);
                        double montoRetencion = Convert.ToDouble(Matrix.GetCellSpecific("Col_12", e.Row).Value);
                        double tipoCambio = Convert.ToDouble(Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_TIPC"));

                        Matrix.Columns.Item("Col_13").Cells.Item(e.Row).Specific.Value = (totalPagar - montoRetencion).ToString(); //TOTAL PAGAR SOLES
                        Matrix.Columns.Item("Col_24").Cells.Item(e.Row).Specific.Value = moneda == "SOL" ? (totalPagar - montoRetencion).ToString() : Math.Round(((totalPagar - montoRetencion) / tipoCambio), decimales).ToString(); //TOTAL PAGAR MONEDA DE DOCUMENTO

                        //ACTUALIZAMOS EL MODELO
                        Modelo.Filas.Where(x => x.FilaMatrix == e.Row).FirstOrDefault().TotalRetencionML = montoRetencion;
                        Modelo.Filas.Where(x => x.FilaMatrix == e.Row).FirstOrDefault().TotalProveedorML = totalPagar - montoRetencion;

                        Matrix.FlushToDataSource();
                        Matrix.LoadFromDataSource();
                        Matrix.AutoResizeColumns();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally { Form.Freeze(false); }
            }

            return true;
        }

        private bool SeleccionarArchivoSUNAT(ItemEvent e)
        {
            if (!e.BeforeAction)
            {
                Thread ShowFolderBrowserThread = new Thread(ShowFolderBrowser);

                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    ShowFolderBrowserThread.SetApartmentState(ApartmentState.STA);
                    ShowFolderBrowserThread.Start();
                }

                if (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    ShowFolderBrowserThread.Start();
                    ShowFolderBrowserThread.Join();
                }

                while (ShowFolderBrowserThread.ThreadState == System.Threading.ThreadState.Running)
                    System.Windows.Forms.Application.DoEvents();
            }

            return true;
        }

        private void ShowFolderBrowser()
        {
            Process[] myProcs;
            OpenFileDialog openFile = new OpenFileDialog();
            System.Windows.Forms.Form formBusqueda = new System.Windows.Forms.Form();

            myProcs = Process.GetProcessesByName("SAP Business One");

            if (myProcs.Length >= 1)
            {
                formBusqueda.MaximizeBox = false;
                formBusqueda.MinimizeBox = false;
                formBusqueda.Width = 1;
                formBusqueda.Height = 1;
                formBusqueda.StartPosition = FormStartPosition.CenterScreen;
                formBusqueda.Activate();
                formBusqueda.BringToFront();
                formBusqueda.Visible = true;
                formBusqueda.TopMost = true;

                formBusqueda.Focus();

                openFile.Title = "Seleccionar respueta de SUNAT";
                openFile.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                openFile.DefaultExt = "csv";
                openFile.ShowReadOnly = true;

                DialogResult ret = openFile.ShowDialog(formBusqueda);
                formBusqueda.Hide();

                if (ret == DialogResult.OK)
                    ProcesarRespuestaRetencion(openFile.FileName);
                else
                    System.Windows.Forms.Application.ExitThread();
            }
        }

        private void ProcesarRespuestaRetencion(string filePath)
        {
            Modelo.RptaSUNAT = Controller.GetRespuetaSUNAT(filePath);

            if (Modelo.RptaSUNAT != null)
            {
                Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_ARRE", filePath);
                //DESBLOQUEAMOS LOS CAMPOS DE RETENCIÓN Y PROVEEDOR
                DesbloquearColumnasRetencion();
                RegistrarEmbargos();
                Matrix.AutoResizeColumns();
            }
        }

        private void DesbloquearColumnasRetencion()
        {
            Matrix.Columns.Item("Col_12").Editable = true;
        }

        private void RegistrarEmbargos()
        {
            try
            {
                Form.Freeze(true);
                foreach (DatosSUNATRetencion lineaRpta in Modelo.RptaSUNAT)
                {
                    LiberarTercero_Fila filaDatoMatrix = Modelo.Filas.Where(x => x.Proveedor == ("P" + lineaRpta.RUC)).FirstOrDefault();
                    if (filaDatoMatrix != null) //ENCONTRAMOS EL PROVEEDOR EN LA MATRIX
                    {
                        Matrix.GetCellSpecific("Col_8", filaDatoMatrix.FilaMatrix).Value = lineaRpta.NroResolucion;
                        Matrix.GetCellSpecific("Col_9", filaDatoMatrix.FilaMatrix).Value = lineaRpta.MontoEmbargo.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally { Form.Freeze(false); }
        }

        private bool GenerarTXT(ItemEvent e)
        {
            if (e.BeforeAction)
            {
                int rpta = Globales.Aplication.MessageBox("Está a punto de generar los txt de todos los bancos. ¿Desea continuar?", 2, "Sí", "No");
                return rpta == 1;
            }

            if (!e.BeforeAction)
            {
                if (Modelo.Filas.Count == 0)
                    throw new Exception("No hay datos para generar los archivos");

                var lstBancos = Modelo.Filas.Select(s => new { CodBanco = s.BancoCode, CuentaBanco = s.GLCuentaBanco , CodMoneda = s.Moneda }).Distinct().ToList();

                foreach (var codigoBanco in lstBancos)
                {
                    try
                    {
                        string ruta = "C:\\PagosMasivos\\";
                        string docEntryPM = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_NRPM");


                        Controller.GenerarTXTBancos(codigoBanco.CodBanco ,codigoBanco.CuentaBanco, Convert.ToInt32(docEntryPM), codigoBanco.CodMoneda);

                        Globales.Aplication.StatusBar.SetText($"Se han generado los TXT exitosamente, estos se encuentran en la ruta por defecto: {ruta}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                    catch (Exception ex)
                    {
                        Globales.Aplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                }
            }

            return true;
        }

        private bool DataLoad(BusinessObjectInfo e)
        {
            if (!e.BeforeAction && e.ActionSuccess)
                DataLoad();

            return true;
        }

        private void DataLoad()
        {
            try
            {
                Form.Freeze(true);
                int docEntry = Convert.ToInt32(Form.GetDBDataSource(HEADER).GetValueExt("DocEntry"));
                ConsultarLiberacionSUNAT(docEntry);
                Controller.BloquearLineaConPago(Form, Matrix);
                ConfigurarMatrix();
                Matrix.AutoResizeColumns();
            }
            catch (Exception)
            {
                throw;
            }
            finally { Form.Freeze(false); }

        }

        private bool BotonCrear(ItemEvent e)
        {
            if (e.BeforeAction && (Form.Mode == BoFormMode.fm_ADD_MODE || Form.Mode == BoFormMode.fm_UPDATE_MODE))
            {
                int rpta = Globales.Aplication.MessageBox("Está a punto de registrar los pagos. ¿Desea continuar?", 2, "Sí", "No");

                if (rpta != 1)
                    return false;

                CrearPagos();
            }

            if (!e.BeforeAction && Form.Mode == BoFormMode.fm_ADD_MODE)
            {
                Globales.Aplication.ActivateMenuItem("1288");
            }

            if (!e.BeforeAction && Form.Mode == BoFormMode.fm_OK_MODE)
            {
                Globales.Aplication.ActivateMenuItem("1304");
            }

            return true;
        }

        private bool SeleccionarDocumento(ItemEvent e)
        {
            if (!e.BeforeAction && e.ColUID == "Col_0" && e.Row > 0)
            {
                try
                {
                    Form.Freeze(true);
                    SAPbouiCOM.CheckBox check = Matrix.GetCellSpecific("Col_0", e.Row);
                    if (!check.Checked)
                    {
                        //LIMPIAMOS LOS VALORES DE PAGO
                        Matrix.GetCellSpecific("Col_12", e.Row).Value = "0.0";
                        Matrix.GetCellSpecific("Col_13", e.Row).Value = "0.0";
                    }

                    //actualizamos el modelo
                    Modelo.Filas.Where(x => x.FilaMatrix == e.Row).FirstOrDefault().Pagar = check.Checked;
                }
                catch (Exception)
                {
                    throw;
                }
                finally { Form.Freeze(false); }
            }

            return true;
        }

        //private bool ValidateTipoCambio(ItemEvent e)
        //{
        //    if (!e.BeforeAction && e.ItemChanged)
        //    {
        //        ConsultarPagosRetenidos();
        //    }

        //    return true;
        //}

        private bool ActualizarTipoCambio(ItemEvent e)
        {
            try
            {
                Form.Freeze(true);
                if (!e.BeforeAction && e.ItemChanged)
                {
                    string fecha = Form.GetDBDataSource(HEADER).GetValueExt("U_EXD_FPAG");
                    if (string.IsNullOrEmpty(fecha))
                        Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_TIPC", "0.0");

                    DateTime dtFecha = DateTime.ParseExact(fecha, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    Form.GetDBDataSource(HEADER).SetValueExt("U_EXD_TIPC", Controller.GetTipoCambio(dtFecha).ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally { Form.Freeze(false); }

            return true;
        }

        protected override void CargarFormularioInicial()
        {
        }
    }
}