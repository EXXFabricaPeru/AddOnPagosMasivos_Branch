using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dao
{


    public class ConexionDAO
    {

        public string BaseDatos (SAPbouiCOM.Application sboApplication, ref string err)
        {
            SAPbouiCOM.DataTable registros = null;

            List<string> list = null;
            string Query = "";
            string BaseDatos = "";

            Query = "CALL \"SMC_APM_OBTENERCONFIG\"";

            SAPbobsCOM.Company oCompany;
            oCompany = (SAPbobsCOM.Company)sboApplication.Company.GetDICompany();
            SAPbobsCOM.Recordset oRecordSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRecordSet.DoQuery(Query);
            int dd = oRecordSet.RecordCount;
            if (oRecordSet.RecordCount > 0)
            {
                
                oRecordSet.MoveFirst();
                while (!oRecordSet.EoF)
                {
                    BaseDatos = oRecordSet.Fields.Item(1).Value.ToString();
                    //list.Add(oRecordSet.Fields.Item(0).Value.ToString());
                    oRecordSet.MoveNext();
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);
            Query = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            return BaseDatos;

        }


    }
}
