using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace LasyTests
{
    public class DictBasedTestObject : Dictionary<string,object>
    {
        public bool IsSet
        {
            get { return this.Val("IsSet", false).ConvertTo<bool>(); }
            set { this["IsSet"] = value; }
        }

        public string Name
        {
            get { return this.Val("Name", "").ConvertTo<string>(); }
            set { this["Name"] = value; }
        }
    }
}
