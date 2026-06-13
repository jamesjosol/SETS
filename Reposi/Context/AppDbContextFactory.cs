using Microsoft.EntityFrameworkCore;

namespace Reposi.Context
{
    public class AppDbContextFactory
    {
        public AppDbContext CreateContext(string connectionString)
        {
            //var options = new DbContextOptionsBuilder<AppDbContext>()
            //    .UseSqlServer(connectionString)
            //    .Options;

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(100))
                .Options;


            return new AppDbContext(options);
        }
    }
}