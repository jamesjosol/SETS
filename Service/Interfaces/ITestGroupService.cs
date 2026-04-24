using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ITestGroupService
    {
        List<Test_Group> GetAll();
        Test_Group? GetByCode(string code);
    }
}