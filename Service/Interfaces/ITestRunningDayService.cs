using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ITestRunningDayService
    {
        List<Test_RunningDay> GetAll();
        List<Test_RunningDay> GetByTestGroupCodes(List<string> testGroupCodes);
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

        /// <summary>
        /// Searches HCLAB tests by code/name.
        /// Optionally filtered to the given test group codes (for section TLs).
        /// Pass null to return all results (admin).
        /// </summary>
        Task<List<Model.HCLAB.Test>> GetHCLABTests(string param, List<string>? testGroupCodes = null);
    }
}