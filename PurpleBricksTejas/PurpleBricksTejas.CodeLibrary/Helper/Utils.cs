using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PurpleBricksTejas.CodeLibrary
{
    public static class Utils
    {
        public static string FormatMoney(double? val, bool withDollar = false)
        {
            if (val == null) return "";
            if (withDollar) return val.Value.ToString("C");            

            return val.Value.ToString("F2");
        }
    }
}