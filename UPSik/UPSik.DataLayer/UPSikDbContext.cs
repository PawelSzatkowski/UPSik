using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using UPSik.DataLayer.Models;

namespace UPSik.DataLayer
{
    public interface IUPSikDbContext : IDisposable
    {
        DbSet<Address> Addresses { get; set; }
        DbSet<Package> Packages { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Vehicle> Vehicles { get; set; }
        DbSet<PackingListInfo> PackingListsInfo { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }

    public class UPSikDbContext : DbContext, IUPSikDbContext
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<PackingListInfo> PackingListsInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Database=UPSik;Trusted_Connection=True");
        }
    }
}
