using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Task_Management.Models;
namespace Task_Management.Controllers
{   [Authorize(Roles ="Admin")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int perPage = 3;
        // GET: Users
        public ActionResult Index()
        {  
            if (TempData.ContainsKey("Message"))
                ViewBag.Message = TempData["Message"];
            if (TempData.ContainsKey("ErrMessage"))
                ViewBag.Message = TempData["ErrMessage"];
            var users = db.Users.ToList();
            var currentPage = Convert.ToInt32(Request.Params.Get("page")); //extrage paramentul numarul paginii din ruta

            var totalItems = users.Count();
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this.perPage;
            }

        
            ViewBag.totalItems = totalItems;
            ViewBag.lastPage = Math.Ceiling((float) totalItems / (float) this.perPage);
            ViewBag.Users = users.Skip(offset).Take(this.perPage); //sar peste taskurile deja afisate pe paginile anterioare
            return View();
        }

        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            string currentRole = user.Roles.FirstOrDefault().RoleId;
            var userRoleName = db.Roles.Where(role => role.Id == currentRole).First();
            ViewBag.roleName = userRoleName;
            return View(user);
        }

        public ActionResult Edit(string id)
        {
            //EditUserModel e folosit pentru a putea face validari
            EditUserModel dummy = new EditUserModel();
            ApplicationUser user = db.Users.Find(id);
            //Pun datele utilizatorului in dummy
            dummy.Id = id;
            dummy.UserName = user.UserName;
            dummy.Email = user.Email;
            dummy.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            return View(dummy);
        }

        [HttpPut]
        public ActionResult Edit(string id, EditUserModel UpdatedUser)
        {
            
            ApplicationUser user = db.Users.Find(id);
           
            try
            {

                var roleManager = new RoleManager<IdentityRole>(new
               RoleStore<IdentityRole>(db));
                var UserManager = new UserManager<ApplicationUser>(new
               UserStore<ApplicationUser>(db));
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(UpdatedUser))
                    {
                        user.UserName = UpdatedUser.UserName;
                        user.Email = UpdatedUser.Email;
                        var roles = (from role in db.Roles select role).ToList();
                        foreach (var role in roles)
                        {
                            UserManager.RemoveFromRole(id, role.Name);
                        }
                        var selectedRole =
                        db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                        UserManager.AddToRole(id, selectedRole.Name);
                        db.SaveChanges();
                    }
                    else
                    {
                       
                        UpdatedUser.AllRoles = GetAllRoles();
                        UpdatedUser.Id = id;
                        return View(UpdatedUser);
                    }
                    TempData["Message"] = "Sucesfully edited user!";
                    return RedirectToAction("Index");
                }
                else
                {
                    UpdatedUser.AllRoles = GetAllRoles();
                    UpdatedUser.Id = id;
                    return View(UpdatedUser);
                }
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                TempData["ErrMessage"] = "User edit failed because of the following error: "+e.Message;
                UpdatedUser.AllRoles = GetAllRoles();
                UpdatedUser.Id = id;
                return View(UpdatedUser);
            }
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            var UserManager = new UserManager<ApplicationUser>(new
UserStore<ApplicationUser>(db));
            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);
            string userName = user.UserName;
            //Stergem entitatile care au legatura cu userul pentru a respecta constrangerea de cheie externa
            var projects = db.Projects.Where(a => a.UserId == id);
            foreach (var project in projects)
                db.Projects.Remove(project);

            var comments = db.Comments.Where(comm => comm.UserId == id);
            foreach (var comment in comments)
                db.Comments.Remove(comment);

            var projectUsers = db.ProjectUsers.Where(m => m.UserId == id);
            foreach (var temp in projectUsers)
                db.ProjectUsers.Remove(temp);
            db.SaveChanges();
            UserManager.Delete(user);
            TempData["Message"] = "Sucessfully deleted user \"" + userName + "\"";
            return RedirectToAction("Index");
        }
      
        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = db.Roles;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem { Value = role.Id, Text = role.Name });
            }
            return selectList;
        }
    }
}