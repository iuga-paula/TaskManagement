using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web.Routing;
using Task_Management.Models;
using Microsoft.AspNet.Identity;

namespace Task_Management.Filters
{
    public class CustomFilter: ActionFilterAttribute
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Log("OnActionExecuting", filterContext.RouteData);

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var currentUserId = HttpContext.Current.User.Identity.GetUserId();


                if (HttpContext.Current.User.IsInRole("Admin"))
                {
                    //daca e admin are acces la toate taskurile + echipele

                   var tasks = (from task in db.Tasks
                             select task).ToList(); //preiau toate taskurile

                    var projects = db.ProjectUsers.Include("User").Include("Project").ToList();
                    filterContext.Controller.ViewBag.UserTasks = tasks;
                    filterContext.Controller.ViewBag.UserProjects = projects;

                }
                else
                {
                   var tasks = (from task in db.Tasks
                             where task.UserId == currentUserId
                             select task).ToList(); //preiau taskurile userului curent

                    var projects = db.ProjectUsers.Include("User").Include("Project").Where(u => u.UserId == currentUserId && u.Accepted == true).ToList();////preiau proiectele la care paticipa userul curent
                    filterContext.Controller.ViewBag.UserTasks = tasks;
                    filterContext.Controller.ViewBag.UserProjects = projects;
                }

            }
           
        }


        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0} controller:{1} action:{2}", methodName, controllerName, actionName);
            Debug.WriteLine(message, "Action Filter Log");
        }
    }
}