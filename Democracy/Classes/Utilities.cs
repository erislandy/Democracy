using Democracy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace Democracy.Classes
{
    public class Utilities : IDisposable
    {
        private static DemocracyContext db = new DemocracyContext();

        public static void UploadPhoto(HttpPostedFileBase file)
        {
            //Upload file
            string path = string.Empty;
            string pic = string.Empty;

            if (file != null)
            {
                pic = Path.GetFileName(file.FileName);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Photos"), pic);
                file.SaveAs(path);

                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }
        }

        public static void CreateASPUser(UserView userView)
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

        internal static void ChangeUserName(string currentUserName, UserChange user)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.FindByEmail(currentUserName);

            if(userASP == null)
            {
                return;
            }

            userManager.Delete(userASP);

            userASP = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.UserName,
                PhoneNumber = user.Phone
            };
            userManager.Create(userASP, user.CurrentPassword);
            userManager.AddToRole(userASP.Id, "User");
        }

        public static List<Voting> MyVotings(User user)
        {
            //Get event votings at the correct time
            var state = GetState("Opened");

            var votings = db.Votings.Where(v => v.DateTimeStart <= DateTime.Now &&
                                                v.DateTimeEnd >= DateTime.Now &&
                                                v.StateId == state.StateId)
                                    .Include(v => v.Candidates)
                                    .Include(v => v.VotingGroups)
                                    .Include(v => v.State)
                                    .ToList();

            //Discard events where user has voted

            foreach (var voting in votings.ToList())
            {
                var votingDetail = db.VotingDetails.Where(vd => vd.UserId == user.UserId &&
                                                                vd.VotingId == voting.VotingId)
                                                   .FirstOrDefault();
                if (votingDetail != null)
                {
                    votings.Remove(voting);
                }
            }

            //Discard events by groups where user is not included

            foreach (var voting in votings.ToList())
            {
                if (voting.IsForAllUsers)
                    continue;

                bool userBelongToGroup = false;

                foreach (var votingGroup in voting.VotingGroups)
                {
                    var userGroup = votingGroup.Group
                                               .GroupMembers
                                               .Where(gm => gm.UserId == user.UserId)
                                               .FirstOrDefault();
                    if (userGroup != null)
                    {
                        userBelongToGroup = true;
                        break;
                    }
                }

                if (!userBelongToGroup)
                {
                    votings.Remove(voting);
                }
            }

            return votings;
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public static State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Description == stateName)
                                 .FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    Description = stateName
                };
                db.States.Add(state);
                db.SaveChanges();
            }
            return state;
        }

    }
}