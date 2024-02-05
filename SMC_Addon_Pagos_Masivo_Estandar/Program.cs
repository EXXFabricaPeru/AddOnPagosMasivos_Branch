using System;
using System.Windows.Forms;
using SMC_APM.Conexion;
using SMC_APM.Controladores;
using SMC_APM.Util;
using EXX_MetaData;
using EXX_Metadata.BL;
using System.Reflection;
using System.Collections.Generic;

namespace SMC_Addon_Pagos_Masivo_Estandar
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            conexSBO conexSBO = null;
            ctrPrincipal ctrPrincipal = null;

            try
            {
                //realiza conexion 
                conexSBO = new conexSBO();
                if ((conexSBO != null) && (conexSBO.sboCompany.Connected))
                {
                    MDResources.Messages = (string m, MessageType t) =>
                    {
                        switch (t)
                        {
                            case MessageType.Info:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                break;
                            case MessageType.Success:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                break;
                            case MessageType.Error:
                                conexSBO.sboApplication.StatusBar.SetText(m, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                break;
                            default:
                                break;
                        }
                    };

                    if (MDResources.loadMetaData(Assembly.GetExecutingAssembly().GetName().Version, conexSBO.sboApplication, "EXX", "PGOMSV"))
                    {
                        var utblMD = (SAPbobsCOM.UserTablesMD)conexSBO.sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                        if (utblMD.GetByKey("SMC_APM_CONFIAPM"))
                        {
                            var lstOpciones = new List<dynamic>
                            {
                                new { Code = "2",Name="Agente de retención",Valor="N" },
                                new { Code = "3",Name="Tercero retenedor",Valor="N" },
                                new { Code = "4",Name="Autorización escenario",Valor="N" },
                                new { Code = "5",Name="Autorización pagos",Valor="N" },
                                new { Code = "6",Name="Medio de pago efectivo",Valor="999" },
                                new { Code = "7",Name="Medio de pago tranferencia",Valor="999" },
                                new { Code = "8",Name="Medio de pago cheque",Valor="999" },
                                new { Code = "9",Name="Sucursales",Valor="N" },
                                new { Code = "10",Name="Host to Host",Valor="N" },                               
                            };
                            //Establezco opciones por defecto
                            var tblConfPM = conexSBO.sboCompany.UserTables.Item("SMC_APM_CONFIAPM");
                            foreach (var item in lstOpciones)
                            {
                                if (!tblConfPM.GetByKey(item.Code))
                                {
                                    tblConfPM.Code = item.Code;
                                    tblConfPM.Name = item.Name;
                                    tblConfPM.UserFields.Fields.Item("U_VALOR").Value = item.Valor;
                                    tblConfPM.Add();
                                }
                            }
                            //inicia el addon
                            ctrPrincipal = new ctrPrincipal(conexSBO.sboApplication, conexSBO.sboCompany);
                            ctrPrincipal.iniciarAddon();
                            GC.KeepAlive(conexSBO);
                            GC.KeepAlive(ctrPrincipal);
                            Application.Run();
                        }
                        else
                            throw new InvalidOperationException("PM: No se encontro table de configuración");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(SMC_APM.Properties.Resources.nombreAddon + " : MAIN > " + ex.Message, "Aceptar",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                conexSBO.DesconectarCompany();
            }
        }
    }
}
