using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Democracy.Classes;
using Democracy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;

namespace Democracy.Controllers.API
{
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();

        [HttpPost]
        [Route("Login")]
        public IHttpActionResult Login(JObject form)
        {
            dynamic jsonObject = form;
            string email = string.Empty;
            string password = string.Empty;

            try
            {
                email = jsonObject.email.Value;
            }
            catch (Exception)
            {

          
            }

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Incorrect call");
            }

            try
            {
                password = jsonObject.password.Value;
            }
            catch (Exception)
            {


            }

            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("Incorrect call");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(email, password);

            if(userASP == null)
            {
                return BadRequest("Incorrect user or password");
            }

            var user = db.Users.Where(u => u.UserName == email).FirstOrDefault();

            if(user == null)
            {
                return BadRequest("Problem better call saul");
            }
            return Ok(user);
        }

        [HttpPut]
        [Route("ChangePassword/{id}")]
        public IHttpActionResult ChangePassword(int id, JObject form)
        {
            dynamic jsonObject = form;
            var oldPassword = string.Empty;
            var newPassword = string.Empty;
            int userId = id;

            try
            {
                oldPassword = jsonObject.OldPassword.Value;
                newPassword = jsonObject.NewPassword.Value;
            }
            catch (Exception)
            {
                return BadRequest("Incorrect call");

            }

           
            var user = db.Users.Find(userId);

            if(user == null)
            {
                return BadRequest("User not found");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(user.UserName, oldPassword);

            if(userASP == null)
            {
                return BadRequest("Incorrect old password");
            }

            var response = userManager.ChangePassword(userASP.Id, oldPassword, newPassword);

            if (response.Succeeded)
            {
                return Ok<object>(new
                {
                    Message = "Password was changed successfully"
                });
            }
            else
            {
                return BadRequest(response.Errors.ToString());
            }



        }

        // PUT: api/Users/5
        [HttpPut]
        public IHttpActionResult PutUser(int id, UserChange user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

            var currentUser = db.Users.Find(id);
            if(currentUser == null)
            {
                BadRequest("User not found");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(currentUser.UserName, user.CurrentPassword);

            if(userASP == null)
            {
                return BadRequest("Password wrong");
            }

            if (currentUser.UserName != user.UserName)
            {
                Utilities.ChangeUserName( currentUser.UserName,user);
                currentUser.UserName = user.UserName;
               
            }

            db.Entry(currentUser).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message.ToString());
            }

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        [ResponseType(typeof(RegisterUserView))]
        public IHttpActionResult PostUser(RegisterUserView userView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User()
            {
                Address = userView.Address,
                FirstName = userView.FirstName,
                Grade = userView.Grade,
                Group = userView.Group,
                LastName = userView.LastName,
                Phone = userView.Phone,
                UserName = userView.UserName,
            };

            db.Users.Add(user);
            db.SaveChanges();
            Utilities.CreateASPUser(userView);

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}