using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class OnSite_AllowedLabNo
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
}