using Model.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IFlagService
    {
        FlagSpecimenResult FlagSpecimen(FlagSpecimenRequest request);
        FlagSpecimenResult UnflagSpecimen(UnflagSpecimenRequest request);
    }
}
