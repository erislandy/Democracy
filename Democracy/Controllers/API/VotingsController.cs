using Democracy.Classes;
using Democracy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Democracy.Controllers.API
{
    [RoutePrefix("api/Votings")]

    public class VotingsController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();

        [HttpGet]
        [Route("{userId}")]
        public IHttpActionResult MyVotings(int userId)
        {
            var user = db.Users.Find(userId);
            if(user == null)
            {
                BadRequest("User not found");
            }

            var votings = Utilities.MyVotings(user);
            var votingResponse = new List<VotingResponse>();

            
            foreach (var voting in votings)
            {
                User winner = null;

                if (voting.CandidateWinId != 0)
                {
                    winner = db.Users.Find(voting.CandidateWinId);
                }

                var candidates = new List<CandidateResponse>();
                foreach (var candidate in voting.Candidates)
                {
                    candidates.Add(new CandidateResponse
                    {
                        CandidateId = candidate.CandidateId,
                        QuantityVotes = candidate.QuantityVotes,
                        User = candidate.User
                    });
                }
                votingResponse.Add(new VotingResponse
                {
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                     Description = voting.Description,
                     IsEnabledBlankVote = voting.IsEnabledBlankVote,
                     IsForAllUsers = voting.IsForAllUsers,
                     QuantityBlankVotes = voting.QuantityVotes,
                     Remarks = voting.Remarks,
                     Candidates = candidates,
                     State = voting.State,
                     VotingId = voting.VotingId,
                     Winner = winner
                     
                });

            }
            return Ok(votingResponse);
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