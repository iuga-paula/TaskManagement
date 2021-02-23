using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Task_Management.Filters;
using Task_Management.Models;


namespace TaskManagement.Controllers
{
    [Authorize(Roles = "User,TeamLeader,Admin")]
    [CustomFilter]
    public class TasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int perPage = 3;
        // GET: Task
        public ActionResult Index()
        {
            ViewBag.Tasks = db.Tasks;


            return View();
        }

        public ActionResult Show(int id)
        {
            Task thistask = db.Tasks.Find(id);
            int projectId = thistask.ProjectId;
            ViewBag.ProjectId = projectId;
            ViewBag.TaskId = id;
            ViewBag.CurrentUser = User.Identity.GetUserId();


            var taskComments = (from task in db.Tasks.Include("Comments").Include("Comments.User")
                                where task.TaskId == id
                                select task).ToList();
            ViewBag.TaskComments = taskComments;
            //pt a primi un comm paramentru de la edit ca sa afiseze validarile incarcate trb sa primeasca view -ul un obiect de tip comment

            Comment comm = new Comment();

            if (TempData.ContainsKey("messagedelcomm"))
            {
                ViewBag.messagedelcomm = TempData["messagedelcomm"].ToString();
            }

            return View(comm);
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        [HttpPost]
        public ActionResult New(Task Task)
        {
            Project projectTasks = (from project in db.Projects.Include("Tasks").Include("ProjectUsers").Include("User")
                                    where project.ProjectId == Task.ProjectId
                                    select project).First();
            ViewBag.ProjectTasks = projectTasks;



            try
            {
                if (projectTasks.UserId == User.Identity.GetUserId() || User.IsInRole("Admin")) //doar teamleaderul echipei respective are voie sa adauge taskuri
                {
                    if (Task.StartDate != null && Task.StartDate <= projectTasks.StartDate)
                    {
                        ViewBag.StartDate = "Invalid task start date";
                    }

                    if(Task.FinDate != null && (Task.FinDate >= projectTasks.FinDate || Task.FinDate <= Task.StartDate))
                    {
                        ViewBag.FinDate = "Invalid task final date";
                    }

                    if (ModelState.IsValid && ViewBag.StartDate == null && ViewBag.FinDate == null)
                    {
                       
                            if (DateTime.Now < Task.StartDate)
                                Task.Status = Status.NotStarted;
                            else
                                Task.Status = Status.InProgress;
                            db.Tasks.Add(Task);
                            db.SaveChanges();

                            TempData["tasknewmessage"] = "Task sucessfully added!";
                            return RedirectToAction("Show", "Projects", new { id = Task.ProjectId });
                       
                    }
                    else
                    {
                        ViewBag.Date = DateTime.Now;

                        Project projectTasks1 = (from project in db.Projects.Include("Tasks").Include("ProjectUsers").Include("User")
                                                where project.ProjectId == Task.ProjectId
                                                select project).First();
                        ViewBag.ProjectTasks = projectTasks1;
                        if (projectTasks.ProjectUsers.Count() > 1)
                            ViewBag.HasMembers = true;
                        var currentTasks = projectTasks.Tasks;
                        ViewBag.CurrentUser = User.Identity.GetUserId();
                        var currentPage = Convert.ToInt32(Request.Params.Get("page")); //extrage paramentul numarul paginii din ruta

                        var totalItems = currentTasks.Count();
                        var offset = 0;

                        if (!currentPage.Equals(0))
                        {
                            offset = (currentPage - 1) * this.perPage;
                        }

                        var paginatedTasks = currentTasks.Skip(offset).Take(this.perPage); //sar peste taskurile deja afisate pe paginile anterioare

                       


                        ViewBag.total = totalItems;
                        ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this.perPage);
                        if (paginatedTasks.Count() > 0)
                            ViewBag.currentTasks = paginatedTasks;
                        Task.Members = GetTeamMembers(Task.ProjectId);

                        return View("~/Views/Projects/Show.cshtml", Task);
                    }
                }
                else
                {
                    TempData["err"] = "You are not allowed to do this!";
                    return RedirectToAction("Show", "Projects", new { id = Task.ProjectId });
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e;

                return View("~/Views/Projects/Show.cshtml", Task);
            }
            //return RedirectToAction("Show", "Projects", new { id = Task.ProjectId });
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Task toDelete = db.Tasks.Find(id);
            int projectId = toDelete.ProjectId;

            if (toDelete.Project.UserId == User.Identity.GetUserId() || User.IsInRole("Admin")) //doar teamleaderul echipei respective are voie sa stearga taskuri
            {
                
                db.Tasks.Remove(toDelete);
                db.SaveChanges();

                TempData["taskmessage"] = toDelete.Title + " task has been deleted.";
            }
            else
            {
                ViewBag.err = "You are not allowed to do this.";

            }
            return Redirect("/Projects/Show/" + projectId);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetTeamMembers(int projectId)
        {
            var selectList = new List<SelectListItem>();

            var project = db.Projects.Include("ProjectUsers").Include("User").Where(m => m.ProjectId == projectId).First();


            foreach (ProjectUser user in project.ProjectUsers)//in dropdown nu trb sa apara si membrii care nu au acceptat inca invitatia la proiect
            {
                if (user.Accepted)
                {
                    var membru = new SelectListItem();
                    membru.Value = user.UserId;
                    membru.Text = user.User.UserName.ToString();
                    selectList.Add(membru);
                }
            }
            return selectList;
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        public ActionResult Edit(int id)
        {
            ViewBag.Date = DateTime.Now;
            Task task = db.Tasks.Find(id);
            if (task.Project.UserId == User.Identity.GetUserId() || User.IsInRole("Admin")) //doar teamleaderul echipei respective are voie sa editeze taskuri
            {
                task.Stat = GetAllStat();
                task.Members = GetTeamMembers(task);
            }
            else
            {
                ViewBag.err = "You are not allowed to do this.";
            }

            return View(task);
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        [HttpPut]
        public ActionResult Edit(int id, Task requestTask)
        {
            requestTask.Members = GetTeamMembers(requestTask);
            requestTask.Stat = GetAllStat();
            Task Task = db.Tasks.Find(id);
            Project project = db.Projects.Find(Task.ProjectId);

            try
            {
                if (Task.Project.UserId == User.Identity.GetUserId() || User.IsInRole("Admin")) //doar teamleaderul echipei respective are voie sa editeze taskuri
                {
                    if (requestTask.StartDate <= project.StartDate)
                    {
                        ViewBag.StartDate = "Invalid task start date";
                    }

                    if (requestTask.FinDate >= project.FinDate)
                    {
                        ViewBag.FinDate = "Final date can\'t be after project deadline";
                    }
                    if(requestTask.FinDate <= requestTask.StartDate)
                    {
                        ViewBag.FinDate += "\nStart date can\'t be after finish date";
                    }
                    if (ModelState.IsValid && ViewBag.StartDate == null && ViewBag.FinDate == null)
                    {
                        
                        if (TryUpdateModel(Task))
                        {
                            System.Diagnostics.Debug.WriteLine("se prelucreaza task");

                            Task.Title = requestTask.Title;
                            Task.StartDate = requestTask.StartDate;
                            Task.FinDate = requestTask.FinDate;
                            Task.Description = requestTask.Description;
                            Task.UserId = requestTask.UserId;
                            Task.ProjectId = requestTask.ProjectId;
                            Task.Status = requestTask.Status;
                            db.SaveChanges();
                        }
                        else
                        {
                            TempData["err"] = "Invalid task update";
                            return View(requestTask);

                        }
                        TempData["taskmessage"] = "Task \"" + Task.Title + "\" succesfully edited!";
                        return Redirect("/Projects/Show/" + Task.ProjectId);
                    }
                    else
                    {
                        TempData["err"] = "Invalid task update";
                        return View(requestTask);
                    }
                }
                else
                {
                    TempData["err"] = "You are not allowed to do this.";
                    return Redirect("/Projects/Show/" + requestTask.ProjectId);
                }
                //return View(requestTask);
            }
            catch (Exception e)
            {
                ViewBag.Error = e;

                return Redirect("/Tasks/Edit/" + id);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStat()
        {
            var selectList = new List<SelectListItem>();

            var listItm1 = new SelectListItem
            {
                Value = 0.ToString(),
                Text = "Not Started"
            };
            selectList.Add(listItm1);
            var listItm2 = new SelectListItem
            {
                Value = 1.ToString(),
                Text = "In Progress"
            };
            selectList.Add(listItm2);
            var listItm3 = new SelectListItem
            {
                Value = 2.ToString(),
                Text = "Completed"
            };
            selectList.Add(listItm3);
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetTeamMembers(Task thisTask)
        {
            var selectList = new List<SelectListItem>();

            var projects = db.Projects.Include("ProjectUsers").Include("User").ToList();

            foreach(Project proj in projects)
            {
                if(proj.ProjectId == thisTask.ProjectId)
                {
                    foreach(ProjectUser user in proj.ProjectUsers)//in dropdown nu trb sa apara si membrii care nu au acceptat inca invitatia la proiect
                    {
                        if (user.Accepted)
                        {
                            var membru = new SelectListItem();
                            membru.Value = user.UserId;
                            membru.Text = user.User.UserName.ToString();
                            selectList.Add(membru);
                        }

                    }
                    
                }
            }

            return selectList;
        }

        public ActionResult ChangeStatus(int id)
        {
            Task task = db.Tasks.Find(id);
            
                task.Stat = GetAllStat();
                task.Members = GetTeamMembers(task);
            
            return View(task);
        }

        [HttpPut]
        public ActionResult ChangeStatus(int id, Task requestTask)
        {
            requestTask.Members = GetTeamMembers(requestTask);
            requestTask.Stat = GetAllStat();
            Task task = db.Tasks.Find(id);
            task.Members = GetTeamMembers(task);
            var isMember = false;

            foreach(SelectListItem member in task.Members)
            {
                if(member.Value == User.Identity.GetUserId())
                {
                    isMember = true;
                    break;
                }

            }

            try
            {
                if (isMember || User.IsInRole("Admin")) 
                {
                    if (ModelState.IsValid)
                    {
                        Task Task = db.Tasks.Find(id);
                        if (TryUpdateModel(Task))
                        { 
                            Task.Status = requestTask.Status;
                            db.SaveChanges();
                        }

                        return Redirect("/Tasks/Show/" + Task.TaskId);
                    }
                }
                else
                {
                    ViewBag.err = "You are not allowed to do this.";
                }
                return View(requestTask);
            }
            catch (Exception e)
            {
                return Redirect("/Tasks/Show/" + requestTask.TaskId);
            }
        }
    }
}