using SupplyMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SupplyMe.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.bodyClass = "login-body";
            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            return null;
        }

        public ActionResult Request()
        {
            ViewBag.bodyClass = "request-body";
            ItemRowPartial row = new ItemRowPartial();
            row.RowIndex = 0;
            return View(row);
        }

        [HttpPost]
        public ActionResult AddItemPartialView(int RowIndex)
        {
            ItemRowPartial newitem = new ItemRowPartial();
            newitem.RowIndex = RowIndex + 1;
            return PartialView("_ItemRowPartial", newitem);
        }
    }
}
