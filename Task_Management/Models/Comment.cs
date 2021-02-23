using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Task_Management.Models
{
    public class Comment
    {

        [Key]
        public int CommentId { get; set; }
        [Required(ErrorMessage = "Comment Content cannot be empty")]
        public string Content { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Must be a date format")]
        public DateTime Date { get; set; }

        public int TaskId { get; set; }
        public virtual Task Task { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } ///un comm este lasat de un  user
    }
}