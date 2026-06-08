using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Issue_LabEntry
    {
        public int Id { get; set; }
        public int SubCategoryId { get; set; }
        public string SpecimenNo { get; set; }
        public string? PID { get; set; }
        public string? PatientName { get; set; }
        public string? SampleTypeCode { get; set; }
        public string? SampleTypeName { get; set; }
        public DateOnly EntryDate { get; set; }
        public string LoggedBy { get; set; }
        public DateTime LoggedAt { get; set; }
        public string? Remarks { get; set; }
    }
}
