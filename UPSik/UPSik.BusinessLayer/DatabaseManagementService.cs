using System;
using UPSik.DataLayer;

namespace UPSik.BusinessLayer
{
    public interface IDatabaseManagementService
    {
        void EnsureDatabaseCreation();
    }

    public class DatabaseManagementService : IDatabaseManagementService
    {
        private readonly Func<IUPSikDbContext> _UPSikDbContextFactoryMethod;

        public DatabaseManagementService(Func<IUPSikDbContext> UPSikDbContextFactoryMethod)
        {
            _UPSikDbContextFactoryMethod = UPSikDbContextFactoryMethod;
        }
        public void EnsureDatabaseCreation()
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
