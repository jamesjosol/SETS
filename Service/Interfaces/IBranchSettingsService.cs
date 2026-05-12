using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IBranchSettingsService
    {
        Branch_Settings Get();
        void SetOutbound(bool enabled, string updatedBy);
    }
}