using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PurpleBricksTejas.Models;

namespace PurpleBricksTejas.Controllers
{
    public class PurpleBoardController : Controller
    {
        //
        // GET: /PurpleBoard/       

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

            model.CalculateBoardPrice();
           
            return Json("{\"Cost\": " + model.Cost + "}", "application/json", JsonRequestBehavior.AllowGet);           
        }


    }
}
