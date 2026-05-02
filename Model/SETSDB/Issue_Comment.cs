using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Issue_Comment
    {
        public int Id { get; set; }
        public int IncidentTypeId { get; set; }
        public string CommentText { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RevisedBy { get; set; }
        public DateTime? RevisedAt { get; set; }
    }
}