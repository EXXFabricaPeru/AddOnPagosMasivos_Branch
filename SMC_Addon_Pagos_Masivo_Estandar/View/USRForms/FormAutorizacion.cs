using SAP_AddonFramework;
using SAP_AddonExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SMC_APM.View.USRForms
{
    public class FormAutorizacion : IUSAP
    {
        public const string TYPE = "FrmAUT";
        public const string UNQID = "FrmAUT01";
        public const string MENU = "MNUID_AUT";
        public const string PATH = "Resources/FrmAutorizacion.srf";

        public FormAutorizacion(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
        }
        protected override void CargarEventos()
        {

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnBuscar", e =>
            {
                if (!e.BeforeAction) MostrarDocumentosPorAutorizar();
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, "btnEjec", e =>
            {
                if (!e.BeforeAction)
                {
                    var rslt = Globales.Aplication.MessageBox("Se procederán a ejecutar las acciones seleccionadas \n¿Desea continuar?", 1, "SI", "NO");
                    if (rslt != 1) return false;
                    var ventana = Form.GetUserDataSource("UD_VENT").Value;
                    var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    var cntMaxAut = 0;
                    var sqlQry = $"select \"U_CANTAUTO\" from \"@EXD_PM_CONFAUT\" where \"U_VENTANA\" = '{ventana}'";
                    var sqlQry2 = string.Empty;
                    recSet.DoQuery(sqlQry);
                    if (!recSet.EoF) cntMaxAut = Convert.ToInt32(recSet.Fields.Item(0).Value);

                    if (ventana == "E")
                    {
                        sqlQry = "UPDATE \"@SMC_APM_AUTORIZAR\" set \"U_EXD_CNTAUT\" = {0} where \"Code\" = '{1}'";
                        sqlQry2 = "update \"@SMC_APM_ESCCAB\" set \"U_EXD_ESTE\" = '{0}' where \"Code\" = '{1}'";
                    }
                    else
                    {
                        sqlQry = "UPDATE \"@EXP_OPMP\" set \"U_EXD_CNTAUT\" = {0} where \"DocEntry\" = '{1}'";
                        sqlQry2 = "update \"@EXP_OPMP\" set \"U_EXP_ESTADO\" = '{0}' where \"DocEntry\" = '{1}'";
                    }

                    Form.GetMatrix("mtxDocs").FlushToDataSource();
                    var dtblDocs = Form.DataSources.DataTables.Item("DT_0");

                    for (int i = 0; i < dtblDocs.Rows.Count; i++)
                    {
                        var accion = dtblDocs.GetValue("Accion", i);
                        var codEscPag = dtblDocs.GetValue("Codigo", i);
                        var cntAutAct = Convert.ToInt32(dtblDocs.GetValue("CntActualAut", i));
                        if (accion == "A")
                        {
                            if (cntMaxAut == cntAutAct + 1)
                                recSet.DoQuery(string.Format(sqlQry2, "A", codEscPag));
                            recSet.DoQuery(string.Format(sqlQry, cntAutAct + 1, codEscPag));
                        }
                        else if (accion == "R")
                        {
                            recSet.DoQuery(string.Format(sqlQry2, "R", codEscPag));
                            recSet.DoQuery(string.Format(sqlQry, 0, codEscPag));
                        }
                    }
                    MostrarDocumentosPorAutorizar();
                }
                return true;
            }));
        }

        protected override void CargarFormularioInicial()
        {
            Form.DataSources.UserDataSources.Item("dtuFechaI").Value = DateTime.Today.ToString("yyyyMMdd");
            Form.DataSources.UserDataSources.Item("dtuFechaF").Value = DateTime.Today.ToString("yyyyMMdd");
            Form.DataSources.UserDataSources.Item("UD_VENT").Value = "E";
        }

        private void MostrarDocumentosPorAutorizar()
        {
            var ventana = Form.GetUserDataSource("UD_VENT").Value;
            var codAutori = Globales.Company.UserName;
            var fechaDsd = Convert.ToDateTime(Form.GetUserDataSource("dtuFechaI").Value);
            var fechaHst = Convert.ToDateTime(Form.GetUserDataSource("dtuFechaF").Value);
            var cntAutori = 0;
            var dtblDocs = Form.DataSources.DataTables.Item("DT_0");
            var recSet = (SAPbobsCOM.Recordset)Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            var sqlQry = $"CALL EXP_SP_PMP_OBTENER_CNT_AUT_POR_AUTORIZADOR('{ventana}','{codAutori}')";
            recSet.DoQuery(sqlQry);
            if (!recSet.EoF) cntAutori = Convert.ToInt32(recSet.Fields.Item(0).Value);

            sqlQry = $"CALL EXP_SP_PMP_LISTAR_DOCUMENTOS_PARA_AUTORIZACION('{codAutori}','{fechaDsd.ToString("yyyyMMdd")}','{fechaHst.ToString("yyyyMMdd")}','{cntAutori}','{ventana}')";
            dtblDocs.ExecuteQuery(sqlQry);
            Form.GetMatrix("mtxDocs").LoadFromDataSource();
            Form.GetMatrix("mtxDocs").AutoResizeColumns();
        }
    }
}
