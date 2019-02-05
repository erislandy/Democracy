using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class VotingDetail
    {
        [Key]
        public int VotingDetailId { get; set; }

        public int UserId { get; set; }
        public int CandidateId { get; set; }
        public int VotingId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual Candidate Candidate { get; set; }
        [JsonIgnore]
        public virtual Voting Voting { get; set; }

    }
}