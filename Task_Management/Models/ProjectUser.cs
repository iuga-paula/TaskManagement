using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Task_Management.Models
{
    public class ProjectUser
    {
        public int ProjectUserId { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }


        public bool Accepted { get; set; }
        public DateTime JoinDate { get; set; }

        
      
    }
}