using System;
using System.Collections.Generic;
using System.Text;

namespace WX.Common
{
    public static class DoObjectExtensions
    {
        public static bool ToBool(this object obj) { return obj.ToBool(false); }
        public static bool ToBool(this object obj, bool defValue) { obj = obj ?? defValue; bool def; bool.TryParse(obj.ToString(), out def); return !def ? defValue : def; }
    }
}
