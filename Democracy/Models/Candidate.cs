using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }
        public int VotingId { get; set; }
        public int UserId { get; set; }
        public int QuantityVotes { get; set; }

        [JsonIgnore]
        public virtual Voting Voting { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<VotingDetail> VotingDetails { get; set; }

    }
}