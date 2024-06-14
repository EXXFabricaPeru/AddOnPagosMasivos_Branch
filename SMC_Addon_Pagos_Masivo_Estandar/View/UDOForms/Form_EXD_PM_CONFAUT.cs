using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.View.UDOForms
{
    public class Form_EXD_PM_CONFAUT : IUSAP
    {
        public const string TYPE = "EXD_PM_CONFAUT";
        public Form_EXD_PM_CONFAUT(string id) : base(null)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
        }
        protected override void CargarEventos()
        {
            this.Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "1", e =>
            {
                if (e.BeforeAction)
                {
                    var activeForm = Globales.Aplication.Forms.ActiveForm;
                    if (activeForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                    {
                        var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        var sqlQry = "select 'E' from \"@EXD_OEPG\" where coalesce(\"U_ESTADO\",'') = 'E' and coalesce(\"Canceled\",'') = 'N'";
                        recSet.DoQuery(sqlQry);
                        if (!recSet.EoF) throw new InvalidOperationException("Existen escenarios pendientes de aprobación, no es posible la actualización");

                        sqlQry = "select 'E' from \"@EXP_OPMP\" where coalesce(\"U_EXP_ESTADO\",'') = 'E' and coalesce(\"Canceled\",'') = 'N'";
                        recSet.DoQuery(sqlQry);
                        if (!recSet.EoF) throw new InvalidOperationException("Existen pagos masivos pendientes de aprobación, no es posible la actualización");
                    }
                }
                return true;
            }));
        }

        protected override void CargarFormularioInicial()
        {
        }
    }
}
