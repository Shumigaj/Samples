using Microsoft.EntityFrameworkCore;

namespace XamarinAndroidEfCoreApp
{
    public class ApplicationLogDbContext : DbContext
    {
        public ApplicationLogDbContext(DbContextOptions<ApplicationLogDbContext> options)
            : base(options) { }

        public DbSet<Log> Logs { get; set; }
    }
}
