using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class AddGroupView
    {
        [Required(ErrorMessage = "You must select a group")]
        public int GroupId { get; set; }
        public int VotingId { get; set; }

    }
}