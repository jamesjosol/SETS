using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ISectionTestGroupService
    {
        List<Section_TestGroup> GetAll();
        List<Section_TestGroup> GetBySectionCode(string sectionCode);
        List<Section_TestGroup> GetActive();
        Section_TestGroup? GetBySectionAndGroup(string sectionCode, string testGroupCode);
        List<string> ResolveSectionCodes(List<string> testGroupCodes);
        void Add(Section_TestGroup item);
        void Update(Section_TestGroup item);
        void Delete(int id);
    }
}