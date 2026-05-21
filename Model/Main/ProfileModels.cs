using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class UserProfileResult
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public byte Theme { get; set; }
        public byte AccentColor { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime Created { get; set; }

        public List<UserProfileSection> Sections { get; set; } = new();
        public UserProfileStats Stats { get; set; } = new();
    }

    public class UserProfileSection
    {
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int RoleID { get; set; }
    }

    public class UserProfileStats
    {
        public int TotalBatchesEndorsed { get; set; }
        public int TotalBatchesReceived { get; set; }
        public int TotalSpecimensCompleted { get; set; }
    }

    public class UpdateProfilePictureRequest
    {
        public string? ProfilePicture { get; set; }
    }
}