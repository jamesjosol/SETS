using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IProcessingOptionsService
    {
        Processing_Options GetOptions();
        void UpsertOptions(bool showTemperature, bool showTempRemarks, bool showBagNo, string userID);
    }
}