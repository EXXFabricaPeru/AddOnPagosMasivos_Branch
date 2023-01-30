using SAP_AddonExtensions;
using SAP_AddonFramework;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMC_APM.View.USRForms
{
    public class FormModal : IUSAP
    {
        public const string TYPE = "FrmMDL";
        public const string UNQID = "FrmMDL";
        public const string MENU = "";
        public const string PATH = "Resources/FrmModal.srf";

        private AutoResetEvent signal;
        public string BaseUID { get; set; }

        public FormModal(string id, string formBaseID, AutoResetEvent signalRef) : base(TYPE, MENU, id, PATH)
        {
            BaseUID = formBaseID;
            signal = signalRef;
        }
        protected override void CargarEventos()
        {
            //Eventos.Add(new EventoItem(BoEventTypes.et_ITEM_PRESSED, "btnOK", e => EnviarConfirmacion(e)));
        }

        //private bool EnviarConfirmacion(ItemEvent e)
        //{
        //    if (e.BeforeAction && Form.GetUserDataSource("UD_PR").Value == "N" && Form.GetUserDataSource("UD_PP").Value == "N")
        //    {
        //        Globales.Aplication.StatusBar.SetText("Debe, al menos, elegir una operación");
        //        return false;
        //    }

        //    if(!e.BeforeAction)
        //    {
        //        FormLiberarTercero formLiberarTercero = (FormLiberarTercero)UIFormFactory.GetFormByUID(BaseUID);
        //        formLiberarTercero.PagosRetenciones = Form.GetUserDataSource("UD_PR").Value == "Y";
        //        formLiberarTercero.PagosProveedores = Form.GetUserDataSource("UD_PP").Value == "Y";

        //        formLiberarTercero.CrearPagos();

        //        signal.Set();
        //        Form.Close();

        //    }

        //    return true;
        //}

        protected override void CargarFormularioInicial()
        {
            CentrarFormulario();

            Form.GetUserDataSource("UD_PR").Value = "Y";
            Form.GetUserDataSource("UD_PP").Value = "Y";


            Form.Visible = true;
        }

        private void CentrarFormulario()
        {
            if (Globales.Aplication.ClientType == BoClientType.ct_Desktop)
            {
                Form.Left = (Globales.Aplication.Desktop.Width - Form.Width) / 2;
                Form.Top = (Globales.Aplication.Desktop.Height - Form.Height) / 2;
            }
        }

        internal void GetRespuestaModal()
        {
            throw new NotImplementedException();
        }
    }
}
