using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Task_Management.Models;

namespace Task_Management.Controllers
{
    [Authorize(Roles = "User,TeamLeader,Admin")]
    [Filters.CustomFilter]
    public class ProjectUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ProjectUsers
       
        public ActionResult Index()//Invitations
        {
            var currentUserId = User.Identity.GetUserId();
            var invitations = db.ProjectUsers.Include("User").Include("Project").Where(u => u.UserId == currentUserId && u.Accepted == false); 
            ViewBag.Invitations = invitations;
            ViewBag.nrInv = invitations.Count();
            if (ViewBag.nrInv != 0)
                ViewBag.hasInv = true;
            else ViewBag.hasInv = false;

            if (TempData.ContainsKey("AcceptedInv"))
                ViewBag.Accepted = TempData["AcceptedInv"];

            return View();
        }

        public ActionResult Accept(int id)
        {
            try
            {
                ProjectUser projectUser = db.ProjectUsers.Include("Project").Include("Project.User").Where(m => m.ProjectUserId == id).First();
                if (projectUser.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    if (TryUpdateModel(projectUser))
                    { projectUser.Accepted = true;
                        projectUser.JoinDate = DateTime.Now;
                        ApplicationDbContext.SendEmailNotification(projectUser.Project.User.Email, projectUser.User.UserName + " has joined your project " + projectUser.Project.Name, "THE MORE THE MERRIER");
                        db.SaveChanges();
                    }
                TempData["AcceptedInv"] = "You have joined the team!";
                return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                return RedirectToAction("Index");
            }
         }


        public ActionResult Reject(int id)
        {
            try
            {
                ProjectUser projectUser = db.ProjectUsers.Include("Project").Include("Project.User").Where(m => m.ProjectUserId == id).First();
                if (projectUser.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    db.ProjectUsers.Remove(projectUser);
                    ApplicationDbContext.SendEmailNotification(projectUser.Project.User.Email, "Your invitation to " + projectUser.User.UserName + " has been declined", "That's so sad");
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
        }
    }

   
}