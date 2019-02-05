using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
    }
}