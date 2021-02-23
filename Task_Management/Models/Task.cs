using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Task_Management.Models
{   public enum Status {NotStarted, InProgress,Completed }
    public class Task
    {
        [Key]
        public int TaskId { get; set; }
        [Required(ErrorMessage = "Task title cannot be empty")]
        public string Title{ get; set; }
         [Required(ErrorMessage = "Task description cannot be empty")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Must be a date format")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Must be a date format")]
        public DateTime FinDate { get; set; }

        public Status Status { get; set; }


        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public string UserId { get; set; }//cel care completeaza taskul
        public virtual ApplicationUser User { get; set; } ///un task este asignat unui user
        public IEnumerable<SelectListItem> Members { get; set; }//pt dropdownul cu membrii echipei
        public IEnumerable<SelectListItem> Stat { get; set; }
    }
}