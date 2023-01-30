using SAP_AddonFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dao
{
    public delegate T GetDBValues<T>(Dictionary<string, string> prm);

    class QueryResultManager
    {
        public static IEnumerable<T> executeQueryAsType<T>(string sqlQry, GetDBValues<T> getDBValues, params string[] prms)
        {
            SAPbobsCOM.Recordset recSet = null;
            Dictionary<string, string> keyValues = null;
            recSet = Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            keyValues = new Dictionary<string, string>();
            sqlQry = string.Format(sqlQry, prms);
            recSet.DoQuery(sqlQry);
            while (!recSet.EoF)
            {
                recSet.Fields.Cast<SAPbobsCOM.Field>().ToList().ForEach(f => { keyValues[f.Name] = f.Value.ToString(); });
                yield return getDBValues(keyValues);
                recSet.MoveNext();
            }
        }

        public static SAPbobsCOM.Recordset executeQueryAsRecordSet(string sqlQry, params string[] prms)
        {
            SAPbobsCOM.Recordset recSet = null;

            recSet = Globales.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            sqlQry = string.Format(sqlQry, prms);
            recSet.DoQuery(sqlQry);
            return recSet;
        }

    }
}

