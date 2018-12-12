using Democracy.Migrations;
using Democracy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Democracy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DemocracyContext,Configuration>());
            CheckSuperUser();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void CheckSuperUser()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            CheckRole("Admin", userContext);
            CheckRole("User", userContext);
            var user = db.Users.Where(u => u.UserName.ToLower()
                               .Equals("erislandy.cabrales@gmail.com"))
                               .FirstOrDefault();

            if(user == null)
            {
                user = new User()
                {
                    Address = "Calle Luna, Calle Sol",
                    FirstName = "Erislandy",
                    LastName = "Cabrales",
                    Phone = "+53 52180537",
                    UserName = "erislandy.cabrales@gmail.com",
                    Photo = "~/Content/Photos/100_5299.JPG"

                };
                db.Users.Add(user);
                db.SaveChanges();
            }

            var userASP = userManager.FindByEmail(user.UserName);

            if(userASP == null)
            {
                userASP = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.UserName,
                    PhoneNumber = user.Phone
                };

                userManager.Create(userASP, "Erycab*/2017");
            }

            userManager.AddToRole(userASP.Id, "Admin");
            userManager.AddToRole(userASP.Id, "User");

        }

        private void CheckRole(string roleName, ApplicationDbContext userContext)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));
         
            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

        }
    }
}
