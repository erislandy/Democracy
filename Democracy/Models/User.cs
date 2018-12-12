using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Democracy.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "E-Mail")]
        [StringLength(100, ErrorMessage =
                        "The field {0} can contain maximum {1} and minimun {2} character",
                        MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        [Index("UserNameIndex", IsUnique = true)]
        public string UserName { get; set; }

        [StringLength(50, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 2)]
        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Display(Name = "Last name")]
        [StringLength(50, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 2)]
        public string LastName { get; set; }

        [Display(Name = "User")]
        public string FullName { get {return string.Format("{0} {1}",FirstName,LastName); } }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 7)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(100, ErrorMessage =
                       "The field {0} can contain maximum {1} and minimun {2} character",
                       MinimumLength = 10)]
        public string Address { get; set; }
        public string Grade { get; set; }
        public string Group { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Photo { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; }
        public virtual ICollection<Candidate> Candidates { get; set; }
        public virtual ICollection<VotingDetail> VotingDetails { get; set; }

    }
}