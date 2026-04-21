using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Specimen_Section_Test
    {
        public int Id { get; set; }
        public int HeaderId { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Status { get; set; }
        public string? ScheduleTag { get; set; }
        public DateOnly? RunningDate { get; set; }
        public string? AssignedRMT { get; set; }
        public DateTime? Assigned { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}