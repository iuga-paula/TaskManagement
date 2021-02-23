using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Task_Management.Models;

namespace Task_Management
{
    public class MvcApplication : System.Web.HttpApplication
    {
       /* private ApplicationDbContext db = new ApplicationDbContext();//acces la baza de date*/

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start()
        {
            /*var tasks = (select Task).ToList();
           var*/
            System.Web.HttpContext.Current.Session["projects"] = "";
            System.Web.HttpContext.Current.Session["tasks"] = "";

        }
    }
}
