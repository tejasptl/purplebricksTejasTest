using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace PurpleBricksTejas.CodeLibrary
{
    public class Customer
    {
        #region Properties

        [Display(Name = "Id")]
        public int CustomerID { get; set; }

        [Display(Name = "Given Names")]
        public string GivenNames { get; set; }

        [Display(Name = "Sur Name")]
        public string Surname { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Contact No")]
        public int ContactNo { get; set; }

        #endregion


        #region Constructor

        public Customer()
        {

        }

        public Customer(Customer customer)
        {
            this.CustomerID = customer.CustomerID;
            this.GivenNames = customer.GivenNames;
            this.Surname = customer.GivenNames;
            this.Email = customer.Email;
            this.ContactNo = customer.ContactNo;
        }

        #endregion


        #region Methods

        // In this region,
        // Need to Implement 
        // Find, Add/Update and Delete methods by use database access wrapper class.

        #endregion

    }
}
