using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ProcessingOptionsRequest
    {
        public bool ShowTemperature { get; set; }
        public bool ShowTempRemarks { get; set; }
        public bool ShowBagNo { get; set; }
    }
}