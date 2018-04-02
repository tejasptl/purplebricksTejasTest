using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PurpleBricksTejas.CodeLibrary
{
    public class PathProviderXML
    {
        /// <summary>
        /// This is used to get Price XML File location for Test cases
        /// </summary>
        /// <returns></returns>
        public virtual string GetPathForTest()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                                    , "..\\..\\..\\PurpleBricksTejas\\App_Data\\PurpleBoardsLeases.xml");
        }        
    }
}