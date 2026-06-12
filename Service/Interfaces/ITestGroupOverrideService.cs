using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;

namespace Service.Interfaces
{
    public interface ITestGroupOverrideService
    {
        List<TestGroupOverrideItem> GetAll();
        string? ResolveGroup(string testCode);
        void Add(AddTestGroupOverrideRequest request);
        void Update(int id, UpdateTestGroupOverrideRequest request);
        void Toggle(int id, string userID);
        void Delete(int id);
    }
}