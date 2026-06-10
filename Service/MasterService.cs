using Reposi.Context;
using Service.Interfaces;
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
        public HealthService Health { get; private set; }
        public RunnerService Runner { get; private set; }
        public SpecimenSectionService SpecimenSection { get; private set; }
        public SectionTestGroupService SectionTestGroup { get; set; }
        public TestGroupService TestGroup { get; set; }
        public TestRunningDayService TestRunningDay { get; set; }
        public TatService Tat { get; private set; }
        public TatOutboundService TatOutbound { get; private set; }
        public OnSiteService OnSite { get; private set; }
        public OnSiteSettingsService OnSiteSettings { get; private set; }
        public ProcessingOptionsService ProcessingOptions { get; private set; }
        public AuditService Audit { get; private set; }
        public SpecimenIssueService SpecimenIssue { get; private set; }
        public ContingencyService Contingency { get; private set; }
        public AnnouncementService Announcement { get; private set; }
        public NotificationService Notification { get; private set; }
        public ReportService Report { get; set; }
        public BranchSettingsService BranchSettings { get; private set; }
        public FlagService Flag { get; private set; }
        public ChangelogService Changelog { get; private set; }
        public TestCodeMapService TestCodeMap { get; private set; }

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
            Health = new HealthService(_factory, _branch);
            Runner = new RunnerService(_factory, _branch);
            SpecimenSection = new SpecimenSectionService(_factory, _branch);
            SectionTestGroup = new SectionTestGroupService(_factory, _branch);
            TestGroup = new TestGroupService(_factory, _branch);
            TestRunningDay = new TestRunningDayService(_factory, _branch);
            Tat = new TatService(_factory, _branch);
            TatOutbound = new TatOutboundService(_factory, _branch);
            OnSite = new OnSiteService(_factory, _branch);
            OnSiteSettings = new OnSiteSettingsService(_factory, _branch);
            ProcessingOptions = new ProcessingOptionsService(_factory, _branch);
            Audit = new AuditService(_factory, _branch);
            SpecimenIssue = new SpecimenIssueService(_factory, _branch);
            Contingency = new ContingencyService(_factory, _branch);
            Announcement = new AnnouncementService(_factory, _branch);
            Notification = new NotificationService(_factory, _branch);
            Report = new ReportService(_factory, _branch);
            BranchSettings = new BranchSettingsService(_factory, _branch);
            Flag = new FlagService(_factory, _branch);
            Changelog = new ChangelogService(_factory, _branch);
            TestCodeMap = new TestCodeMapService(_factory, _branch);
        }

        public void Dispose()
        {
            
        }
    }
}