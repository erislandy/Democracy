using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class VotingGroup
    {
        public int VotingGroupId { get; set; }
        public int GroupId { get; set; }
        public int VotingId { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }

        [JsonIgnore]
        public virtual Voting Voting { get; set; }


    }
}