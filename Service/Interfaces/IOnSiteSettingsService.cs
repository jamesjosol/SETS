using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IOnSiteSettingsService
    {
        OnSite_Settings? GetSettings();
        void SetEnabled(bool isEnabled, string updatedBy);

        List<OnSite_AllowedLabNo> GetAllAllowedLabNos();
        void AddAllowedLabNo(string prefix, string? description, string createdBy);
        void ToggleAllowedLabNo(int id, string updatedBy);
        void DeleteAllowedLabNo(int id);
    }
}