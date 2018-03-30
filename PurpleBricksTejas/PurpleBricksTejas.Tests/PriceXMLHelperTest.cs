using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurpleBricksTejas.CodeLibrary;
using System.IO;
using System.Web;
using System.Xml.Linq;

namespace PurpleBricksTejas.Tests
{
    /// <summary>
    /// Summary description for PriceXMLHelperTest
    /// </summary>
    [TestClass]
    public class PriceXMLHelperTest
    {
        public PriceXMLHelperTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetPricefor_VIC_Small_WithDiscount()
        {
            string state = "VIC";
            string boardSize = "Small";
            int daysOrder = 12;
            double actual = 0;
            double expected = 0.0;

            expected = 18;            
            XDocument xDoc = XDocument.Load("..//..//..//PurpleBricksTejas//App_Data//PurpleBoardsLeases.xml");
            actual = PurpleBoardPriceXMLHelper.GetPriceByFilter(xDoc, state, boardSize, daysOrder);

            Assert.AreEqual(expected, actual, "No Matching");
        }
    }
}

