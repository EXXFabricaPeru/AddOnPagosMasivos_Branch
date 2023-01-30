using System;
using System.Collections.Generic;
using SMC_APM.dto;

namespace SMC_APM.SAP
{
    class sapMatrix
    {
        #region Atributos
        private SAPbouiCOM.Application sboApplication;
        private SAPbobsCOM.Company sboCompany;
        private SAPbouiCOM.ItemEvent ItemEvent;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Item oItemMatrix;
        private SAPbouiCOM.Matrix matrix;
        private SAPbouiCOM.Item oItemEditText;
        private SAPbouiCOM.EditText editText;
        private SAPbouiCOM.Item oItemEditText1;
        private SAPbouiCOM.EditText editText1;
        List<dtoChlValues> values = null;
        #endregion

        #region Constructor
        public sapMatrix(SAPbouiCOM.Application _sboApplication, SAPbobsCOM.Company _sboCompany)
        {
            sboApplication = _sboApplication;
            sboCompany = _sboCompany;
        }
        #endregion

        #region Metodos publicos
        public void addLineMatrixDBDataSource(SAPbouiCOM.Matrix oMatrix, SAPbouiCOM.DBDataSource source)
        {
            try
            {
                oMatrix.FlushToDataSource();
                source.InsertRecord(source.Size - 1);
                source.Offset = source.Size - 1;
                oMatrix.LoadFromDataSource();
                //oMatrix.SelectRow(oMatrix.RowCount,true,false);
                oMatrix.ClearRowData(oMatrix.RowCount);

            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(" Error: sapMatrix.cs > addLineMatrixDBDataSource(() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void deleteLineMatrixDBDataSource(SAPbouiCOM.Matrix oMatrix, SAPbouiCOM.DBDataSource source)
        {
            try
            {
                if (oMatrix.RowCount > 1)
                {
                    oMatrix.FlushToDataSource();
                    source.RemoveRecord(source.Size - 1);
                    oMatrix.DeleteRow(source.Size);
                    //source.Offset = source.Size - 1;
                    oMatrix.SelectRow(oMatrix.RowCount, true, false);
                    oMatrix.ClearRowData(oMatrix.RowCount);
                }
                else
                    sboApplication.StatusBar.SetText(" :: Error: No se puede eliminar más líneas"
                        , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

            }
            catch (Exception ex)
            {
                sboApplication.StatusBar.SetText(" Error: sapMatrix.cs > addLineMatrixDBDataSource(() > "
                    + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void callTxtEventosListas(SAPbouiCOM.ItemEvent ItemEvent, List<dtoChlValues> values)
        {
            this.ItemEvent = ItemEvent;
            this.values = values;

            oForm = sboApplication.Forms.Item(ItemEvent.FormUID);
            oItemEditText = oForm.Items.Item(ItemEvent.ItemUID);
            editText = (SAPbouiCOM.EditText)oItemEditText.Specific;

            editText.ChooseFromListAfter += new SAPbouiCOM._IEditTextEvents_ChooseFromListAfterEventHandler(txtEventosListas);
        }

        public void callMtxEventosListas(SAPbouiCOM.ItemEvent ItemEvent, List<dtoChlValues> values)
        {
            this.ItemEvent = ItemEvent;
            this.values = values;

            oForm = sboApplication.Forms.Item(ItemEvent.FormUID);
            oItemMatrix = oForm.Items.Item(ItemEvent.ItemUID);
            matrix = (SAPbouiCOM.Matrix)oItemMatrix.Specific;

            matrix.ChooseFromListAfter += new SAPbouiCOM._IMatrixEvents_ChooseFromListAfterEventHandler(mtxEventosListas);
        }
        #endregion

        #region Metodos Privados
        private void txtEventosListas(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            SAPbouiCOM.ChooseFromListEvent oCflEvent = null;
            try
            {
                oCflEvent = (SAPbouiCOM.ChooseFromListEvent)ItemEvent;


                if (oCflEvent.SelectedObjects != null)
                {
                    for (int i = 1; i < values.Count; i++)
                    {
                        if (!values[i].Code.Equals(""))
                        {
                            oItemEditText1 = oForm.Items.Item(values[i].Code);
                            editText1 = (SAPbouiCOM.EditText)oItemEditText1.Specific;
                            editText1.Value = oCflEvent.SelectedObjects.GetValue(values[i].Name, 0).ToString();
                        }
                    }

                    editText.Value = oCflEvent.SelectedObjects.GetValue(values[0].Name, 0).ToString();
                }
            }
            catch (Exception ex)
            {
                string errorM = ex.Message;
                string errorL = ex.ToString();
            }
        }

        private void mtxEventosListas(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            SAPbouiCOM.ChooseFromListEvent oCflEvent = null;
            try
            {
                oCflEvent = (SAPbouiCOM.ChooseFromListEvent)ItemEvent;

                if (oCflEvent.SelectedObjects != null)
                {
                    for (int i = 1; i < values.Count; i++)
                    {
                        if (!values[i].Code.Equals(""))
                            try
                            {
                                ((SAPbouiCOM.EditText)matrix.GetCellSpecific(values[i].Code, pVal.Row)).Value = oCflEvent.SelectedObjects.GetValue(values[i].Name, 0).ToString();
                            }
                            catch (Exception ex)
                            {
                                string errorM = ex.Message;
                                string errorL = ex.ToString();
                            }
                    }
                    ((SAPbouiCOM.EditText)matrix.GetCellSpecific(pVal.ColUID, pVal.Row)).Value = oCflEvent.SelectedObjects.GetValue(values[0].Name, 0).ToString();
                }
            }
            catch (Exception ex)
            {
                string errorM = ex.Message;
                string errorL = ex.ToString();
            }
        }
        #endregion
    }
}
