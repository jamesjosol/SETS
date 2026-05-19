using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HCLAB;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class TestRunningDayService : ITestRunningDayService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        // Maps day-name strings (as stored) to DayOfWeek enum
        private static readonly Dictionary<string, DayOfWeek> _dayMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Sunday",    DayOfWeek.Sunday    },
                { "Monday",    DayOfWeek.Monday    },
                { "Tuesday",   DayOfWeek.Tuesday   },
                { "Wednesday", DayOfWeek.Wednesday },
                { "Thursday",  DayOfWeek.Thursday  },
                { "Friday",    DayOfWeek.Friday    },
                { "Saturday",  DayOfWeek.Saturday  },
            };

        public TestRunningDayService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        public List<Test_RunningDay> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestRunningDays.GetAll();
            }
            catch { throw; }
        }

        /// <summary>
        /// Returns only entries whose TestGroupCode matches one of the
        /// section's assigned test groups. Used for lab section TL views.
        /// </summary>
        public List<Test_RunningDay> GetByTestGroupCodes(List<string> testGroupCodes)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestRunningDays.GetByTestGroupCodes(testGroupCodes);
            }
            catch { throw; }
        }

        public Test_RunningDay? GetByTestCode(string testCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestRunningDays.GetByTestCode(testCode);
            }
            catch { throw; }
        }

        public void Add(Test_RunningDay item)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.TestRunningDays.Add(item);
            }
            catch { throw; }
        }

        public void Update(Test_RunningDay item)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.TestRunningDays.Update(item);
            }
            catch { throw; }
        }

        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.TestRunningDays.Delete(id);
            }
            catch { throw; }
        }

        /// <summary>
        /// Returns the nearest date >= <paramref name="from"/> that falls on one of
        /// the configured running days for the given test code.
        /// Returns today if today is already a running day.
        /// Returns null if the test has no running day setup.
        /// </summary>
        public DateOnly? GetNearestRunningDate(string testCode, DateOnly from)
        {
            var setup = GetByTestCode(testCode);
            if (setup == null) return null;

            var configuredDays = setup
                .GetDayList()
                .Select(d => _dayMap.TryGetValue(d, out var dow) ? (DayOfWeek?)dow : null)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToHashSet();

            if (!configuredDays.Any()) return null;

            // Walk forward from today (max 7 days — guaranteed to find a match)
            for (int i = 0; i <= 7; i++)
            {
                var candidate = from.AddDays(i);
                if (configuredDays.Contains(candidate.DayOfWeek))
                    return candidate;
            }

            return null;
        }

        /// <summary>
        /// Searches HCLAB tests by code/name.
        /// When testGroupCodes is provided (non-admin TL), results are filtered
        /// to only those belonging to the section's test groups.
        /// </summary>
        public async Task<List<Model.HCLAB.Test>> GetHCLABTests(string param, List<string>? testGroupCodes = null)
        {
            try
            {
                var results = await HclabMaster.HCLABTests.GetTests(
                    HclabConnection.ConnectionString(_branch_raw), param);

                // If test group codes supplied, restrict to section's tests only
                if (testGroupCodes != null && testGroupCodes.Any())
                    results = results
                        .Where(t => testGroupCodes.Contains(t.TestGroup, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                return results;
            }
            catch { throw; }
        }
    }
}