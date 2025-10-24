using OfficeOpenXml;
using SAP_AddonExtensions;
using SAP_AddonFramework;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SMC_APM.View.USRForms
{
    class FormSlcProveedores : IUSAP
    {
        public const string TYPE = "FrmSLCPV";
        public const string UNQID = "FrmSLCPV";
        public const string PATH = "Resources/FrmSlcProveedores.srf";
        private Row[] _arrRows = null;
        private List<KeyValuePair<int, string>> lstClientes = null;

        //DataSources
        private SAPbouiCOM.DataTable _dttProveedoresMain = null;
        private SAPbouiCOM.DataTable _dttProveedores = null;
        private SAPbouiCOM.DataTable _dttProveedoresSlc = null;

        //Controles
        private SAPbouiCOM.Matrix mtxProveedores = null;
        private SAPbouiCOM.Matrix mtxProveedoresSlc = null;

        private SAPbouiCOM.Button btnOK = null;
        private SAPbouiCOM.Button btnLeerArchivo = null;
        private SAPbouiCOM.Button btnMover = null;
        private SAPbouiCOM.Button btnRegresar = null;

        private SAPbouiCOM.EditText edtNombreProveedor = null;


        public FormSlcProveedores(string id, SAPbouiCOM.DataTable dttProveedores) : base(TYPE, null, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
            _dttProveedoresMain = dttProveedores;

            var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
            var strXMLDTDocs = _dttProveedoresMain.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
            var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

            _arrRows = _dsrXmlDTDocs.Rows;
            MostrarDatosEnMatrix();
        }
        protected override void CargarFormularioInicial()
        {
            _dttProveedores = Form.DataSources.DataTables.Item("DT_PRV");
            _dttProveedoresSlc = Form.DataSources.DataTables.Item("DT_PRV_SLC");

            mtxProveedores = Form.GetMatrix("Item_0");
            mtxProveedoresSlc = Form.GetMatrix("Item_1");

            btnOK = Form.GetButton("Item_4");
            btnLeerArchivo = Form.GetButton("Item_6");
            btnMover = Form.GetButton("Item_2");
            btnRegresar = Form.GetButton("Item_3");

            edtNombreProveedor = (SAPbouiCOM.EditText)Form.GetItem("Item_5").Specific;
        }
        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnOK.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    _dttProveedoresMain.Rows.Clear();

                    var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                    var strXMLDTDocs = _dttProveedoresMain.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                    var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

                    _dsrXmlDTDocs.Rows = _arrRows;
                    using (var strWritter = new StringWriter())
                    {
                        _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                        _dttProveedoresMain.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                    }
                    this.Form.Close();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnLeerArchivo.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    SeleccionarArchivo();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnMover.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    mtxProveedores.FlushToDataSource();
                    var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                    var strXMLDTDocs = _dttProveedores.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                    var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

                    var lstCardCodes = _dsrXmlDTDocs.Rows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "Slc").Value == "Y")
                    .Select(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "CardCode").Value);
                    _arrRows.All(r =>
                     {
                         if (lstCardCodes.Any(cd => cd == r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCode")).Value))
                             r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Slc")).Value = "Y";
                         return true;
                     });
                    var valorBusqueda = edtNombreProveedor.Value;
                    MostrarDatosEnMatrix(valorBusqueda);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnRegresar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    mtxProveedoresSlc.FlushToDataSource();
                    var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                    var strXMLDTDocs = _dttProveedoresSlc.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                    var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

                    var lstCardCodes = _dsrXmlDTDocs.Rows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "Slc").Value == "Y")
                    .Select(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "CardCode").Value);
                    _arrRows.All(r =>
                    {
                        if (lstCardCodes.Any(cd => cd == r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCode")).Value))
                            r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Slc")).Value = "N";
                        return true;
                    });
                    var valorBusqueda = edtNombreProveedor.Value;
                    MostrarDatosEnMatrix(valorBusqueda);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_KEY_DOWN, edtNombreProveedor.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var valorBusqueda = edtNombreProveedor.Value;
                    /*
                    var rowNum = lstClientes.FirstOrDefault(c => c.Value.ToLower().Contains(valorBusqueda.ToLower())).Key;
                    if (rowNum > 0) mtxProveedores.SelectRow(rowNum + 1, true, false);
                    */
                    MostrarDatosEnMatrix(valorBusqueda.ToLower());
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, mtxProveedores.Item.UniqueID, e =>
            {
                if (!e.BeforeAction && e.Row == 0 && e.ColUID == "Col_1")
                {
                    var valorBusqueda = edtNombreProveedor.Value;
                    MostrarDatosEnMatrix(valorBusqueda);
                }
                return true;
            }));
        }

        private bool SeleccionarArchivo()
        {
            Thread ShowFolderBrowserThread = new Thread(ShowFileDialog);

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

            return true;
        }

        private void ShowFileDialog()
        {
            Process[] myProcess = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            System.Windows.Forms.Form formBusqueda = new System.Windows.Forms.Form();

            myProcess = Process.GetProcessesByName("SAP Business One");

            if (myProcess.Length >= 1)
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
                DialogResult rst = openFileDialog.ShowDialog(formBusqueda);
                formBusqueda.Hide();

                if (rst == DialogResult.OK)
                    seleccionarProveedores(openFileDialog.FileName);
                else
                    System.Windows.Forms.Application.ExitThread();
            }
        }

        private void seleccionarProveedores(object selectedPath)
        {
            var rutaArchivoExcel = selectedPath.ToString();
            if (string.IsNullOrWhiteSpace(rutaArchivoExcel))
            {
                MessageBox.Show("Seleccione un archivo");
                return;
            }
            var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
            var strXMLDTDocs = _dttProveedores.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
            var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
            FileInfo bpList = new FileInfo(rutaArchivoExcel);
            using (ExcelPackage package = new ExcelPackage(bpList))
            {
                // Get the work book in the file
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelWorkbook workBook = package.Workbook;
                if (workBook != null)
                {
                    if (workBook.Worksheets.Count > 0)
                    {
                        var worksheet = workBook.Worksheets.First();
                        var cnt = worksheet.Dimension.End.Row;
                        var columnIdx = 0;
                        for (int i = 1; i < 100; i++)
                        {
                            if (OfficeOpenXml.ExcelCellAddress.GetColumnLetter(i) == "A")
                            {
                                columnIdx = i;
                                break;
                            }
                        }
                        for (int i = 0; i < cnt; i++)
                        {
                            var cardCode = worksheet.GetValue(i + 1, columnIdx).ToString();
                            if (!string.IsNullOrWhiteSpace(cardCode))
                            {
                                _dsrXmlDTDocs.Rows.All(r =>
                                {
                                    if (r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCode")).Value == cardCode)
                                        r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Slc")).Value = "Y";
                                    return true;
                                });
                            }
                        }

                        using (var strWritter = new StringWriter())
                        {
                            _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                            _dttProveedores.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                            mtxProveedores.LoadFromDataSource();
                        }
                    }
                }
                package.Dispose();
            }
        }


        private void MostrarDatosEnMatrix(string indicioNombProv = "")
        {
            try
            {
                Form.Freeze(true);
                var _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                var strXMLDTDocs = _dttProveedores.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                var _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

                var arrProvSlc = _arrRows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "Slc").Value == "Y").ToArray();
                var arrProvNoSlc = _arrRows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "Slc").Value != "Y")
                    .Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "CardName").Value.ToLower().Contains(indicioNombProv)).ToArray();

                //if (arrProvSlc.Length == 0 || arrProvNoSlc.Length == 0)
                //{
                Form.Items.Item("Item_5").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                //}

                _dsrXmlDTDocs.Rows = arrProvNoSlc;
                using (var strWritter = new StringWriter())
                {
                    _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                    _dttProveedores.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                    mtxProveedores.LoadFromDataSourceEx();
                }

                lstClientes = _dsrXmlDTDocs.Rows.Select((r, i) => new KeyValuePair<int, string>(i, r.Cells.FirstOrDefault(c => c.ColumnUid == "CardName").Value)).ToList();

                _dsrXmlDTDocs.Rows = arrProvSlc;
                using (var strWritter = new StringWriter())
                {
                    _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                    _dttProveedoresSlc.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                    mtxProveedoresSlc.LoadFromDataSourceEx();
                }
            }
            catch (Exception ex)
            {
                Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                Form.Freeze(false);
            }
        }
    }
}
