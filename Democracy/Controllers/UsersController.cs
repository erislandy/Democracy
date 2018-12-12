using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Democracy.Controllers
{
    
    public class UsersController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "User")]
        public ActionResult MySettings()
        {
            var user = db.Users
                            .Where(u => u.UserName == User.Identity.Name)
                            .FirstOrDefault();
            var view = new UserSettingsView()
            {
                Address = user.Address,
                FirstName = user.FirstName,
                Grade = user.Grade,
                Group = user.Group,
                LastName = user.LastName,
                Phone = user.Phone,
                Photo = user.Photo,
                UserId = user.UserId,
                UserName = user.UserName
            };

            return View(view);
        }

        [HttpPost]
        public ActionResult MySettings(UserSettingsView userView)
        {
            if (ModelState.IsValid)
            {
                //Upload Image

                string path = string.Empty;
                string pic = string.Empty;

                if (userView.NewPhoto != null)
                {
                    pic = Path.GetFileName(userView.NewPhoto.FileName);
                    path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                    userView.NewPhoto.SaveAs(path);

                    using (var ms = new MemoryStream())
                    {
                        userView.NewPhoto.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                var user = db.Users.Find(userView.UserId);
                user.Address = userView.Address;
                user.FirstName = userView.FirstName;
                user.LastName = userView.LastName;
                user.Phone = userView.Phone;
                user.Grade = userView.Grade;
                user.Group = userView.Group;

                if (!string.IsNullOrEmpty(pic))
                {
                    user.Photo = pic == string.Empty ? string.Empty : string.Format("~/Content/Photos/{0}", pic);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();


                return RedirectToAction("Index", "Home");
            }

            return View(userView);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult OnOffAdmin(int id)
        {
            var user = db.Users.Find(id);

            if(user != null)
            {
                var userContext = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
                var userASP = userManager.FindByEmail(user.UserName);

                if (userManager.IsInRole(userASP.Id, "Admin"))
                {
                    userManager.RemoveFromRole(userASP.Id, "Admin");
                }
                else
                {
                    userManager.AddToRole(userASP.Id, "Admin");
                }
            }
            return RedirectToAction("Index");
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var users = db.Users.ToList();
            var usersIndexView = new List<UserIndexView>();

            foreach (var user in users)
            {
                var userASP = userManager.FindByEmail(user.UserName);

                var userIndexView = new UserIndexView()
                {
                    Address = user.Address,
                    Candidates = user.Candidates,
                    FirstName = user.FirstName,
                    Grade = user.Grade,
                    Group = user.Group,
                    GroupMembers = user.GroupMembers,
                    IsAdmin = userASP != null && userManager.IsInRole(userASP.Id, "Admin"),
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Photo = user.Photo,
                    UserId = user.UserId,
                    UserName = user.UserName
                };

                usersIndexView.Add(userIndexView);

            }
            return View(usersIndexView);
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Upload Image

            string path = string.Empty;
            string pic = string.Empty;

            if(userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"),pic);
                userView.Photo.SaveAs(path);

                using (var ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }
            //Save record

            var user = new User()
            {
                Address = userView.Address,
                FirstName = userView.FirstName,
                Grade = userView.Grade,
                Group = userView.Group,
                LastName = userView.LastName,
                Phone = userView.Phone,
                UserName = userView.UserName,
                Photo = pic == string.Empty ? string.Empty : string.Format("~/Content/Photos/{0}", pic)
            };
            db.Users.Add(user);

            try
            {
                db.SaveChanges();
                CreateASPUser(userView);
            }
            catch (Exception ex)
            {

               if((ex.InnerException != null) && 
                    (ex.InnerException.InnerException != null)&&
                    (ex.InnerException.InnerException.Message.Contains("UserNameIndex")))
                {
                    ViewBag.Error = "The email has already use by another user";
                }
                else
                {
                    ViewBag.Error = ex.Message;
                }
                return View(userView);

            }
            return RedirectToAction("Index");
        }

        private void CreateASPUser(UserView userView)
        {
            // User management
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            // Create User role
            string roleName = "User";

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            // Create the ASP NET User
            var userASP = new ApplicationUser
            {
                UserName = userView.UserName,
                Email = userView.UserName,
                PhoneNumber = userView.Phone,
            };

            userManager.Create(userASP, userASP.UserName);

            // Add user to role
            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");

        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userView = new UserView()
            {
                Address = user.Address,
                FirstName = user.FirstName,
                Grade = user.Grade,
                Group = user.Group,
                LastName = user.LastName,
                Phone = user.Phone,
                UserName = user.UserName,
                UserId = user.UserId
            };

            return View(userView);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Upload Image

            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);

                using (var ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = db.Users.Find(userView.UserId);
            user.Address = userView.Address;
            user.FirstName = userView.FirstName;
            user.LastName = userView.LastName;
            user.Phone = userView.Phone;
            user.Grade = userView.Grade;
            user.Group = userView.Group;

            if (!string.IsNullOrEmpty(pic))
            {
                user.Photo = pic == string.Empty ? string.Empty : string.Format("~/Content/Photos/{0}", pic);
            }

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);

            try
            {
                db.SaveChanges();

            }
            catch (Exception ex)
            {

                if(ex.InnerException != null &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException.Message.Contains("REFERENCE"))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The record can not be deleted, because it has related records");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        ex.Message
                        );
                }
                return View(user);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
