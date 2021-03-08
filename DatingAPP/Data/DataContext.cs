using DatingAPP.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingAPP.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opt):base(opt)
        {

        }

        public DbSet<AppUser> Users { get; set; }
    }
}
