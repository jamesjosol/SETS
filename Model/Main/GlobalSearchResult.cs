using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class GlobalSearchResult
    {
        public string Type { get; set; }          // "specimen" | "batch"
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string? LabNo { get; set; }
        public string? PID { get; set; }
        public string? PatientName { get; set; }
        public string? SampleTypeName { get; set; }
        public string Status { get; set; }        // P, R, C, X
        public string LocationCode { get; set; }
        public string Location { get; set; }      // human-readable section name
        public string ProcDestination { get; set; }
        public DateTime Endorsed { get; set; }
    }
}