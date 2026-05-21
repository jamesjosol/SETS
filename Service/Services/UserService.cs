using HCLAB;
using Model.HCLAB;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public UserService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        public bool Login(string userid, string password, string branch)
        {
            try
            {
                int _auth = HclabMaster.HCLABUsers.Auth(
                    userid,
                    password,
                    HclabConnection.ConnectionString(branch)
                );
                return _auth != 0;
            }
            catch { throw; }
        }

        public async Task<List<Model.HCLAB.User>> GetHCLABUsers(string param)
        {
            try
            {
                return await HclabMaster.HCLABUsers.GetUsers(
                    HclabConnection.ConnectionString(_branch_raw), param);
            }
            catch { throw; }
        }

        public User_Master GetUser(string userid, string branch)
        {
            try
            {
                using var context = _factory.CreateContext(SetsConnection.ConnectionString(branch));
                using var unit = new UnitOfWork(context);
                return unit.Users.GetByUserID(userid);
            }
            catch { throw; }
        }

        public List<User_Master> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Users.GetAll().ToList();
            }
            catch { throw; }
        }

        public void Add(User_Master user)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Users.Add(user);
            }
            catch { throw; }
        }

        public void Update(User_Master user)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Users.Update(user);
            }
            catch { throw; }
        }

        public UserProfileResult GetProfile(string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var user = context.User_Master.FirstOrDefault(u => u.UserID == userID)
                    ?? throw new Exception("User not found.");

                // ── Active sections with roles ─────────────────────────────
                var userSections = context.User_Section
                    .Where(us => us.UserID == userID && us.Active)
                    .ToList();

                var sectionCodes = userSections.Select(us => us.SectionCode).ToList();

                var sections = context.Section_Master
                    .ToList()
                    .Where(s => sectionCodes.Contains(s.Code))
                    .ToDictionary(s => s.Code);

                var profileSections = userSections
                    .Where(us => sections.ContainsKey(us.SectionCode))
                    .Select(us => new UserProfileSection
                    {
                        SectionCode = us.SectionCode,
                        SectionName = sections[us.SectionCode].Name,
                        Category = sections[us.SectionCode].Category,
                        RoleID = us.RoleID,
                    })
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.SectionName)
                    .ToList();

                // ── Stats ──────────────────────────────────────────────────
                // Endorser: distinct batches endorsed by this user
                var totalEndorsed = context.Batch_Header
                    .Count(b => b.EndorsedBy == userID);

                // Receiver: distinct receiving records by this user
                var totalReceived = context.Batch_Specimen_Receiving
                    .Count(r => r.ProcReceivedBy == userID);

                // Runner: completed specimen section headers routed by this user
                var totalCompleted = context.Specimen_Section_Header
                    .Count(h => h.ReceivedBy == userID && h.Status == "C");

                return new UserProfileResult
                {
                    UserID = user.UserID,
                    UserName = user.UserName,
                    IsAdmin = user.IsAdmin,
                    Theme = user.Theme,
                    AccentColor = user.AccentColor,
                    ProfilePicture = user.ProfilePicture,
                    Created = user.Created,
                    Sections = profileSections,
                    Stats = new UserProfileStats
                    {
                        TotalBatchesEndorsed = totalEndorsed,
                        TotalBatchesReceived = totalReceived,
                        TotalSpecimensCompleted = totalCompleted,
                    }
                };
            }
            catch { throw; }
        }

        public void UpdateProfilePicture(string userID, string? base64Image)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                var user = context.User_Master.FirstOrDefault(u => u.UserID == userID)
                    ?? throw new Exception("User not found.");

                user.ProfilePicture = base64Image;
                user.Updated = DateTime.Now;
                user.UpdatedBy = userID;
                context.SaveChanges();
            }
            catch { throw; }
        }
    }
}