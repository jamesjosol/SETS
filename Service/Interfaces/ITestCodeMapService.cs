using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;

namespace Service.Interfaces
{
    public interface ITestCodeMapService
    {
        List<TestCodeMapItem> GetAll();
        string ResolveCode(string code);
        void Add(AddTestCodeMapRequest request);
        void Update(int id, UpdateTestCodeMapRequest request);
        void Toggle(int id, string userID);
        void Delete(int id);
    }
}