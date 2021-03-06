﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class GroupDetailsView
    {
        public int GroupId { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public List<GroupMember> GroupMembers { get; set; }
    }
}