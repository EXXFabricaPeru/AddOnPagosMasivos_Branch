using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMC_APM.Modelo
{
    [XmlRoot(ElementName = "DataTable")]
    public class XMLDataTable
    {
        [XmlAttribute("Uid")]
        public string Uid { get; set; }
        [XmlArray("Rows")]
        [XmlArrayItem("Row", typeof(Row))]
        public Row[] Rows { get; set; }
    }

    public class Row
    {
        [XmlArray("Cells")]
        [XmlArrayItem("Cell", typeof(Cell))]
        public Cell[] Cells { get; set; }
    }

    public class Cell
    {
        [XmlElement("ColumnUid")]
        public string ColumnUid { get; set; }
        [XmlElement("Value")]
        public string Value { get; set; }
    }
}
