using Microsoft.Extensions.Configuration;

namespace Reposi
{
    public static class SetsConnection
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string ConnectionString(string branch)
        {
            return _configuration.GetConnectionString(branch)
                ?? throw new Exception($"Connection string for branch '{branch}' not found.");
        }
    }
}