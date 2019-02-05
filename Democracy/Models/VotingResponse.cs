using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    [NotMapped]
    public class VotingResponse
    {
        public int VotingId { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public bool IsForAllUsers { get; set; }
        public bool IsEnabledBlankVote { get; set; }
        public int QuantityVotes { get; set; }
        public int QuantityBlankVotes { get; set; }

        public User Winner { get; set; }
        public State State { get; set; }
        public List<CandidateResponse> Candidates { get; set; }
    }
}