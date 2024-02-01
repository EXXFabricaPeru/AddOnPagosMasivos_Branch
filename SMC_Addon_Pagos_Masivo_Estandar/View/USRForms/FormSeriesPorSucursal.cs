using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.View.USRForms
{
    public class FormSeriesPorSucursal : IUSAP
    {

        public const string TYPE = "FrmSRESUC";
        public const string UNQID = "FrmSRESUC";
        public const string MENU = "NN";
        public const string PATH = "Resources/FrmSeriesXSucursal.srf";

        private SAPbouiCOM.DBDataSource _dbsPMP2 = null;
        private SAPbouiCOM.DBDataSource _dbsPMP2Main = null;


        private SAPbouiCOM.Matrix mtxSucursales = null;

        public FormSeriesPorSucursal(string id, string tipoSerie, SAPbouiCOM.DBDataSource dbsPMP2) : base(TYPE, MENU, id, PATH)
        {
            _dbsPMP2Main = dbsPMP2;

            //Oculto columnas por tipo de serie
            //Serie pago Col_1
            mtxSucursales.Columns.Item("Col_4").Visible = false;
            mtxSucursales.Columns.Item("Col_1").Visible = tipoSerie.Equals("P");
            //Serie retencion Col_2
            mtxSucursales.Columns.Item("Col_2").Visible = tipoSerie.Equals("R");
            mtxSucursales.AutoResizeColumns();
            var position = 0;
            _dbsPMP2.Clear();
            for (int i = 0; i < _dbsPMP2Main.Size; i++)
            {
                if (tipoSerie == "R" && _dbsPMP2Main.GetValue("U_RETPRO", i).Trim().Equals("N")) continue;
                _dbsPMP2.InsertRecord(position);
                _dbsPMP2.Offset = position;
                _dbsPMP2.SetValue("U_COD_SUCURSAL", position, _dbsPMP2Main.GetValue("U_COD_SUCURSAL", i));
                _dbsPMP2.SetValue("U_NOM_SUCURSAL", position, _dbsPMP2Main.GetValue("U_NOM_SUCURSAL", i));
                _dbsPMP2.SetValue("U_COD_SERIE_PAGO", position, _dbsPMP2Main.GetValue("U_COD_SERIE_PAGO", i));
                _dbsPMP2.SetValue("U_COD_SERIE_RETEN", position, _dbsPMP2Main.GetValue("U_COD_SERIE_RETEN", i));
                _dbsPMP2.SetValue("U_NOM_SERIE_PAGO", position, _dbsPMP2Main.GetValue("U_NOM_SERIE_PAGO", i));
                _dbsPMP2.SetValue("U_NOM_SERIE_RETEN", position, _dbsPMP2Main.GetValue("U_NOM_SERIE_RETEN", i));
                _dbsPMP2.SetValue("U_RETPRO", position, _dbsPMP2Main.GetValue("U_RETPRO", i));
                position++;
            }
            mtxSucursales.LoadFromDataSource();
        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_CLICK, mtxSucursales.Item.UniqueID, e =>
            {
                if (e.BeforeAction && e.ColUID == "Col_1")
                {
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var codSucursal = _dbsPMP2.GetValue("U_COD_SUCURSAL", e.Row - 1).Trim();
                    var sqlQry = $"select \"SeriesName\",\"Series\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'N'";
                    var cmbSeriePago = (SAPbouiCOM.ComboBox)mtxSucursales.GetCellSpecific(e.ColUID, e.Row);
                    recSet.DoQuery(sqlQry);
                    while (cmbSeriePago.ValidValues.Count > 0) cmbSeriePago.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSeriePago.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                }
                else if (e.BeforeAction && e.ColUID == "Col_2")
                {
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var codSucursal = _dbsPMP2.GetValue("U_COD_SUCURSAL", e.Row - 1).Trim();
                    var sqlQry = $"select \"SeriesName\",\"Series\" from NNM1 where \"ObjectCode\" = '46' and \"BPLId\" = '{codSucursal}' and coalesce(U_EXC_CR,'') = 'Y'";
                    var cmbSeriePago = (SAPbouiCOM.ComboBox)mtxSucursales.GetCellSpecific(e.ColUID, e.Row);
                    recSet.DoQuery(sqlQry);
                    while (cmbSeriePago.ValidValues.Count > 0) cmbSeriePago.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);
                    while (!recSet.EoF)
                    {
                        cmbSeriePago.ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                        recSet.MoveNext();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {
                    //_dbsPMP2Main.Clear();
                    mtxSucursales.FlushToDataSource();
                    for (int i = 0; i < _dbsPMP2.Size; i++)
                    {
                        for (int j = 0; j < _dbsPMP2Main.Size; j++)
                        {
                            if (_dbsPMP2Main.GetValue("U_COD_SUCURSAL", j) == _dbsPMP2.GetValue("U_COD_SUCURSAL", i))
                            {
                                _dbsPMP2Main.SetValue("U_COD_SUCURSAL", i, _dbsPMP2.GetValue("U_COD_SUCURSAL", i));
                                _dbsPMP2Main.SetValue("U_NOM_SUCURSAL", i, _dbsPMP2.GetValue("U_NOM_SUCURSAL", i));
                                _dbsPMP2Main.SetValue("U_COD_SERIE_PAGO", i, _dbsPMP2.GetValue("U_COD_SERIE_PAGO", i));
                                _dbsPMP2Main.SetValue("U_COD_SERIE_RETEN", i, _dbsPMP2.GetValue("U_COD_SERIE_RETEN", i));
                                _dbsPMP2Main.SetValue("U_NOM_SERIE_PAGO", i, _dbsPMP2.GetValue("U_NOM_SERIE_PAGO", i));
                                _dbsPMP2Main.SetValue("U_NOM_SERIE_RETEN", i, _dbsPMP2.GetValue("U_NOM_SERIE_RETEN", i));
                                break;
                            }
                        }
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT, mtxSucursales.Item.UniqueID, e =>
             {
                 if (!e.BeforeAction && e.ColUID == "Col_1")
                 {
                     var slcCodSerie = ((SAPbouiCOM.ComboBox)mtxSucursales.GetCellSpecific(e.ColUID, e.Row)).Selected.Description;
                     mtxSucursales.SetCellWithoutValidation(e.Row, "Col_5", slcCodSerie);
                 }
                 else if (!e.BeforeAction && e.ColUID == "Col_2")
                 {
                     var slcCodSerie = ((SAPbouiCOM.ComboBox)mtxSucursales.GetCellSpecific(e.ColUID, e.Row)).Selected.Description;
                     mtxSucursales.SetCellWithoutValidation(e.Row, "Col_6", slcCodSerie);
                 }
                 return true;
             }));
        }

        protected override void CargarFormularioInicial()
        {
            mtxSucursales = (SAPbouiCOM.Matrix)Form.Items.Item("Item_0").Specific;
            _dbsPMP2 = Form.DataSources.DBDataSources.Item("@EXP_PMP2");
            mtxSucursales.Columns.Item("Col_1").ExpandType = SAPbouiCOM.BoExpandType.et_ValueOnly;
            mtxSucursales.Columns.Item("Col_2").ExpandType = SAPbouiCOM.BoExpandType.et_ValueOnly;
            mtxSucursales.Columns.Item("Col_5").Visible = false;
            mtxSucursales.Columns.Item("Col_6").Visible = false;
        }
    }
}
