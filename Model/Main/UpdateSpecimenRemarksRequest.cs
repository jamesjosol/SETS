using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class UpdateSpecimenRemarksRequest
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string? ReceivingRemarks { get; set; }
    }
}