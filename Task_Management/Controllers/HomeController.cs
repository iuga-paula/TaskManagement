using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Task_Management.Filters;
using Task_Management.Models;

namespace Task_Management.Controllers
{
    [CustomFilter]
    public class HomeController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                var currentUserId = User.Identity.GetUserId();
                ViewBag.nrInv = db.ProjectUsers.Include("User").Include("Project").Where(u => u.UserId == currentUserId && u.Accepted == false).Count();
                if (ViewBag.nrInv > 0 && Session["visits"] == null) //Session["visits"].Equals(null))
                {
                    ViewBag.hasInv = true;
                    Session["visits"] = 0;
                }
                else ViewBag.hasInv = false;

                
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}