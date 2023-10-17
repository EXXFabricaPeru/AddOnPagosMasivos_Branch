using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMC_APM.Modelo
{
    [XmlRoot(ElementName = "dbDataSources")]
    public class XMLDBDataSource
    {
        [XmlAttribute("uid")]
        public string Uid { get; set; }
        [XmlArray("rows")]
        [XmlArrayItem("row", typeof(RowDBS))]
        public RowDBS[] Rows { get; set; }
    }

    public class RowDBS
    {
        [XmlArray("cells")]
        [XmlArrayItem("cell", typeof(CellDBS))]
        public CellDBS[] Cells { get; set; }
    }

    public class CellDBS
    {
        [XmlElement("uid")]
        public string Uid { get; set; }
        [XmlElement("value")]
        public string Value { get; set; }
    }
}
