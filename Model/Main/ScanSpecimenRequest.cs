using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ScanSpecimenRequest
    {
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string UserID { get; set; }
    }
}