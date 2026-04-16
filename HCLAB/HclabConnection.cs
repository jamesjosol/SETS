using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCLAB
{
    public static class HclabConnection
    {
        public static string ConnectionString(string branch)
        {
            string _constring = "";

            if (branch == "WES") _constring = hclab.Default.WES;
            if (branch == "NAGA") _constring = hclab.Default.NAGA;
            if (branch == "LILOAN") _constring = hclab.Default.LILOAN;
            if (branch == "MACTAN") _constring = hclab.Default.MACTAN;
            if (branch == "DIAMOND") _constring = hclab.Default.DIAMOND;
            if (branch == "TABUNOK") _constring = hclab.Default.TABUNOK;
            if (branch == "CONSOLACION") _constring = hclab.Default.CONSOLACION;
            return _constring;
        }

        
    }
}
