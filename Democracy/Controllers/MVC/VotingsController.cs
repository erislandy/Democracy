using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy.Classes;
using Democracy.Models;

namespace Democracy.Controllers
{
    public class VotingsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Close(int id)
        {
            var voting = db.Votings.Find(id);

            if(voting != null)
            {
                var candidate = db.Candidates.Where(c => c.VotingId == voting.VotingId)
                                             .OrderByDescending(c => c.QuantityVotes)
                                             .FirstOrDefault();
                if(candidate != null)
                {
                    var state = Utilities.GetState("Closed");
                    voting.StateId = state.StateId;
                    voting.CandidateWinId = candidate.User.UserId;
                    db.Entry(voting).State = EntityState.Modified;
                    db.SaveChanges();

                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ShowResults(int id)
        {
            //Falta codigo para hacer reporte

            return null;
        }
        



        [Authorize(Roles = "User")]
        public ActionResult Results()
        {
            var state = Utilities.GetState("Closed");

            var votings = db.Votings
                            .Where(v => v.StateId == state.StateId)
                            .Include(v => v.State);

            var db2 = new DemocracyContext();
            var view = new List<VotingIndexView>();

            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                view.Add(new VotingIndexView()
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnabledBlankVote = voting.IsEnabledBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user

                }

                    );
            }
            return View(view);
        }



        [Authorize(Roles = "User")]
        public ActionResult VoteForCandidate(int candidateId, int votingId)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            if(user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var voting = db.Votings.Find(votingId);

            if(voting == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var candidate = db.Candidates.Find(candidateId);

            if (candidate == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if(VoteCandidate(user, candidate, voting))
            {
                return RedirectToAction("MyVotings");

            }

            return RedirectToAction("MyVotings");
        }

        private bool VoteCandidate(User user, Candidate candidate, Voting voting)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                var votingDetail = new VotingDetail()
                {
                    CandidateId = candidate.CandidateId,
                    UserId = user.UserId,
                    VotingId = voting.VotingId
                };
                db.VotingDetails.Add(votingDetail);

                candidate.QuantityVotes++;
                db.Entry(candidate).State = EntityState.Modified;

                voting.QuantityVotes++;
                db.Entry(voting).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                }
                return false;
            }
        }

        [Authorize(Roles = "User")]
        public ActionResult Vote(int votingId)
        {
            var voting = db.Votings.Find(votingId);
            var view = new VotingVoteView()
            {
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVote = voting.IsEnabledBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                VotingId = voting.VotingId,
                MyCandidates = voting.Candidates.ToList(),
            };

            return View(view);
        }

        [Authorize(Roles = "User")]
        public ActionResult MyVotings()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            if(user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "there is an error with the current user, call to support system");
                return View();
            }

            var votings = Utilities.MyVotings(user);

            return View(votings);
        }

     
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DeleteCandidate(int id)
        {
            var candidate = db.Candidates.Find(id);

            if(candidate != null)
            {
                db.Candidates.Remove(candidate);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", candidate.VotingId));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DeleteGroup(int id)
        {
            var votingGroup = db.VotingGroups.Find(id);

            if(votingGroup != null)
            {
                db.VotingGroups.Remove(votingGroup);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", votingGroup.VotingId));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddCandidate(int id)
        {
            ViewBag.UserId = new SelectList(
                                        db.Users.OrderBy(g => g.FirstName).ThenBy(g => g.LastName),
                                        "UserId",
                                        "FullName");
            var view = new AddCandidateView
            {
                VotingId = id
            };

            return View(view);
        }

        //POST:AddPost

        [HttpPost]
        public ActionResult AddCandidate(AddCandidateView view)
        {
            if (ModelState.IsValid)
            {
                var candidate = db.Candidates
                                    .Where(c => c.VotingId == view.VotingId &&
                                                 c.UserId == view.UserId)
                                    .FirstOrDefault();

                if (candidate != null)
                {
                    ModelState.AddModelError(string.Empty, "The candidate already belongs to voting");
                    ViewBag.UserId = new SelectList(
                                       db.Users.OrderBy(g => g.FirstName).ThenBy(g => g.LastName),
                                       "UserId",
                                       "FullName");

                    return View(view);
                }

                candidate = new Candidate
                {
                    UserId = view.UserId,
                    VotingId = view.VotingId
                };

                db.Candidates.Add(candidate);
                db.SaveChanges();

                return RedirectToAction(string.Format("Details/{0}", candidate.VotingId));



            }

            ViewBag.UserId = new SelectList(
                                       db.Users.OrderBy(g => g.FirstName).ThenBy(g => g.LastName),
                                       "UserId",
                                       "FullName");


            return View(view);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddGroup(int id)
        {
            ViewBag.GroupId = new SelectList(
                                        db.Groups.OrderBy(g => g.Description), 
                                        "GroupId",
                                        "Description");
            var view = new AddGroupView
            {
                VotingId = id
            };

            return View(view);
        }

        //POST:AddGroup

        [HttpPost]
        public ActionResult AddGroup(AddGroupView view)
        {
            if (ModelState.IsValid)
            {
                var votingGroup = db.VotingGroups
                                    .Where(vg => vg.VotingId == view.VotingId &&
                                                 vg.GroupId == view.GroupId)
                                    .FirstOrDefault();

                if(votingGroup != null)
                {
                    ModelState.AddModelError(string.Empty, "The group already belongs to voting");
                    ViewBag.GroupId = new SelectList(
                                      db.Groups.OrderBy(g => g.Description),
                                      "GroupId",
                                      "Description");
                    return View(view);
                }

                votingGroup = new VotingGroup
                {
                    GroupId = view.GroupId,
                    VotingId = view.VotingId
                };

                db.VotingGroups.Add(votingGroup);
                db.SaveChanges();

                return RedirectToAction(string.Format("Details/{0}", votingGroup.VotingId));


                
            }

            ViewBag.GroupId = new SelectList(
                                       db.Groups.OrderBy(g => g.Description),
                                       "GroupId",
                                       "Description");

            return View(view);
        }
        // GET: Votings
        public ActionResult Index()
        {
            var votings = db.Votings.Include(v => v.State);
            var db2 = new DemocracyContext();
            var view = new List<VotingIndexView>();

            foreach (var voting in votings)
            {
                User user = null;
                if(voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                view.Add( new VotingIndexView()
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnabledBlankVote = voting.IsEnabledBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user

                }

                    );
            }
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }

            var detailsVotingView = new DetailsVotingView
            {
                Candidates = voting.Candidates.ToList(),
                CandidateWinId = voting.CandidateWinId,
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVote = voting.IsEnabledBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                QuantityBlankVotes = voting.QuantityBlankVotes,
                QuantityVotes = voting.QuantityVotes,
                Remarks = voting.Remarks,
                State = voting.State,
                StateId = voting.StateId,
                VotingGroups = voting.VotingGroups.ToList(),
                VotingId = voting.VotingId,
            };

            return View(detailsVotingView);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description");
            var view = new VotingView
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now
            };
            return View(view);
        }

        // POST: Votings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting()
                {
                    DateTimeStart = view.DateStart
                                        .AddHours(view.TimeStart.Hour)
                                        .AddMinutes(view.TimeStart.Minute),
                    DateTimeEnd = view.DateEnd
                                        .AddHours(view.TimeEnd.Hour)
                                        .AddMinutes(view.TimeEnd.Minute),
                    Description = view.Description,
                    IsEnabledBlankVote = view.IsEnabledBlankVote,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId

                };
                db.Votings.Add(voting);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", voting.StateId);

            var view = new VotingView
            {
                DateEnd = voting.DateTimeEnd,
                DateStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVote = voting.IsEnabledBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                TimeEnd = voting.DateTimeEnd,
                TimeStart = voting.DateTimeStart,
                VotingId = voting.VotingId,
            };
            return View(view);
        }

        // POST: Votings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting()
                {
                    DateTimeStart = view.DateStart
                                        .AddHours(view.TimeStart.Hour)
                                        .AddMinutes(view.TimeStart.Minute),
                    DateTimeEnd = view.DateEnd
                                        .AddHours(view.TimeEnd.Hour)
                                        .AddMinutes(view.TimeEnd.Minute),
                    Description = view.Description,
                    IsEnabledBlankVote = view.IsEnabledBlankVote,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                    VotingId = view.VotingId

                };

                db.Entry(voting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        // POST: Votings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Voting voting = db.Votings.Find(id);
            db.Votings.Remove(voting);
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
