using SAP_AddonFramework;
using SAPbobsCOM;
using SMC_APM.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.View.USRForms
{
    public class FormNumeroDeOperacion : IUSAP
    {
        public const string TYPE = "FRMNROOPE";
        public const string UNQID = "FRMNROOPE01";
        public const string MENU = "MNUID_NROOPE";
        public const string PATH = "Resources/FrmPMNroOpe.srf";

        private SAPbouiCOM.DBDataSource _dbsPMP3 = null;
        private SAPbouiCOM.DBDataSource _dbsPMP3Main = null;

        private SAPbouiCOM.Matrix mtxSucursales = null;
        private Action _pmAction = null;

        public FormNumeroDeOperacion(string id, SAPbouiCOM.DBDataSource dbsPMP3, Action pmAction) : base(TYPE, MENU, id, PATH)
        {
            _dbsPMP3Main = dbsPMP3;
            _pmAction = pmAction;

            var sqlQry = "select \"GLAccount\",\"Account\" from DSC1 where coalesce(\"U_EXM_PMASIVO\",'') = 'Y'";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            recSet.DoQuery(sqlQry);
            while (!recSet.EoF)
            {
                try
                {
                    mtxSucursales.Columns.Item("Col_4").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                }
                catch (Exception) { }
                recSet.MoveNext();
            }

            var position = 0;
            _dbsPMP3.Clear();
            for (int i = 0; i < _dbsPMP3Main.Size; i++)
            {
                _dbsPMP3.InsertRecord(position);
                _dbsPMP3.Offset = position;
                _dbsPMP3.SetValue("U_COD_SUCURSAL", position, _dbsPMP3Main.GetValue("U_COD_SUCURSAL", i));
                _dbsPMP3.SetValue("U_COD_BANCO", position, _dbsPMP3Main.GetValue("U_COD_BANCO", i));
                _dbsPMP3.SetValue("U_COD_MONEDA", position, _dbsPMP3Main.GetValue("U_COD_MONEDA", i));
                _dbsPMP3.SetValue("U_COD_CTAPAGO", position, _dbsPMP3Main.GetValue("U_COD_CTAPAGO", i));
                _dbsPMP3.SetValue("U_NRO_OPERACION", position, _dbsPMP3Main.GetValue("U_NRO_OPERACION", i));
                position++;
            }
            mtxSucursales.LoadFromDataSource();
        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {
                    //_dbsPMP2Main.Clear();
                    mtxSucursales.FlushToDataSource();
                    for (int i = 0; i < _dbsPMP3.Size; i++)
                    {
                        _dbsPMP3Main.SetValue("U_COD_SUCURSAL", i, _dbsPMP3.GetValue("U_COD_SUCURSAL", i));
                        _dbsPMP3Main.SetValue("U_COD_BANCO", i, _dbsPMP3.GetValue("U_COD_BANCO", i));
                        _dbsPMP3Main.SetValue("U_COD_MONEDA", i, _dbsPMP3.GetValue("U_COD_MONEDA", i));
                        _dbsPMP3Main.SetValue("U_COD_CTAPAGO", i, _dbsPMP3.GetValue("U_COD_CTAPAGO", i));
                        _dbsPMP3Main.SetValue("U_NRO_OPERACION", i, _dbsPMP3.GetValue("U_NRO_OPERACION", i));
                    }
                    _pmAction();
                }
                return true;
            }));
        }

        protected override void CargarFormularioInicial()
        {
            mtxSucursales = (SAPbouiCOM.Matrix)Form.Items.Item("Item_0").Specific;
            _dbsPMP3 = Form.DataSources.DBDataSources.Item("@EXP_PMP3");

            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            recSet.DoQuery("select \"BPLId\",\"BPLName\" from OBPL order by 1");
            //dbsPMP2.Clear();
            while (!recSet.EoF)
            {
                mtxSucursales.Columns.Item("Col_0").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }

            recSet = PagoMasivoController.ObtenerInfoBancos();
            while (!recSet.EoF)
            {
                mtxSucursales.Columns.Item("Col_1").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }
        }
    }
}
