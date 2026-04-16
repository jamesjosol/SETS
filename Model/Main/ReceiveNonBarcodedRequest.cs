using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ReceiveNonBarcodedRequest
    {
        public string UserID { get; set; }
        public List<int> ItemIDs { get; set; } = new();
    }
}