using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPSik.BusinessLayer.TimeWarper;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IPackageService
    {
        Task AddNewPackageAsync(Package newPackage);
        void AddPackageToVehiclePackingList(Package package, Vehicle courierVehicle);
        void ChangeAddedToPackingListPackagesStateToShipping();
        void ChangePackagesState(List<Package> packages, Package.PackageState packageState);
        void ChangeShippingPackagesStateToAwaitingPickup(List<Package> courierPackingList);
        int DeliverPackage(int packageId, string delivery, string etaToReceiver);
        Package GetPackageByNumber(Package package);
        List<Package> GetPackagesByPriority(Package.PackagePriority packagePriority);
        List<Package> GetPackagesForCourierPackingList(User courier);
        void ManageUndeliveredPackages(List<Package> courierPackingList);
        void UpdatePackage(Package package);
    }

    public class PackageService : IPackageService
    {
        private readonly Func<IUPSikDbContext> _dbContextFactoryMethod;
        private readonly ITimeCalculator _timeCalculator;

        public PackageService(
            Func<IUPSikDbContext> dbContextFactoryMethod,
            ITimeCalculator timeCalculator)
        {
            _dbContextFactoryMethod = dbContextFactoryMethod;
            _timeCalculator = timeCalculator;
        }

        public Package GetPackageByNumber(Package package)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Packages.FirstOrDefault(x => x.Number == package.Number);
            }
        }

        public List<Package> GetPackagesByPriority(Package.PackagePriority packagePriority)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.Packages
                        .Include(x => x.Sender)
                            .ThenInclude(x => x.Address)
                        .Include(x => x.Receiver)
                            .ThenInclude(x => x.Address)
                        .Where(x => x.Priority == packagePriority && x.State == Package.PackageState.AwaitingAddingToPackingList)
                        .ToList();
            }
        }

        public async Task AddNewPackageAsync(Package newPackage)
        {
            newPackage.Number = Guid.NewGuid();
            newPackage.DateOfAdding = _timeCalculator.GetCurrentWarpedTime();
            newPackage.State = Package.PackageState.AwaitingAddingToPackingList;

            using (var context = _dbContextFactoryMethod())
            {

                context.Users.Attach(newPackage.Sender);
                context.Users.Attach(newPackage.Receiver);
                context.Packages.Add(newPackage);
                await context.SaveChangesAsync();
            }
        }

        public List<Package> GetPackagesForCourierPackingList(User courier)
        {
            using (var context = _dbContextFactoryMethod())
            {
                var vehicle = context.Vehicles
                    .Include(x => x.CourierPackingList)
                        .ThenInclude(x => x.Receiver)
                            .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                        .ThenInclude(x => x.Sender)
                            .ThenInclude(x => x.Address)
                    .Include(x => x.Driver)
                        .ThenInclude(x => x.Address)
                    .FirstOrDefault(x => x.Driver.Id == courier.Id);

                return vehicle.CourierPackingList;
            }
        }

        public void AddPackageToVehiclePackingList(Package package, Vehicle courierVehicle)
        {
            if (courierVehicle != null)
            {
                using (var context = _dbContextFactoryMethod())
                {
                    courierVehicle.CourierPackingList.Add(package);
                    courierVehicle.CurrentLoad += (int)package.Weight;

                    context.Vehicles.Update(courierVehicle);
                    context.SaveChanges();
                }
            }
        }

        public void UpdatePackage(Package package)
        {
            using (var context = _dbContextFactoryMethod())
            {
                context.Packages.Update(package);
                context.SaveChanges();
            }
        }

        public void ChangeAddedToPackingListPackagesStateToShipping()
        {
            using (var context = _dbContextFactoryMethod())
            {
                var packages = context.Packages
                    .AsQueryable()
                    .Where(x => x.State == Package.PackageState.AddedToPackingList)
                    .ToList();

                ChangePackagesState(packages, Package.PackageState.Shipping);
            }
        }

        public void ChangeShippingPackagesStateToAwaitingPickup(List<Package> courierPackingList)
        {
            using (var context = _dbContextFactoryMethod())
            {
                var packages = context.Packages
                    .AsQueryable()
                    .Where(x => x.State == Package.PackageState.Shipping && courierPackingList.Contains(x))
                    .ToList();

                ChangePackagesState(packages, Package.PackageState.AwaitingPickup);
            }
        }

        public int DeliverPackage(int packageId, string delivery, string etaToReceiver)
        {
            var deliveryDate = DateTime.Parse(delivery);
            var etaToReceiverDate = DateTime.Parse(etaToReceiver);

            using (var context = _dbContextFactoryMethod())
            {
                var package = context.Packages
                    .FirstOrDefault(x => x.Id == packageId && x.State != Package.PackageState.Delivered);

                if (package == null)
                {
                    return 0;
                }

                package.State = Package.PackageState.Delivered;
                package.CourierRatingForDelivery = CalculateCourierRatingForDelivery(deliveryDate, etaToReceiverDate);

                context.Packages.Update(package);
                context.SaveChanges();

                return package.CourierRatingForDelivery;
            }
        }

        private int CalculateCourierRatingForDelivery(DateTime deliveryDate, DateTime etaToReceiver)
        {
            var maxScore = 5;

            var pointsSubstracted = Convert.ToInt32(Math.Floor(Math.Abs((deliveryDate - etaToReceiver).TotalMinutes / 10)));

            return maxScore - pointsSubstracted;
        }

        public void ManageUndeliveredPackages(List<Package> courierPackingList)
        {
            using (var context = _dbContextFactoryMethod())
            {
                var undeliveredPackages = context.Packages
                    .AsQueryable()
                    .Where(x => x.State != Package.PackageState.Delivered && courierPackingList.Contains(x))
                    .ToList();

                ChangePackagesState(undeliveredPackages, Package.PackageState.AwaitingAddingToPackingList);
            }
        }

        public void ChangePackagesState(List<Package> packages, Package.PackageState packageState)
        {
            using (var context = _dbContextFactoryMethod())
            {
                foreach (var package in packages)
                {
                    package.State = packageState;
                    context.Packages.Update(package);
                }
                context.SaveChanges();
            }
        }
    }
}
