using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class AddTestRunningDayRequest
    {
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public List<string> Days { get; set; } = new();
    }

    public class UpdateTestRunningDayRequest
    {
        public List<string> Days { get; set; } = new();
    }
}
