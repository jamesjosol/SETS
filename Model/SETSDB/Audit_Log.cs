using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Audit_Log
    {
        public int Id { get; set; }
        public string EventCode { get; set; } = string.Empty;
        public string? SpecimenNo { get; set; }
        public string? BatchNo { get; set; }
        public string? PatientName { get; set; }
        public string? PID { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string? Status { get; set; }
        public DateTime? RunningDate { get; set; }
        public DateTime? TxDate { get; set; }
        public string UserID { get; set; } = string.Empty;
        public DateTime LoggedAt { get; set; }
        public string? Remarks { get; set; }
    }
}