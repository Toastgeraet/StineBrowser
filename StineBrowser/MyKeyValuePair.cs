using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StineBrowser
{
    class MyKeyValuePair
    {
        public string name { get; set; }
        public string value { get; set; }
        public MyKeyValuePair(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
