using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class BatchDetailResponse
    {
        public string BatchNo { get; set; }
        public string Location { get; set; }
        public string LocationCode { get; set; }
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public string Destination { get; set; }
        public string DestinationCode { get; set; }
        public string Status { get; set; }
        public DateTime? ProcReceived { get; set; }
        public DateTime? Completed { get; set; }
        public List<BatchSpecimenDetail> Specimens { get; set; } = new();  // ← updated type
        public List<Batch_NonBarcoded> NonBarcoded { get; set; } = new();
    }
}
