using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Task_Management.Filters;
using Task_Management.Models;

namespace ProjectManagement.Controllers
{
    [Authorize(Roles = "User,TeamLeader,Admin")]
    [CustomFilter]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int perPageProj = 2;
        private int perPageTask = 3;

        // GET: Project
        public ActionResult Index()
        {
            #region TempDataMsg
            if (TempData.ContainsKey("projectnewmessage"))
            {
                ViewBag.projectnewmessage = TempData["projectnewmessage"].ToString();
            }
            if (TempData.ContainsKey("projectmessage"))
            {
                ViewBag.projectmessage = TempData["projectmessage"].ToString();
            }
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            Session["inv"] = 1;
            #endregion

            string currentUserId = User.Identity.GetUserId();
            var AllProjects = db.Projects.Include("ProjectUsers").Include("User").ToList();
            List<Project> projects = new List<Project>();
            if (User.IsInRole("Admin"))
            { 
                projects = AllProjects;
            }
            else
            {
                foreach (var project in AllProjects)
                    if (HasMember(currentUserId, project.ProjectUsers))
                        projects.Add(project);
            }
            var currentPage = Convert.ToInt32(Request.Params.Get("page")); //extrage paramentul numarul paginii din ruta

            var totalItems = projects.Count();
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this.perPageProj;
            }

            var paginatedProjects = projects.Skip(offset).Take(this.perPageProj); //sar peste taskurile deja afisate pe paginile anterioare
            ViewBag.totalItems = totalItems;
            ViewBag.lastPage = Math.Ceiling((float) totalItems / (float) this.perPageProj);
            ViewBag.Projects = paginatedProjects;
            return View();
        }

        public ActionResult Show(int id)
        {
            ViewBag.Date = DateTime.Now;

            Project projectTasks = (from project in db.Projects.Include("Tasks").Include("ProjectUsers").Include("User")
                               where project.ProjectId == id
                               select project).First();
            ViewBag.ProjectTasks = projectTasks;
            if (projectTasks.ProjectUsers.Count() > 1)
                ViewBag.HasMembers = true;
            var currentTasks = projectTasks.Tasks;
            ViewBag.CurrentUser = User.Identity.GetUserId();
            var currentPage = Convert.ToInt32(Request.Params.Get("page")); //extrage paramentul numarul paginii din ruta
            
            var totalItems = currentTasks.Count();
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this.perPageTask;
            }

            var paginatedTasks = currentTasks.Skip(offset).Take(this.perPageTask); //sar peste taskurile deja afisate pe paginile anterioare

            System.Diagnostics.Debug.WriteLine("Nr pagina " +  currentPage.ToString() + " nr taskuri paginate " + paginatedTasks.Count() + " id " + id);

            
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this.perPageTask);
            if(paginatedTasks.Count() >0)
            ViewBag.currentTasks = paginatedTasks;

            #region TempDataMsg
            if (TempData["err"] != null)
            {
                ViewBag.err = TempData["err"].ToString();

            }
            if (TempData.ContainsKey("taskmessage"))
            {
                ViewBag.taskmessage = TempData["taskmessage"].ToString();
            }
            if (TempData.ContainsKey("tasknewmessage"))
            {
                ViewBag.taskmessage = TempData["tasknewmessage"].ToString();
            }
            if (TempData.ContainsKey("Error"))
                ViewBag.Error = TempData["Error"];
            #endregion
            Task potentiallyNew = new Task();
            potentiallyNew.Members = GetTeamMembers(id);
            return View(potentiallyNew);
        }


        public ActionResult New()
        {
            Project project = new Project();
            project.UserId = User.Identity.GetUserId();
            ViewBag.Date = DateTime.Now;
            return View(project);
        }

        [HttpPost]
        public ActionResult New(Project Project)
        {
            var currentUser = GetApplicationUser();
            ViewBag.Date = DateTime.Now;
            try
            {
                if (ModelState.IsValid)
                { 
                    ProjectUser temp = new ProjectUser();
                    temp.ProjectId = Project.ProjectId;
                    temp.UserId = currentUser.Id;
                    temp.JoinDate = DateTime.Now;
                    temp.Accepted = true;
                    Project.UserId = currentUser.Id;
                    db.ProjectUsers.Add(temp);
                    db.Projects.Add(Project);
                    db.SaveChanges();
                    if (!User.IsInRole("Admin"))
                    {
                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                        if (TryUpdateModel(currentUser))
                        {

                            var roles = db.Roles.ToList();
                            foreach (var role in roles)
                            {
                                UserManager.RemoveFromRole(currentUser.Id, role.Name);
                            }
                            UserManager.AddToRole(currentUser.Id, "TeamLeader");

                        }
                        db.SaveChanges();
                    }
                     TempData["projectnewmessage"] = "Project succesfully added!";
                    ApplicationDbContext.SendEmailNotification(currentUser.Email, "Your project has been created!", "Thank yooooooou!");
                    return RedirectToAction("Index");
                }
              
                return View(Project);
            }
            catch (Exception e)
            {
                
                return View(Project);
            }

        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Project toDelete = db.Projects.Find(id);
            db.Projects.Remove(toDelete);
            db.SaveChanges();
            TempData["projectmessage"] = "Project \"" + toDelete.Name + "\" has been succesfully deleted.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        public ActionResult Edit(int id)
        {
            ViewBag.Date = DateTime.Now;

            Project project = db.Projects.Find(id);
            if (User.Identity.GetUserId() == project.User.Id || User.IsInRole("Admin"))//un teamleader poate sa isi editeze doar echipele facute de el, adminul poate sa editeze orice commentariu
            {
                return View(project);

            }
            else
            {
                TempData["message"] = "You are not allowed to edit this project";
                return RedirectToAction("Index", "Projects");
            }
                
        }

        [Authorize(Roles = "TeamLeader,Admin")]
        [HttpPut]
        public ActionResult Edit(int id, Project requestProject)
        {

            try
            {
                Project project = db.Projects.Find(id);
                if (User.Identity.GetUserId() == project.User.Id || User.IsInRole("Admin"))//un teamleader poate sa isi editeze doar echipele facute de el, adminul poate sa editeze orice commentariu
                {
                    if (ModelState.IsValid)
                    {
                        Project Project = db.Projects.Find(id);
                        if (TryUpdateModel(Project))
                        {
                            Project.Name = requestProject.Name;
                            Project.StartDate = requestProject.StartDate;
                            Project.FinDate = requestProject.FinDate;
                            Project.Description = requestProject.Description;
                            db.SaveChanges();
                        }
                    TempData["projectmessage"] = "Project \"" + project.Name + "\" succefully edited!";
                    return RedirectToAction("Index");
                    }
                }
                return View(requestProject);
            }
            catch (Exception e)
            {
                return View(requestProject);
            }
        }

        public ActionResult AddMember(string UserName, int ProjectId)
        {
            ProjectUser projectUser = new ProjectUser();
            var searchedUser = db.Users.Where(u => u.UserName == UserName).ToList();

            if (searchedUser.Count() >0)
            {

                var searchedUserId = searchedUser.First().Id;
                if (db.Projects.Find(ProjectId).UserId == searchedUserId)
                {
                    TempData["err"] = "User already in project.";
                    return Redirect("/Projects/Show/" + ProjectId);
                }
                var projectUsers = db.ProjectUsers.Include("Project").Include("Project.User").Where(u => u.ProjectId == ProjectId && u.UserId == searchedUserId).ToList();
                if (projectUsers.Count() == 0 )
                {
                    projectUser.UserId = searchedUserId;
                    projectUser.Accepted = false;
                    projectUser.ProjectId = ProjectId;
                    projectUser.JoinDate = DateTime.Now;
                    //Trimit mail celui care a fost invitat din partea team leader-ului
                    /*ApplicationDbContext.SendEmailNotification(searchedUser.First().Email, projectUser.Project.User.UserName + " has invited to join project " + projectUser.Project.Name, "You can't miss this opportunity!");*/
                    db.ProjectUsers.Add(projectUser);
                    db.SaveChanges();
                }
                else
                   TempData["err"] = "User already in project.";
            }
            else
                TempData["err"] = "Couldn't find the user.";
            return Redirect("/Projects/Show/" + ProjectId);
        }
        [NonAction]
        private bool HasMember(string id, ICollection<ProjectUser> members)
        {
            foreach (var member in members)
                if (member.UserId == id && member.Accepted)
                    return true;
            return false;
        }
        [NonAction]
        private ApplicationUser GetApplicationUser()
        {
            return db.Users.Find(User.Identity.GetUserId());
        }
        [NonAction]
        public IEnumerable<SelectListItem> GetTeamMembers(int projectId)
        {
            var selectList = new List<SelectListItem>();

            var project = db.Projects.Include("ProjectUsers").Include("User").Where(m=> m.ProjectId == projectId).First();


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
    }
}