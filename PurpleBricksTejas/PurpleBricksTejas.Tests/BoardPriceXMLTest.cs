using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit;
using NUnit.Framework;
using PurpleBricksTejas.CodeLibrary;

namespace PurpleBricksTejas.Tests
{
    [TestFixture]
    public class BoardPriceXMLTest
    {
        [TestCase("VIC", "Small", 2, ExpectedResult = 20)]
        [TestCase("VIC", "Small", 2, ExpectedResult = 18)]        
        public double TestForGetBPriceByFilter(string state,string boardSize, int daysOrder)
        {
            XDocument xDoc = XDocument.Load("..//..//..//PurpleBricksTejas//App_Data//PurpleBoardsLeases.xml");
            return PurpleBoardPriceXMLHelper.GetPriceByFilter(xDoc, state, boardSize, daysOrder);
        }


        [TestCase("VIC", 2, ExpectedResult = 0)]
        [TestCase("VIC", 12, ExpectedResult = 10)]
        [TestCase("NSW", 2, ExpectedResult = 0)]
        [TestCase("NSW", 14, ExpectedResult = 15)]
        public double TestForGetDiscountRate(string state, int daysOrder)
        {
            XDocument xDoc = XDocument.Load("..//..//..//PurpleBricksTejas//App_Data//PurpleBoardsLeases.xml");
            return PurpleBoardPriceXMLHelper.GetDiscountRate(xDoc, state, daysOrder);
        }



    }
}
