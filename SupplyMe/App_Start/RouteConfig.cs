using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SupplyMe
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "login",
                url: "login",
                defaults: new { controller = "Home", action = "Login" }
            );

            routes.MapRoute(
                name: "supplyrequest",
                url: "supplyrequest",
                defaults: new { controller = "Home", action = "SupplyRequest" }
            );

            routes.MapRoute(
                name: "thankyou",
                url: "thankyou",
                defaults: new { controller = "Home", action = "ThankYou" }
            );

            routes.MapRoute(
                name: "logoff",
                url: "logoff",
                defaults: new { controller = "Home", action = "LogOff" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "SupplyRequest", id = UrlParameter.Optional }
            );
        }
    }
}