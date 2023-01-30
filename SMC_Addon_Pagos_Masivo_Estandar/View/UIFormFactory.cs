using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM
{
    public class UIFormFactory
    {
        private static Dictionary<string, IUSAP> dictionaryForms = new Dictionary<string, IUSAP>();

        internal static void AddUSRForm(string formUID, IUSAP uiForm)
        {
            if (!dictionaryForms.ContainsKey(formUID))
                dictionaryForms.Add(formUID, uiForm);
        }

        internal static IUSAP GetForm(dynamic obj)
        {
            if (obj is SAPbouiCOM.ItemEvent)
            {
                SAPbouiCOM.ItemEvent itemEvent = obj;

                if (!itemEvent.BeforeAction && itemEvent.EventType == SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD)
                {
                    if (dictionaryForms.ContainsKey(itemEvent.FormUID))
                        dictionaryForms.Remove(itemEvent.FormUID);
                }
            }

            if (dictionaryForms.ContainsKey(obj.FormUID))
                return dictionaryForms[obj.FormUID];

            return null;
        }

        internal static bool FormUIDExists(string uNQID)
        {
            return dictionaryForms.ContainsKey(uNQID);
        }

        internal static IUSAP GetFormByUID(string uNQID)
        {
            return dictionaryForms[uNQID];
        }
    }
}
