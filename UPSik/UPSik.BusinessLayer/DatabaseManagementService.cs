using Microsoft.EntityFrameworkCore;
using System;
using UPSik.DataLayer;

namespace UPSik.BusinessLayer
{
    public interface IDatabaseManagementService
    {
        void EnsureDatabaseMigration();
    }

    public class DatabaseManagementService : IDatabaseManagementService
    {
        private readonly Func<IUPSikDbContext> _UPSikDbContextFactoryMethod;

        public DatabaseManagementService(Func<IUPSikDbContext> UPSikDbContextFactoryMethod)
        {
            _UPSikDbContextFactoryMethod = UPSikDbContextFactoryMethod;
        }
        public void EnsureDatabaseMigration()
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                context.Database.Migrate();
            }
        }
    }
}
