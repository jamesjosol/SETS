using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    /// <summary>
    /// Cancels an entire specimen (header + all non-released tests) from the runner/lab-section side.
    /// TL / Admin only.
    /// </summary>
    public class CancelSectionSpecimenRequest
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;

        /// <summary>True when the specimen is from the OnSite tables.</summary>
        public bool IsOnSite { get; set; }
    }

    /// <summary>
    /// Aborts a single running test back to pending.
    /// Available to all users.
    /// </summary>
    public class AbortRunningTestRequest
    {
        public int TestId { get; set; }
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;

        /// <summary>True when the specimen is from the OnSite tables.</summary>
        public bool IsOnSite { get; set; }
    }
}
