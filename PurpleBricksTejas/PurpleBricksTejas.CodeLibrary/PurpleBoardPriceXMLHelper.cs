using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace PurpleBricksTejas.CodeLibrary
{
    public class PurpleBoardPriceXMLHelper
    {
        public decimal BoardPrice { get; set; }
        public string State { get; set; }
        public string BoardSize { get; set; }

        /// <summary>
        /// This method returns discounted final price through XML file
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="state"></param>
        /// <param name="boardSize"></param>
        /// <param name="daysOrder"></param>
        /// <returns></returns>
        public static double GetPriceByFilter(XDocument xDoc, string state, string boardSize, int daysOrder)
        {
            try
            {
                if (xDoc == null || String.IsNullOrWhiteSpace(state)
                    || String.IsNullOrWhiteSpace(boardSize))
                {
                    throw new ApplicationException("All manadtory fields should be provided.");
                }

                double finalPrice = 0;
                double price = xDoc.Descendants("PriceRecord")
                                .Where(r => r.Element("State").Value == state
                                        && r.Element("Size").Value == boardSize)
                                .Select(r => Convert.ToDouble(r.Element("Price").Value)).FirstOrDefault();

                double discountRate = GetDiscountRate(xDoc, state, daysOrder);

                if (discountRate > 0)
                    finalPrice = price - ((price * discountRate) / 100);
                else
                    finalPrice = price;

                return finalPrice;
            }
            catch(Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// This method returns discount rate according to filter through XML file
        /// </summary>
        /// <param name="xDoc"></param>
        /// <param name="state"></param>
        /// <param name="daysOrder"></param>
        /// <returns></returns>
        public static double GetDiscountRate(XDocument xDoc, string state, int daysOrder)
        {           
            return xDoc.Descendants("DiscountRecords")
                                .Where(r => daysOrder > Convert.ToInt32(r.Element("DaysOrder").Value)
                                        && state == r.Element("State").Value)
                                    .Select(r => Convert.ToDouble(r.Element("Discount").Value)).FirstOrDefault();
        }     

    }
}
