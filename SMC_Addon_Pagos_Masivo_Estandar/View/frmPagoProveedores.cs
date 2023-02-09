using System;
using System.Collections.Generic;
using SMC_APM.SAP;
using SMC_APM.dto;
using SMC_APM.dao;
using SMC_APM.Controladores;
using SMC_APM.Controller;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using SAPbouiCOM;
using SMC_APM.View.USRForms;

namespace SMC_APM.View
{
    class frmPagoProveedores
    {
        #region Atributos

        //inicializacion de variables
        private SAPbouiCOM.Application sboApplication;
        private SAPbobsCOM.Company sboCompany;
        ctrFrmLiberarTerceroRet _ctrFrmLiberarTerceroRet = null;
        private SAPbouiCOM.Form oForm;

        private SAPbouiCOM.Item oItemlblBanco;
        private SAPbouiCOM.Item oItemlblMoneda;
        private SAPbouiCOM.Item oItemlblFecha;
        private SAPbouiCOM.Item oItemlblProve;
        private SAPbouiCOM.Item oItemlblMon;
        private SAPbouiCOM.StaticText lblBanco;
        private SAPbouiCOM.StaticText lblMoneda;
        private SAPbouiCOM.StaticText lblFecha;
        private SAPbouiCOM.StaticText lblProve;
        private SAPbouiCOM.StaticText lblMon;

        private SAPbouiCOM.Item oItemtxtMoneda;
        private SAPbouiCOM.Item oItemtxtFecha;
        private SAPbouiCOM.Item oItemtxtFechaF;
        private SAPbouiCOM.Item oItemtxtProve;
        private SAPbouiCOM.Item oItemtxtBank;
        private SAPbouiCOM.Item oItemtxtSaldo;
        private SAPbouiCOM.Item oItemtxtEsc;
        private SAPbouiCOM.Item oItemtxtTotEsc;
        private SAPbouiCOM.EditText txtMoneda;
        private SAPbouiCOM.EditText txtFecha;
        private SAPbouiCOM.EditText txtFechaF;
        private SAPbouiCOM.EditText txtProve;
        private SAPbouiCOM.EditText txtBank;
        private SAPbouiCOM.EditText txtSaldo;
        private SAPbouiCOM.EditText txtEsc;
        private SAPbouiCOM.EditText txtTotEsc;

        private SAPbouiCOM.Item oItemcmbBanco;
        private SAPbouiCOM.Item oItemcmbFiltroBanco;
        private SAPbouiCOM.Item oItemcmbMon;
        private SAPbouiCOM.ComboBox cmbBanco;
        private SAPbouiCOM.ComboBox cmbFiltroBanco;
        private SAPbouiCOM.ComboBox cmbMon;

        private SAPbouiCOM.Item oItembtnBuscar;
        private SAPbouiCOM.Item oItembtnAgregar;
        private SAPbouiCOM.Item oItembtnQuitar;
        private SAPbouiCOM.Item oItembtnSig;
        private SAPbouiCOM.Button btnBuscar;
        private SAPbouiCOM.Button btnAgregar;
        private SAPbouiCOM.Button btnQuitar;
        private SAPbouiCOM.Button btnSig;

        private SAPbouiCOM.Item oItemmtxFact;
        private SAPbouiCOM.Item oItemmtxSelect;
        private SAPbouiCOM.Matrix mtxFact;
        //private SAPbouiCOM.Cell mtxFactCelda;
        private SAPbouiCOM.Matrix mtxSelect;

        private SAPbouiCOM.Item oItemtabDoc;
        private SAPbouiCOM.Folder tabDoc;
        private SAPbouiCOM.Item oItemtabSelect;
        private SAPbouiCOM.Folder tabSelect;

        private SAPbouiCOM.DataTable dtaFact;
        private SAPbouiCOM.DataTable dtaSelect;

        private SAPbouiCOM.UserDataSource dtuFecha;
        private SAPbouiCOM.UserDataSource dtuCheck;
        private SAPbouiCOM.UserDataSource dtuCheck2;

        private SAPbouiCOM.LinkedButton lnkPurchaseFac;
        private SAPbouiCOM.LinkedButton lnkPurchaseSelect;

        private List<dtoBanco> lstBancos;
        private List<dtoBanco> lstFiltroBancos;
        dtoBanco dtoBanco;
        List<string> lstDocEntry = new List<string>();
        daoBanco daoBanco;
        daoEscenario daoEscenario;
        sapMatrix _sapMatrix;
        string mensaje = "";
        string codEscenario = "";
        string tipoEscenario = "";
        string tipoBanco = "";
        string filtrobanco = "";
        List<dtoChlValues> lista;
        public const string PATH = "Resources/frmSMC_PM_PagoProveedores.srf";
        #endregion

        #region Constructor
        public frmPagoProveedores(SAPbouiCOM.Application sboApplication, SAPbobsCOM.Company sboCompany, string formUID, string Tipo)
        {
            //carga el formulario de sap
            this.sboApplication = sboApplication;
            this.sboCompany = sboCompany;
            sapObjetos sapObj = new sapObjetos();
            _sapMatrix = new sapMatrix(sboApplication, sboCompany);
            //carga formulario pasandolo como parametro el formulario creado en sap studio (XML)

            string xml = GetXMLFromForm(PATH);

            sapObj.sapCargarFormulario(this.sboApplication, formUID, xml, "");
            this.oForm = sboApplication.Forms.Item(formUID);
            this.tipoEscenario = Tipo;
            iniciaFormulario(Tipo);
        }

        private string GetXMLFromForm(string filePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filePath);
                return xmlDocument.InnerXml;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Metodos Privados
        private void iniciaFormulario(string Tipo)
        {
            sboApplication.StatusBar.SetText("Cargando formulario, espere por favor...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            #region Objetos.Net  
            lstBancos = new List<dtoBanco>();
            lstFiltroBancos = new List<dtoBanco>();
            daoBanco = new daoBanco();
            daoEscenario = new daoEscenario();
            if (tipoEscenario.Equals("PRO"))
            {
                lstBancos = daoBanco.listarBancos(sboApplication, ref mensaje); // consulta cuentas de los bancos
                lstFiltroBancos = daoBanco.listarFiltroBancos(sboApplication, ref mensaje); // consulta bancos para filtrarlo
            }
            else
            {
                lstBancos = daoBanco.listarBancosDet(sboApplication, ref mensaje);
            }
            #endregion

            //inicializa los compmonentes
            #region UIComponents
            oItemlblBanco = oForm.Items.Item("lblBanco");
            lblBanco = (SAPbouiCOM.StaticText)oItemlblBanco.Specific;
            oItemlblMoneda = oForm.Items.Item("lblMoneda");
            lblMoneda = (SAPbouiCOM.StaticText)oItemlblMoneda.Specific;
            oItemlblFecha = oForm.Items.Item("lblFecha");
            lblFecha = (SAPbouiCOM.StaticText)oItemlblFecha.Specific;
            oItemlblProve = oForm.Items.Item("lblProve");
            lblProve = (SAPbouiCOM.StaticText)oItemlblProve.Specific;
            oItemlblMon = oForm.Items.Item("lblMon");
            lblMon = (SAPbouiCOM.StaticText)oItemlblMon.Specific;

            oItemtxtMoneda = oForm.Items.Item("txtMoneda");
            txtMoneda = (SAPbouiCOM.EditText)oItemtxtMoneda.Specific;
            oItemtxtFecha = oForm.Items.Item("txtFecha");
            txtFecha = (SAPbouiCOM.EditText)oItemtxtFecha.Specific;
            oItemtxtFechaF = oForm.Items.Item("txtFechaF");
            txtFechaF = (SAPbouiCOM.EditText)oItemtxtFechaF.Specific;
            oItemtxtProve = oForm.Items.Item("txtProve");
            txtProve = (SAPbouiCOM.EditText)oItemtxtProve.Specific;
            oItemtxtBank = oForm.Items.Item("txtBank");
            txtBank = (SAPbouiCOM.EditText)oItemtxtBank.Specific;
            oItemtxtSaldo = oForm.Items.Item("txtSaldo");
            txtSaldo = (SAPbouiCOM.EditText)oItemtxtSaldo.Specific;
            oItemtxtEsc = oForm.Items.Item("txtEsc");
            txtEsc = (SAPbouiCOM.EditText)oItemtxtEsc.Specific;
            oItemtxtTotEsc = oForm.Items.Item("txtTotEsc");
            txtTotEsc = (SAPbouiCOM.EditText)oItemtxtTotEsc.Specific;

            oItemcmbBanco = oForm.Items.Item("cmbBanco");
            cmbBanco = (SAPbouiCOM.ComboBox)oItemcmbBanco.Specific;

            oItemcmbFiltroBanco = oForm.Items.Item("FiltroBank");
            cmbFiltroBanco = (SAPbouiCOM.ComboBox)oItemcmbFiltroBanco.Specific;

            oItemcmbMon = oForm.Items.Item("cmbMon");
            cmbMon = (SAPbouiCOM.ComboBox)oItemcmbMon.Specific;

            oItembtnBuscar = oForm.Items.Item("btnBuscar");
            btnBuscar = (SAPbouiCOM.Button)oItembtnBuscar.Specific;
            oItembtnAgregar = oForm.Items.Item("btnAgregar");
            btnAgregar = (SAPbouiCOM.Button)oItembtnAgregar.Specific;
            oItembtnQuitar = oForm.Items.Item("btnQuitar");
            btnQuitar = (SAPbouiCOM.Button)oItembtnQuitar.Specific;
            oItembtnSig = oForm.Items.Item("btnSig");
            btnSig = (SAPbouiCOM.Button)oItembtnSig.Specific;

            oItemmtxFact = oForm.Items.Item("mtxFact");
            mtxFact = (SAPbouiCOM.Matrix)oItemmtxFact.Specific;

            oItemmtxSelect = oForm.Items.Item("mtxSelect");
            mtxSelect = (SAPbouiCOM.Matrix)oItemmtxSelect.Specific;

            oItemtabDoc = oForm.Items.Item("tabDoc");
            tabDoc = (SAPbouiCOM.Folder)oItemtabDoc.Specific;

            dtaFact = oForm.DataSources.DataTables.Item("dtaFact");
            dtaSelect = oForm.DataSources.DataTables.Item("dtaSelect");

            ComboBox cmbMedioPago = oForm.Items.Item("Item_5").Specific;
            cmbMedioPago.ExpandType = BoExpandType.et_DescriptionOnly;

            cmbMedioPago.Select(0, BoSearchKey.psk_Index);

            ComboBox cmbEstado = oForm.Items.Item("Item_7").Specific;
            cmbEstado.ExpandType = BoExpandType.et_DescriptionOnly;

            cmbEstado.Select("P", BoSearchKey.psk_ByValue);


            dtuFecha = oForm.DataSources.UserDataSources.Item("dtuFecha");
            dtuCheck = oForm.DataSources.UserDataSources.Item("dtuCheck");
            dtuCheck2 = oForm.DataSources.UserDataSources.Item("dtuCheck2");

            #endregion

            #region Textos
            txtMoneda.Item.Enabled = false;
            txtBank.Item.Enabled = false;
            txtSaldo.Item.Enabled = false;
            if (tipoEscenario.Equals("DET"))
            {
                //oItemlblMon.Visible = true;
                //oItemcmbMon.Visible = true;
            }
            #endregion



            //lee la lista de los bancos y llena el combo
            #region ComboBox
            for (int i = 0; i < lstBancos.Count; i++)
            {
                cmbBanco.ValidValues.Add(lstBancos[i].CuentaCont, lstBancos[i].AcctName);
            }
            cmbBanco.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
            cmbBanco.Item.DisplayDesc = true;

            cmbMon.ValidValues.Add("USD", "USD");
            cmbMon.ValidValues.Add("SOL", "SOL");
            cmbMon.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
            cmbMon.Item.DisplayDesc = true;
            cmbMon.Select("USD", SAPbouiCOM.BoSearchKey.psk_ByValue);
            #endregion


            //llena el combo de filtro de bancos 
            #region ComboBox2
            for (int i = 0; i < lstFiltroBancos.Count; i++)
            {
                cmbFiltroBanco.ValidValues.Add(lstFiltroBancos[i].BankCode, lstFiltroBancos[i].BankName);
            }
            cmbFiltroBanco.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly;
            cmbFiltroBanco.Item.DisplayDesc = true;

            #endregion


            //inicializa las columnas del datatable de la matrix pendientes
            #region DataSources
            dtaFact.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("DocEntry", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("DocNum", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("FechaContable", SAPbouiCOM.BoFieldsType.ft_Date);
            dtaFact.Columns.Add("FechaVencimiento", SAPbouiCOM.BoFieldsType.ft_Date);
            dtaFact.Columns.Add("CardCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("CardName", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("NumAtCard", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("DocCur", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("CodigoRetencion", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Retencion", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("TotalPagar", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("RUC", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Cuenta", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("CuentaMoneda", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("BankCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Atraso", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Marca", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Estado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

            dtaFact.Columns.Add("Documento", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("NombreBanco", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("Origen", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaFact.Columns.Add("BloqueoPago", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
            dtaFact.Columns.Add("DetraccionPend", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 1);
          
            dtaFact.Columns.Add("NroCuota", SAPbouiCOM.BoFieldsType.ft_ShortNumber, 4);
            dtaFact.Columns.Add("LineaAsiento", SAPbouiCOM.BoFieldsType.ft_ShortNumber, 4);

            //inicializa las columnas del datatable de la matrix seleccionados
            dtaSelect.Columns.Add("FILA", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("DocEntry", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("DocNum", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("FechaContable", SAPbouiCOM.BoFieldsType.ft_Date, 254);
            dtaSelect.Columns.Add("FechaVencimiento", SAPbouiCOM.BoFieldsType.ft_Date, 254);
            dtaSelect.Columns.Add("CardCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("CardName", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("NumAtCard", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("DocCur", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Total", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("CodigoRetencion", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Retencion", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("TotalPagar", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("RUC", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Cuenta", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("CuentaMoneda", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("BankCode", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Atraso", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Marca", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Estado", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);

            dtaSelect.Columns.Add("Documento", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("NombreBanco", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            dtaSelect.Columns.Add("Origen", SAPbouiCOM.BoFieldsType.ft_AlphaNumeric, 254);
            
            dtaSelect.Columns.Add("NroCuota", SAPbouiCOM.BoFieldsType.ft_ShortNumber, 4);
            dtaSelect.Columns.Add("LineaAsiento", SAPbouiCOM.BoFieldsType.ft_ShortNumber, 4);
            #endregion

            #region Matrices
            mtxFact.Columns.Item("lSelect").ValOn = "Y";
            mtxFact.Columns.Item("lSelect").ValOff = "N";
            oItemmtxFact.ToPane = 1;
            oItemmtxFact.FromPane = 1;

            mtxSelect.Columns.Item("lSelect").ValOn = "Y";
            mtxSelect.Columns.Item("lSelect").ValOff = "N";
            oItemmtxSelect.ToPane = 2;
            oItemmtxSelect.FromPane = 2;


            //relaciona la columna del databable con la columna de la matrix para los pendinetes
            mtxFact.Columns.Item("#").DataBind.Bind("dtaFact", "FILA");
            mtxFact.Columns.Item("fEntry").DataBind.Bind("dtaFact", "DocEntry");
            mtxFact.Columns.Item("fNumSap").DataBind.Bind("dtaFact", "DocNum");
            mtxFact.Columns.Item("fFecCon").DataBind.Bind("dtaFact", "FechaContable");
            mtxFact.Columns.Item("fFecVen").DataBind.Bind("dtaFact", "FechaVencimiento");
            mtxFact.Columns.Item("fCard").DataBind.Bind("dtaFact", "CardCode");
            mtxFact.Columns.Item("fRZ").DataBind.Bind("dtaFact", "CardName");
            mtxFact.Columns.Item("lblNum").DataBind.Bind("dtaFact", "NumAtCard");
            mtxFact.Columns.Item("fMon").DataBind.Bind("dtaFact", "DocCur");
            mtxFact.Columns.Item("fTotal").DataBind.Bind("dtaFact", "Total");
            mtxFact.Columns.Item("Col_0").DataBind.Bind("dtaFact", "CodigoRetencion");
            mtxFact.Columns.Item("fReten").DataBind.Bind("dtaFact", "Retencion");
            mtxFact.Columns.Item("fTotalP").DataBind.Bind("dtaFact", "TotalPagar");
            mtxFact.Columns.Item("fAtraso").DataBind.Bind("dtaFact", "Atraso");

            mtxFact.Columns.Item("fDoc").DataBind.Bind("dtaFact", "Documento");//new
            mtxFact.Columns.Item("NBank").DataBind.Bind("dtaFact", "NombreBanco");//new
            mtxFact.Columns.Item("origen").DataBind.Bind("dtaFact", "Origen");//new

            mtxFact.Columns.Item("Col_1").DataBind.Bind("dtaFact", "BloqueoPago");//BLOQUEO PAGO
            mtxFact.Columns.Item("Col_2").DataBind.Bind("dtaFact", "DetraccionPend");//DETRACCIÓN PENDIENTE
            mtxFact.Columns.Item("Col_3").DataBind.Bind("dtaFact", "Cuenta");//DETRACCIÓN PENDIENTE

            mtxFact.Columns.Item("clmNroCuo").DataBind.Bind("dtaFact", "NroCuota");
            mtxFact.Columns.Item("clmNrLnAS").DataBind.Bind("dtaFact", "LineaAsiento");
            mtxFact.Columns.Item("Col_4").DataBind.Bind("dtaFact", "BankCode"); //BANCO PROVEEDOR



            //relaciona la columna del databable con la columna de la matrix para los selecionados
            mtxSelect.Columns.Item("#").DataBind.Bind("dtaSelect", "FILA");
            mtxSelect.Columns.Item("fEntry").DataBind.Bind("dtaSelect", "DocEntry");
            mtxSelect.Columns.Item("fNumSap").DataBind.Bind("dtaSelect", "DocNum");
            mtxSelect.Columns.Item("fFecCon").DataBind.Bind("dtaSelect", "FechaContable");
            mtxSelect.Columns.Item("fFecVen").DataBind.Bind("dtaSelect", "FechaVencimiento");
            mtxSelect.Columns.Item("fCard").DataBind.Bind("dtaSelect", "CardCode");
            mtxSelect.Columns.Item("fRZ").DataBind.Bind("dtaSelect", "CardName");
            mtxSelect.Columns.Item("lblNum").DataBind.Bind("dtaSelect", "NumAtCard");
            mtxSelect.Columns.Item("fMon").DataBind.Bind("dtaSelect", "DocCur");
            mtxSelect.Columns.Item("fTotal").DataBind.Bind("dtaSelect", "Total");
            mtxSelect.Columns.Item("Col_0").DataBind.Bind("dtaSelect", "CodigoRetencion");
            mtxSelect.Columns.Item("fReten").DataBind.Bind("dtaSelect", "Retencion");
            mtxSelect.Columns.Item("fTotalP").DataBind.Bind("dtaSelect", "TotalPagar");
            mtxSelect.Columns.Item("fAtraso").DataBind.Bind("dtaSelect", "Atraso");

            mtxSelect.Columns.Item("fDoc").DataBind.Bind("dtaSelect", "Documento");//new
            mtxSelect.Columns.Item("NBank").DataBind.Bind("dtaSelect", "NombreBanco");//new
            mtxSelect.Columns.Item("origen").DataBind.Bind("dtaSelect", "Origen");//new

            mtxSelect.Columns.Item("clmNroCuo").DataBind.Bind("dtaSelect", "NroCuota");
            mtxSelect.Columns.Item("clmNrLnAS").DataBind.Bind("dtaSelect", "LineaAsiento");
            mtxSelect.Columns.Item("Col_1").DataBind.Bind("dtaSelect", "Cuenta");
            mtxSelect.Columns.Item("Col_2").DataBind.Bind("dtaSelect", "BankCode");

            SAPbouiCOM.Column oColumn1 = mtxSelect.Columns.Item("fTotal");
            SAPbouiCOM.Column oColumn2 = mtxSelect.Columns.Item("fReten");
            oColumn1.Visible = false;

            #endregion


            //#region Busquedas
            //lnkPurchaseFac = (SAPbouiCOM.LinkedButton)mtxFact.Columns.Item("fEntry").ExtendedObject;
            //lnkPurchaseFac.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_None;
            //lnkPurchaseFac.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_PurchaseInvoice;

            //lnkPurchaseSelect = (SAPbouiCOM.LinkedButton)mtxSelect.Columns.Item("fEntry").ExtendedObject;
            //lnkPurchaseSelect.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_None;
            //lnkPurchaseSelect.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_PurchaseInvoice;
            //#endregion



            #region Buttons
            oItembtnAgregar.ToPane = 1;
            oItembtnAgregar.FromPane = 1;

            oItembtnQuitar.ToPane = 2;
            oItembtnQuitar.FromPane = 2;
            #endregion

            #region TabPanel
            oItemtabSelect = oForm.Items.Add("tabSelec", SAPbouiCOM.BoFormItemTypes.it_FOLDER);
            oItemtabDoc.Width = 120;
            oItemtabSelect.Top = oItemtabDoc.Top;
            oItemtabSelect.Height = oItemtabDoc.Height;
            oItemtabSelect.Width = oItemtabDoc.Width;
            oItemtabSelect.Left = oItemtabDoc.Width;
            tabSelect = (SAPbouiCOM.Folder)oItemtabSelect.Specific;
            tabSelect.Caption = "Seleccionados";
            tabSelect.GroupWith("tabDoc");

            tabDoc.Item.FontSize = 11;
            tabSelect.Item.FontSize = 11;
            //tabDoc.AutoPaneSelection = true;
            #endregion


            //if (tipoEscenario.Equals("PRO"))
            //    oForm.Title = "SMC - Pagos Masivos Proveedores";
            //else
            //    oForm.Title = "SMC - Pagos Masivos Detracciones";

            string estado = cmbEstado.Value;
            oForm.Mode = estado == "E" || estado == "R" ? BoFormMode.fm_VIEW_MODE : BoFormMode.fm_OK_MODE;

            oForm.Left = 350;
            oForm.Top = 10;
            oForm.PaneLevel = 1;
            oForm.Visible = true;
        }
        #endregion

        #region Metodos Publicos
        public void registrarItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                switch (pVal.BeforeAction)
                {
                    case false:
                        switch (pVal.EventType)
                        {
                            case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST:
                                switch (pVal.ItemUID)
                                {
                                    //no se utiliza
                                    case "txtProve":
                                        lista = new List<dtoChlValues>();
                                        lista.Add(new dtoChlValues("", "CardCode"));
                                        _sapMatrix.callTxtEventosListas(pVal, lista);
                                        break;
                                }
                                break;
                            case SAPbouiCOM.BoEventTypes.et_CLICK:
                                switch (pVal.ItemUID)
                                {
                                    case "btnUpdate":
                                        //actualiza fecha de pago
                                        daoEscenario.actualizarFechaPago(codEscenario, sboApplication, txtFecha.Value.ToString(), ref mensaje);
                                        sboApplication.MessageBox("Se actualizó la fecha de pago correctamente");
                                        break;
                                    case "btnBuscar":
                                        //buscar documentos
                                        if (txtEsc.Value.ToString().Equals(""))
                                        {
                                            codEscenario = "";
                                        }
                                        else
                                        {
                                            codEscenario = txtEsc.Value.ToString();
                                        }

                                        if (txtFechaF.Value.ToString().Equals(""))
                                        {
                                            sboApplication.MessageBox("Debe indicar la Fecha de Vencimiento para filtrar los comprobantes");
                                            return;
                                        }
                                        try
                                        {
                                            string valid = cmbBanco.Selected.ToString();
                                        }
                                        catch
                                        {
                                            sboApplication.MessageBox("Debe indicar un Banco para filtrar los comprobantes");
                                            return;
                                        }
                                        //buscar documentos
                                        this.consultarFact();
                                        break;
                                    case "btnAgregar":

                                        //agregar documentos
                                        if (txtFecha.Value.ToString().Equals(""))
                                        {
                                            sboApplication.MessageBox("Debe indicar la Fecha de Pago para agregar los comprobantes");
                                            return;
                                        }

                                        //creacion de hilo del metodo agregar documentos
                                        Thread th = new Thread(new ThreadStart(agregarDoc));
                                        th.Start();


                                        break;
                                    case "btnQuitar":

                                        //quitar documentos
                                        this.quitarDoc(Properties.Resources.nomFormulario1, codEscenario);
                                        break;
                                    case "btnCar":

                                        //cargar el escenario
                                        if (txtEsc.Value.ToString().Equals(""))
                                        {
                                            sboApplication.MessageBox("Debe indicar el código del escenario de pago");
                                            return;
                                        }

                                        try
                                        {
                                            string estado = "P";
                                            codEscenario = txtEsc.Value.ToString();
                                            dtoBanco = daoBanco.consultarBanco(codEscenario, sboApplication, ref mensaje);
                                            cmbBanco.Select(dtoBanco.CuentaCont);
                                            //cmbFiltroBanco.Select(dtoBanco.BankCode);
                                            txtMoneda.Value = dtoBanco.Moneda;
                                            txtBank.Value = dtoBanco.BankName;
                                            txtSaldo.Value = dtoBanco.Saldo.ToString();
                                            txtFecha.Value = dtoBanco.FechaPago.ToString("yyyyMMdd");
                                            txtFechaF.Value = "";

                                            ComboBox cmb;

                                            if (!string.IsNullOrEmpty(dtoBanco.MedioPago))
                                            {
                                                cmb = oForm.Items.Item("Item_5").Specific;
                                                cmb.Select(dtoBanco.MedioPago, BoSearchKey.psk_ByValue);
                                            }

                                            if (!string.IsNullOrEmpty(dtoBanco.Estado))
                                            {
                                                cmb = oForm.Items.Item("Item_7").Specific;
                                                cmb.Select(dtoBanco.Estado, BoSearchKey.psk_ByValue);
                                                estado = cmb.Value;
                                            }


                                            //obtiene el total de escenario
                                            cargarTotalEscenario(codEscenario);
                                            //consulta los documentos
                                            consultarFact();

                                            oForm.Items.Item("Item_5").Enabled = dtoBanco.Estado == "P" || dtoBanco.Estado == "R";
                                            oForm.Items.Item("btnSig").Enabled = dtoBanco.Estado == "P";


                                            oForm.Mode = estado == "E" || estado == "A" ? BoFormMode.fm_VIEW_MODE : BoFormMode.fm_OK_MODE;
                                        }
                                        catch (Exception ex)
                                        {
                                            sboApplication.MessageBox(ex.Message.ToString());
                                            return;
                                            throw;
                                        }

                                        break;

                                    case "btnSig":

                                        int rpta = sboApplication.MessageBox("Está a punto de enviar las facturas seleccionadas a aprobación. ¿Desea continuar?", 2, "Sí", "No");

                                        if (rpta == 1)
                                            EnviarAAutorizacion();

                                        //valida las autorizaciones
                                        //var ValidaAutorizacion = daoBanco.ObtenerAccion(codEscenario, sboApplication, ref mensaje);

                                        ////valida que tenga las dos Y para que continue
                                        //if ((ValidaAutorizacion[0] == "N" && ValidaAutorizacion[1] == "N")
                                        //    || (ValidaAutorizacion[0] == "Y" && ValidaAutorizacion[1] == "N")
                                        //    || (ValidaAutorizacion[0] == "N" && ValidaAutorizacion[1] == "Y"))
                                        //{
                                        //    sboApplication.MessageBox("Requiere Autorizacion");
                                        //    return;
                                        //}


                                        //consulta banco del escenario
                                        //dtoBanco = daoBanco.consultarBanco(codEscenario,sboApplication, ref mensaje);

                                        ////obtiene total del escenario
                                        //decimal TotalEscenario = obtenerTotalEscenario(codEscenario);

                                        //List<decimal> listatopes = new List<decimal>();
                                        ////consulta el monto tope 
                                        //listatopes = daoBanco.MontosTopes(sboApplication, ref mensaje);

                                        ////valida montos topes
                                        //if (dtoBanco.Moneda == "SOL")
                                        //{
                                        //    if (TotalEscenario > listatopes[0])
                                        //    {
                                        //        sboApplication.MessageBox("Se excedio el monto total");
                                        //        return;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    if (TotalEscenario > listatopes[1])
                                        //    {
                                        //        sboApplication.MessageBox("Se excedio el monto total");
                                        //        return;
                                        //    }
                                        //}


                                        ;

                                        ////inicia formulario
                                        //if (tipoEscenario.Equals("PRO"))
                                        //{
                                        //    ctrFrmDatosCuenta ctrFrmDatos = new ctrFrmDatosCuenta(sboApplication, sboCompany);
                                        //    try
                                        //    {
                                        //        sboApplication.Forms.Item("frmSMC2").Select();
                                        //        ctrFrmDatos.iniciarFormulario("frmSMC2", dtaSelect, txtMoneda.Value.ToString(), dtoBanco.CuentaBank, tipoEscenario,codEscenario, tipoBanco);
                                        //    }
                                        //    catch
                                        //    {
                                        //        ctrFrmDatos.cargarFormulario(dtaSelect, txtMoneda.Value.ToString(), dtoBanco.CuentaBank, tipoEscenario, "frmSMC2", codEscenario, tipoBanco);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    ctrFrmDatosCuenta ctrFrmDatos = new ctrFrmDatosCuenta(sboApplication, sboCompany);
                                        //    try
                                        //    {
                                        //        sboApplication.Forms.Item("frmSMC4").Select();
                                        //        ctrFrmDatos.iniciarFormulario("frmSMC4", dtaSelect, txtMoneda.Value.ToString(), dtoBanco.CuentaBank, tipoEscenario, codEscenario, tipoBanco);
                                        //    }
                                        //    catch
                                        //    {
                                        //        ctrFrmDatos.cargarFormulario(dtaSelect, txtMoneda.Value.ToString(), dtoBanco.CuentaBank, tipoEscenario, "frmSMC4", codEscenario, tipoBanco);
                                        //    }

                                        //}

                                        break;

                                    case "btnLib":

                                        //se ejecura el formulario de tercero retendor
                                        _ctrFrmLiberarTerceroRet = new ctrFrmLiberarTerceroRet(sboApplication, sboCompany);
                                        try
                                        {
                                            sboApplication.Forms.Item("frmSMC3").Select();
                                            _ctrFrmLiberarTerceroRet.iniciarFormulario("frmSMC3");
                                        }
                                        catch
                                        {
                                            _ctrFrmLiberarTerceroRet.cargarFormulario("frmSMC3");
                                        }
                                        break;
                                }
                                break;
                            case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED:
                                switch (pVal.ItemUID)
                                {
                                    case "tabSelec":
                                        oForm.PaneLevel = 2;
                                        break;
                                    case "tabDoc":
                                        oForm.PaneLevel = 1;
                                        break;


                                        //case "Item_10"://TEST LIBERACIÓN TERCERO

                                        //    var formUID = string.Concat(FormLiberarTercero.TYPE, new Random().Next(0, 1000));

                                        //    if (!UIFormFactory.FormUIDExists(formUID))
                                        //        UIFormFactory.AddUSRForm(formUID, new FormLiberarTercero(formUID));

                                        //    break;


                                }
                                break;

                            case SAPbouiCOM.BoEventTypes.et_COMBO_SELECT:
                                switch (pVal.ItemUID)
                                {
                                    case "cmbBanco":
                                        //ejecuta el evento select para para el combo de bancos
                                        for (int i = 0; i < lstBancos.Count; i++)
                                        {
                                            if (cmbBanco.Selected.Value.ToString().Equals(lstBancos[i].CuentaCont))
                                            {
                                                txtMoneda.Value = lstBancos[i].Moneda;
                                                txtBank.Value = lstBancos[i].BankName;
                                                txtSaldo.Value = lstBancos[i].Saldo.ToString();
                                                tipoBanco = lstBancos[i].BankCode.ToString();
                                            }
                                        }
                                        break;
                                    case "FiltroBank":
                                        //ejecuta el evento select para para el combo de fitrar bancos
                                        for (int i = 0; i < lstFiltroBancos.Count; i++)
                                        {
                                            if (cmbFiltroBanco.Selected.Value.ToString().Equals(lstFiltroBancos[i].BankCode))
                                            {
                                                filtrobanco = lstFiltroBancos[i].BankCode.ToString();
                                            }
                                        }
                                        break;
                                }
                                break;
                            case SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED: //evento al linkedbutton

                                if (pVal.ColUID == "fEntry")
                                {
                                    switch (pVal.ItemUID)
                                    {
                                        case "mtxSelect": //abre formulario identificando su documento 
                                            SAPbouiCOM.EditText campoDocEntry = null;

                                            int fila = pVal.Row;



                                            campoDocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fDoc").Cells.Item(fila).Specific;

                                            if (campoDocEntry.Value.ToString() == "NC-C")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_InvoiceCreditMemo, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry.Value.ToString() == "FT-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_PurchaseInvoice, "", DocEntry.Value.ToString());

                                            }
                                            else if (campoDocEntry.Value.ToString() == "FA-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry.Value.ToString() == "SA-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry.Value.ToString() == "AS")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry.Value.ToString() == "SP")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)140, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry.Value.ToString() == "PR")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(fila).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                            }

                                            break;
                                        case "mtxFact": //abre formulario identificando su documento 

                                            SAPbouiCOM.EditText campoDocEntry1 = null;

                                            int fila1 = pVal.Row;

                                            campoDocEntry1 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fDoc").Cells.Item(fila1).Specific;

                                            if (campoDocEntry1.Value.ToString() == "NC-C")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_InvoiceCreditMemo, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry1.Value.ToString() == "FT-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_PurchaseInvoice, "", DocEntry.Value.ToString());

                                            }
                                            else if (campoDocEntry1.Value.ToString() == "FA-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry1.Value.ToString() == "SA-P")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)204, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry1.Value.ToString() == "AS")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry1.Value.ToString() == "SP")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)140, "", DocEntry.Value.ToString());
                                            }
                                            else if (campoDocEntry1.Value.ToString() == "PR")
                                            {
                                                var DocEntry = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(fila1).Specific;
                                                sboApplication.OpenForm((SAPbouiCOM.BoFormObjectEnum)30, "", DocEntry.Value.ToString());
                                            }
                                            break;

                                    }
                                }


                                break;
                        }
                        break;

                    //BEFORE ACTION
                    case true:

                        switch (pVal.EventType)
                        {
                            case BoEventTypes.et_CLICK:

                                switch (pVal.ItemUID)
                                {
                                    case "mtxFact":

                                        if (pVal.ColUID == "lSelect" && pVal.Row > 0) //CLICK EN SELECCIONAR DOCUMENTO
                                        {
                                            Matrix matrix = oForm.Items.Item("mtxFact").Specific;
                                            CheckBox check = matrix.GetCellSpecific("lSelect", pVal.Row);
                                            if (!check.Checked)
                                            {
                                                string bloqueoPago = matrix.GetCellSpecific("Col_1", pVal.Row).Value;
                                                string detPendiente = matrix.GetCellSpecific("Col_2", pVal.Row).Value;

                                                BubbleEvent = (bloqueoPago == "N" && detPendiente == "N");
                                                if (!BubbleEvent)
                                                    sboApplication.MessageBox("No puede seleccionar facturas con bloqueo de pago o con cuota de detracción pendientes");
                                            }
                                        }

                                        break;
                                }

                                break;
                            case BoEventTypes.et_DOUBLE_CLICK:
                                break;

                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Error: ctrFrmProveedores.cs > registrarItemEvent(() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void EnviarAAutorizacion()
        {
            try
            {
                string codigoEscenario = oForm.Items.Item("txtEsc").Specific.Value;
                if (string.IsNullOrEmpty(codigoEscenario))
                    throw new Exception("Debe enviar a autorización un código de escenario de pago");

                daoEscenario.ActualizarEstadoAutorizacion(sboCompany, codigoEscenario, "A"); //AUTORIZAMOS AUTOMÁTICAMENTE PARA ANTES DE LA IMPLEMENTACIÓN DEL MÓDULO DE APROBACIONES
                oForm.Items.Item("btnCar").Click(BoCellClickType.ct_Regular); //VOLVEMOS A CARGAR LOS DATOS

                sboApplication.StatusBar.SetText("Escenario de pago enviado a autorización", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }
        #endregion

        #region Metodos Privados

        //lista los documentos pendientes 
        private void consultarFact()
        {
            string consulta = "";
            string consulta1 = "";





            try
            {
                btnBuscar.Item.Enabled = false;
                btnBuscar.Caption = "Cargando...";
                oForm.Freeze(true);

                dtaFact.Rows.Clear();
                dtaSelect.Rows.Clear();

                //asigna variables a la consulta
                if (tipoEscenario.Equals("PRO"))
                {
                    consulta = "CALL \"SMC_APM_LISTAR_FACPENDIENTES_PP\" ('" + txtFechaF.Value.ToString() + "','" + txtMoneda.Value.ToString() + "','" + txtProve.Value.ToString() + "','" + codEscenario + "','" + tipoBanco + "','" + filtrobanco + "') ";
                    consulta1 = "CALL \"SMC_APM_LISTAR_FACSELECT_PP\" ('" + txtFechaF.Value.ToString() + "','" + txtMoneda.Value.ToString() + "','" + txtProve.Value.ToString() + "','" + codEscenario + "','" + tipoBanco + "','" + filtrobanco + "') ";
                }
                else
                {
                    consulta = "CALL \"SMC_APM_LISTAR_FACPENDIENTES_DET\" ('" + txtFechaF.Value.ToString() + "','" + cmbMon.Selected.Value.ToString() + "','" + txtProve.Value.ToString() + "','" + codEscenario + "') ";
                    consulta1 = "CALL \"SMC_APM_LISTAR_FACSELECT_DET\" ('" + txtFechaF.Value.ToString() + "','" + cmbMon.Selected.Value.ToString() + "','" + txtProve.Value.ToString() + "','" + codEscenario + "') ";
                }

                //ejecuta consulta
                dtaFact.ExecuteQuery(consulta);
                dtaSelect.ExecuteQuery(consulta1);

                //limpia carga y ordena columnas del matrix
                mtxFact.Clear();
                mtxFact.LoadFromDataSource();
                mtxFact.AutoResizeColumns();

                mtxSelect.Clear();
                mtxSelect.LoadFromDataSource();
                mtxSelect.AutoResizeColumns();

                mtxSelect.Columns.Item("lblNum").TitleObject.Sortable = true;

                tabDoc.Select();

                oForm.Freeze(false);
                btnBuscar.Item.Enabled = true;
                btnBuscar.Caption = "Buscar";
            }
            catch (Exception ex)
            {

            }
        }

        public void eventFrmLiberarTercero(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            _ctrFrmLiberarTerceroRet.registrarItemEvent(FormUID, ref pVal, out BubbleEvent);
        }

        //agrea documento seleccionado
        private void agregarDoc()
        {
            SAPbouiCOM.CheckBox ocheckBox = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.EditText oEdit1 = null;
            SAPbouiCOM.EditText oEdit2 = null;
            SAPbouiCOM.EditText oEdit3 = null;
            SAPbouiCOM.EditText oEdit4 = null;
            SAPbouiCOM.EditText oEdit5 = null;
            SAPbouiCOM.EditText oEdit6 = null;

            ConexionDAO conexion = new ConexionDAO();
            string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);

            var UserNameConectado = sboCompany.UserName.ToString();

            try
            {
                btnAgregar.Item.Enabled = false;
                btnAgregar.Caption = "Cargando...";
                //oForm.Freeze(true);

                if (codEscenario.Equals(""))
                {
                    //registra cabecera por primera vez
                    string medioPago = oForm.DataSources.UserDataSources.Item("UD_MP").Value;
                    string estado = oForm.DataSources.UserDataSources.Item("UD_EST").Value;

                    if (string.IsNullOrEmpty(medioPago))
                        throw new Exception("Debe seleccionar el medio de pago");

                    codEscenario = daoEscenario.obtenerCodigo(NombreBaseDatos, ref mensaje);
                    daoEscenario.registrarCabecera(NombreBaseDatos, UserNameConectado, codEscenario, DateTime.Now.ToString("dd-MM-yyy") + " " + DateTime.Now.ToString("hh:mm:ss") + " " + txtMoneda.Value.ToString(),
                                cmbBanco.Selected.Value.ToString(), tipoEscenario, txtFecha.Value.ToString(), medioPago, estado, ref mensaje);
                }



                int rows = dtaFact.Rows.Count;


                //valida que el escenario este autorizado 
                if (codEscenario != "")
                {
                    var ValidaAutorizacion = daoBanco.ObtenerAccion(codEscenario, sboApplication, ref mensaje);
                    if ((ValidaAutorizacion[0] == "Y" && ValidaAutorizacion[1] == "Y"))
                    {
                        sboApplication.MessageBox("No puede realizar modificacion sobre un lote autorizado");
                        oForm.Freeze(false);
                        btnAgregar.Item.Enabled = true;
                        btnAgregar.Caption = "Agregar";
                        return;
                    }
                }


                //registra detalle
                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;


                    ocheckBox = (SAPbouiCOM.CheckBox)mtxFact.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        oEdit = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fEntry").Cells.Item(j).Specific;
                        oEdit1 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fTotalP").Cells.Item(j).Specific;
                        oEdit2 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("fDoc").Cells.Item(j).Specific;
                        oEdit3 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("clmNroCuo").Cells.Item(j).Specific;
                        oEdit4 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("clmNrLnAS").Cells.Item(j).Specific;
                        oEdit5 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("Col_3").Cells.Item(j).Specific;
                        oEdit6 = (SAPbouiCOM.EditText)mtxFact.Columns.Item("Col_4").Cells.Item(j).Specific;
                        //Debug.Print("Detalle: " + i + " Chequeado DocEntry: "+ oEdit.Value.ToString()+" Documento: "+ oEdit2.Value.ToString());

                        sboApplication.StatusBar.SetText(SMC_APM.Properties.Resources.nombreAddon + " Evaluando fila " + j + " DocEntry: " + oEdit.Value.ToString()
                    , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);


                        daoEscenario.registrarDetalle(NombreBaseDatos, UserNameConectado, codEscenario, oEdit.Value.ToString(), oEdit1.Value.ToString()
                            , oEdit2.Value.ToString(), Convert.ToInt32(oEdit3.Value), Convert.ToInt32(oEdit4.Value), oEdit5.Value , oEdit6.Value, ref mensaje);
                    }
                }


                txtEsc.Value = codEscenario;
                //actualiza total escenario
                cargarTotalEscenario(codEscenario);
                //consulta documetnos
                consultarFact();

                //oForm.Freeze(false);
                btnAgregar.Item.Enabled = true;
                btnAgregar.Caption = "Agregar";
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        //quita documetnos seleccionados
        private void quitarDoc(string FormUID, string codEscenario)
        {
            SAPbouiCOM.CheckBox ocheckBox = null;
            SAPbouiCOM.EditText oEdit = null;
            SAPbouiCOM.EditText oEdit1 = null;
            try
            {

                ConexionDAO conexion = new ConexionDAO();
                string NombreBaseDatos = conexion.BaseDatos(sboApplication, ref mensaje);



                btnQuitar.Item.Enabled = false;
                btnQuitar.Caption = "Cargando...";
                oForm.Freeze(true);

                int rows = dtaSelect.Rows.Count;

                //valida si el lote fue autorizado no se le agregue mas documentos

                var ValidaAutorizacion = daoBanco.ObtenerAccion(codEscenario, sboApplication, ref mensaje);
                if ((ValidaAutorizacion[0] == "Y" && ValidaAutorizacion[1] == "Y"))
                {
                    sboApplication.MessageBox("No puede realizar modificacion sobre un lote autorizado");
                    oForm.Freeze(false);
                    btnQuitar.Item.Enabled = true;
                    btnQuitar.Caption = "Quitar";
                    return;
                }



                //quita los documentos
                for (int j = 1; j <= rows; j++)
                {
                    int i = j - 1;
                    ocheckBox = (SAPbouiCOM.CheckBox)mtxSelect.Columns.Item("lSelect").Cells.Item(j).Specific;
                    if (ocheckBox.Checked == true)
                    {
                        oEdit = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fEntry").Cells.Item(j).Specific;
                        oEdit1 = (SAPbouiCOM.EditText)mtxSelect.Columns.Item("fDoc").Cells.Item(j).Specific;
                        daoEscenario.eliminarDetalle(NombreBaseDatos, codEscenario, oEdit.Value.ToString(), oEdit1.Value.ToString(), ref mensaje);
                    }
                }



                //actualiza total escenario
                cargarTotalEscenario(codEscenario);
                //consulta documetnos
                consultarFact();

                oForm.Freeze(false);
                btnQuitar.Item.Enabled = true;
                btnQuitar.Caption = "Quitar";

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }

        //consulta total del escenario
        private void cargarTotalEscenario(string codEscenario)
        {
            SAPbobsCOM.Recordset oRecord = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            oRecord.DoQuery("SELECT SUM(U_SMC_MONTO) FROM \"@SMC_APM_ESCDET\" WHERE U_SMC_ESCCAB = '" + codEscenario + "'");

            txtTotEsc.Value = oRecord.Fields.Item(0).Value.ToString();
        }

        //consulta total del escenario
        private decimal obtenerTotalEscenario(string codEscenario)
        {
            decimal Total = 0;
            SAPbobsCOM.Recordset oRecord = (SAPbobsCOM.Recordset)sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            oRecord.DoQuery("SELECT SUM(U_SMC_MONTO) FROM \"@SMC_APM_ESCDET\" WHERE U_SMC_ESCCAB = '" + codEscenario + "'");

            Total = decimal.Parse(oRecord.Fields.Item(0).Value.ToString());
            return Total;
        }

        #endregion
    }
}
