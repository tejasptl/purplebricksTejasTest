using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using NUnit.Framework;
using PurpleBricksTejas.CodeLibrary;

namespace PurpleBricksTejas.Tests
{
    [TestFixture]
    public class BoardPriceXMLTest
    {
        [TestCase("VIC", "Small", 2, ExpectedResult = 20)]
        [TestCase("VIC", "Small", 12, ExpectedResult = 18)]
        [TestCase("VIC", "Large", 2, ExpectedResult = 30)]
        [TestCase("VIC", "Large", 12, ExpectedResult = 27)]
        [TestCase("NSW", "Small", 2, ExpectedResult = 50)]
        [TestCase("NSW", "Small", 12, ExpectedResult = 42.50)]
        [TestCase("NSW", "Large", 2, ExpectedResult = 60)]
        [TestCase("NSW", "Large", 12, ExpectedResult = 51)]                
        public double TestForGetPriceByFilter(string state,string boardSize, int daysOrder)
        {
            PathProviderXML pathProvider = new PathProviderXML();           
            string xmlDocPath = pathProvider.GetPathForTest();
       
            XDocument xDoc = XDocument.Load(xmlDocPath);            
            return PurpleBoardPriceXMLHelper.GetPriceByFilter(xDoc, state, boardSize, daysOrder);
        }

        [Test]
        [TestCase("", "Large", 12)]
        [TestCase("VIC", "", 12)]        
        public void GetPriceWithoutSomeFilters(string state, string boardSize, int daysOrder)
        {
            PathProviderXML pathProvider = new PathProviderXML();
            string xmlDocPath = pathProvider.GetPathForTest();
            XDocument xDoc = XDocument.Load(xmlDocPath);

            Assert.That(() => PurpleBoardPriceXMLHelper.GetPriceByFilter(xDoc, state, boardSize, daysOrder),
               Throws.TypeOf<ApplicationException>());            
        }

        [TestCase("VIC", 2, ExpectedResult = 0)]
        [TestCase("VIC", 12, ExpectedResult = 10)]
        [TestCase("NSW", 2, ExpectedResult = 0)]
        [TestCase("NSW", 14, ExpectedResult = 15)]
        public double TestForGetDiscountRate(string state, int daysOrder)
        {
            PathProviderXML pathProvider = new PathProviderXML();           
            string xmlDocPath = pathProvider.GetPathForTest();

            XDocument xDoc = XDocument.Load(xmlDocPath);
            return PurpleBoardPriceXMLHelper.GetDiscountRate(xDoc, state, daysOrder);
        }
    }
}
