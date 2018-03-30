using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using PurpleBricksTejas.CodeLibrary;

namespace PurpleBricksTejas.Models
{
    public class PurpleBoardLeaseModel : Customer
    {
        #region Properties

        [Display(Name = "Board Size")]
        public string BoardSize { get; set; }        

        [Display(Name = "Date From")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Date To")]
        public DateTime? ToDate { get; set; }

        public Property Property = new Property();

        [Display(Name = "Price Per Day")]
        public Double PricePerDay { get; set; }       

        [Display(Name = "Days Order")]
        public int DaysOrder { get; set; }

        [Display(Name = "Cost")]
        public Double Cost { get; set; }

        [Display(Name = "Discount")]
        public Double Discount { get; set; }


        #endregion

        #region Constructor

        public PurpleBoardLeaseModel()
        {
            this.FromDate = null;
            this.ToDate = null;
        }

        #endregion


        #region Methods

        public List<SelectListItem> LoadBoardSize()
        {
            List<SelectListItem> list = new List<SelectListItem>();            
            list.Add(new SelectListItem() { Text = "Small", Value = "Small" });
            list.Add(new SelectListItem() { Text = "Large", Value = "Large" });
            return list;
        }

        public List<SelectListItem> LoadState()
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

        public double CalculateBoardCost()
        {
            try
            {
                if (this.FromDate == null || this.ToDate == null)
                    throw new ApplicationException("From and To Date must be provided.");

                string xmlDocPath = Path.Combine(HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data"),
                                        "PurpleBoardsLeases.xml");
                XDocument xDoc = XDocument.Load(xmlDocPath);

                this.DaysOrder = (int)((DateTime)this.ToDate - (DateTime)this.FromDate).TotalDays + 1;
                if (this.DaysOrder < 0)
                    return 0;                

                this.PricePerDay = PurpleBoardPriceXMLHelper.GetPriceByFilter(xDoc, this.Property.State, this.BoardSize, this.DaysOrder);
                this.Discount = PurpleBoardPriceXMLHelper.GetDiscountRate(xDoc, this.Property.State, this.DaysOrder);
                this.Cost = Math.Round(PricePerDay * DaysOrder, 2);

                return this.Cost;
            }
            catch(Exception)
            {
                return 0;
            }
        }

        #endregion





    }
}