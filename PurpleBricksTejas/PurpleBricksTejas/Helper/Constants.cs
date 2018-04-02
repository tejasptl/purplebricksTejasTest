using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PurpleBricksTejas
{
    public class Constants
    {
        /// <summary>
        /// To generate list of Board Size
        /// </summary>
        /// <returns>List of Pairs Board Size Value and Text</returns>
        public static List<SelectListItem> LoadBoardSize()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Small", Value = "Small" });
            list.Add(new SelectListItem() { Text = "Large", Value = "Large" });
            return list;
        }

        /// <summary>
        /// To generate list of State
        /// </summary>
        /// <returns>List of Pairs State Value and Text</returns>
        public static List<SelectListItem> LoadState()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "NSW", Value = "NSW" });
            list.Add(new SelectListItem() { Text = "NT", Value = "NT" });
            list.Add(new SelectListItem() { Text = "QLD", Value = "QLD" });
            list.Add(new SelectListItem() { Text = "SA", Value = "SA" });
            list.Add(new SelectListItem() { Text = "TAS", Value = "TAS" });
            list.Add(new SelectListItem() { Text = "VIC", Value = "VIC" });
            list.Add(new SelectListItem() { Text = "WA", Value = "WA" });
            return list;
        }

    }
}
