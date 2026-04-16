using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ISampleTypeService
    {
        Sample_Type? GetSampleType(string code);
    }
}
