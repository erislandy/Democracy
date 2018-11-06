using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy.Models;

namespace Democracy.Controllers
{
    public class GroupsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        // GET: Groups
        public ActionResult Index()
        {
            return View(db.Groups.ToList());
        }

        // GET: AddMembers
        [HttpGet]
        public ActionResult AddMember(int? id)
        {
            ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName),"UserId","FullName");

            if(id == null)
            {
                return RedirectToAction("Index");
            }
           
            var addMemberView = new AddMemberView()
            {
                GroupId = (int)id
            };
            return View(addMemberView);
        }

        // GET: DeleteMembers
        [HttpGet]
        public ActionResult DeleteMember(int? id)
        {
            var member = db.GroupMembers.Find(id);
            if (member != null)
            {
                db.GroupMembers.Remove(member);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", member.GroupId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(AddMemberView addMemberView)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName), "UserId", "FullName");
                return View(addMemberView);
            }

            var groupMember = db.GroupMembers
                                .Where(gm =>
                                            gm.GroupId == addMemberView.GroupId && 
                                            gm.UserId == addMemberView.UserId)
                                .FirstOrDefault();

            if(groupMember != null)
            {

                ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName), "UserId", "FullName");
                ViewBag.Error = "The user already belongs at group";
                return View(addMemberView);
            }

            groupMember = new GroupMember
            {
                GroupId = addMemberView.GroupId,
                UserId = addMemberView.UserId
            };

            db.GroupMembers.Add(groupMember);
            db.SaveChanges();
            return RedirectToAction(string.Format("Details/{0}",addMemberView.GroupId));
        }



        // GET: Groups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            var groupDetailsView = new GroupDetailsView
            {
                GroupId = group.GroupId,
                Description = group.Description,
                GroupMembers = group.GroupMembers.ToList()
            };
            return View(groupDetailsView);
        }

        // GET: Groups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GroupId,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Groups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GroupId,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: Groups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();
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
