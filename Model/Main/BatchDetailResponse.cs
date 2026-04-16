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
        public string Location { get; set; }          // endorser section full name
        public string LocationCode { get; set; }      // endorser section code
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public string Destination { get; set; }       // processing section full name
        public string DestinationCode { get; set; }   // processing section code
        public string Status { get; set; }
        public List<Batch_Specimen> Specimens { get; set; } = new();
        public List<Batch_NonBarcoded> NonBarcoded { get; set; } = new();
    }
}
