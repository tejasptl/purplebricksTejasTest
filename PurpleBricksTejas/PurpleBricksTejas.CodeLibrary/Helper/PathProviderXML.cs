using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PurpleBricksTejas.CodeLibrary
{
    public class PathProviderXML
    {
        public virtual string GetPathForTest()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                                    , "..\\..\\..\\PurpleBricksTejas\\App_Data\\PurpleBoardsLeases.xml");
        }        
    }
}