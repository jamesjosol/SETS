using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ToggleOnSiteRequest
    {
        public bool IsEnabled { get; set; }
    }

    public class AddAllowedLabNoRequest
    {
        public string Prefix { get; set; }
        public string? Description { get; set; }
    }
}