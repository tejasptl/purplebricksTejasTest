using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PurpleBricksTejas.CodeLibrary
{
    public static class Utils
    {
        /// <summary>
        /// This method is used to convet double value to valid money format
        /// </summary>
        /// <param name="val"></param>
        /// <param name="withDollar"></param>
        /// <returns></returns>
        public static string FormatMoney(double? val, bool withDollar = false)
        {
            if (val == null) return "";
            if (withDollar) return val.Value.ToString("C");            

            return val.Value.ToString("F2");
        }
    }
}