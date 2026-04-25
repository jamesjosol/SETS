using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.HCLAB
{
    public class Ord_Dtl
    {
        public string LABNO { get; set; }
        public string TESTCODE { get; set; }
        public string TESTNAME { get; set; }
        public string SAMPLETYPECODE { get; set; }
        public string TESTGROUP { get; set; }
        public string ITEM_TYPE { get; set; }     
        public DateTime? RELEASE_ON { get; set; }
    }
}
