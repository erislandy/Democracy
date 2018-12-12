using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class RegisterUserView : UserView
    {
        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Password")]
        [StringLength(20, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 7)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Confirm Password")]
        [StringLength(20, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 7)]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }
    }
}