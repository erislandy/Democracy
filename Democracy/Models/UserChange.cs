using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    [NotMapped]
    public class UserChange : User
    {
        [Display(Name = "Password")]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage =
                      "The field {0} can contain maximum {1} and minimun {2} character",
                      MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}