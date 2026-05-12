using Model.SETSDB;
using Reposi.Context;
using Reposi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposi
{
    public class UnitOfWork : IDisposable
    {
        private readonly AppDbContext _context;

        public UserRepo Users { get; private set; }
        public BranchRepo Branches { get; private set; }
        public SectionRepo Sections { get; private set; }
        public UserSectionRepo UserSections { get; private set; }
        public PCRepo PCs { get; private set; }
        public PCSectionRepo PCSections { get; private set; }
        public SampleTypeRepo SampleTypes { get; private set; }
        public BatchHeaderRepo BatchHeaders { get; private set; }
        public BatchSpecimenRepo BatchSpecimens { get; private set; }
        public BatchNonBarcodedRepo BatchNonBarcodeds { get; private set; }
        public RefCodesRepo RefCodes { get; private set; }
        public BatchSpecimenReceivingRepo BatchSpecimenReceivings { get; private set; }
        public SectionTestGroupRepo SectionTestGroups { get; private set; }
        public SpecimenSectionHeaderRepo SpecimenSectionHeaders { get; private set; }
        public SpecimenSectionTestRepo SpecimenSectionTests { get; private set; }
        public TestGroupRepo TestGroups { get; set; }
        public TestRunningDayRepo TestRunningDays { get; set; }
        public TatSectionRepo TatSections { get; private set; }
        public TatCycleLogRepo TatCycleLogs { get; private set; }
        public OnSiteSectionHeaderRepo OnSiteSectionHeaders { get; private set; }
        public OnSiteSectionTestRepo OnSiteSectionTests { get; private set; }
        public OnSiteAllowedLabNoRepo OnSiteAllowedLabNos { get; private set; }
        public OnSiteSettingsRepo OnSiteSettings { get; private set; }
        public TatProcessingRepo TatProcessing { get; private set; }
        public ProcessingOptionsRepo ProcessingOptions { get; private set; }
        public AuditLogRepo AuditLogs { get; private set; }
        public IssueIncidentTypeRepo IssueIncidentTypes { get; private set; }
        public IssueSubCategoryRepo IssueSubCategories { get; private set; }
        public IssueTagRepo IssueTags { get; private set; }
        public IssueLabEntryRepo IssueLabEntries { get; private set; }
        public IssueCommentRepo IssueComments { get; private set; }
        public ContingencyConfigRepo ContingencyConfig { get; private set; }
        public ContingencyBatchRepo ContingencyBatches { get; private set; }
        public ContingencySpecimenRepo ContingencySpecimens { get; private set; }
        public AnnouncementRepo Announcement { get; private set; }
        public NotificationRepo Notifications { get; private set; }
        public BranchSettingsRepo BranchSettings { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepo(_context);
            Branches = new BranchRepo(_context);
            Sections = new SectionRepo(_context);
            UserSections = new UserSectionRepo(_context);
            PCs = new PCRepo(_context);
            PCSections = new PCSectionRepo(_context);
            SampleTypes = new SampleTypeRepo(_context);
            BatchHeaders = new BatchHeaderRepo(_context);
            BatchSpecimens = new BatchSpecimenRepo(_context);
            BatchNonBarcodeds = new BatchNonBarcodedRepo(_context);
            RefCodes = new RefCodesRepo(_context);
            BatchSpecimenReceivings = new BatchSpecimenReceivingRepo(_context);
            SectionTestGroups = new SectionTestGroupRepo(_context);
            SpecimenSectionHeaders = new SpecimenSectionHeaderRepo(_context);
            SpecimenSectionTests = new SpecimenSectionTestRepo(_context);
            TestGroups = new TestGroupRepo(_context);
            TestRunningDays = new TestRunningDayRepo(_context);
            TatSections = new TatSectionRepo(_context);
            TatCycleLogs = new TatCycleLogRepo(_context);
            OnSiteSectionHeaders = new OnSiteSectionHeaderRepo(_context);
            OnSiteSectionTests = new OnSiteSectionTestRepo(_context);
            OnSiteAllowedLabNos = new OnSiteAllowedLabNoRepo(_context);
            OnSiteSettings = new OnSiteSettingsRepo(_context);
            TatProcessing = new TatProcessingRepo(_context);
            ProcessingOptions = new ProcessingOptionsRepo(_context);
            AuditLogs = new AuditLogRepo(_context);
            IssueIncidentTypes = new IssueIncidentTypeRepo(_context);
            IssueSubCategories = new IssueSubCategoryRepo(_context);
            IssueTags = new IssueTagRepo(_context);
            IssueLabEntries = new IssueLabEntryRepo(_context);
            IssueComments = new IssueCommentRepo(_context);
            ContingencyConfig = new ContingencyConfigRepo(_context);
            ContingencyBatches = new ContingencyBatchRepo(_context);
            ContingencySpecimens = new ContingencySpecimenRepo(_context);
            Announcement = new AnnouncementRepo(context);
            Notifications = new NotificationRepo(context);
            BranchSettings = new BranchSettingsRepo(context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
