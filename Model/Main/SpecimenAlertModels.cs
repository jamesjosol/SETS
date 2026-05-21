using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class SetSpecimenAlertRequest
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string Alert { get; set; }
        public string UserID { get; set; }
    }

    public class ClearSpecimenAlertRequest
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string UserID { get; set; }
    }

    public class SpecimenAlertResult
    {
        public string PatientName { get; set; }
        public string PID { get; set; }
        public string LocationName { get; set; }
        public string SectionCode { get; set; }   // for notification targeting
    }
}