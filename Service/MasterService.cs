using Reposi.Context;
using Service.Services;

namespace Service
{
    public class MasterService : IDisposable
    {
        public UserService User { get; private set; }
        public BranchService Branch { get; private set; }
        public SectionService Section { get; private set; }
        public UserSectionService UserSection { get; private set; }
        public PCService PC { get; private set; }
        public TransactionService Transaction { get; private set; }
        public SampleTypeService SampleType { get; set; }
        public BatchService Batch { get; private set; }
        public ReceivingService Receiving { get; private set; }

        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public MasterService(string branch)
        {
            _factory = new AppDbContextFactory();
            _branch = branch;
            Register();
        }

        private void Register()
        {
            User = new UserService(_factory, _branch);
            Branch = new BranchService(_factory, _branch);
            Section = new SectionService(_factory, _branch);
            UserSection = new UserSectionService(_factory, _branch);
            PC = new PCService(_factory, _branch);
            Transaction = new TransactionService(_branch);
            SampleType = new SampleTypeService(_factory, _branch);
            Batch = new BatchService(_factory, _branch);
            Receiving = new ReceivingService(_factory, _branch);
        }

        public void Dispose()
        {
            User = null;
            Branch = null;
            Section = null;
            UserSection = null;
            PC = null;
            Transaction = null;
            SampleType = null;
            Receiving = null;
        }
    }
}