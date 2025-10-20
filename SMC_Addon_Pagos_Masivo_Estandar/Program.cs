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

                    var recset = (SAPbobsCOM.Recordset)conexSBO.sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    recset.DoQuery("select 'E' from CUFD where \"TableID\" = 'OIDC' and \"AliasID\" = 'EXD_PAGO_MASIVO'");
                    var existePrevCampoPMEnOIDC = !recset.EoF;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recset);
                    recset = null;
                    GC.Collect();

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
                                new { Code = "11",Name="Cta. puente",Valor="" },
                                new { Code = "12",Name="Validar pago de detracciones",Valor="Y" },
                                new { Code = "13",Name="Obt. serie pago desde ctas. banco propio",Valor="N" },
                                new { Code = "14",Name="Cta. de ajuste por redondeo",Valor="" },
                                new { Code = "15",Name="ID de flujo de caja",Valor="" },
                                new { Code = "16",Name="Validar pago de retenciones",Valor="N" },
                                new { Code = "17",Name="Cancelar pagos a la fecha actual",Valor="N" },
                                new { Code = "18",Name="Version extendida del TXT BCP",Valor="N" },
                                new { Code = "19",Name="Mostrar asientos con bloqueo de pago",Valor="Y" },
                                new { Code = "20",Name="Nueva version de telecredito TXT BCP",Valor="N" },
                                new { Code = "21",Name="Agrupar pagos por proveedor TXT BCP",Valor="N" }
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

                            var lstDocumentos = new List<dynamic>
                            {
                                new { Code = "VR",Name="Varios" },
                                new { Code = "FT-P",Name="Factura de proveedores"},
                                new { Code = "SP",Name="Pago borrador"},
                            };
                            //Establezco opciones por defecto
                            var tblConfDocumentos = conexSBO.sboCompany.UserTables.Item("EXD_PM_TIPODOC");
                            foreach (var item in lstDocumentos)
                            {
                                if (!tblConfDocumentos.GetByKey(item.Code))
                                {
                                    tblConfDocumentos.Code = item.Code;
                                    tblConfDocumentos.Name = item.Name;
                                    tblConfDocumentos.Add();
                                }
                            }

                            var lstTiposDocumentoEP = new List<dynamic>
                            {
                                new { Code = "AS",Name="Asiento" },
                                new { Code = "FA-P",Name="Factura de anticipo de proveedores"},
                                new { Code = "FT-P",Name="Factura de proveedores"},
                                new { Code = "NC-C",Name="Nota de credito de clientes" },
                                new { Code = "PR",Name="Pago recibido"},
                                new { Code = "SA-P",Name="Solicitud de anticipo de proveedores"},
                                new { Code = "SP",Name="Pago borrador"}
                            };
                            //Establezco opciones por defecto
                            var tblTiposDocumentoEP = conexSBO.sboCompany.UserTables.Item("EXD_PM_EP_TIPDOC");
                            foreach (var item in lstTiposDocumentoEP)
                            {
                                if (!tblTiposDocumentoEP.GetByKey(item.Code))
                                {
                                    tblTiposDocumentoEP.Code = item.Code;
                                    tblTiposDocumentoEP.Name = item.Name;
                                    tblTiposDocumentoEP.Add();
                                }
                            }

                            if (!existePrevCampoPMEnOIDC)
                            {
                                var recsetAux = (SAPbobsCOM.Recordset)conexSBO.sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                recsetAux.DoQuery("select 'E' from CUFD where \"TableID\" = 'OIDC' and \"AliasID\" = 'EXD_PAGO_MASIVO'");
                                if (!recsetAux.EoF) recsetAux.DoQuery("update OIDC set U_EXD_PAGO_MASIVO = 'Y'");
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(recsetAux);
                                recsetAux = null;
                            }

                            //inicia el addon
                            ctrPrincipal = new ctrPrincipal(conexSBO.sboApplication, conexSBO.sboCompany);
                            ctrPrincipal.iniciarAddon();
                            GC.KeepAlive(conexSBO);
                            GC.KeepAlive(ctrPrincipal);
                            Application.Run();
                        }
                        else
                            throw new InvalidOperationException("PM: No se encontro tabla de configuración");
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
