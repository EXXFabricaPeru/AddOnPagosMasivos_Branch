using SAP_AddonExtensions;
using SAP_AddonFramework;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMC_APM.View.USRForms
{
    class FormEscenarioPago : IUSAP
    {
        public const string TYPE = "FrmEP";
        public const string UNQID = "FrmEP";
        public const string MENU = "MNUID_TRET";
        public const string PATH = "Resources/frmSMC_PM_PagoProveedores.srf";

        // Datasources
        private SAPbouiCOM.DBDataSource dbsEXD_OEPG = null;
        private SAPbouiCOM.DBDataSource dbsEXD_EPG1 = null;

        private SAPbouiCOM.DataTable dttFac = null;
        private SAPbouiCOM.DataTable dttProveedores = null;

        private SAPbouiCOM.UserDataSource udsTAB = null;
        private SAPbouiCOM.UserDataSource udsSALDO = null;
        private SAPbouiCOM.UserDataSource udsTOTAL = null;

        // Controles
        private SAPbouiCOM.ComboBox cmbBancos = null;
        private SAPbouiCOM.ComboBox cmbMonedaLoc = null;
        private SAPbouiCOM.ComboBox cmbMonedaExt = null;
        private SAPbouiCOM.ComboBox cmbCtaMonedaLoc = null;
        private SAPbouiCOM.ComboBox cmbCtaMonedaExt = null;
        private SAPbouiCOM.ComboBox cmbSeries = null;

        private SAPbouiCOM.Button btnBuscar = null;
        private SAPbouiCOM.Button btnAgregar = null;
        private SAPbouiCOM.Button btnQuitar = null;
        private SAPbouiCOM.Button btnAddPrv = null;
        private SAPbouiCOM.Button btnEnvApr = null;
        private SAPbouiCOM.Button btnAddSBO = null;

        private SAPbouiCOM.Matrix mtxFact = null;
        private SAPbouiCOM.Matrix mtxSelc = null;

        private Dictionary<string, string> dctMonedasPorCuenta = null;
        private IEnumerable<EPDocumento> lstDocumentos = null;
        private XmlSerializer _xmlSerializer = null;
        private XMLDataTable _dsrXmlDTDocs = null;
        private XMLDBDataSource _dsrXmlDBDataSource = null;
        private FormSlcProveedores frmSlcProveedores = null;

        public FormEscenarioPago(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);

            var banks = (SAPbobsCOM.Banks)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBanks);
            var currencies = (SAPbobsCOM.Currencies)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCurrencyCodes);
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            recSet.DoQuery("select \"BankCode\",\"BankName\" from ODSC");
            banks.Browser.Recordset = recSet;
            while (!banks.Browser.EoF)
            {
                cmbBancos.ValidValues.Add(banks.BankCode, banks.BankName);
                banks.Browser.MoveNext();
            }

            recSet.DoQuery("select \"CurrCode\",\"CurrName\" from OCRN");
            currencies.Browser.Recordset = recSet;
            while (!currencies.Browser.EoF)
            {
                cmbMonedaLoc.ValidValues.Add(currencies.Code, currencies.Name);
                cmbMonedaExt.ValidValues.Add(currencies.Code, currencies.Name);
                currencies.Browser.MoveNext();
            }

            btnAddPrv.Image = Path.Combine(System.Windows.Forms.Application.StartupPath, "Resources\\Img\\CFL.bmp");

            udsTAB.Value = "1";
            dttProveedores.ExecuteQuery("select 'N' as \"Slc\",\"CardCode\",\"CardName\" from OCRD");
            this.FormDataLoadAdd();
        }

        protected override void CargarFormularioInicial()
        {
            dbsEXD_OEPG = Form.GetDBDataSource("@EXD_OEPG");
            dbsEXD_EPG1 = Form.GetDBDataSource("@EXD_EPG1");

            dttFac = Form.DataSources.DataTables.Item("DT_FAC");
            dttProveedores = Form.DataSources.DataTables.Item("DT_PRV");

            udsTAB = Form.GetUserDataSource("UD_TAB");
            udsSALDO = Form.GetUserDataSource("UD_SALDO");
            udsTOTAL = Form.GetUserDataSource("UD_TOTAL");

            cmbBancos = Form.GetComboBox("Item_14");
            cmbMonedaLoc = Form.GetComboBox("Item_15");
            cmbMonedaExt = Form.GetComboBox("Item_17");
            cmbCtaMonedaLoc = Form.GetComboBox("cmbBanco");
            cmbCtaMonedaExt = Form.GetComboBox("Item_12");
            cmbSeries = Form.GetComboBox("Item_23");

            btnBuscar = Form.GetButton("btnBuscar");
            btnAgregar = Form.GetButton("btnAgg");
            btnQuitar = Form.GetButton("btnQuit");
            btnAddPrv = Form.GetButton("btnAddPrv");
            btnEnvApr = Form.GetButton("btnEnvAp");
            btnAddSBO = Form.GetButton("1");

            mtxFact = Form.GetMatrix("mtxFact");
            mtxSelc = Form.GetMatrix("mtxSelect");

            mtxFact.Columns.Item("Col_8").ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
        }

        protected override void CargarEventos()
        {
            //ItemEvents**
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbBancos.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var sboBob = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var codBanco = dbsEXD_OEPG.GetValueExt("U_COD_BANCO");
                    var codMndLoc = sboBob.GetLocalCurrency().Fields.Item(0).Value.ToString();
                    dctMonedasPorCuenta = new Dictionary<string, string>();
                    while (cmbCtaMonedaLoc.ValidValues.Count > 0) cmbCtaMonedaLoc.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (cmbCtaMonedaExt.ValidValues.Count > 0) cmbCtaMonedaExt.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    var qry = $"EXEC SMC_APM_LISTAR_BANCOS '{codBanco}'";
                    if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        qry = $"CALL SMC_APM_LISTAR_BANCOS('{codBanco}')";
                    recSet.DoQuery(qry);
                    while (!recSet.EoF)
                    {
                        dctMonedasPorCuenta[recSet.Fields.Item(0).Value] = recSet.Fields.Item("Moneda").Value;
                        if (recSet.Fields.Item("Moneda").Value == codMndLoc)
                            cmbCtaMonedaLoc.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        else
                            cmbCtaMonedaExt.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                }
                return true;
            }));
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbCtaMonedaLoc.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var codCta = dbsEXD_OEPG.GetValueExt("U_CTA_BANCO_ML");
                    if (dctMonedasPorCuenta != null && dctMonedasPorCuenta.Count > 0)
                    {
                        cmbMonedaLoc.Select(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        try
                        {
                            mtxFact.Columns.Item("Col_8").ValidValues.Remove(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                            mtxSelc.Columns.Item("Col_6").ValidValues.Remove(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        catch (Exception ex) { }
                        mtxFact.Columns.Item("Col_8").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                        mtxSelc.Columns.Item("Col_6").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                    }
                }
                return true;
            }));
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbCtaMonedaExt.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var codCta = dbsEXD_OEPG.GetValueExt("U_CTA_BANCO_ME");
                    if (dctMonedasPorCuenta != null && dctMonedasPorCuenta.Count > 0)
                    {
                        cmbMonedaExt.Select(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        try
                        {
                            mtxFact.Columns.Item("Col_8").ValidValues.Remove(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                            mtxSelc.Columns.Item("Col_6").ValidValues.Remove(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        }
                        catch (Exception ex) { }
                        mtxFact.Columns.Item("Col_8").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                        mtxSelc.Columns.Item("Col_6").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnBuscar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    try
                    {
                        var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        var fechaVenc = dbsEXD_OEPG.GetValueExt("U_FECHA_VENC");
                        var monedaLoc = dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_LOC");
                        var monedaExt = dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_EXT");
                        var codBanco = dbsEXD_OEPG.GetValueExt("U_COD_BANCO");
                        var qry = $"EXEC SMC_APM_LISTAR_FACPENDIENTES_PP '{fechaVenc}','{monedaLoc}','','','{codBanco}',''";
                        if(Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                            qry = $"CALL SMC_APM_LISTAR_FACPENDIENTES_PP('{fechaVenc}','{monedaLoc}','','','{codBanco}','')";
                        dttFac.ExecuteQuery(qry);
                        _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                        var strXMLDTDocs = dttFac.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                        _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                        lstDocumentos = _dsrXmlDTDocs.Rows.Select(r => new EPDocumento
                        {
                            DocEntry = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocEntry"))?.Value),
                            DocNum = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocNum"))?.Value,
                            FechaContable = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("FechaContable"))?.Value,
                            FechaVencimiento = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("FechaVencimiento"))?.Value,
                            CardCode = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCode"))?.Value,
                            CardName = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardName"))?.Value,
                            NumAtCard = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NumAtCard"))?.Value,
                            MonedaPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("MonedaPago"))?.Value,
                            DocCur = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocCur"))?.Value,
                            Total = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Total"))?.Value),
                            CodigoRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodigoRetencion"))?.Value,
                            Retencion = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Retencion"))?.Value),
                            TotalPagar = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("TotalPagar"))?.Value),
                            RUC = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("RUC"))?.Value,
                            Cuenta = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Cuenta"))?.Value,
                            CuentaMoneda = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CuentaMoneda"))?.Value,
                            BankCode = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("BankCode"))?.Value,
                            Atraso = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Atraso"))?.Value,
                            Marca = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Marca"))?.Value,
                            Estado = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Estado"))?.Value,
                            Documento = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Documento"))?.Value,
                            NombreBanco = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NombreBanco"))?.Value,
                            Origen = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Origen"))?.Value,
                            BloqueoPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("BloqueoPago"))?.Value,
                            DetraccionPend = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DetraccionPend"))?.Value,
                            NroCuota = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NroCuota"))?.Value),
                            LineaAsiento = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("LineaAsiento"))?.Value),
                            GlosaAsiento = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("GlosaAsiento"))?.Value,
                            CardCodeFactoring = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCodeFactoring"))?.Value,
                            CardNameFactoring = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardNameFactoring"))?.Value,
                            EstadoExt = "P"
                        }).ToList();

                        strXMLDTDocs = dttProveedores.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                        var _dsrXmlDTProvs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                        var lstProveedoresSlc = _dsrXmlDTProvs.Rows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Slc")).Value == "Y")
                        .Select(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "CardCode").Value).ToList();
                        if (!string.IsNullOrWhiteSpace(dbsEXD_OEPG.GetValueExt("U_COD_PROV"))) lstProveedoresSlc.Add(dbsEXD_OEPG.GetValueExt("U_COD_PROV"));
                        if (lstProveedoresSlc.Count > 0) lstDocumentos = lstDocumentos.Where(d => lstProveedoresSlc.Contains(d.CardCode));
                        this.LoadMatrixByStatus();
                        //mtxFact.LoadFromDataSource();
                    }
                    catch (Exception ex)
                    {
                        Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnQuitar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    mtxSelc.FlushToDataSource();
                    _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
                    var strXMLDTDocs = dbsEXD_EPG1.GetAsXML();
                    _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                    _dsrXmlDBDataSource.Rows.Where(r => r.Cells.FirstOrDefault(c => c.Uid == "U_SELECCIONADO")?.Value == "Y").All(r =>
                    {
                        var doc = lstDocumentos.FirstOrDefault(d => d.DocEntry == Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_DOCENTRY").Value)).EstadoExt = "P";
                        return true;
                    });
                    LoadMatrixByStatus();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnAgregar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    mtxFact.FlushToDataSource();
                    _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                    var strXMLDTDocs = dttFac.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                    _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                    _dsrXmlDTDocs.Rows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "Slc").Value == "Y").All(r =>
                    {
                        var doc = lstDocumentos.FirstOrDefault(d => d.DocEntry == Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid == "DocEntry").Value));
                        doc.EstadoExt = "S";
                        doc.MonedaPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "MonedaPago").Value;
                        return true;
                    });
                    LoadMatrixByStatus();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnAddPrv.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    frmSlcProveedores = new FormSlcProveedores("FrmSLCPV", dttProveedores);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnEnvApr.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    if (Form.Mode != SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        dbsEXD_OEPG.SetValue("U_ESTADO", 0, "A");
                        if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                            Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        btnAddSBO.Item.Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        Form.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnAddSBO.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    if (lstDocumentos == null || lstDocumentos.Where(d => d.EstadoExt == "S").Count() == 0)
                    {
                        Globales.Aplication.StatusBar.SetText("Agregue al menos un documento al escenario", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }
                else
                {
                    if (dbsEXD_OEPG.GetValueExt("U_ESTADO").Equals("P"))
                    {
                        btnEnvApr.Item.Enabled = true;
                    }
                }
                return true;
            }));

            //DataEvents **
            Eventos.Add(new EventoData(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD, this.Form.TypeEx, e =>
            {
                if (!e.BeforeAction)
                {
                    // Form.Mode = "PR".Contains(dbsEXD_OEPG.GetValueExt("U_ESTADO")) ? SAPbouiCOM.BoFormMode. : SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                    btnEnvApr.Item.Enabled = "PR".Contains(dbsEXD_OEPG.GetValueExt("U_ESTADO"));
                }
                return true;
            }));

        }

        internal void LoadDataOnFormAddMode()
        {
            FormDataLoadAdd();
        }

        internal void FormDataLoadAdd()
        {
            dbsEXD_OEPG.SetValueExt("U_ESTADO", "P");
            dbsEXD_OEPG.SetValueExt("U_MEDIO_DE_PAGO", "TB");
            dbsEXD_OEPG.SetValueExt("U_FECHA_PAGO", DateTime.Today.ToString("yyyyMMdd"));
            dbsEXD_OEPG.SetValueExt("U_FECHA_VENC", DateTime.Today.ToString("yyyyMMdd"));
            cmbSeries.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (cmbSeries.ValidValues.Count > 0) cmbSeries.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dbsEXD_OEPG.SetValueExt("DocNum", Form.BusinessObject.GetNextSerialNumber(dbsEXD_OEPG.GetValueExt("Series"), Form.BusinessObject.Type).ToString());
        }

        private void LoadMatrixByStatus()
        {
            _dsrXmlDTDocs = new XMLDataTable();
            _dsrXmlDBDataSource = new XMLDBDataSource();
            _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));

            //Totales ++

            udsSALDO.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "P").Sum(d => d.TotalPagar).ToString();
            udsTOTAL.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "S").Sum(d => d.TotalPagar).ToString();

            _dsrXmlDTDocs.Rows = lstDocumentos.Where(d => d.EstadoExt == "P").Select(d => new Row
            {
                Cells = new List<Cell>
                        {
                            new Cell{ ColumnUid = "FILA", Value = d.FILA.ToString()},
                            new Cell{ ColumnUid = "DocEntry", Value = d.DocEntry.ToString()},
                            new Cell{ ColumnUid = "DocNum", Value = d.DocNum},
                            new Cell{ ColumnUid = "FechaContable", Value = d.FechaContable},
                            new Cell{ ColumnUid = "FechaVencimiento", Value = d.FechaVencimiento},
                            new Cell{ ColumnUid = "CardCode", Value = d.CardCode },
                            new Cell{ ColumnUid = "CardName", Value = d.CardName },
                            new Cell{ ColumnUid = "NumAtCard", Value = d.NumAtCard },
                            new Cell{ ColumnUid = "MonedaPago", Value = d.MonedaPago},
                            new Cell{ ColumnUid = "DocCur", Value = d.DocCur },
                            new Cell{ ColumnUid = "Total", Value = d.Total.ToString() },
                            new Cell{ ColumnUid = "CodigoRetencion", Value = d.CodigoRetencion },
                            new Cell{ ColumnUid = "Retencion", Value = d.Retencion.ToString() },
                            new Cell{ ColumnUid = "TotalPagar", Value = d.TotalPagar.ToString() },
                            new Cell{ ColumnUid = "RUC", Value = d.RUC },
                            new Cell{ ColumnUid = "Cuenta", Value = d.Cuenta },
                            new Cell{ ColumnUid = "CuentaMoneda", Value = d.CuentaMoneda },
                            new Cell{ ColumnUid = "BankCode", Value = d.BankCode },
                            new Cell{ ColumnUid = "Atraso", Value = d.Atraso },
                            new Cell{ ColumnUid = "Marca", Value = d.Marca },
                            new Cell{ ColumnUid = "Estado", Value = d.Estado },
                            new Cell{ ColumnUid = "Documento", Value = d.Documento },
                            new Cell{ ColumnUid = "NombreBanco", Value = d.NombreBanco },
                            new Cell{ ColumnUid = "Origen", Value = d.Origen },
                            new Cell{ ColumnUid = "BloqueoPago", Value = d.BloqueoPago },
                            new Cell{ ColumnUid = "DetraccionPend", Value = d.DetraccionPend },
                            new Cell{ ColumnUid = "NroCuota", Value = d.NroCuota.ToString() },
                            new Cell{ ColumnUid = "LineaAsiento", Value = d.LineaAsiento.ToString() },
                            new Cell{ ColumnUid = "GlosaAsiento", Value = d.GlosaAsiento },
                            new Cell{ ColumnUid = "CardCodeFactoring", Value = d.CardCodeFactoring },
                            new Cell{ ColumnUid = "CardNameFactoring", Value = d.CardNameFactoring },

                        }.ToArray()
            }).ToArray();
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                dttFac.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                mtxFact.LoadFromDataSource();
            }

            _dsrXmlDBDataSource.Rows = lstDocumentos.Where(d => d.EstadoExt == "S").Select(d => new RowDBS
            {
                Cells = new List<CellDBS>
                        {
                            new CellDBS{ Uid = "U_DOCENTRY", Value = d.DocEntry.ToString()},
                            new CellDBS{ Uid = "U_NUM_SAP", Value = d.DocNum},
                            new CellDBS{ Uid = "U_FECHA_DOCUMENTO", Value = d.FechaContable },
                            new CellDBS{ Uid = "U_FECHA_VENCIMIENTO", Value = d.FechaVencimiento },
                            new CellDBS{ Uid = "U_CARDCODE", Value = d.CardCode },
                            new CellDBS{ Uid = "U_CARDNAME", Value = d.CardName },
                            new CellDBS{ Uid = "U_NUMERO_DOC", Value = d.NumAtCard },
                            new CellDBS{ Uid = "U_MONEDA_PAGO", Value = d.MonedaPago },
                            new CellDBS{ Uid = "U_COD_BANCO", Value = d.BankCode },
                            new CellDBS{ Uid = "U_BANCO_PROV", Value = d.NombreBanco },
                            new CellDBS{ Uid = "U_CUENTA_PROV", Value = d.Cuenta },
                            new CellDBS{ Uid = "U_MONEDA", Value = d.DocCur },
                            new CellDBS{ Uid = "U_ORIGEN", Value = d.Origen },
                            new CellDBS{ Uid = "U_TOTAL", Value = d.Total.ToString() },
                            new CellDBS{ Uid = "U_COD_RETENCION", Value = d.CodigoRetencion },
                            new CellDBS{ Uid = "U_RETENCION", Value = d.Retencion.ToString() },
                            new CellDBS{ Uid = "U_TOTAL_PAGO", Value = d.TotalPagar.ToString() },
                            new CellDBS{ Uid = "U_GLOSA_ASIENTO", Value = d.GlosaAsiento },
                            new CellDBS{ Uid = "U_ATRASO", Value = d.Atraso },
                            new CellDBS{ Uid = "U_NRO_CUOTA", Value = d.NroCuota.ToString() },
                            new CellDBS{ Uid = "U_NRO_LINEA_AS", Value = d.LineaAsiento.ToString() },
                            new CellDBS{ Uid = "U_COD_PROV_FACTO", Value = d.CardCodeFactoring },
                            new CellDBS{ Uid = "U_NOM_PROV_FACTO", Value = d.CardNameFactoring },
                            new CellDBS{ Uid = "U_TIPO_DOCUMENTO", Value = d.Documento }
                        }.ToArray()
            }).ToArray();

            _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDBDataSource);
                dbsEXD_EPG1.LoadFromXML(strWritter.ToString());
                mtxSelc.LoadFromDataSource();
            }
        }
    }
}
