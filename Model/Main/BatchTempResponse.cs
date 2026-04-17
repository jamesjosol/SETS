using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class BatchTempResponse
    {
        public string BatchNo { get; set; }
        public string? Temp { get; set; }
        public string? TempRemarks { get; set; }
        public string? BagNo { get; set; }
    }
}