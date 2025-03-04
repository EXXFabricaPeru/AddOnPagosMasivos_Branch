using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.View.USRForms
{
    public class FormMetodoEnvBanco : IUSAP
    {
        public const string TYPE = "FRMMETENVBCO";
        public const string UNQID = "FRMMETENVBCO01";
        public const string MENU = "MNUID_MTENBCO";
        public const string PATH = "Resources/FrmMetEnvBanco.srf";

        private SAPbouiCOM.DBDataSource _dbsPMP4 = null;
        private SAPbouiCOM.DBDataSource _dbsPMP4Main = null;

        private SAPbouiCOM.Matrix mtxBancos = null;
        private Action _pmAction = null;

        public FormMetodoEnvBanco(string id, SAPbouiCOM.DBDataSource dbsPMP4, Action pmAction) : base(TYPE, MENU, id, PATH)
        {
            _dbsPMP4Main = dbsPMP4;
            _pmAction = pmAction;

            var sqlQry = "select \"BankCode\",\"BankName\" from ODSC";
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            recSet.DoQuery(sqlQry);
            while (!recSet.EoF)
            {
                mtxBancos.Columns.Item("Col_1").ValidValues.Add(recSet.Fields.Item(0).Value, recSet.Fields.Item(1).Value);
                recSet.MoveNext();
            }

            _dbsPMP4.Clear();
            var position = 0;
            for (int i = 0; i < _dbsPMP4Main.Size; i++)
            {
                _dbsPMP4.InsertRecord(position);
                _dbsPMP4.Offset = position;
                _dbsPMP4.SetValue("U_COD_BANCO", position, _dbsPMP4Main.GetValue("U_COD_BANCO", i));
                _dbsPMP4.SetValue("U_COD_METODO", position, _dbsPMP4Main.GetValue("U_COD_METODO", i));
                position++;
            }
            mtxBancos.LoadFromDataSource();
        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {
                    mtxBancos.FlushToDataSource();
                    for (int i = 0; i < _dbsPMP4.Size; i++)
                    {
                        _dbsPMP4Main.SetValue("U_COD_BANCO", i, _dbsPMP4.GetValue("U_COD_BANCO", i));
                        _dbsPMP4Main.SetValue("U_COD_METODO", i, _dbsPMP4.GetValue("U_COD_METODO", i));
                    }
                    _pmAction();
                }
                return true;
            }));
        }

        protected override void CargarFormularioInicial()
        {
            mtxBancos = (SAPbouiCOM.Matrix)Form.Items.Item("Item_0").Specific;
            _dbsPMP4 = Form.DataSources.DBDataSources.Item("@EXP_PMP4");
        }
    }
}
