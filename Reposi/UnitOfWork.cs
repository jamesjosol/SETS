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
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
