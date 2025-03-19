using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace SMC_APM.Modelo
{
    [XmlRoot(ElementName = "BOM")]
    public class XMLRecordSet
    {
        [XmlElement("BO")]
        public RecordSetBO BO { get; set; }
    }

    public class RecordSetBO
    {
        [XmlArray("Recordset")]
        [XmlArrayItem("row", typeof(object))]
        public List<object> Rows { get; set; }
    }
}
