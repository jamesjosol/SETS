using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Contingency_Specimen
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public string SpecimenNo { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public bool IsReceived { get; set; } = false;
        public string? ReceivedBy { get; set; }
        public DateTime? ReceivedAt { get; set; }
    }
}