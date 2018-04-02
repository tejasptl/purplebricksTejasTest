using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace PurpleBricksTejas.CodeLibrary
{
    public class Property
    {
        #region Properties

        [Display(Name = "Id")]
        public int PropertyID { get; set; }

        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Display(Name = "Property State")]
        public string State { get; set; }

        [Display(Name = "PostCode")]
        public int PostCode { get; set; }

        #endregion

        #region Constructors

        public Property()
        {

        }

        public Property(Property property)
        {
            this.PropertyID = property.PropertyID;
            this.StreetAddress = property.StreetAddress;
            this.Suburb = property.Suburb;
            this.State = property.State;
            this.PostCode = property.PostCode;
        }

        #endregion

        #region Methods

        // In this region,
        // Need to Implement 
        // Find, Add/Update and Delete methods by use database access wrapper class.

        #endregion

    }
}
