using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;

namespace Service.Interfaces
{
    public interface IAnnouncementService
    {
        AnnouncementDto? GetActive();
        List<AnnouncementDto> GetAll();
        void Create(CreateAnnouncementRequest req);
        void Deactivate(int id, string deactivatedBy);
    }
}