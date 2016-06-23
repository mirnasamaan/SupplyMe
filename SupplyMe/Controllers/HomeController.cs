using DataLayer.Context;
using DataLayer.Repositories;
using Infrastructure.Interfaces;
using SupplyMe.Models;
using SupplyMe.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Infrastructure.Services;
using System.Net.Mail;
using System.Net;

namespace SupplyMe.Controllers
{
    public class HomeController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        HotelRepository _hotelRepo = new HotelRepository();
        RequestRepository _reqRepo = new RequestRepository();

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            base.Initialize(requestContext);
        }

        public ActionResult Login()
        {
            ViewBag.bodyClass = "login-body";
            return View();
        }

        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(HotelLogin hotel)
        {
            string remoteip = Request.UserHostAddress;
            string recaptcha = Request.Form["g-recaptcha-response"];
            bool valid = CaptchaHelper.ValidateCaptcha("6LdemCATAAAAANy-Bgy9Mz7BAse9cvptGV4qFJsD", recaptcha, remoteip);
            if (valid)
            {
                if (ModelState.IsValid)
                {
                    //string hashedPassword = PasswordHash.GetMd5Hash(hotel.Password);
                    Hotel db_hotel = _hotelRepo.HotelExists(hotel.UserName, hotel.Password);
                    if (db_hotel == null)
                    {
                        ModelState.AddModelError("", "Please check your username and password");
                        ViewBag.bodyClass = "login-body";
                        return View(hotel);
                    }
                    FormsService.SignIn(hotel.UserName, false);
                    _hotelRepo.ModifyLastLogin(db_hotel);
                }
                return Redirect("/supplyrequest");
            }
            return RedirectToAction("Login");
        }

        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsService.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult SupplyRequest()
        {
            if (_hotelRepo.GetHotelByUserName(User.Identity.Name) != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    ViewBag.bodyClass = "request-body";
                    //List<Category> cats = _reqRepo.GetParentCategories().ToList();
                    //ViewBag.Cats = cats;
                    IQueryable<Category> cats = _reqRepo.GetParentCategories();
                    List<CategoryVM> parent_cats = new List<CategoryVM>();
                    foreach (var item in cats)
                    {
                        parent_cats.Add(new CategoryVM { ID = item.CategoryId, Name = item.CategoryName });
                    }
                    ViewBag.Cats = parent_cats;
                    return View(new OrderVM());
                }
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult SupplyRequest(OrderVM ordervm)
        {
            if (_hotelRepo.GetHotelByUserName(User.Identity.Name) != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    ViewBag.bodyClass = "request-body";
                    if (!ModelState.IsValid || ordervm.OrderDetails.Any(i => i.ItemId == 0) || ordervm.OrderDetails.Any(i => i.OrderMessage == ""))
                    {
                        if (ordervm.OrderDetails.Any(i => i.ItemId == 0))
                        {
                            ModelState.AddModelError("NoCategory", "The Item field is required.");
                        }
                        if (ordervm.OrderDetails.Any(i => i.OrderMessage == ""))
                        {
                            ModelState.AddModelError("NoMessage", "The Order Details field is required.");
                        }
                        IQueryable<Category> cats = _reqRepo.GetParentCategories();
                        List<CategoryVM> parent_cats = new List<CategoryVM>();
                        foreach (var item in cats)
                        {
                            parent_cats.Add(new CategoryVM { ID = item.CategoryId, Name = item.CategoryName });
                        }
                        ViewBag.Cats = parent_cats;
                        return View(ordervm);
                    }
                    else
                    {
                        Order order = new Order();
                        order.UserId = _hotelRepo.GetHotelByUserName(User.Identity.Name).HotelId;
                        foreach (var item in ordervm.OrderDetails)
                        {
                            OrderDetail detail = new OrderDetail();
                            detail.OrderItemId = item.ItemId;
                            detail.OrderQty = 0;
                            detail.OrderMessage = item.OrderMessage;
                            order.OrderDetails.Add(detail);
                        }
                        Order addedOrder = _reqRepo.AddOrder(order);
                        if (addedOrder != null)
                        {
                            OrderReport reportedOrder = new OrderReport();
                            reportedOrder.OrderId = addedOrder.OrderId;
                            reportedOrder.OrderReportDetails = new List<OrderReportDetail>();
                            foreach (var item in addedOrder.OrderDetails)
                            {
                                string unitname = "";
                                if (item.OrderUnitId == null)
                                {
                                    unitname = "Default";
                                }
                                else
                                {
                                    unitname = _reqRepo.GetUnitById(item.OrderUnitId.Value).UnitName;
                                }
                                Item cat_item = _reqRepo.GetItemById(item.OrderItemId);
                                reportedOrder.OrderReportDetails.Add(
                                    new OrderReportDetail
                                    {
                                        CategoryName = cat_item.Category.CategoryName,
                                        ItemName = cat_item.ItemName,
                                        Qty = item.OrderQty,
                                        UnitName = unitname,
                                        OrderMessage = item.OrderMessage
                                    });
                            }

                            AdminEmail(reportedOrder);
                            TempData["order"] = reportedOrder;
                            return RedirectToAction("ThankYou");
                        }
                        //return RedirectToAction("Request");
                    }
                }
                return View(ordervm);
            }
            return RedirectToAction("Login");
        }

        public ActionResult AddItemPartialView()
        {
            //    List<Category> cats = _reqRepo.GetParentCategories().ToList();
            //    ViewBag.Cats = cats;
            IQueryable<Category> cats = _reqRepo.GetParentCategories();
            List<CategoryVM> parent_cats = new List<CategoryVM>();
            foreach (var item in cats)
            {
                parent_cats.Add(new CategoryVM { ID = item.CategoryId, Name = item.CategoryName });
            }
            ViewBag.Cats = parent_cats;
            return PartialView("_ItemRowPartial", new OrderDetailsVM());
        }

        [HttpPost]
        public ActionResult GetUnitsByItemId(int itemId)
        {
            IQueryable<Unit> units = _reqRepo.GetUnitsByItemId(itemId);
            List<UnitVM> data = new List<UnitVM>();
            foreach (var item in units)
            {
                data.Add(new UnitVM { UnitId = item.UnitId, UnitName = item.UnitName });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ThankYou()
        {
            ViewBag.bodyClass = "request-body";
            OrderReport order = (OrderReport)TempData["order"];
            if (order == null)
            {
                return RedirectToAction("SupplyRequest");
            }
            return View(order);
        }

        [HttpPost]
        public ActionResult GetCategoryId(int catId)
        {
            Category cat = _reqRepo.GetCategoryByID(catId);
            return PartialView("_ItemSelectPartial", cat);
        }

        private void AdminEmail(OrderReport order)
        {
            Hotel hotel = _hotelRepo.GetHotelByUserName(User.Identity.Name);
            MailMessage message = new MailMessage();
            message.To.Add("hammad.mt@gmail.com");
            message.From = new MailAddress("noreply@supplyme.com", "No Reply");
            message.Subject = "Supply Me Order Request";
            message.IsBodyHtml = true;
            string msg = "<p>A new order has been requested with the following details.</p></br>";
            msg = msg + "<p>Hotel Name: " + hotel.HotelName + "</p></br><p>Hotel Email: " + hotel.HotelEmail + "</p></br>";
            msg = msg + "<p>Phone Number: " + hotel.HotelPhoneNumber + "</p></br><p>Contact Person: " + hotel.HotelContactPerson + "</p></br>";
            msg = msg + "<p>Order Id: " + order.OrderId + "</p></br>";
            msg += "<table style='width: 100%'><thead><tr><th style='border: 1px solid; padding: 10px;' width='100px'>Category</th><th style='border: 1px solid; padding: 10px;' width='100px'>Item</th>";
            msg += "<th style='border: 1px solid'>Details</th></tr></thead><tbody>";
            foreach (var item in order.OrderReportDetails)
            {
                msg += "<tr><td style='border: 1px solid; padding: 10px;' width='100px'>" + item.CategoryName + "</td><td style='border: 1px solid; padding: 10px;' width='100px'>" + item.ItemName + "</td>";
                msg += "<td style='border: 1px solid'>" + item.OrderMessage + "</td></tr>";
            }
            msg += "</tbody></table>";
            message.Body = msg;

            try
            {
                System.Net.Mail.SmtpClient smtp = new SmtpClient("relay-hosting.secureserver.net", 25);
                smtp.Send(message);
            }
            catch (Exception ex)
            {
            }
        }

    }
}
