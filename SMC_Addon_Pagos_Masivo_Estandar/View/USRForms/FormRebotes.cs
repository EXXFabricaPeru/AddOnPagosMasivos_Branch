using SAP_AddonFramework;
using SAP_AddonExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SMC_APM.Modelo;
using System.IO;
using System.Threading;
using System.Xml;

namespace SMC_APM.View.USRForms
{
    public class FormRebotes : IUSAP
    {
        public const string TYPE = "FrmPMRbt";
        public const string UNQID = "FrmPMRbt";
        public const string MENU = "NN";
        public const string PATH = "Resources/FrmPMRebotes.srf";

        private SAPbouiCOM.DBDataSource dbsORBT = null;
        private SAPbouiCOM.DBDataSource dbsRBT1 = null;

        private SAPbouiCOM.ComboBox cmbSucursales = null;
        private SAPbouiCOM.ComboBox cmbBancos = null;
        private SAPbouiCOM.ComboBox cmbUsuario = null;

        private SAPbouiCOM.EditText edtFechaCreacion = null;

        private SAPbouiCOM.Button btnEjecutar = null;
        private SAPbouiCOM.Button btnBuscar = null;

        private SAPbouiCOM.Matrix mtxDocs = null;

        public FormRebotes(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
            CargarDatosAlFormulario();
        }
        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "Item_10", e =>
            {
                if (!e.BeforeAction)
                {
                    var nroPM = dbsORBT.GetValueExt("U_DOCENTRY_PM").Trim();
                    var codSucursal = dbsORBT.GetValueExt("U_COD_SUCURSAL").Trim();
                    var codBanco = dbsORBT.GetValueExt("U_COD_BANCO").Trim();
                    var codProveedor = dbsORBT.GetValueExt("U_CARDCODE").Trim();

                    if (string.IsNullOrWhiteSpace(nroPM))
                    {
                        Globales.Aplication.MessageBox("Ingrese un número de pago masivo");
                        return false;
                    }

                    codSucursal = string.IsNullOrWhiteSpace(codSucursal) ? "-1" : codSucursal;

                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var sqlQry = $"EXEC EXD_SP_PM_LISTAR_PAGOS_X_REBOTE '{nroPM}','{codSucursal}','{codBanco}','{codProveedor}'";
                    if (Globales.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        sqlQry = $"CALL EXD_SP_PM_LISTAR_PAGOS_X_REBOTE('{nroPM}','{codSucursal}','{codBanco}','{codProveedor}')";
                    recSet.DoQuery(sqlQry);
                    dbsRBT1.Clear();
                    mtxDocs.LoadFromDataSource();
                    if (recSet.RecordCount > 0)
                    {
                        LoadMatrixFromRecordSet(recSet);
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST, "Item_1", e =>
            {
                var cfle = (SAPbouiCOM.ChooseFromListEvent)e;
                if (!cfle.BeforeAction && cfle.SelectedObjects is SAPbouiCOM.DataTable dtbl)
                {
                    dbsORBT.SetValueExt("U_DOCENTRY_PM", ((int)dtbl.GetValue(0, 0)).ToString());
                    dbsORBT.SetValueExt("U_DOCNUM_PM", ((int)dtbl.GetValue("DocNum", 0)).ToString());
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST, "Item_9", e =>
            {
                var cfle = (SAPbouiCOM.ChooseFromListEvent)e;
                if (!cfle.BeforeAction && cfle.SelectedObjects is SAPbouiCOM.DataTable dtbl)
                {
                    dbsORBT.SetValueExt("U_CARDCODE", ((string)dtbl.GetValue(0, 0)).ToString());
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction)
                {

                    if (mtxDocs.VisualRowCount == 0)
                    {
                        Globales.Aplication.StatusBar.SetText("Debe agregar al menos un documento", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }

                    QuitarFilasNoSeleccionadas();
                    //mtxDocs.FlushToDataSource();

                    /*
                    var existeSeleccionado = false;
                    for (int i = 0; i < dbsRBT1.Size; i++)
                    {
                        if (dbsRBT1.GetValue("U_SELECCION", i) == "Y")
                        {
                            existeSeleccionado = true;
                            break;
                        }
                    }
                    if (!existeSeleccionado)
                    {
                        Globales.Aplication.StatusBar.SetText("Debe seleccionar al menos un documento", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                    */
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_CLICK, mtxDocs.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.Row > 0)
                {
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var docEntryPago = ((SAPbouiCOM.EditText)mtxDocs.GetCellSpecific("Col_6", e.Row)).Value;
                    var sqlQry = $"select 'E' from OVPM T0 inner join JDT1 T1 on T0.\"TransId\" = T1.\"TransId\" " +
                    $"and T0.\"DocEntry\" = '{docEntryPago}' and coalesce(T1.\"ExtrMatch\",'0') <> '0'";
                    recSet.DoQuery(sqlQry);
                    if (!recSet.EoF)
                    {
                        Globales.Aplication.StatusBar.SetText("Este pago tiene reconciliación, no se puede cancelar", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }
                return true;
            }));


            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnEjecutar.Item.UniqueID, e =>
            {
                if (!e.BeforeAction)
                {
                    var mntoTotalAnulado = 0.00;
                    var rslt = 0;
                    var idPM = Convert.ToInt32(dbsORBT.GetValueExt("U_DOCENTRY_PM"));

                    mtxDocs.FlushToDataSource();
                    var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
                    var strXMLDTDocs = dbsRBT1.GetAsXML();
                    var _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(new StringReader(strXMLDTDocs));

                    var lstDocsRebotes = _dsrXmlDBDataSource.Rows.Where(r => r.Cells.FirstOrDefault(c => c.Uid == "U_SELECCION").Value == "Y")
                    .Select(r => new PMDocumentoRebote
                    {
                        DocEntry = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_DOCENTRY").Value),
                        DocType = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_DOCTYPE").Value),
                        Importe = Convert.ToDouble(r.Cells.FirstOrDefault(c => c.Uid == "U_IMPORTE").Value),
                        DocEntryPago = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_ID_PAGO").Value),
                        DocEntryPago2 = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_ID_PAGO2").Value),
                        NroLineaPM = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_LINEA_PM").Value),
                        NroEP = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_DOCENTRY_EP").Value),
                        NroLineaEP = Convert.ToInt32(r.Cells.FirstOrDefault(c => c.Uid == "U_LINEA_EP").Value)
                    });

                    //Inicia proceso de cancelacion
                    var vendorPayment = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                    var vendorPaymentNew = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);

                    var lstRebotes = lstDocsRebotes.GroupBy(g => g.DocEntryPago2).Select(s =>
                    new
                    {
                        IDPagoSucursal = s.Key,
                        LstPagosDocumento = s.GroupBy(g2 => g2.DocEntryPago)
                        .Select(s2 => new
                        {
                            ID = s2.Key,
                            LstDocumentos = s2.Select(s3 => new { ID = s3.DocEntry, Tipo = s3.DocType, IdLineaPM = s3.NroLineaPM, DocEntryEP = s3.NroEP, IdLineaEP = s3.NroLineaEP }),
                            ImpTotal = s2.Sum(sm => sm.Importe)
                        }),
                        ImpTotal = s.Sum(sm => sm.Importe)
                    });

                    foreach (var rebote in lstRebotes)
                    {
                        foreach (var pagoDocumento in rebote.LstPagosDocumento)
                        {
                            if (vendorPayment.GetByKey(pagoDocumento.ID))
                            {
                                //Cancelo pago documentos
                                if (vendorPayment.Cancelled == SAPbobsCOM.BoYesNoEnum.tNO)
                                {
                                    rslt = vendorPayment.CancelbyCurrentSystemDate();
                                    if (rslt != 0) throw new InvalidOperationException($"Error al cancelar pago con ID: {pagoDocumento.ID}, error: {Globales.Company.GetLastErrorDescription()}");
                                }

                                if (vendorPayment.CashSum - pagoDocumento.ImpTotal > 0)
                                {
                                    //Genero pago de documentos abiertos                             
                                    vendorPaymentNew.Series = vendorPayment.Series;
                                    vendorPaymentNew.CardCode = vendorPayment.CardCode;
                                    vendorPaymentNew.PaymentType = vendorPayment.PaymentType;
                                    vendorPaymentNew.DocDate = vendorPayment.DocDate;
                                    vendorPaymentNew.TaxDate = vendorPayment.TaxDate;
                                    vendorPaymentNew.DueDate = vendorPayment.DueDate;
                                    vendorPaymentNew.Reference1 = vendorPayment.Reference1;
                                    vendorPaymentNew.Reference2 = vendorPayment.Reference2;
                                    vendorPaymentNew.CounterReference = vendorPayment.CounterReference;
                                    vendorPaymentNew.BPLID = vendorPayment.BPLID;
                                    /*
                                    vendorPaymentNew.TransferAccount = vendorPayment.TransferAccount;
                                    vendorPaymentNew.TransferDate = vendorPayment.TransferDate;
                                    vendorPaymentNew.TransferReference = vendorPayment.TransferReference;
                                    vendorPaymentNew.TransferSum = vendorPayment.TransferSum;
                                    vendorPaymentNew.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = vendorPayment.UserFields.Fields.Item("U_EXX_MPTRABAN").Value;
                                    */
                                    vendorPaymentNew.CashAccount = vendorPayment.CashAccount;
                                    vendorPaymentNew.CashSum = vendorPayment.CashSum - pagoDocumento.ImpTotal;
                                    vendorPaymentNew.UserFields.Fields.Item("U_EXX_MPFONDEF").Value = vendorPayment.UserFields.Fields.Item("U_EXX_MPFONDEF").Value;
                                    mntoTotalAnulado += pagoDocumento.ImpTotal;
                                    var lineaPagoNew = 0;
                                    for (int i = 0; i < vendorPayment.Invoices.Count; i++)
                                    {
                                        vendorPayment.Invoices.SetCurrentLine(i);
                                        if (pagoDocumento.LstDocumentos.Any(d => d.ID == vendorPayment.Invoices.DocEntry && d.Tipo == (int)vendorPayment.Invoices.InvoiceType)) continue;
                                        vendorPaymentNew.Invoices.SetCurrentLine(lineaPagoNew);
                                        vendorPaymentNew.Invoices.DocEntry = vendorPayment.Invoices.DocEntry;
                                        vendorPaymentNew.Invoices.InvoiceType = vendorPayment.Invoices.InvoiceType;
                                        vendorPaymentNew.Invoices.DocLine = vendorPayment.Invoices.DocLine;
                                        vendorPaymentNew.Invoices.InstallmentId = vendorPayment.Invoices.InstallmentId;
                                        vendorPaymentNew.Invoices.SumApplied = vendorPayment.Invoices.SumApplied;
                                        vendorPaymentNew.Invoices.AppliedFC = vendorPayment.Invoices.AppliedFC;
                                        vendorPaymentNew.Invoices.Add();
                                        lineaPagoNew++;
                                    }
                                    rslt = vendorPaymentNew.Add();
                                    if (rslt != 0) throw new InvalidOperationException($"Error al crear pago documento, error: {Globales.Company.GetLastErrorDescription()}");

                                    var idPagoDocNvo = Globales.Company.GetNewObjectKey();
                                    ActualizarReferenciasPagoDoc(idPagoDocNvo, pagoDocumento.ID.ToString(), idPM.ToString());

                                }
                                pagoDocumento.LstDocumentos.ToList().ForEach(d =>
                                {
                                    LiberarDocumentoDePMEP(d.DocEntryEP.ToString(), d.IdLineaEP.ToString(), idPM.ToString(), d.IdLineaPM.ToString());
                                    //Se crea pago masivo borrador
                                    if (d.Tipo == 140)
                                    {
                                        var pagoBorradorOld = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPaymentsDrafts);
                                        var pagoBorradorNew = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPaymentsDrafts);

                                        pagoBorradorOld.GetByKey(d.ID);
                                        pagoBorradorNew.CardCode = pagoBorradorOld.CardCode;
                                        pagoBorradorNew.BPLID = pagoBorradorOld.BPLID;
                                        pagoBorradorNew.TaxDate = pagoBorradorOld.TaxDate;
                                        pagoBorradorNew.DocDate = pagoBorradorOld.DocDate;
                                        pagoBorradorNew.DueDate = pagoBorradorOld.DueDate;
                                        pagoBorradorNew.DocType = pagoBorradorOld.DocType;
                                        pagoBorradorNew.DocTypte = pagoBorradorOld.DocTypte;
                                        pagoBorradorNew.DocObjectCode = pagoBorradorOld.DocObjectCode;
                                        pagoBorradorNew.TransferAccount = pagoBorradorOld.TransferAccount;
                                        pagoBorradorNew.TransferSum = pagoBorradorOld.TransferSum;
                                        pagoBorradorNew.JournalRemarks = pagoBorradorOld.JournalRemarks;
                                        pagoBorradorNew.Remarks = pagoBorradorOld.Remarks;
                                        pagoBorradorNew.ControlAccount = pagoBorradorOld.ControlAccount;
                                        pagoBorradorNew.UserFields.Fields.Item("U_EXX_MPTRABAN").Value = pagoBorradorOld.UserFields.Fields.Item("U_EXX_MPTRABAN").Value;
                                        pagoBorradorNew.UserFields.Fields.Item("U_EXX_NUMEREND").Value = pagoBorradorOld.UserFields.Fields.Item("U_EXX_NUMEREND").Value;
                                        pagoBorradorNew.UserFields.Fields.Item("U_EXX_PRIPAG").Value = pagoBorradorOld.UserFields.Fields.Item("U_EXX_PRIPAG").Value;

                                        var rslt2 = pagoBorradorNew.Add();
                                        if (rslt2 != 0)
                                        {
                                            Globales.Aplication.StatusBar.SetText(Globales.Company.GetLastErrorDescription(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        }
                                    }
                                });
                            }
                        }

                        vendorPayment = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                        if (vendorPayment.GetByKey(rebote.IDPagoSucursal))
                        {
                            if (vendorPayment.Cancelled == SAPbobsCOM.BoYesNoEnum.tNO)
                            {
                                rslt = vendorPayment.CancelbyCurrentSystemDate();
                                if (rslt != 0) throw new InvalidOperationException($"Error al cancelar pago con ID: {rebote.IDPagoSucursal}, error: {Globales.Company.GetLastErrorDescription()}");
                            }

                            if ((vendorPayment.Checks.CheckSum + vendorPayment.TransferSum + vendorPayment.CashSum) - rebote.ImpTotal > 0)
                            {
                                vendorPaymentNew = (SAPbobsCOM.Payments)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments);
                                vendorPaymentNew.Series = vendorPayment.Series;
                                vendorPaymentNew.DocType = vendorPayment.DocType;
                                vendorPaymentNew.DocCurrency = vendorPayment.DocCurrency;
                                vendorPaymentNew.DocRate = vendorPayment.DocRate;
                                vendorPaymentNew.DocDate = vendorPayment.DocDate;
                                vendorPaymentNew.TaxDate = vendorPayment.TaxDate;
                                vendorPaymentNew.DueDate = vendorPayment.DueDate;
                                vendorPaymentNew.Reference1 = vendorPayment.Reference1;
                                vendorPaymentNew.Reference2 = vendorPayment.Reference2;
                                vendorPaymentNew.CounterReference = vendorPayment.CounterReference;
                                vendorPaymentNew.BPLID = vendorPayment.BPLID;
                                vendorPaymentNew.ProjectCode = vendorPayment.ProjectCode;

                                if (vendorPayment.Checks.CheckSum > 0)
                                {
                                    vendorPaymentNew.Checks.AccounttNum = vendorPayment.Checks.AccounttNum;
                                    vendorPaymentNew.Checks.BankCode = vendorPayment.Checks.BankCode;
                                    vendorPaymentNew.Checks.Branch = vendorPayment.Checks.Branch;
                                    vendorPaymentNew.Checks.CheckAccount = vendorPayment.Checks.CheckAccount;
                                    vendorPaymentNew.Checks.CheckSum = vendorPayment.Checks.CheckSum - rebote.ImpTotal;
                                    vendorPaymentNew.Checks.CountryCode = vendorPayment.Checks.CountryCode;
                                    vendorPaymentNew.Checks.Trnsfrable = vendorPayment.Checks.Trnsfrable;
                                }

                                if (vendorPayment.TransferSum > 0)
                                {
                                    vendorPaymentNew.TransferAccount = vendorPayment.TransferAccount;
                                    vendorPaymentNew.TransferDate = vendorPayment.TransferDate;
                                    vendorPaymentNew.TransferReference = vendorPayment.TransferReference;
                                    vendorPaymentNew.TransferSum = vendorPayment.TransferSum - rebote.ImpTotal;
                                }

                                if (vendorPayment.CashSum > 0)
                                {
                                    vendorPaymentNew.CashAccount = vendorPayment.CashAccount;
                                    vendorPaymentNew.CashSum = vendorPayment.CashSum - rebote.ImpTotal;
                                }
                                vendorPaymentNew.PrimaryFormItems.CashFlowLineItemID = vendorPayment.PrimaryFormItems.CashFlowLineItemID;
                                vendorPaymentNew.PrimaryFormItems.PaymentMeans = vendorPayment.PrimaryFormItems.PaymentMeans;

                                for (int i = 0; i < vendorPayment.AccountPayments.Count; i++)
                                {
                                    vendorPayment.AccountPayments.SetCurrentLine(i);
                                    vendorPaymentNew.AccountPayments.SetCurrentLine(i);
                                    vendorPaymentNew.AccountPayments.AccountCode = vendorPayment.AccountPayments.AccountCode;
                                    vendorPaymentNew.AccountPayments.AccountName = vendorPayment.AccountPayments.AccountName;
                                    vendorPaymentNew.AccountPayments.GrossAmount = vendorPayment.AccountPayments.GrossAmount;
                                    vendorPaymentNew.AccountPayments.SumPaid = vendorPayment.AccountPayments.SumPaid - rebote.ImpTotal;
                                    vendorPaymentNew.AccountPayments.Decription = vendorPayment.AccountPayments.Decription;
                                    vendorPaymentNew.AccountPayments.ProjectCode = vendorPayment.AccountPayments.ProjectCode;
                                    vendorPaymentNew.AccountPayments.ProfitCenter = vendorPayment.AccountPayments.ProfitCenter;
                                    vendorPaymentNew.AccountPayments.ProfitCenter2 = vendorPayment.AccountPayments.ProfitCenter2;
                                    vendorPaymentNew.AccountPayments.ProfitCenter3 = vendorPayment.AccountPayments.ProfitCenter3;
                                    vendorPaymentNew.AccountPayments.ProfitCenter4 = vendorPayment.AccountPayments.ProfitCenter4;
                                    vendorPaymentNew.AccountPayments.ProfitCenter5 = vendorPayment.AccountPayments.ProfitCenter5;
                                    vendorPaymentNew.AccountPayments.Add();
                                }

                                rslt = vendorPaymentNew.Add();
                                if (rslt != 0) throw new InvalidOperationException($"Error al crear pago sucursal, error: {Globales.Company.GetLastErrorDescription()}");
                                var idPagoSucNvo = Globales.Company.GetNewObjectKey();
                                ActualizarReferenciasPagoSuc(idPagoSucNvo, rebote.IDPagoSucursal.ToString(), idPM.ToString());
                            }
                        }
                    }
                    dbsORBT.SetValueExt("Status", "C");
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE) Form.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    Form.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular);
                    Globales.Aplication.ActivateMenuItem("1304");
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED, mtxDocs.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.ColUID == "Col_4")
                {
                    var objType = dbsRBT1.GetValue("U_DOCTYPE", e.Row - 1).Trim();
                    objType = objType == "24" ? "30" : objType;
                    var linkedButton = (SAPbouiCOM.LinkedButton)mtxDocs.Columns.Item("Col_4").ExtendedObject;
                    linkedButton.LinkedObjectType = objType;
                }
                return true;
            }));

            Eventos.Add(new EventoData(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD, TYPE, e =>
            {
                if (!e.BeforeAction)
                {
                    HabilitarControlesPorEstado();
                }
                return true;
            }));
        }

        protected override void CargarFormularioInicial()
        {
            dbsORBT = Form.GetDBDataSource("@EXD_PM_ORBT");
            dbsRBT1 = Form.GetDBDataSource("@EXD_PM_RBT1");

            this.cmbSucursales = Form.GetComboBox("Item_4");
            this.cmbBancos = Form.GetComboBox("Item_7");
            this.cmbUsuario = Form.GetComboBox("Item_18");

            this.edtFechaCreacion = (SAPbouiCOM.EditText)Form.GetItem("Item_17").Specific;

            this.btnEjecutar = Form.GetButton("Item_14");
            this.btnBuscar = Form.GetButton("Item_10");

            this.mtxDocs = Form.GetMatrix("Item_11");
        }

        private void CargarDatosAlFormulario()
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = "select \"BPLId\",\"BPLName\" from OBPL";

            while (cmbSucursales.ValidValues.Count > 0) cmbSucursales.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
            recSet.DoQuery(sqlQry);
            cmbSucursales.ValidValues.Add("-1", "Todas");
            while (!recSet.EoF)
            {
                cmbSucursales.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                mtxDocs.Columns.Item("Col_0").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }

            sqlQry = "select \"BankCode\",\"BankName\" from ODSC order by 1";
            while (cmbBancos.ValidValues.Count > 0) cmbBancos.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
            recSet.DoQuery(sqlQry);
            cmbBancos.ValidValues.Add("", "");
            while (!recSet.EoF)
            {
                cmbBancos.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                mtxDocs.Columns.Item("Col_1").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }

            sqlQry = "select USERID,USER_CODE from OUSR";
            while (cmbUsuario.ValidValues.Count > 0) cmbUsuario.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
            recSet.DoQuery(sqlQry);
            while (!recSet.EoF)
            {
                cmbUsuario.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }

            Combo = (SAPbouiCOM.ComboBox)Form.Items.Item("Item_19").Specific;
            Combo.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (Combo.ValidValues.Count > 0) Combo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);

            var cnds = (SAPbouiCOM.Conditions)Globales.Aplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_Conditions);
            var cnd = cnds.Add();
            cnd.Alias = "CardType";
            cnd.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL;
            cnd.CondVal = "S";

            var cflProv = Form.ChooseFromLists.Item("CFL_OCRD");
            cflProv.SetConditions(null);
            cflProv.SetConditions(cnds);

            LoadDataOnFormAddMode();
        }

        internal void LoadDataOnFormAddMode()
        {
            dbsORBT.SetValueExt("UserSign", Globales.Company.UserSignature.ToString());
            dbsORBT.SetValueExt("CreateDate", (DateTime.Today.ToString("yyyyMMdd")));
            Combo.ValidValues.LoadSeries(Form.BusinessObject.Type, SAPbouiCOM.BoSeriesMode.sf_Add);
            if (Combo.ValidValues.Count > 0) Combo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            dbsORBT.SetValue("DocNum", 0, Form.BusinessObject.GetNextSerialNumber(dbsORBT.GetValue("Series", 0).Trim(), Form.BusinessObject.Type).ToString());
            dbsORBT.SetValue("U_COD_SUCURSAL", 0, "-1");
            HabilitarControlesPorEstado();
        }

        private void LoadMatrixFromRecordSet(SAPbobsCOM.Recordset recSet)
        {
            var _dsrXmlDBDataSource = new XMLDBDataSource();
            var _xmlSerializer = new XmlSerializer(typeof(XMLRecordSet));
            var ver = recSet.GetAsXML();
            var _dsrRecSet = (XMLRecordSet)_xmlSerializer.Deserialize(new StringReader(recSet.GetAsXML()));

            _dsrXmlDBDataSource.Rows = _dsrRecSet.BO.Rows.Select(d =>
            {
                var rowsRS = (System.Xml.XmlNode[])d;
                return new RowDBS
                {
                    Cells = new List<CellDBS>
                    {
                        new CellDBS{ Uid = "U_COD_SUCURSAL", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_COD_SUCURSAL").InnerText ??  string.Empty},
                        new CellDBS{ Uid = "U_COD_BANCO", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_CODBANCO").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_CARDCODE", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_CARDCODE").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_CARDNAME", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_CARDNAME").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_DOCENTRY", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_DOCENTRYDOC").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_DOCTYPE", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_TIPODOC").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_NRO_SUNAT", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_NROSUNAT").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_IMPORTE", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_IMPORTE").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_MONEDA", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_MONEDA").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_ID_PAGO", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_NROPGOEFEC").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_ID_PAGO2", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_NROPGOEFEC2").InnerText ??  string.Empty },
                        new CellDBS{ Uid = "U_LINEA_PM", Value = rowsRS.FirstOrDefault( r => r.LocalName == "LineId").InnerText ??  "0" },
                        new CellDBS{ Uid = "U_DOCENTRY_EP", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_COD_ESCENARIOPAGO").InnerText ??  "0"},
                        new CellDBS{ Uid = "U_LINEA_EP", Value = rowsRS.FirstOrDefault( r => r.LocalName == "U_EXP_NROLINEA_EP").InnerText ?? "0"},
                        new CellDBS{ Uid = "U_SELECCION", Value = "N" },
                    }.ToArray()
                };
            }).ToArray();

            _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDBDataSource);
                var verTmp = strWritter.ToString();
                dbsRBT1.LoadFromXML(strWritter.ToString());
                mtxDocs.LoadFromDataSource();
                mtxDocs.AutoResizeColumns();
            }
        }

        private void ActualizarReferenciasPagoDoc(string idPagoDocNvo, string idPagoDocAnt, string DocEntryPM)
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"update \"@EXP_PMP1\" set U_EXP_NROPGOEFEC = '{idPagoDocNvo}' where \"DocEntry\" =  '{DocEntryPM}' and U_EXP_NROPGOEFEC = '{idPagoDocAnt}'";

            recSet.DoQuery(sqlQry);
        }

        private void ActualizarReferenciasPagoSuc(string idPagoSucNvo, string idPagoSucAnt, string DocEntryPM)
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"update \"@EXP_PMP1\" set U_EXP_NROPGOEFEC2 = '{idPagoSucNvo}' where \"DocEntry\" =  '{DocEntryPM}' and U_EXP_NROPGOEFEC2 = '{idPagoSucAnt}'";

            recSet.DoQuery(sqlQry);
        }

        private void LiberarDocumentoDePMEP(string docEntryEP, string lineIdEP, string docEntryPM, string lineIdPM)
        {
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var sqlQry = $"delete from \"@EXD_EPG1\" where \"DocEntry\" = '{docEntryEP}' and \"LineId\" = '{lineIdEP}'";
            recSet.DoQuery(sqlQry);

            sqlQry = $"delete from \"@EXP_PMP1\" where \"DocEntry\" = '{docEntryPM}' and \"LineId\" = '{lineIdPM}'";
            recSet.DoQuery(sqlQry);
        }

        private void HabilitarControlesPorEstado()
        {
            var estadoForm = dbsORBT.GetValueExt("Status");
            try
            {
                //Form.Freeze(true);
                Form.GetItem("Item_1").Visible = false;
                Form.GetItem("Item_22").Visible = true;
                Form.GetItem("Item_4").Enabled = false;
                Form.GetItem("Item_7").Enabled = false;
                Form.GetItem("Item_9").Visible = false;
                Form.GetItem("Item_23").Visible = true;
                Form.GetItem("Item_16").Enabled = false;
                btnEjecutar.Item.Enabled = false;
                btnBuscar.Item.Enabled = false;
                mtxDocs.Columns.Item("Col_8").Editable = false;
                if (estadoForm == "O")
                {
                    btnEjecutar.Item.Enabled = true;
                    if (Form.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                    {
                        Form.GetItem("Item_1").Visible = true;
                        Form.GetItem("Item_22").Visible = false;
                        Form.GetItem("Item_4").Enabled = true;
                        Form.GetItem("Item_7").Enabled = true;
                        Form.GetItem("Item_9").Visible = true;
                        Form.GetItem("Item_23").Visible = false;
                        Form.GetItem("Item_16").Enabled = true;
                        mtxDocs.Columns.Item("Col_8").Editable = true;
                        btnEjecutar.Item.Enabled = false;
                        btnBuscar.Item.Enabled = true;
                    }
                }
                else if (estadoForm == "C")
                {
                    btnEjecutar.Item.Enabled = false;
                }
            }
            finally
            {
                /*
                Form.Freeze(false);
                Form.Update();
                Form.Refresh();
                mtxDocs.AutoResizeColumns();
                */
            }
        }

        private void QuitarFilasNoSeleccionadas()
        {
            mtxDocs.FlushToDataSource();
            var _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            var strXMLDTDocs = dbsRBT1.GetAsXML();
            var xr = XmlReader.Create(new StringReader(strXMLDTDocs), new XmlReaderSettings { IgnoreWhitespace = false });
            var _dsrXmlDBDataSource = (XMLDBDataSource)_xmlSerializer.Deserialize(xr);
            _dsrXmlDBDataSource.Rows = _dsrXmlDBDataSource.Rows.ToList().Where(r => r.Cells.FirstOrDefault(c => c.Uid == "U_SELECCION").Value == "Y").ToArray();
            if (_dsrXmlDBDataSource.Rows.Length == 0) throw new Exception("Debe seleccionar al menos un documento para el pago masivo");
            _xmlSerializer = new XmlSerializer(typeof(XMLDBDataSource));
            using (var strWritter = new StringWriter())
            {
                _xmlSerializer.Serialize(strWritter, _dsrXmlDBDataSource);
                var verTmp = strWritter.ToString();
                dbsRBT1.LoadFromXML(strWritter.ToString());
                mtxDocs.LoadFromDataSource();
            }
        }
    }
}