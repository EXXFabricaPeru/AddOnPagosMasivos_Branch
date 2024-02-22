using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.View.USRForms
{
    public class FormConfiguracionH2H : IUSAP
    {
        public const string TYPE = "FRNCNFH2H";
        public const string UNQID = "FRNCNFH2H01";
        public const string MENU = "MNUID_CNFH2H";
        public const string PATH = "Resources/FrmConfH2H.srf";

        private const string FLD_BCP = "1";
        private const string FLD_BBVA = "2";
        private const string FLD_SCBK = "3";
        private const string FLD_IBK = "4";

        private SAPbouiCOM.Button btnActualizar = null;
        private SAPbouiCOM.Folder fldBCP = null;
        private SAPbouiCOM.Folder fldBBVA = null;
        private SAPbouiCOM.Folder fldSCBK = null;
        private SAPbouiCOM.Folder fldIBK = null;
        private SAPbouiCOM.UserDataSource udsFolder = null;
        private SAPbouiCOM.DBDataSource dbsCONFH2H = null;
        public FormConfiguracionH2H(string id) : base(TYPE, MENU, id, PATH)
        {
            if (!UIFormFactory.FormUIDExists(id)) UIFormFactory.AddUSRForm(id, this);
            var tblConfH2H = Globales.Company.UserTables.Item("EXD_PM_CONFH2H");
            if (!tblConfH2H.GetByKey("001"))
            {
                tblConfH2H.Code = "001";
                tblConfH2H.Name = "001";
                tblConfH2H.Add();
            }
            if (!tblConfH2H.GetByKey("002"))
            {
                tblConfH2H.Code = "002";
                tblConfH2H.Name = "002";
                tblConfH2H.Add();
            }
            if (!tblConfH2H.GetByKey("003"))
            {
                tblConfH2H.Code = "003";
                tblConfH2H.Name = "003";
                tblConfH2H.Add();
            }
            if (!tblConfH2H.GetByKey("004"))
            {
                tblConfH2H.Code = "004";
                tblConfH2H.Name = "004";
                tblConfH2H.Add();
            }
            dbsCONFH2H.Query(ObtenerConfPorBanco("00" + FLD_BCP));
            udsFolder.Value = FLD_BCP;
            Form.Update();
        }

        protected override void CargarFormularioInicial()
        {
            this.btnActualizar = (SAPbouiCOM.Button)Form.Items.Item("1").Specific;
            this.fldBCP = (SAPbouiCOM.Folder)Form.Items.Item("Item_1").Specific;
            this.fldBBVA = (SAPbouiCOM.Folder)Form.Items.Item("Item_2").Specific;
            this.fldSCBK = (SAPbouiCOM.Folder)Form.Items.Item("Item_3").Specific;
            this.fldIBK = (SAPbouiCOM.Folder)Form.Items.Item("Item_4").Specific;
            this.udsFolder = Form.DataSources.UserDataSources.Item("UD_FLD");
            this.dbsCONFH2H = Form.DataSources.DBDataSources.Item("@EXD_PM_CONFH2H");

        }

        protected override void CargarEventos()
        {
            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnActualizar.Item.UniqueID, e =>
            {
                if (e.BeforeAction && Form.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                {
                    var tblConfH2H = Globales.Company.UserTables.Item("EXD_PM_CONFH2H");
                    var slcBanco = "00" + udsFolder.Value;
                    if (tblConfH2H.GetByKey(slcBanco))
                    {
                        tblConfH2H.Code = slcBanco;
                        tblConfH2H.Name = slcBanco;
                        tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value = dbsCONFH2H.GetValue("U_IP_FTP", 0);
                        tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value = dbsCONFH2H.GetValue("U_PUERTO", 0);
                        tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value = dbsCONFH2H.GetValue("U_USUARIO", 0);
                        tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value = dbsCONFH2H.GetValue("U_PASSWORD", 0);
                        tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value = dbsCONFH2H.GetValue("U_RUTA_FLD_IN", 0);
                        tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_OUT").Value = dbsCONFH2H.GetValue("U_RUTA_FLD_OUT", 0);
                        tblConfH2H.Update();
                    }
                    else
                    {
                        tblConfH2H.Code = slcBanco;
                        tblConfH2H.Name = slcBanco;
                        tblConfH2H.UserFields.Fields.Item("U_IP_FTP").Value = dbsCONFH2H.GetValue("U_IP_FTP", 0);
                        tblConfH2H.UserFields.Fields.Item("U_PUERTO").Value = dbsCONFH2H.GetValue("U_PUERTO", 0);
                        tblConfH2H.UserFields.Fields.Item("U_USUARIO").Value = dbsCONFH2H.GetValue("U_USUARIO", 0);
                        tblConfH2H.UserFields.Fields.Item("U_PASSWORD").Value = dbsCONFH2H.GetValue("U_PASSWORD", 0);
                        tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_IN").Value = dbsCONFH2H.GetValue("U_RUTA_FLD_IN", 0);
                        tblConfH2H.UserFields.Fields.Item("U_RUTA_FLD_OUT").Value = dbsCONFH2H.GetValue("U_RUTA_FLD_OUT", 0);
                        tblConfH2H.Add();
                    }
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, fldBCP.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    dbsCONFH2H.Query(ObtenerConfPorBanco("00" + FLD_BCP));
                    Form.Update();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, fldBBVA.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    dbsCONFH2H.Query(ObtenerConfPorBanco("00" + FLD_BBVA));
                    Form.Update();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, fldSCBK.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    dbsCONFH2H.Query(ObtenerConfPorBanco("00" + FLD_SCBK));
                    Form.Update();
                }
                return true;
            }));

            Eventos.Add(new EventoItem(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, fldIBK.Item.UniqueID, e =>
            {
                if (e.BeforeAction)
                {
                    dbsCONFH2H.Query(ObtenerConfPorBanco("00" + FLD_IBK));
                    Form.Update();
                }
                return true;
            }));
        }

        private SAPbouiCOM.Conditions ObtenerConfPorBanco(string codFldBanco)
        {
            var cnds = (SAPbouiCOM.Conditions)Globales.Aplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_Conditions);
            var cnd = cnds.Add();
            cnd.Alias = "Code";
            cnd.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL;
            cnd.CondVal = codFldBanco;
            return cnds;
        }
    }
}
