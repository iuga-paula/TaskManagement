using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Task_Management.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [StringLength(40, ErrorMessage = "Name is too long")] [Required(ErrorMessage = "Project name cannot be empty\n")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Project description cannot be empty")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Must be a date format")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Must be a date format")]
        public DateTime FinDate { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers {get;set;}
    }
}