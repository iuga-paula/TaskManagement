using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Task_Management.Filters;
using Task_Management.Models;

namespace TaskManagement.Controllers
{
    [CustomFilter]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();//acces la baza de date

        // GET: Comments
        public ActionResult Index()
        {
            return View();
        }


        // POST - NEW : Comments
        [HttpPost]
        public ActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;
            comm.UserId = User.Identity.GetUserId();

            Task t = db.Tasks.Find(comm.TaskId);

           var taskComments =  from task in db.Tasks.Include("Comments").Include("Comments.User")
                               where task.TaskId == comm.TaskId
                               select task;

            ViewBag.TaskComments = taskComments;

            /*var comms = (from comment in db.Comments.Include("User")
                         where comment.TaskId == comm.TaskId
                         select comment).ToList();

            ViewBag.Comments = comms;*/

            try
            {
                if (ModelState.IsValid)
                {
                    db.Comments.Add(comm);
                    db.SaveChanges();
                    return RedirectToAction("Show", "Tasks", new { id = comm.TaskId });
                }
                else
                {
                    ViewBag.ProjectId = t.ProjectId;
                    return View("~/Views/Tasks/Show.cshtml", comm);
                }

            }
            catch (Exception e)
            {
                ViewBag.ProjectId = t.ProjectId;

                return View("~/Views/Tasks/Show.cshtml", comm);
            }

        }
        
        //GET EDIT:Comments  => afis comm ce urm a fi editat
        public ActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                
                ViewBag.Comment = comm;
                return View(comm);
            }
            else
            {
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
        }

        //PUT EDIT:Comments => modif comm-ul
        [HttpPut]
        public ActionResult Edit(int id, Comment requestComment)
        {
            Comment comment = db.Comments.Find(id);
            try
            {
                if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    if (ModelState.IsValid)
                    {
                        Comment comm = db.Comments.Find(id);
                        if (TryUpdateModel(comm))
                        {
                            comm.Content = requestComment.Content;
                            db.SaveChanges();


                        }

                        return Redirect("/Tasks/Show/" + comm.TaskId);

                    }
                    
                   
                }
                
                return View(requestComment);
              
            }
            catch (Exception e)
            {
                return Redirect("/Tasks/Show/" + requestComment.TaskId);
            }

        }

        // DELETE: Comments
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            { var taskCommId = from task in db.Tasks.Include("Comments")
                               where task.TaskId == comm.TaskId
                               select task.TaskId;

                //Task taskc = db.Tasks.Find(taskCommId);

                db.Comments.Remove(comm);
                TempData["messagedelcomm"] = "Your comment has been succefully deleted.";
                db.SaveChanges();
            }

            return Redirect("/Tasks/Show/" + comm.TaskId);
        }
    }
}