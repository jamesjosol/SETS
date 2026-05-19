using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class FlagSpecimenRequest
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string FlagReason { get; set; }
        public string UserID { get; set; }
    }

    public class UnflagSpecimenRequest
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string UserID { get; set; }
    }

    public class FlagSpecimenResult
    {
        public string ProcDestination { get; set; }   // for notification targeting
        public string LocationName { get; set; }       // endorsing section name
        public string PatientName { get; set; }
        public string PID { get; set; }
    }
}