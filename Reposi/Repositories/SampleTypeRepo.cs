using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class SampleTypeRepo : GenericRepo<Sample_Type>
    {
        public SampleTypeRepo(AppDbContext context) : base(context) { }

        public Sample_Type? GetByCode(string code)
            => dbSet.FirstOrDefault(s => s.Code == code && s.Active == true);

        public List<Sample_Type> GetActive()
            => dbSet.Where(s => s.Active == true).ToList();
    }
}