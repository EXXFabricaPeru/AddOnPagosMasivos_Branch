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
            var position = 0;
            for (int i = 0; i < _dbsPMP4Main.Size; i++)
            {
                _dbsPMP4.InsertRecord(position);
                _dbsPMP4.Offset = position;
                _dbsPMP4.SetValue("U_COD_BANCO", position, _dbsPMP4Main.GetValue("U_COD_BANCO", i));
                position++;
            }
            mtxBancos.LoadFromDataSource();
        }

        protected override void CargarEventos()
        {
            mtxBancos = (SAPbouiCOM.Matrix)Form.Items.Item("Item_0").Specific;
            _dbsPMP4 = Form.DataSources.DBDataSources.Item("@EXP_PMP4");
        }

        protected override void CargarFormularioInicial()
        {

        }
    }
}
