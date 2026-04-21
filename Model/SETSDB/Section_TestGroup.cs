using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Section_TestGroup
    {
        public int Id { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
}
