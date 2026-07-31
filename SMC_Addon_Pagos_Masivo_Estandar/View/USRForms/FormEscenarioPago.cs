using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbobsCOM;
using SMC_APM.Modelo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMC_APM.View.USRForms
{
    class FormEscenarioPago : IUSAP
    {
        public const string TYPE = "FrmEP";
        public const string UNQID = "FrmEP";
        public const string MENU = "MNUID_TRET";
        public const string PATH = "Resources/FrmEscenarioPago.srf";

        // Datasources
        private SAPbouiCOM.DBDataSource dbsEXD_OEPG = null;
        private SAPbouiCOM.DBDataSource dbsEXD_EPG1 = null;

        private SAPbouiCOM.DataTable dttFac = null;
        private SAPbouiCOM.DataTable dttProveedores = null;

        private SAPbouiCOM.UserDataSource udsTAB = null;
        private SAPbouiCOM.UserDataSource udsSALDO = null;
        private SAPbouiCOM.UserDataSource udsTOTAL = null;
        private SAPbouiCOM.UserDataSource udsSALDO_USD = null;
        private SAPbouiCOM.UserDataSource udsTOTAL_USD = null;

        // Controles
        private SAPbouiCOM.ComboBox cmbBancos = null;
        //private SAPbouiCOM.ComboBox cmbMonedaLoc = null;
        //private SAPbouiCOM.ComboBox cmbMonedaExt = null;
        //private SAPbouiCOM.ComboBox cmbCtaMonedaLoc = null;
        //private SAPbouiCOM.ComboBox cmbCtaMonedaExt = null;
        private SAPbouiCOM.ComboBox cmbSeries = null;
        private SAPbouiCOM.ComboBox cmbSucursales = null;
        private SAPbouiCOM.ComboBox cmbPrioridad = null;
        private SAPbouiCOM.ComboBox cmbTipoDocumento = null;
        private SAPbouiCOM.ComboBox cmbAutorizarPor = null;

        private SAPbouiCOM.Button btnBuscar = null;
        private SAPbouiCOM.Button btnAgregar = null;
        private SAPbouiCOM.Button btnQuitar = null;
        private SAPbouiCOM.Button btnAddPrv = null;
        private SAPbouiCOM.Button btnEnvApr = null;
        private SAPbouiCOM.Button btnAddSBO = null;

        private SAPbouiCOM.Matrix mtxFact = null;
        private SAPbouiCOM.Matrix mtxSelc = null;

        private Dictionary<string, string> dctMonedasPorCuenta = null;
        private List<EPDocumento> lstDocumentos = null;
        private XmlSerializer _xmlSerializer = null;
        private XMLDataTable _dsrXmlDTDocs = null;
        private XMLDBDataSource _dsrXmlDBDataSource = null;
        private FormSlcProveedores frmSlcProveedores = null;

        private bool tieneAutorizaciones = false;
        private bool tieneSucursales = true;

        private bool esSeleccionarTodo = false;

        private string codMonedaLocal = string.Empty;


        public FormEscenarioPago(string id) : base(TYPE, MENU, id, PATH)
        {
            try
            {
                if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
                Form.Freeze(true);
                // Deshabilito la opcion de restablecer
                Form.EnableMenu("1285", false);
                // Deshabilito la opcion de cerrar
                Form.EnableMenu("1286", false);

                var tblConf = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
                tieneAutorizaciones = tblConf.GetByKey("4") && tblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                //tieneSucursales = tblConf.GetByKey("9") && tblConf.UserFields.Fields.Item("U_VALOR").Value == "Y";
                var banks = (SAPbobsCOM.Banks)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBanks);
                var currencies = (SAPbobsCOM.Currencies)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCurrencyCodes);
                var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                var sboBOB = (SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(BoObjectTypes.BoBridge);

                codMonedaLocal = sboBOB.GetLocalCurrency().Fields.Item(0).Value;

                recSet.DoQuery("select \"BankCode\",\"BankName\" from ODSC");
                banks.Browser.Recordset = recSet;
                cmbBancos.ValidValues.Add("000", "Todos");
                while (!banks.Browser.EoF)
                {
                    cmbBancos.ValidValues.Add(banks.BankCode, banks.BankName);
                    //mtxFact.Columns.Item("Col_12").ValidValues.Add(banks.BankCode, banks.BankName);
                    //mtxSelc.Columns.Item("Col_10").ValidValues.Add(banks.BankCode, banks.BankName);
                    banks.Browser.MoveNext();
                }

                recSet.DoQuery("select \"CurrCode\",\"CurrName\" from OCRN");
                currencies.Browser.Recordset = recSet;
                while (!currencies.Browser.EoF)
                {
                    //cmbMonedaLoc.ValidValues.Add(currencies.Code, currencies.Name);
                    //cmbMonedaExt.ValidValues.Add(currencies.Code, currencies.Name);
                    if (tieneSucursales)
                    {
                        mtxFact.Columns.Item("Col_8").ValidValues.Add(currencies.Code, currencies.Code);
                    }
                    currencies.Browser.MoveNext();
                }

                cmbSucursales.ValidValues.Add("-1", "Todas");
                recSet.DoQuery("select \"BPLId\",\"BPLName\" from OBPL");
                while (!recSet.EoF)
                {
                    cmbSucursales.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    mtxFact.Columns.Item("Col_9").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    mtxSelc.Columns.Item("Col_9").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                cmbPrioridad.ValidValues.Add(string.Empty, string.Empty);
                recSet.DoQuery("select \"Code\",\"Name\" from \"@EXX_PRIPAG\"");
                while (!recSet.EoF)
                {
                    cmbPrioridad.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                //cmbTipoDocumento.ValidValues.Add(string.Empty, string.Empty);
                recSet.DoQuery("select \"Code\",\"Name\" from \"@EXD_PM_TIPODOC\"");
                while (!recSet.EoF)
                {
                    cmbAutorizarPor.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                    recSet.MoveNext();
                }

                btnAddPrv.Image = Path.Combine(System.Windows.Forms.Application.StartupPath, "Resources\\Img\\CFL.bmp");
                Form.GetComboBox("Item_14").Active = true;
                udsTAB.Value = "1";
                dttProveedores.ExecuteQuery("select 'N' as \"Slc\",\"CardCode\",\"CardName\" from OCRD order by \"CardName\" ");

                btnEnvApr.Item.Visible = tieneAutorizaciones;
                if (tieneSucursales) PrepareFormOnSucursales();
                this.FormDataLoadAdd();
                Form.GetComboBox("Item_14").Active = true;
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
        protected override void CargarFormularioInicial()
        {
            dbsEXD_OEPG = Form.GetDBDataSource("@EXD_OEPG");
            dbsEXD_EPG1 = Form.GetDBDataSource("@EXD_EPG1");

            dttFac = Form.DataSources.DataTables.Item("DT_FAC");
            dttProveedores = Form.DataSources.DataTables.Item("DT_PRV");

            udsTAB = Form.GetUserDataSource("UD_TAB");
            udsSALDO = Form.GetUserDataSource("UD_SALDO");
            udsTOTAL = Form.GetUserDataSource("UD_TOTAL");
            udsSALDO_USD = Form.GetUserDataSource("UD_SLD_USD");
            udsTOTAL_USD = Form.GetUserDataSource("UD_TOT_USD");

            cmbBancos = Form.GetComboBox("Item_14");
            //cmbMonedaLoc = Form.GetComboBox("Item_15");
            //cmbMonedaExt = Form.GetComboBox("Item_17");
            //cmbCtaMonedaLoc = Form.GetComboBox("cmbBanco");
            //cmbCtaMonedaExt = Form.GetComboBox("Item_12");
            cmbSeries = Form.GetComboBox("Item_23");
            cmbSucursales = Form.GetComboBox("Item_11");
            cmbPrioridad = Form.GetComboBox("Item_19");
            cmbTipoDocumento = Form.GetComboBox("Item_32");
            cmbAutorizarPor = Form.GetComboBox("Item_13");

            btnBuscar = Form.GetButton("btnBuscar");
            btnAgregar = Form.GetButton("btnAgg");
            btnQuitar = Form.GetButton("btnQuit");
            btnAddPrv = Form.GetButton("btnAddPrv");
            btnEnvApr = Form.GetButton("btnEnvAp");
            btnAddSBO = Form.GetButton("1");

            mtxFact = Form.GetMatrix("mtxFact");
            mtxSelc = Form.GetMatrix("mtxSelect");

            mtxFact.Columns.Item("Col_8").ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
            mtxFact.Columns.Item("Col_10").ExpandType = SAPbouiCOM.BoExpandType.et_ValueOnly;

            mtxFact.Columns.Item("Col_11").Visible = false;
            mtxFact.Columns.Item("Col_12").Visible = false;
            mtxSelc.Columns.Item("Col_8").Visible = false;
            mtxSelc.Columns.Item("Col_10").Visible = false;

            mtxFact.Columns.Item("Col_16").Visible = false;
            mtxFact.Columns.Item("Col_17").Visible = false;
            mtxFact.Columns.Item("Col_18").Visible = false;

            mtxSelc.Columns.Item("Col_14").Visible = false;
            mtxSelc.Columns.Item("Col_15").Visible = false;
            mtxSelc.Columns.Item("Col_16").Visible = false;

            mtxFact.CommonSetting.FixedColumnsCount = 2;
            mtxSelc.CommonSetting.FixedColumnsCount = 2;

        }

        private void PrepareFormOnSucursales()
        {
            /*
            //Form.Items.Item("lblBank").Visible = false;
            Form.Items.Item("Item_13").Visible = false;
            //Form.Items.Item("cmbBanco").Visible = false;
            Form.Items.Item("Item_12").Visible = false;
            //Form.Items.Item("lblMoneda").Visible = false;
            //Form.Items.Item("Item_16").Visible = false;
            //Form.Items.Item("Item_15").Visible = false;
            //Form.Items.Item("Item_17").Visible = false;       
            Form.Items.Item("Item_3").Visible = true;
            Form.Items.Item("Item_11").Visible = true;
            Form.Items.Item("Item_18").Top = Form.Items.Item("Item_1").Top;
            Form.Items.Item("Item_19").Top = Form.Items.Item("Item_5").Top;
            Form.Items.Item("Item_1").Top = Form.Items.Item("lblBank").Top;
            Form.Items.Item("Item_5").Top = Form.Items.Item("cmbBanco").Top;
            Form.Items.Item("Item_3").Top = Form.Items.Item("Item_13").Top;
            Form.Items.Item("Item_11").Top = Form.Items.Item("Item_12").Top;
            Form.Items.Item("Item_31").Top = Form.Items.Item("Item_18").Top + Form.Items.Item("Item_18").Height + 1;
            Form.Items.Item("Item_32").Top = Form.Items.Item("Item_19").Top + Form.Items.Item("Item_19").Height + 1;
            Form.Items.Item("Item_33").Top = Form.Items.Item("Item_31").Top + Form.Items.Item("Item_31").Height + 1;
            Form.Items.Item("Item_34").Top = Form.Items.Item("Item_32").Top + Form.Items.Item("Item_32").Height + 1;
            */
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
                    //while (cmbCtaMonedaLoc.ValidValues.Count > 0) cmbCtaMonedaLoc.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    //while (cmbCtaMonedaExt.ValidValues.Count > 0) cmbCtaMonedaExt.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    dbsEXD_OEPG.SetValueExt("U_CTA_BANCO_ML", null);
                    dbsEXD_OEPG.SetValueExt("U_CTA_BANCO_ME", null);
                    dbsEXD_OEPG.SetValueExt("U_COD_MONEDA_LOC", null);
                    dbsEXD_OEPG.SetValueExt("U_COD_MONEDA_EXT", null);
                    var qry = $"EXEC SMC_APM_LISTAR_BANCOS '{codBanco}'";
                    if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        qry = $"CALL SMC_APM_LISTAR_BANCOS('{codBanco}')";
                    recSet.DoQuery(qry);
                    if (!recSet.EoF)
                    {
                        //cmbCtaMonedaLoc.ValidValues.Add("", "");
                        //cmbCtaMonedaExt.ValidValues.Add("", "");
                        while (!recSet.EoF)
                        {
                            dctMonedasPorCuenta[recSet.Fields.Item(0).Value] = recSet.Fields.Item("Moneda").Value;
                            /*
                            if (recSet.Fields.Item("Moneda").Value == codMndLoc)
                                //cmbCtaMonedaLoc.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                            else
                                //cmbCtaMonedaExt.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                            */
                            recSet.MoveNext();
                        }
                    }
                }
                return true;
            }));

            /*
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbCtaMonedaLoc.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var codMonLoc = ((SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge))
                    .GetLocalCurrency().Fields.Item(0).Value.ToString();
                    var codCta = dbsEXD_OEPG.GetValueExt("U_CTA_BANCO_ML");
                    dbsEXD_OEPG.SetValueExt("U_COD_MONEDA_LOC", null);

                    for (int i = 0; i < mtxFact.Columns.Item("Col_8").ValidValues.Count; i++)
                    {
                        if (mtxFact.Columns.Item("Col_8").ValidValues.Item(i).Value == codMonLoc)
                        {
                            mtxFact.Columns.Item("Col_8").ValidValues.Remove(codMonLoc, SAPbouiCOM.BoSearchKey.psk_ByValue);
                            break;
                        }
                    }

                    for (int i = 0; i < mtxSelc.Columns.Item("Col_6").ValidValues.Count; i++)
                    {
                        if (mtxSelc.Columns.Item("Col_6").ValidValues.Item(i).Value == codMonLoc)
                        {
                            mtxSelc.Columns.Item("Col_6").ValidValues.Remove(codMonLoc, SAPbouiCOM.BoSearchKey.psk_ByValue);
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(codCta) && dctMonedasPorCuenta != null && dctMonedasPorCuenta.Count > 0)
                    {
                        cmbMonedaLoc.Select(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        mtxFact.Columns.Item("Col_8").ValidValues.Add(codMonLoc, codMonLoc);
                        mtxSelc.Columns.Item("Col_6").ValidValues.Add(codMonLoc, codMonLoc);
                    }
                }
                return true;
            }));
        

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbCtaMonedaExt.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var codMonLoc = ((SAPbobsCOM.SBObob)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge))
                    .GetLocalCurrency().Fields.Item(0).Value.ToString();
                    var codCta = dbsEXD_OEPG.GetValueExt("U_CTA_BANCO_ME");
                    dbsEXD_OEPG.SetValueExt("U_COD_MONEDA_EXT", null);

                    for (int i = 0; i < mtxFact.Columns.Item("Col_8").ValidValues.Count; i++)
                    {
                        if (mtxFact.Columns.Item("Col_8").ValidValues.Item(i).Value != codMonLoc)
                        {
                            mtxFact.Columns.Item("Col_8").ValidValues.Remove(mtxFact.Columns.Item("Col_8").ValidValues.Item(i).Value, SAPbouiCOM.BoSearchKey.psk_ByValue);
                            break;
                        }
                    }

                    for (int i = 0; i < mtxSelc.Columns.Item("Col_6").ValidValues.Count; i++)
                    {
                        if (mtxSelc.Columns.Item("Col_6").ValidValues.Item(i).Value != codMonLoc)
                        {
                            mtxSelc.Columns.Item("Col_6").ValidValues.Remove(mtxSelc.Columns.Item("Col_6").ValidValues.Item(i).Value, SAPbouiCOM.BoSearchKey.psk_ByValue);
                            break;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(codCta) && dctMonedasPorCuenta != null && dctMonedasPorCuenta.Count > 0)
                    {
                        cmbMonedaExt.Select(dctMonedasPorCuenta[codCta], SAPbouiCOM.BoSearchKey.psk_ByValue);
                        mtxFact.Columns.Item("Col_8").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                        mtxSelc.Columns.Item("Col_6").ValidValues.Add(dctMonedasPorCuenta[codCta], dctMonedasPorCuenta[codCta]);
                    }
                }
                return true;
            }));
            */

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnBuscar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    try
                    {
                        var tblConfPM = Globales.Company.UserTables.Item("SMC_APM_CONFIAPM");
                        var lstDocumentosNuevos = new List<EPDocumento>();
                        var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        var fechaVencD = dbsEXD_OEPG.GetValueExt("U_FECHA_VENC");
                        var fechaVencH = dbsEXD_OEPG.GetValueExt("U_FECHA_VENH");
                        var monedaLoc = string.IsNullOrWhiteSpace(dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_LOC")) ? "XZY" : dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_LOC");
                        var monedaExt = string.IsNullOrWhiteSpace(dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_EXT")) ? "XZY" : dbsEXD_OEPG.GetValueExt("U_COD_MONEDA_EXT");
                        var codBanco = dbsEXD_OEPG.GetValueExt("U_COD_BANCO");
                        var codSucursal = dbsEXD_OEPG.GetValueExt("U_COD_SUCURSAL");
                        var codPrioridad = dbsEXD_OEPG.GetValueExt("U_COD_PRIORIDAD");
                        var codTipoDocumento = dbsEXD_OEPG.GetValueExt("U_TIPO_DOC");
                        var codAutorizarPor = dbsEXD_OEPG.GetValueExt("U_AUTORIZAR_POR");
                        var montoMinimo = Convert.ToDouble(dbsEXD_OEPG.GetValueExt("U_MONTO_MINIMO"));
                        var qry = string.Empty;

                        //Valido Pago de retenciones
                        if (tblConfPM.GetByKey("16") && tblConfPM.UserFields.Fields.Item("U_VALOR").Value == "Y")
                        {
                            qry = $"EXEC EXD_PM_VALIDAR_PAGO_RETENCIONES";
                            if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                                qry = $"CALL EXD_PM_VALIDAR_PAGO_RETENCIONES()";
                            recSet.DoQuery(qry);
                            var nrosPM = string.Empty;
                            while (!recSet.EoF)
                            {
                                nrosPM += recSet.Fields.Item("DocNum").Value + "\n";
                                recSet.MoveNext();
                            }

                            Globales.Aplication.MessageBox("Los siguientes numeros de pago masivo tienen documentos con retencion pendientes de pago: \n" + nrosPM + "regularicelos para crear mas escenarios");

                            return false;
                        }

                        var tipoDocFiltro = codAutorizarPor;
                        if (codAutorizarPor == "VR")
                            if (codTipoDocumento != "NN")
                                tipoDocFiltro = codTipoDocumento;

                        if (!tieneSucursales && monedaLoc.Equals("XZY") && monedaExt.Equals("XZY"))
                        {
                            Globales.Aplication.StatusBar.SetText("Seleccione al menos una cuenta de banco", SAPbouiCOM.BoMessageTime.bmt_Short,
                                SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        dttFac.Rows.Clear();
                        mtxFact.LoadFromDataSourceEx();
                        qry = $"EXEC SMC_APM_LISTAR_FACPENDIENTES_PP '{fechaVencD}','{fechaVencH}','{monedaLoc}','{monedaExt}','','','{codBanco}','','{codSucursal}','{codPrioridad}','{tipoDocFiltro}','{montoMinimo}'";
                        if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                            qry = $"CALL SMC_APM_LISTAR_FACPENDIENTES_PP('{fechaVencD}','{fechaVencH}','{monedaLoc}','{monedaExt}','','','{codBanco}','','{codSucursal}','{codPrioridad}','{tipoDocFiltro}','{montoMinimo}')";
                        dttFac.ExecuteQuery(qry);
                        if (dttFac.Rows.Count == 1 && string.IsNullOrWhiteSpace(dttFac.GetValue("CardCode", 0).ToString()))
                        {
                            Globales.Aplication.StatusBar.SetText("La busqueda no obtuvo resultados", SAPbouiCOM.BoMessageTime.bmt_Short,
                                SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            //return false;
                        }
                        else
                        {
                            _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));
                            var strXMLDTDocs = dttFac.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                            _dsrXmlDTDocs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                            var lstDocumentosNuevosAux = _dsrXmlDTDocs.Rows.Select(r => new EPDocumento
                            {
                                CodSucursal = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodSucursal"))?.Value),
                                FILA = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("FILA"))?.Value),
                                DocEntry = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocEntry"))?.Value),
                                DocNum = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocNum"))?.Value,
                                FechaContable = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("FechaContable"))?.Value,
                                FechaVencimiento = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("FechaVencimiento"))?.Value,
                                CardCode = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardCode"))?.Value,
                                CardName = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CardName"))?.Value,
                                NumAtCard = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NumAtCard"))?.Value,
                                CodBancoPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodBancoPago"))?.Value,
                                NomBancoPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NomBancoPago"))?.Value,
                                MonedaPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("MonedaPago"))?.Value,
                                DocCur = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("DocCur"))?.Value,
                                Total = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Total"))?.Value),
                                CodigoRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodigoRetencion"))?.Value,
                                Retencion = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Retencion"))?.Value),
                                Saldo = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Saldo"))?.Value),
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
                                CodCtaPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodCtaPago"))?.Value,
                                NroCtaPago = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("NroCtaPago"))?.Value,
                                CodPrioridad = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("CodPrioridad"))?.Value,
                                AfectoRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("AfectoRetencion"))?.Value,
                                TieneRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("TieneRetencion"))?.Value,
                                AplicaRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("AplicaRetencion"))?.Value,
                                Comentarios = r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Comentarios"))?.Value,
                                RetencionMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("RetencionMS"))?.Value),
                                TotalMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("TotalMS"))?.Value),
                                SaldoMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("SaldoMS"))?.Value),
                                TotalPagarMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("TotalPagarMS"))?.Value),
                                EstadoExt = "P"
                            }).ToList();

                            strXMLDTDocs = dttProveedores.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly);
                            var _dsrXmlDTProvs = (XMLDataTable)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                            var lstProveedoresSlc = _dsrXmlDTProvs.Rows.Where(r => r.Cells.FirstOrDefault(c => c.ColumnUid.Equals("Slc")).Value == "Y")
                            .Select(r => r.Cells.FirstOrDefault(c => c.ColumnUid == "CardCode").Value).ToList();
                            if (!string.IsNullOrWhiteSpace(dbsEXD_OEPG.GetValueExt("U_COD_PROV"))) lstProveedoresSlc.Add(dbsEXD_OEPG.GetValueExt("U_COD_PROV"));
                            if (lstProveedoresSlc.Count > 0) lstDocumentosNuevosAux = lstDocumentosNuevosAux.Where(d => lstProveedoresSlc.Contains(d.CardCode)).ToList();
                            lstDocumentosNuevos.AddRange(lstDocumentosNuevosAux);
                        }

                        lstDocumentos.RemoveAll(d => d.EstadoExt == "P");
                        var lstSeleccionados = lstDocumentos.Where(d => d.EstadoExt == "S").ToList();
                        var lstSeleccionadosPendientes = lstDocumentos.Where(d => d.EstadoExt == "P").ToList();

                        if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE) lstSeleccionadosPendientes.Clear();

                        lstDocumentosNuevos = lstDocumentosNuevos.Where(d => !lstSeleccionados.Any(ds => ds.Documento == d.Documento && ds.DocEntry == d.DocEntry
                        && ds.NroCuota == d.NroCuota && ds.LineaAsiento == d.LineaAsiento)).ToList();


                        lstDocumentosNuevos = lstDocumentosNuevos.Where(d => !lstSeleccionadosPendientes.Any(ds => ds.Documento == d.Documento && ds.DocEntry == d.DocEntry
                           && ds.NroCuota == d.NroCuota && ds.LineaAsiento == d.LineaAsiento)).ToList();


                        lstDocumentos.Clear();
                        lstDocumentos.AddRange(lstDocumentosNuevos);
                        lstDocumentos.AddRange(lstSeleccionados);
                        lstDocumentos.AddRange(lstSeleccionadosPendientes);

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
                        var doc = lstDocumentos.FirstOrDefault(d => d.DocEntry == Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_DOCENTRY").Value)
                        && d.Documento == r.Cells.FirstOrDefault(c => c.Uid == "U_TIPO_DOCUMENTO").Value
                        && d.NroCuota == Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_NRO_CUOTA").Value)
                        && d.LineaAsiento == Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_NRO_LINEA_AS").Value));

                        doc.EstadoExt = "P";
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
                        try
                        {
                            var docEntry = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid == "DocEntry").Value);
                            var documento = r.Cells.FirstOrDefault(c => c.ColumnUid == "Documento").Value;
                            var nroCuota = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid == "NroCuota").Value);
                            var lineaAsiento = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.ColumnUid == "LineaAsiento").Value);
                            var docNum = r.Cells.FirstOrDefault(c => c.ColumnUid == "DocNum").Value;
                            var codCtaPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "CodCtaPago").Value;
                            var nroCtaPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "NroCtaPago").Value;
                            var montoRetencion = r.Cells.FirstOrDefault(c => c.ColumnUid == "Retencion").Value;

                            var doc = lstDocumentos.FirstOrDefault(d => d.DocEntry == docEntry
                            && d.Documento == documento
                            && d.NroCuota == nroCuota
                            && d.LineaAsiento == lineaAsiento);

                            if (doc == null) throw new Exception($"No se encontro documento de tipo: {documento} y id: {docNum}");
                            else if (r.Cells.FirstOrDefault(c => c.ColumnUid == "BloqueoPago").Value == "Y") throw new Exception($"El documento:{documento} - {docNum}, tiene bloqueo de pago");
                            else if (r.Cells.FirstOrDefault(c => c.ColumnUid == "DetraccionPend").Value == "Y") throw new Exception($"El documento:{documento} - {docNum}, tiene pendiente el pago de detracción");
                            else if (string.IsNullOrWhiteSpace(r.Cells.FirstOrDefault(c => c.ColumnUid == "NroCtaPago")?.Value.Trim())) throw new Exception($"En el documento:{documento} - {docNum}, no se ha asignado el número de cuenta del banco de pago");
                            else if (string.IsNullOrWhiteSpace(r.Cells.FirstOrDefault(c => c.ColumnUid == "Cuenta")?.Value.Trim())) throw new Exception($"En el documento:{documento} - {docNum}, el número de cuenta del banco del proveedor esta vacio");
                            else if (!EsCuentaPagoMasivo(nroCtaPago, codCtaPago)) throw new Exception($"El número de cuenta: {nroCtaPago}, no esta asiganada a pago masivo");

                            doc.EstadoExt = "S";
                            doc.MonedaPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "MonedaPago").Value;
                            doc.CodBancoPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "CodBancoPago").Value;
                            doc.NomBancoPago = r.Cells.FirstOrDefault(c => c.ColumnUid == "NomBancoPago").Value;
                            doc.CodCtaPago = codCtaPago;
                            doc.NroCtaPago = nroCtaPago;
                            doc.BankCode = r.Cells.FirstOrDefault(c => c.ColumnUid == "BankCode")?.Value;
                            doc.NombreBanco = r.Cells.FirstOrDefault(c => c.ColumnUid == "NombreBanco")?.Value;
                            doc.Cuenta = r.Cells.FirstOrDefault(c => c.ColumnUid == "Cuenta")?.Value;
                            doc.TotalPagar = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.ColumnUid == "TotalPagar").Value);
                            doc.Retencion = Convert.ToDouble(montoRetencion);
                        }
                        catch (Exception ex)
                        {
                            Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        }
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
                        var tipoDocumento = dbsEXD_OEPG.GetValueExt("U_AUTORIZAR_POR");
                        if (!TieneModeloAutorizacion(tipoDocumento))
                        {
                            Globales.Aplication.StatusBar.SetText("El tipo de documento seleccionado no tiene un modelo de autorización, no se puede enviar a aprobación", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        dbsEXD_OEPG.SetValue("U_ESTADO", 0, "E");
                        if (Form.Mode != SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                            Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                        btnAddSBO.Item.Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                        HabiltarControlesPorEstado("E");
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnAddSBO.Item.UniqueID, e =>
            {
                if (e.InnerEvent == false && (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE || Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE))
                {
                    if (e.BeforeAction)
                    {
                        if (lstDocumentos == null || lstDocumentos.Where(d => d.EstadoExt == "S").Count() == 0 || mtxSelc.VisualRowCount == 0
                        || string.IsNullOrWhiteSpace(((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fCard", 1)).Value))
                        {
                            Globales.Aplication.StatusBar.SetText("Agregue al menos un documento al escenario", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                    }
                    else if (!e.BeforeAction && e.ActionSuccess)
                    {
                        if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE) FormDataLoadAdd();
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

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_CLICK, mtxFact.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    if (e.Row > 0 && e.ColUID.Equals("lSelect") && !esSeleccionarTodo)
                    {
                        var filaSelec = ((SAPbouiCOM.CheckBox)mtxFact.GetCellSpecific("lSelect", e.Row)).Checked;
                        var bloqueoPgo = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_1", e.Row)).Value;
                        var detracPend = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_2", e.Row)).Value;
                        var codCtaBcoProv = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_3", e.Row)).Value;
                        var codBanco = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                        var nroCuenta = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_10", e.Row)).Value;

                        if (filaSelec) return true;
                        if (bloqueoPgo == "Y" || detracPend == "Y")
                        {
                            Globales.Aplication.MessageBox("No se puede seleccionar facturas con bloqueo de pago o con cuota de detracción pendientes");
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(codBanco))
                        {
                            Globales.Aplication.MessageBox("Seleccione un banco");
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(nroCuenta))
                        {
                            Globales.Aplication.MessageBox("Seleccione un nro de cuenta");
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(codCtaBcoProv))
                        {
                            Globales.Aplication.MessageBox("EL proveedor no tiene número de cuenta de banco");
                            return false;
                        }
                    }
                    else if (e.Row > 0 && e.ColUID.Equals("Col_10") && e.Row <= mtxFact.RowCount)
                    {
                        try
                        {
                            Form.Freeze(true);
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var codBanco = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                            var codSucursal = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_9", e.Row)).Value;
                            var codMoneda = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;
                            var sqlQry = $"select \"Account\",TX0.\"GLAccount\" from DSC1 TX0 inner join OACT TX1 on TX0.\"GLAccount\" = TX1.\"AcctCode\" " +
                            $"where TX0.\"BankCode\" = '{codBanco}' and coalesce(TX0.\"Branch\",'0') = '{codSucursal}' and TX0.\"UsrNumber4\" = '{codMoneda}' and coalesce(TX0.\"U_EXM_PMASIVO\",'') = 'Y'";

                            var cmbNroCtaPgo = (SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_10", e.Row);
                            while (cmbNroCtaPgo.ValidValues.Count > 0) cmbNroCtaPgo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            recSet.DoQuery(sqlQry);
                            while (!recSet.EoF)
                            {
                                cmbNroCtaPgo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                                recSet.MoveNext();
                            }
                        }
                        finally
                        {
                            Form.Freeze(false);
                        }

                    }
                    else if (e.Row > 0 && e.ColUID.Equals("Col_14"))
                    {
                        try
                        {
                            Form.Freeze(true);
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var codSucursal = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_9", e.Row)).Value;
                            var sqlQry = $"select distinct TX0.\"BankCode\",TX0.\"BankName\" from ODSC TX0 inner join DSC1 TX1 on TX0.\"BankCode\" = TX1.\"BankCode\" where coalesce(TX1.\"Branch\",'0') = '{codSucursal}' and coalesce(TX1.\"U_EXM_PMASIVO\",'') = 'Y'";
                            recSet.DoQuery(sqlQry);
                            while (mtxFact.Columns.Item("Col_14").ValidValues.Count > 0) mtxFact.Columns.Item("Col_14").ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            while (!recSet.EoF)
                            {
                                mtxFact.Columns.Item("Col_14").ValidValues.Add(recSet.Fields.Item(1).Value, recSet.Fields.Item(0).Value);
                                recSet.MoveNext();
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
                    else if (e.Row > 0 && e.ColUID.Equals("NBank"))
                    {
                        try
                        {
                            Form.Freeze(true);

                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var cardCode = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fCard", e.Row)).Value;
                            var sqlQry = $"select distinct T0.\"BankCode\" as \"Code\",T1.\"BankName\" from OCRB T0 inner join ODSC T1 on T0.\"BankCode\" = T1.\"BankCode\" where T0.\"CardCode\" = '{cardCode}' and coalesce(T0.U_EXD_PAGO_MASIVO,'') = 'Y'";
                            recSet.DoQuery(sqlQry);
                            while (mtxFact.Columns.Item("NBank").ValidValues.Count > 0) mtxFact.Columns.Item("NBank").ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            while (!recSet.EoF)
                            {
                                mtxFact.Columns.Item("NBank").ValidValues.Add(recSet.Fields.Item(1).Value, recSet.Fields.Item(0).Value);
                                recSet.MoveNext();
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
                    else if (e.Row > 0 && e.ColUID.Equals("Col_3"))
                    {
                        try
                        {
                            Form.Freeze(true);
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var codBankPago = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                            var codBankProv = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_4", e.Row)).Value;
                            var cardCode = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fCard", e.Row)).Value;
                            var codMonedaPago = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;

                            var sqlQry = $"select distinct \"Account\" from OCRB where \"CardCode\" = '{cardCode}' and \"BankCode\" = '{codBankProv}' and \"UsrNumber1\" = '{codMonedaPago}' and coalesce(U_EXD_PAGO_MASIVO,'')= 'Y'";

                            if (codBankPago != codBankProv)
                            {
                                sqlQry = $"select distinct U_EXM_INTERBANCARIA from OCRB where \"CardCode\" = '{cardCode}' and \"BankCode\" = '{codBankProv}' and \"UsrNumber1\" = '{codMonedaPago}' and coalesce(U_EXD_PAGO_MASIVO,'')= 'Y'";
                            }

                            recSet.DoQuery(sqlQry);
                            while (mtxFact.Columns.Item("Col_3").ValidValues.Count > 0) mtxFact.Columns.Item("Col_3").ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                            while (!recSet.EoF)
                            {
                                mtxFact.Columns.Item("Col_3").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(0).Value);
                                recSet.MoveNext();
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
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED, mtxFact.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.ColUID == "fEntry")
                {
                    var tipoDocumento = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fDoc", e.Row)).Value;
                    var idDocumento = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fEntry", e.Row)).Value;
                    AbrirLinkDocumento(tipoDocumento, idDocumento);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED, mtxSelc.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.ColUID == "fEntry")
                {
                    var tipoDocumento = ((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fDoc", e.Row)).Value;
                    var idDocumento = ((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fEntry", e.Row)).Value;
                    AbrirLinkDocumento(tipoDocumento, idDocumento);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, mtxFact.Item.UniqueID, e =>
            {
                if (!e.BeforeAction && e.ColUID == "Col_8")
                {
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var codBanco = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                    var codSucursal = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_9", e.Row)).Value;
                    var codMoneda = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;
                    var sqlQry = $"select \"Account\",TX0.\"GLAccount\" from DSC1 TX0 inner join OACT TX1 on TX0.\"GLAccount\" = TX1.\"AcctCode\" " +
                    $"where TX0.\"BankCode\" = '{codBanco}' and coalesce(TX0.\"Branch\",'0') = coalesce('{codSucursal}','0') and TX0.\"UsrNumber4\" = '{codMoneda}' and coalesce(U_EXM_PMASIVO,'') = 'Y'";


                    var cmbNroCtaPgo = (SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_10", e.Row);
                    var edtCodCtaPgo = (SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_11", e.Row);

                    while (cmbNroCtaPgo.ValidValues.Count > 0) cmbNroCtaPgo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    recSet.DoQuery(sqlQry);
                    if (recSet.RecordCount > 0)
                    {
                        while (!recSet.EoF)
                        {
                            cmbNroCtaPgo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                            edtCodCtaPgo.Value = recSet.Fields.Item(1).Value;
                            recSet.MoveNext();
                        }
                    }
                    else
                    {
                        cmbNroCtaPgo.ValidValues.Add("", "");
                        edtCodCtaPgo.Value = string.Empty;
                    }
                    cmbNroCtaPgo.SelectExclusive(0, SAPbouiCOM.BoSearchKey.psk_Index);

                    var codMonedaPago = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;
                    var codBancoPago = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                    var codBancoProv = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_4", e.Row)).Value;
                    var codProveedor = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fCard", e.Row)).Value;

                    /*
                    sqlQry = $"select coalesce(\"Account\",'') as \"CC\",coalesce(\"U_EXM_INTERBANCARIA\",'') as \"CCI\" " +
                    $"from OCRB where \"CardCode\" = '{codProveedor}' and \"BankCode\" = '{codBancoProv}' and coalesce(\"UsrNumber1\",'') = '{codMonedaPago}' and coalesce(\"U_EXC_ACTIVO\",'') = 'Y'";
                    recSet.DoQuery(sqlQry);
                    ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_3", e.Row)).Value = "";
                    if (!recSet.EoF) ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_3", e.Row)).Value = (codBancoProv == codBancoPago ? recSet.Fields.Item(0).Value : recSet.Fields.Item(1).Value);
                    */

                    mtxFact.FlushToDataSource();

                    dttFac.SetValue("BankCode", e.Row - 1, "");
                    dttFac.SetValue("NombreBanco", e.Row - 1, "");
                    dttFac.SetValue("Cuenta", e.Row - 1, "");

                    sqlQry = $"select coalesce(\"Account\",'') as \"CC\",coalesce(U_EXM_INTERBANCARIA,'') as \"CCI\" from " +
                    $"OCRB where \"CardCode\" = '{codProveedor}' and \"BankCode\" = '{codBancoPago}' and coalesce(\"UsrNumber1\",'') = '{codMonedaPago}' " +
                    $"and coalesce(U_EXD_PAGO_MASIVO,'') = 'Y' and coalesce(\"Account\",'') <> '' ";
                    recSet.DoQuery(sqlQry);

                    if (recSet.RecordCount > 0)
                    {
                        dttFac.SetValue("BankCode", e.Row - 1, codBancoPago);
                        dttFac.SetValue("NombreBanco", e.Row - 1, dttFac.GetValue("NomBancoPago", e.Row - 1));
                        dttFac.SetValue("Cuenta", e.Row - 1, recSet.Fields.Item(0).Value.ToString());
                    }

                    mtxFact.LoadFromDataSourceEx();
                }
                else if (e.ColUID == "Col_10")
                {
                    var cmbNroCtaPgo = (SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_10", e.Row);
                    var edtCodCtaPgo = (SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_11", e.Row);

                    edtCodCtaPgo.Value = cmbNroCtaPgo.Selected?.Description;
                }
                else if (e.ColUID == "Col_14")
                {
                    if (e.BeforeAction)
                    {
                        /*
                        var codBancoProv = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_", e.Row)).Value;
                        if (string.IsNullOrWhiteSpace(codBancoProv))
                        {
                            Globales.Aplication.MessageBox("El proveedor no tiene definida ninguna cuenta de banco");
                            return false;
                        }
                        */
                    }
                    else
                    {
                        try
                        {

                            Form.Freeze(true);

                            var cmbBancoPago = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific(e.ColUID, e.Row));
                            var edtCodBancoPago = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row));
                            var codMonedaPago = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;
                            edtCodBancoPago.Value = cmbBancoPago.Selected.Description;

                            //Nro de cuenta del proveedor
                            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                            var codBancoPago = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_12", e.Row)).Value;
                            //var codBancoProv = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_4", e.Row)).Value;
                            var codProveedor = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fCard", e.Row)).Value;


                            mtxFact.FlushToDataSource();

                            dttFac.SetValue("BankCode", e.Row - 1, "");
                            dttFac.SetValue("NombreBanco", e.Row - 1, "");
                            dttFac.SetValue("Cuenta", e.Row - 1, "");

                            var sqlQry = $"select coalesce(\"Account\",'') as \"CC\",coalesce(U_EXM_INTERBANCARIA,'') as \"CCI\" from " +
                            $"OCRB where \"CardCode\" = '{codProveedor}' and \"BankCode\" = '{codBancoPago}' and coalesce(\"UsrNumber1\",'') = '{codMonedaPago}' " +
                            $"and coalesce(U_EXD_PAGO_MASIVO,'') = 'Y' and coalesce(\"Account\",'') <> '' ";
                            recSet.DoQuery(sqlQry);

                            if (recSet.RecordCount > 0)
                            {
                                dttFac.SetValue("BankCode", e.Row - 1, codBancoPago);
                                dttFac.SetValue("NombreBanco", e.Row - 1, dttFac.GetValue("NomBancoPago", e.Row - 1));
                                dttFac.SetValue("Cuenta", e.Row - 1, recSet.Fields.Item(0).Value.ToString());
                            }
                            else
                            {
                                sqlQry = $"select T0.\"BankCode\",T1.\"BankName\",coalesce(T0.U_EXM_INTERBANCARIA,'') as \"CCI\" from " +
                                $"OCRB T0 inner join ODSC T1 on T0.\"BankCode\" =T1.\"BankCode\" where \"CardCode\" = '{codProveedor}' and coalesce(\"UsrNumber1\",'') = '{codMonedaPago}' " +
                                $"and coalesce(U_EXD_PAGO_MASIVO,'') = 'Y' and coalesce(U_EXC_ACTIVO,'') = 'Y' and coalesce(\"Account\",'') <> '' ";
                                recSet.DoQuery(sqlQry);
                                if (recSet.RecordCount > 0)
                                {
                                    dttFac.SetValue("BankCode", e.Row - 1, recSet.Fields.Item(0).Value.ToString());
                                    dttFac.SetValue("NombreBanco", e.Row - 1, recSet.Fields.Item(1).Value.ToString());
                                    dttFac.SetValue("Cuenta", e.Row - 1, recSet.Fields.Item(2).Value.ToString());
                                }
                            }

                            mtxFact.LoadFromDataSourceEx();

                            /*
                            ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_3", e.Row)).Value = "";
                            if (!recSet.EoF) ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_3", e.Row)).Value = (codBancoProv == codBancoPago ? recSet.Fields.Item(0).Value : recSet.Fields.Item(1).Value);
                            */

                            //Cuentas del banco seleccionado    
                            var codSucursal = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_9", e.Row)).Value;
                            var codMoneda = ((SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_8", e.Row)).Value;
                            sqlQry = $"select \"Account\",TX0.\"GLAccount\" from DSC1 TX0 inner join OACT TX1 on TX0.\"GLAccount\" = TX1.\"AcctCode\" " +
                            $"where TX0.\"BankCode\" = '{codBancoPago}' and coalesce(TX0.\"Branch\",'0') = coalesce('{codSucursal}','0') and TX0.\"UsrNumber4\" = '{codMoneda}' and coalesce(U_EXM_PMASIVO,'') = 'Y'";

                            var cmbNroCtaPgo = (SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("Col_10", e.Row);
                            var edtCodCtaPgo = (SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_11", e.Row);

                            while (cmbNroCtaPgo.ValidValues.Count > 0) cmbNroCtaPgo.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);

                            recSet.DoQuery(sqlQry);
                            if (recSet.RecordCount > 0)
                            {
                                while (!recSet.EoF)
                                {
                                    cmbNroCtaPgo.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                                    edtCodCtaPgo.Value = recSet.Fields.Item(1).Value;
                                    recSet.MoveNext();
                                }
                            }
                            else
                            {
                                cmbNroCtaPgo.ValidValues.Add("", "");
                                edtCodCtaPgo.Value = string.Empty;
                            }
                            cmbNroCtaPgo.SelectExclusive(0, SAPbouiCOM.BoSearchKey.psk_Index);

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
                else if (!e.BeforeAction && e.ColUID == "NBank")
                {
                    try
                    {
                        Form.Freeze(true);

                        var edtCodBankProv = (SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_4", e.Row);
                        var cmbBancoProv = (SAPbouiCOM.ComboBox)mtxFact.GetCellSpecific("NBank", e.Row);
                        edtCodBankProv.Value = cmbBancoProv.Selected?.Description;

                        mtxFact.FlushToDataSource();
                        dttFac.SetValue("Cuenta", e.Row - 1, "");
                        mtxFact.LoadFromDataSourceEx();
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Form.Freeze(false);
                    }

                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_VALIDATE, mtxSelc.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    if (e.ColUID == "fTotalP")
                    {
                        var total = Convert.ToDouble(((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("Col_13", e.Row)).Value);
                        var totalPagar = double.TryParse(((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fTotalP", e.Row)).Value, out var totalPagarAux) ? totalPagarAux : 0.00;
                        var totMonLoc = 0.00;
                        var totMonExt = 0.00;

                        if (totalPagar <= 0 || totalPagar > total)
                        {
                            Globales.Aplication.StatusBar.SetText("Ingrese un monto valido", SAPbouiCOM.BoMessageTime.bmt_Short);
                            return false;
                        }
                        for (int i = 0; i < mtxSelc.RowCount; i++)
                        {
                            var moneda = ((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fMon", i + 1)).Value;
                            if (moneda != codMonedaLocal)
                            {
                                totMonExt += Convert.ToDouble(((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fTotalP", i + 1)).Value);
                            }
                            else
                            {
                                totMonLoc += Convert.ToDouble(((SAPbouiCOM.EditText)mtxSelc.GetCellSpecific("fTotalP", i + 1)).Value);
                            }
                        }
                        udsTOTAL.ValueEx = totMonLoc.ToString();
                        udsTOTAL_USD.ValueEx = totMonExt.ToString();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbTipoDocumento.Item.UniqueID, e =>
            {
                if (!e.BeforeAction) btnBuscar.Item.Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, cmbAutorizarPor.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    Form.Items.Item("Item_34").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    dbsEXD_OEPG.SetValueExt("U_TIPO_DOC", "NN");
                    Form.Items.Item("Item_32").Enabled = dbsEXD_OEPG.GetValueExt("U_AUTORIZAR_POR").Equals("VR");
                    btnBuscar.Item.Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, mtxFact.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.Row == 0 && e.ColUID == "lSelect")
                {
                    try
                    {
                        esSeleccionarTodo = true;
                        Form.Freeze(true);
                        mtxFact.Columns.Item("lSelect").Cells.Item(mtxFact.RowCount).Click(SAPbouiCOM.BoCellClickType.ct_Regular, (int)SAPbouiCOM.BoModifiersEnum.mt_SHIFT);
                        mtxFact.Columns.Item("lSelect").Cells.Item(1).Click(SAPbouiCOM.BoCellClickType.ct_Regular, (int)SAPbouiCOM.BoModifiersEnum.mt_SHIFT);
                    }
                    catch (Exception ex)
                    {
                        Globales.Aplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    }
                    finally
                    {
                        Form.Freeze(false);
                        esSeleccionarTodo = false;
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_VALIDATE, mtxFact.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    if (e.BeforeAction)
                    {
                        if (e.ColUID == "fTotalP")
                        {
                            Form.Freeze(true);
                            var total = Convert.ToDouble(((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_15", e.Row)).Value);
                            var totalPagar = double.TryParse(((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("fTotalP", e.Row)).Value, out var totalPagarAux) ? totalPagarAux : 0.00;


                            if (totalPagar <= 0 || totalPagar > total)
                            {
                                Globales.Aplication.StatusBar.SetText("Ingrese un monto valido", SAPbouiCOM.BoMessageTime.bmt_Short);
                                Form.Freeze(false);
                                return false;
                            }

                            var codRetencion = ((SAPbouiCOM.EditText)mtxFact.GetCellSpecific("Col_0", e.Row)).Value;
                            if (!string.IsNullOrWhiteSpace(codRetencion))
                            {
                                if (total < 700 && (total != totalPagar))
                                {
                                    Globales.Aplication.StatusBar.SetText("En documentos con retencion menores a 700, el pago debe ser igual al saldo", SAPbouiCOM.BoMessageTime.bmt_Short);
                                    Form.Freeze(false);
                                    return false;
                                }

                                var owht = (SAPbobsCOM.WithholdingTaxCodes)Globales.Company.GetBusinessObject(BoObjectTypes.oWithholdingTaxCodes);
                                owht.GetByKey(codRetencion);
                                var tasaNetaRetencion = (owht.Rate / 118);


                                ((SAPbouiCOM.EditText)mtxFact.Columns.Item("fReten").Cells.Item(e.Row).Specific).Value = (totalPagar * tasaNetaRetencion).ToString();
                                mtxFact.FlushToDataSource();
                                mtxFact.LoadFromDataSourceEx();
                            }

                            Form.Freeze(false);
                        }
                    }
                    return true;
                }
                return true;
            }));

            //DataEvents **
            Eventos.Add(new EventoData(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD, this.Form.TypeEx, e =>
            {
                if (!e.BeforeAction)
                {
                    var cancelado = dbsEXD_OEPG.GetValueExt("Canceled") == "Y";
                    // Form.Mode = "PR".Contains(dbsEXD_OEPG.GetValueExt("U_ESTADO")) ? SAPbouiCOM.BoFormMode. : SAPbouiCOM.BoFormMode.fm_VIEW_MODE;
                    btnEnvApr.Item.Enabled = "PR".Contains(dbsEXD_OEPG.GetValueExt("U_ESTADO"));
                    dbsEXD_OEPG.SetValueExt("U_ESTADO", cancelado ? "N" : dbsEXD_OEPG.GetValueExt("U_ESTADO"));
                    HabiltarControlesPorEstado(dbsEXD_OEPG.GetValueExt("U_ESTADO"));
                    dttFac.Rows.Clear();
                    mtxFact.LoadFromDataSource();
                    _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
                    var strXMLDTDocs = dbsEXD_EPG1.GetAsXML();
                    _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));
                    lstDocumentos = _dsrXmlDBDataSource.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_CARDCODE"))?.Value)).Select(r => new EPDocumento
                    {
                        FILA = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_ID_LINEA"))?.Value ?? "0"),
                        DocEntry = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_DOCENTRY"))?.Value ?? "0"),
                        DocNum = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NUM_SAP"))?.Value,
                        FechaVencimiento = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_FECHA_VENCIMIENTO"))?.Value,
                        FechaContable = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_FECHA_DOCUMENTO"))?.Value,
                        CardCode = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_CARDCODE"))?.Value,
                        CardName = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_CARDNAME"))?.Value,
                        NumAtCard = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NUMERO_DOC"))?.Value ?? "",
                        BankCode = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_BANCO"))?.Value ?? "",
                        NombreBanco = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_BANCO_PROV"))?.Value ?? "",
                        DocCur = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_MONEDA"))?.Value ?? "",
                        Origen = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_ORIGEN"))?.Value ?? "",
                        CodigoRetencion = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_RETENCION"))?.Value ?? "",
                        GlosaAsiento = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_GLOSA_ASIENTO"))?.Value ?? "",
                        BloqueoPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_BLOQUEO_PAGO"))?.Value ?? "N",
                        DetraccionPend = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_DETRA_PENDIENTE"))?.Value ?? "N",
                        NroCuota = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NRO_CUOTA"))?.Value),
                        LineaAsiento = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NRO_LINEA_AS"))?.Value),
                        CardCodeFactoring = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_PROV_FACTO"))?.Value ?? "",
                        CardNameFactoring = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NOM_PROV_FACTO"))?.Value ?? "",
                        MonedaPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_MONEDA_PAGO"))?.Value ?? "",
                        Cuenta = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_CUENTA_PROV"))?.Value ?? "",
                        Atraso = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_ATRASO"))?.Value ?? "",
                        Documento = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TIPO_DOCUMENTO"))?.Value ?? "",
                        Total = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TOTAL"))?.Value),
                        Saldo = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_SALDO"))?.Value),
                        TotalPagar = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TOTAL_PAGO"))?.Value),
                        Retencion = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_RETENCION"))?.Value),
                        CodSucursal = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_SUCURSAL"))?.Value),
                        CodCtaPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_CTA_PAGO"))?.Value ?? "",
                        NroCtaPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NRO_CTA_PAGO"))?.Value ?? "",
                        CodBancoPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_BANCO_PAGO"))?.Value ?? "",
                        NomBancoPago = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_NOM_BANCO_PAGO"))?.Value ?? "",
                        CodPrioridad = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COD_PRIORIDAD"))?.Value ?? "",
                        AfectoRetencion = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_AFECTO_RETENCION"))?.Value ?? "",
                        TieneRetencion = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TIENE_RETENCION"))?.Value ?? "",
                        AplicaRetencion = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_APLICA_RETENCION"))?.Value ?? "",
                        Comentarios = r.Cells.FirstOrDefault(c => c.Uid.Equals("U_COMENTARIOS"))?.Value ?? "",
                        RetencionMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_RETENCION_MS"))?.Value),
                        TotalMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TOTAL_MS"))?.Value),
                        SaldoMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_SALDO_MS"))?.Value),
                        TotalPagarMS = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid.Equals("U_TOTAL_PAGO_MS"))?.Value),
                        RUC = string.Empty,
                        CuentaMoneda = string.Empty,
                        Estado = string.Empty,
                        Marca = string.Empty,
                        EstadoExt = "S"
                    }).ToList();

                    udsSALDO.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "P" && d.DocCur == codMonedaLocal).Sum(d => d.TotalPagar).ToString();
                    udsSALDO_USD.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "P" && d.DocCur == "USD").Sum(d => d.TotalPagar).ToString();
                    udsTOTAL.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "S" && d.DocCur == codMonedaLocal).Sum(d => d.TotalPagar).ToString();
                    udsTOTAL_USD.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "S" && d.DocCur == "USD").Sum(d => d.TotalPagar).ToString();
                }
                return true;
            }));
        }

        internal void ValidarAnulacionEscenario()
        {
            if (tieneAutorizaciones)
            {
                var estado = dbsEXD_OEPG.GetValueExt("U_ESTADO", 0);
                if (estado == "A") throw new Exception("No se puede anular un escenario autorizado");
            }
        }

        internal void LoadDataOnFormAddMode()
        {
            FormDataLoadAdd();
        }

        internal void FormDataLoadAdd()
        {
            dbsEXD_OEPG.SetValueExt("U_CNT_AUT", "0");
            dbsEXD_OEPG.SetValueExt("U_ESTADO", tieneAutorizaciones ? "P" : "A");
            dbsEXD_OEPG.SetValueExt("U_MEDIO_DE_PAGO", "TB");
            dbsEXD_OEPG.SetValueExt("U_COD_BANCO", "000");
            dbsEXD_OEPG.SetValueExt("U_FECHA_PAGO", DateTime.Today.ToString("yyyyMMdd"));
            dbsEXD_OEPG.SetValueExt("U_FECHA_VENC", DateTime.Today.ToString("yyyyMMdd"));
            dbsEXD_OEPG.SetValueExt("U_FECHA_VENH", DateTime.Today.ToString("yyyyMMdd"));
            dbsEXD_OEPG.SetValueExt("U_COD_SUCURSAL", "-1");
            dbsEXD_OEPG.SetValueExt("U_AUTORIZAR_POR", "VR");
            dbsEXD_OEPG.SetValueExt("U_TIPO_DOC", "NN");
            dbsEXD_OEPG.SetValueExt("U_MONTO_MINIMO", "0.00");
            cmbSeries.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (cmbSeries.ValidValues.Count > 0) cmbSeries.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dbsEXD_OEPG.SetValueExt("DocNum", Form.BusinessObject.GetNextSerialNumber(dbsEXD_OEPG.GetValueExt("Series"), Form.BusinessObject.Type).ToString());
            udsSALDO.ValueEx = "0.00";
            udsTOTAL.ValueEx = "0.00";
            udsSALDO_USD.ValueEx = "0.00";
            udsTOTAL_USD.ValueEx = "0.00";
            lstDocumentos = new List<EPDocumento>();
            HabiltarControlesPorEstado(dbsEXD_OEPG.GetValueExt("U_ESTADO"));
        }

        private void LoadMatrixByStatus()
        {
            _dsrXmlDTDocs = new XMLDataTable();
            _dsrXmlDBDataSource = new XMLDBDataSource();
            _xmlSerializer = new XmlSerializer(typeof(XMLDataTable));

            //Totales ++

            udsSALDO.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "P" && d.DocCur == codMonedaLocal).Sum(d => d.TotalPagar).ToString();
            udsSALDO_USD.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "P" && d.DocCur == "USD").Sum(d => d.TotalPagar).ToString();
            udsTOTAL.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "S" && d.DocCur == codMonedaLocal).Sum(d => d.TotalPagar).ToString();
            udsTOTAL_USD.ValueEx = lstDocumentos.Where(d => d.EstadoExt == "S" && d.DocCur == "USD").Sum(d => d.TotalPagar).ToString();

            _dsrXmlDTDocs.Rows = lstDocumentos.Where(d => d.EstadoExt == "P").Select(d => new Row
            {
                Cells = new List<Cell>
                        {
                            new Cell{ ColumnUid = "FILA", Value = d.FILA.ToString()},
                            new Cell{ ColumnUid = "CodSucursal", Value = d.CodSucursal.ToString()},
                            new Cell{ ColumnUid = "CodPrioridad", Value = d.CodPrioridad.ToString()},
                            new Cell{ ColumnUid = "DocEntry", Value = d.DocEntry.ToString()},
                            new Cell{ ColumnUid = "DocNum", Value = d.DocNum},
                            new Cell{ ColumnUid = "FechaContable", Value = d.FechaContable},
                            new Cell{ ColumnUid = "FechaVencimiento", Value = d.FechaVencimiento},
                            new Cell{ ColumnUid = "CardCode", Value = d.CardCode },
                            new Cell{ ColumnUid = "CardName", Value = d.CardName },
                            new Cell{ ColumnUid = "NumAtCard", Value = d.NumAtCard },
                            new Cell{ ColumnUid = "CodBancoPago", Value = d.CodBancoPago },
                            new Cell{ ColumnUid = "NomBancoPago", Value = d.NomBancoPago },
                            new Cell{ ColumnUid = "MonedaPago", Value = d.MonedaPago},
                            new Cell{ ColumnUid = "NroCtaPago", Value = d.NroCtaPago},
                            new Cell{ ColumnUid = "CodCtaPago", Value = d.CodCtaPago},
                            new Cell{ ColumnUid = "DocCur", Value = d.DocCur },
                            new Cell{ ColumnUid = "Total", Value = d.Total.ToString() },
                            new Cell{ ColumnUid = "CodigoRetencion", Value = d.CodigoRetencion },
                            new Cell{ ColumnUid = "Retencion", Value = d.Retencion.ToString() },
                            new Cell{ ColumnUid = "Saldo", Value = d.Saldo.ToString() },
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
                            new Cell{ ColumnUid = "AfectoRetencion", Value = d.AfectoRetencion },
                            new Cell{ ColumnUid = "TieneRetencion", Value = d.TieneRetencion },
                            new Cell{ ColumnUid = "AplicaRetencion", Value = d.AplicaRetencion },
                            new Cell{ ColumnUid = "Comentarios", Value = d.Comentarios },
                            new Cell{ ColumnUid = "RetencionMS", Value = d.RetencionMS.ToString() },
                            new Cell{ ColumnUid = "TotalMS", Value = d.TotalMS.ToString() },
                            new Cell{ ColumnUid = "SaldoMS", Value = d.SaldoMS.ToString() },
                            new Cell{ ColumnUid = "TotalPagarMS", Value = d.TotalPagarMS.ToString() }

                        }.ToArray()
            }).ToArray();
            using (var strWritter = new StringWriter())
            {
                _dsrXmlDTDocs.Uid = "DT_FAC";
                _xmlSerializer.Serialize(strWritter, _dsrXmlDTDocs);
                var verTmp = strWritter.ToString();
                dttFac.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, strWritter.ToString());
                mtxFact.LoadFromDataSource();
                mtxFact.AutoResizeColumns();
            }

            _dsrXmlDBDataSource.Rows = lstDocumentos.Where(d => d.EstadoExt == "S").Select(d => new RowDBS
            {
                Cells = new List<CellDBS>
                        {
                            new CellDBS{ Uid = "U_ID_LINEA", Value = d.FILA.ToString()},
                            new CellDBS{ Uid = "U_DOCENTRY", Value = d.DocEntry.ToString()},
                            new CellDBS{ Uid = "U_NUM_SAP", Value = d.DocNum},
                            new CellDBS{ Uid = "U_FECHA_DOCUMENTO", Value = d.FechaContable },
                            new CellDBS{ Uid = "U_FECHA_VENCIMIENTO", Value = d.FechaVencimiento },
                            new CellDBS{ Uid = "U_CARDCODE", Value = d.CardCode },
                            new CellDBS{ Uid = "U_CARDNAME", Value = d.CardName },
                            new CellDBS{ Uid = "U_NUMERO_DOC", Value = d.NumAtCard },
                            new CellDBS{ Uid = "U_COD_BANCO_PAGO", Value = d.CodBancoPago },
                            new CellDBS{ Uid = "U_NOM_BANCO_PAGO", Value = d.NomBancoPago },
                            new CellDBS{ Uid = "U_MONEDA_PAGO", Value = d.MonedaPago },
                            new CellDBS{ Uid = "U_COD_CTA_PAGO", Value = d.CodCtaPago},
                            new CellDBS{ Uid = "U_NRO_CTA_PAGO", Value = d.NroCtaPago},
                            new CellDBS{ Uid = "U_COD_BANCO", Value = d.BankCode },
                            new CellDBS{ Uid = "U_BANCO_PROV", Value = d.NombreBanco },
                            new CellDBS{ Uid = "U_CUENTA_PROV", Value = d.Cuenta },
                            new CellDBS{ Uid = "U_MONEDA", Value = d.DocCur },
                            new CellDBS{ Uid = "U_ORIGEN", Value = d.Origen ?? "" },
                            new CellDBS{ Uid = "U_TOTAL", Value = d.Total.ToString() },
                            new CellDBS{ Uid = "U_COD_RETENCION", Value = d.CodigoRetencion },
                            new CellDBS{ Uid = "U_RETENCION", Value = d.Retencion.ToString() },
                            new CellDBS{ Uid = "U_SALDO", Value = d.Saldo.ToString() },
                            new CellDBS{ Uid = "U_TOTAL_PAGO", Value = d.TotalPagar.ToString() },
                            new CellDBS{ Uid = "U_GLOSA_ASIENTO", Value = d.GlosaAsiento },
                            new CellDBS{ Uid = "U_ATRASO", Value = d.Atraso },
                            new CellDBS{ Uid = "U_NRO_CUOTA", Value = d.NroCuota.ToString() },
                            new CellDBS{ Uid = "U_NRO_LINEA_AS", Value = d.LineaAsiento.ToString() },
                            new CellDBS{ Uid = "U_COD_PROV_FACTO", Value = d.CardCodeFactoring },
                            new CellDBS{ Uid = "U_NOM_PROV_FACTO", Value = d.CardNameFactoring },
                            new CellDBS{ Uid = "U_TIPO_DOCUMENTO", Value = d.Documento },
                            new CellDBS{ Uid = "U_COD_SUCURSAL", Value = d.CodSucursal.ToString()},
                            new CellDBS{ Uid = "U_COD_PRIORIDAD", Value = d.CodPrioridad.ToString()},
                            new CellDBS{ Uid = "U_AFECTO_RETENCION", Value = d.AfectoRetencion },
                            new CellDBS{ Uid = "U_TIENE_RETENCION", Value = d.TieneRetencion},
                            new CellDBS{ Uid = "U_APLICA_RETENCION", Value = d.AplicaRetencion},
                            new CellDBS{ Uid = "U_COMENTARIOS", Value = d.Comentarios},
                            new CellDBS{ Uid = "U_RETENCION_MS", Value = d.RetencionMS.ToString() },
                            new CellDBS{ Uid = "U_TOTAL_MS", Value = d.TotalMS.ToString() },
                            new CellDBS{ Uid = "U_SALDO_MS", Value = d.SaldoMS.ToString() },
                            new CellDBS{ Uid = "U_TOTAL_PAGO_MS", Value = d.TotalPagarMS.ToString() }
                        }.ToArray()
            }).ToArray();

            _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDBDataSource);
                var verTmp = strWritter.ToString();
                dbsEXD_EPG1.LoadFromXML(strWritter.ToString());
                mtxSelc.LoadFromDataSource();
                mtxSelc.AutoResizeColumns();
            }
        }

        private void AbrirLinkDocumento(string tipo, string docEntry)
        {
            if (tipo == "NC-C")
                Globales.Aplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_InvoiceCreditMemo, "", docEntry.ToString());
            else if (tipo == "FT-P")
                Globales.Aplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_PurchaseInvoice, "", docEntry);
            else if (tipo == "FA-P")
                Globales.Aplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", docEntry);
            else if (tipo == "SA-P")
                Globales.Aplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", docEntry);
            else if (tipo == "AS")
                Globales.Aplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", docEntry);
            else if (tipo == "SP")
                Globales.Aplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)140, "", docEntry);
            else if (tipo == "PR")
                Globales.Aplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", docEntry);
        }

        private void HabiltarControlesPorEstado(string estado)
        {
            var activar = estado == "P" || estado == "R" || Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.GetItem("Item_24").Enabled = false;
            Form.GetItem("txtFecha").Enabled = activar;
            Form.GetItem("Item_14").Enabled = activar;
            //Form.GetItem("cmbBanco").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.GetItem("Item_12").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.GetItem("Item_5").Enabled = activar;
            Form.GetItem("Item_23").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.GetItem("txtFechaF").Enabled = activar;
            Form.GetItem("Item_28").Enabled = activar;
            Form.GetItem("txtProve").Enabled = activar;
            Form.GetItem("FiltroBank").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            Form.GetItem("btnAddPrv").Enabled = activar;
            Form.GetItem("btnBuscar").Enabled = activar;
            Form.GetItem("Item_11").Enabled = activar;
            Form.GetItem("Item_19").Enabled = activar;
            Form.GetItem("Item_32").Enabled = activar && dbsEXD_OEPG.GetValueExt("U_AUTORIZAR_POR").Equals("VR"); ;
            Form.GetItem("Item_34").Enabled = activar;
            Form.GetItem("Item_13").Enabled = Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE;

            Form.GetItem("mtxFact").Enabled = activar;
            Form.GetItem("mtxSelect").Enabled = activar;
            Form.GetItem("btnAgg").Enabled = activar;
            Form.GetItem("btnQuit").Enabled = activar;
        }

        private bool EsCuentaPagoMasivo(string account, string glAccount)
        {
            var sqlQry = $"select 'A' from DSC1 where \"Account\" = '{account}' and \"GLAccount\" = '{glAccount}' and coalesce(\"U_EXM_PMASIVO\",'') = 'Y'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            return !recSet.EoF;
        }

        public bool TieneModeloAutorizacion(string codTipoDocumento)
        {
            var sqlQry = $"select 'E' from \"@EXD_PM_CONFAUT\" where \"U_TIPO_DOC\" = '{codTipoDocumento}'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            return !recSet.EoF;
        }

        public void HabilitarControlesEnModoBuscar()
        {
            Form.Items.Item("Item_24").Enabled = true;
        }
    }
}