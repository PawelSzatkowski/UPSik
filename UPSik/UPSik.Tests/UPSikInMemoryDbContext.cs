using Microsoft.EntityFrameworkCore;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;

namespace UPSik.Tests
{
    public class UPSikInMemoryDbContext : DbContext, IUPSikDbContext
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<PackingListInfo> PackingListsInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("UPSikTests");
        }
    }
}
