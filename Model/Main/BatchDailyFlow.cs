using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class BatchDailyFlow
    {
        public string Day { get; set; }
        public string Date { get; set; }
        public int Count { get; set; }
        public bool IsToday { get; set; }
    }
}
