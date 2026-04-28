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

        public void UpsertFromHclab(List<Model.HCLAB.Ref_Tables> records)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            foreach (var record in records)
            {
                var existing = unit.SampleTypes.GetByCode(record.Code);
                if (existing == null)
                {
                    unit.SampleTypes.Add(new Model.SETSDB.Sample_Type
                    {
                        Code = record.Code,
                        Name = record.Name,
                        Active = true
                    });
                }
                else
                {
                    existing.Name = record.Name;
                    unit.SampleTypes.Update(existing);
                }
            }
        }
    }
}
