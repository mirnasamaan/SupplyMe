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
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(HotelLogin hotel)
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
            return Redirect("/request");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Request()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.bodyClass = "request-body";
                List<Category> cats = _reqRepo.GetParentCategories().ToList();
                ViewBag.Cats = cats;
                return View(new OrderVM());
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Request(OrderVM ordervm)
        {
            if(User.Identity.IsAuthenticated) {
                ViewBag.bodyClass = "request-body";
                if (!ModelState.IsValid || ordervm.OrderDetails.Any(i => i.ItemId == 0) || ordervm.OrderDetails.Any(i => i.Qty == 0))
                {
                    if (ordervm.OrderDetails.Any(i => i.ItemId == 0))
                    {
                        ModelState.AddModelError("NoCategory", "The Item field is required.");
                    } 
                    if (ordervm.OrderDetails.Any(i => i.Qty == 0))
                    {
                        ModelState.AddModelError("NoQty", "The Qty field is required.");
                    }
                    List<Category> cats = _reqRepo.GetParentCategories().ToList();
                    ViewBag.Cats = cats;
                    return View(ordervm);
                }
                else
                {
                    Order order = new Order();
                    order.UserId = _hotelRepo.GetHotelByUserName(User.Identity.Name).HotelId;
                    order.OrderMessage = ordervm.OrderMessage;
                    foreach (var item in ordervm.OrderDetails)
                    {
                        OrderDetail detail = new OrderDetail();
                        detail.OrderItemId = item.ItemId;
                        detail.OrderQty = item.Qty;
                        detail.OrderItemId = item.ItemId;
                        detail.OrderUnitId = item.UnitId;
                        order.OrderDetails.Add(detail);
                    }
                    Order addedOrder = _reqRepo.AddOrder(order);
                    if (addedOrder != null)
                    {
                        OrderReport reportedOrder = new OrderReport();
                        reportedOrder.OrderId = addedOrder.OrderId;
                        reportedOrder.OrderMsg = addedOrder.OrderMessage;
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
                            reportedOrder.OrderReportDetails.Add(
                                new OrderReportDetail { 
                                    ItemName = _reqRepo.GetItemById(item.OrderItemId).ItemName,
                                    Qty = item.OrderQty,
                                    UnitName = unitname
                                });
                        }
                        
                        AdminEmail(reportedOrder);
                        TempData["order"] = reportedOrder;
                        return RedirectToAction("ThankYou");
                    }
                    //return RedirectToAction("Request");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult AddItemPartialView()
        {
            List<Category> cats = _reqRepo.GetParentCategories().ToList();
            ViewBag.Cats = cats;
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
            return View(order);
        }

        private void AdminEmail(OrderReport order)
        {
            Hotel hotel = _hotelRepo.GetHotelByUserName(User.Identity.Name);
            MailMessage message = new MailMessage();
            message.To.Add("m.ashraf@enlightworld.com");
            message.From = new MailAddress("m.ashraf@enlightworld.com");
            message.Subject = "Supply Me Order Request";
            message.IsBodyHtml = true;
            string msg = "<p>A new order has been requested with the following details.</p></br>";
            msg = msg + "<p>Hotel Name: " + hotel.HotelName + "</p></br><p>Hotel Email: " + hotel.HotelEmail + "</p></br>";
            msg = msg + "<p>Phone Number: " + hotel.HotelPhoneNumber + "</p></br><p>Contact Person: " + hotel.HotelContactPerson + "</p></br>";
            msg = msg + "<p>Order Id: " + order.OrderId + "</p></br><p>Order Message: " + order.OrderMsg + "</p></br>";
            msg += "<table style='width: 100%'><thead><tr><th style='border: 1px solid'>Item</th>";
            msg += "<th style='border: 1px solid'>Qty</th><th style='border: 1px solid'>Unit</th></tr></thead><tbody>";
            foreach(var item in order.OrderReportDetails) {
                msg += "<tr><td style='border: 1px solid'>" + item.ItemName + "</td>";
                msg += "<td style='border: 1px solid'>" + item.Qty + "</td><td style='border: 1px solid'>" + item.UnitName + "</td></tr>";
            }
            msg += "</tbody></table>";
            message.Body = msg;

            try
            {
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.googlemail.com";
                client.Port = 587;
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("m.ashraf@enlightworld.com", "MirnaSamaan92");
                client.Send(message); 
            }
            catch (Exception ex)
            {
            }
        }

    }
}
