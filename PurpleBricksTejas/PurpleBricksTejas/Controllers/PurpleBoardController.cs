using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PurpleBricksTejas.CodeLibrary;
using PurpleBricksTejas.Models;

namespace PurpleBricksTejas.Controllers
{
    public class PurpleBoardController : Controller
    {    
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult BoardPriceEstimator()
        {
            PurpleBoardLeaseModel model = new PurpleBoardLeaseModel();           
            return View(model);
        }      

        [AllowAnonymous]
        public JsonResult GetBoardPrice(string boardSize, string propertyState, string fromDate, string toDate)
        {
            PurpleBoardLeaseModel model = new PurpleBoardLeaseModel();

            model.BoardSize = boardSize;
            model.Property.State = propertyState;
            model.FromDate = Convert.ToDateTime(fromDate);
            model.ToDate = Convert.ToDateTime(toDate);

            model.CalculateBoardCost();            

            return Json("{\"Cost\": " + Math.Round(model.Cost, 2, MidpointRounding.AwayFromZero) + ",\"PricePerDay\": " + Utils.FormatMoney(model.PricePerDay)
                            + ",\"DaysOrder\": " + model.DaysOrder + ",\"Discount\": " + Utils.FormatMoney(model.Discount) + "}"
                            , "application/json", JsonRequestBehavior.AllowGet);           
        }


    }
}
