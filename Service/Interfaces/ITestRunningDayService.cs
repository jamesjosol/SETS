using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ITestRunningDayService
    {
        List<Test_RunningDay> GetAll();
        Test_RunningDay? GetByTestCode(string testCode);
        void Add(Test_RunningDay item);
        void Update(Test_RunningDay item);
        void Delete(int id);

        /// <summary>
        /// Given a test code and a reference date (usually today),
        /// returns the nearest upcoming date that falls on one of the
        /// configured running days. Returns null if no setup exists.
        /// If today is a running day it returns today.
        /// </summary>
        DateOnly? GetNearestRunningDate(string testCode, DateOnly from);
        Task<List<Model.HCLAB.Test>> GetHCLABTests(string param);
    }
}