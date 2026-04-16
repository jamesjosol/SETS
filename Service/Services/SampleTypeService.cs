using Reposi;
using Reposi.Context;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SampleTypeService: ISampleTypeService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public SampleTypeService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public Model.SETSDB.Sample_Type? GetSampleType(string code)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);
            return unit.SampleTypes.GetByCode(code);
        }
    }
}
