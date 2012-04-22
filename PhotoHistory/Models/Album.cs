using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHistory.Models
{
    public class Album
    {
        public int? Id {get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public DateTime? NextNotification { get; set; }
        public bool Public { get; set; }
        public string Password { get; set; }
        public bool CommentsAllow { get; set; }
        public bool CommentsAuth { get; set; }

    }
}