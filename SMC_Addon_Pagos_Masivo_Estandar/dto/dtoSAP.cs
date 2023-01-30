using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMC_APM.dto
{
    class dtoSAP
    {

    }

    public class dtoChlValues
    {
        public dtoChlValues()
        {
        }

        public dtoChlValues(string code, string name)
        {
            _code = code;
            _name = name;
        }

        private string _code;
        private string _name;

        public string Code { get => _code; set => _code = value; }
        public string Name { get => _name; set => _name = value; }
    }
}
