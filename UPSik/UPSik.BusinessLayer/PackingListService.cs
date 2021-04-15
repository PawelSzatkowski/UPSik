using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;
using UPSik.BusinessLayer.TimeWarper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UPSik.BusinessLayer
{
    public interface IPackingListService
    {
        void AddPackagesToPackingList(List<Package> packages, List<Vehicle> vehicles);
        void CalculateCourierRatingForPackingList(Vehicle courierVehicle);
        float CalculateTotalCourierRating(int courierId);
        bool CheckIfPackingListIsAlreadyBeingManaged(int courierId);
        bool CheckIfPackingListIsManuallyManaged(int courierId);
        void ChoosePackagesForPackingList();
        void ExportCourierPackingListToJsonFile(List<Package> packagesForPackingList, int courierId);
        void GeneratePackingLists();
        List<PackingListInfo> GetCourierPackingListInfos(int courierId);
        string SetPackingListToManualManaging(int courierId);
        void SetVehiclesCurrentLoadToZero();
        void SwitchPackingListToAlreadyBeingManaged(int courierId);
    }

    public class PackingListService : IPackingListService
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPackageService _packageService;
        private readonly IUserService _userService;
        private readonly ITimeCalculator _timeCalculator;
        private readonly Func<IUPSikDbContext> _dbContextFactoryMethod;
        private readonly IPackageDeliveryConfigValues _packageDeliveryConfigValues;
        private readonly IGeoMapService _geoMapService;
        private readonly IShippingPlannerService _shippingPlannerService;

        public PackingListService(
            IVehicleService vehicleService,
            IPackageService packageService,
            IUserService userService,
            ITimeCalculator timeCalculator,
            Func<IUPSikDbContext> dbContextFactoryMethod,
            IPackageDeliveryConfigValues packageDeliveryConfigValues,
            IGeoMapService geoMapService,
            IShippingPlannerService shippingPlannerService)
        {
            _vehicleService = vehicleService;
            _packageService = packageService;
            _userService = userService;
            _timeCalculator = timeCalculator;
            _dbContextFactoryMethod = dbContextFactoryMethod;
            _packageDeliveryConfigValues = packageDeliveryConfigValues;
            _geoMapService = geoMapService;
            _shippingPlannerService = shippingPlannerService;
        }

        public void GeneratePackingLists()
        {
            _vehicleService.ClearCouriersPackingLists();

            ChoosePackagesForPackingList();

            var couriers = _userService.GetCouriersList();

            foreach (var courier in couriers)
            {
                var packagesForCourierPackingList = _packageService.GetPackagesForCourierPackingList(courier);
                _shippingPlannerService.AssignDistancesBetweenPackages(packagesForCourierPackingList, courier.Address);

                ExportCourierPackingListToJsonFile(packagesForCourierPackingList, courier.Id);
                Console.WriteLine($"File exported succesfuly for {courier.Name} {courier.Surname}");
            }

            SetVehiclesCurrentLoadToZero();
        }

        public void ChoosePackagesForPackingList()
        {
            var highPriority = Package.PackagePriority.High;
            var normalPriority = Package.PackagePriority.Normal;

            var highPriorityPackages = _packageService.GetPackagesByPriority(highPriority);
            var normalPriorityPackages = _packageService.GetPackagesByPriority(normalPriority);

            List<Vehicle> vehicles = _vehicleService.GetVehiclesList();

            AddPackagesToPackingList(highPriorityPackages, vehicles);
            AddPackagesToPackingList(normalPriorityPackages, vehicles);
        }

        public void AddPackagesToPackingList(List<Package> packages, List<Vehicle> vehicles)
        {
            foreach (var package in packages)
            {
                double minDistance = double.MaxValue;

                Vehicle courierVehicle = null;

                foreach (var vehicle in vehicles)
                {
                    var distance = _geoMapService.CalculateDistanceBetweenTwoPoints(package.Sender.Address.Latitude, package.Sender.Address.Longitude, vehicle.Driver.Address.Latitude, vehicle.Driver.Address.Longitude);

                    if (distance < minDistance && distance < _packageDeliveryConfigValues.MaxDistanceToPickupPackage)
                    {
                        minDistance = distance;
                        courierVehicle = vehicle;
                    }
                }

                var maxDistancePossibleToCover = courierVehicle.AverageVelocity * _packageDeliveryConfigValues.WorkingHours;

                courierVehicle.CurrentLoad = _vehicleService.GetVehicleCurrentLoad(courierVehicle);

                courierVehicle.CourierPackingList.Add(package);
                List<GeoCoords> addressesToVisit = _shippingPlannerService.GetCourierRouteCoords(courierVehicle);
                courierVehicle.CourierPackingList.Remove(package);

                courierVehicle.DistanceToCover = _shippingPlannerService.CalculateRouteDistance(addressesToVisit);

                if (courierVehicle.LoadCapacity >= courierVehicle.CurrentLoad + (int)package.Weight && courierVehicle.DistanceToCover <= maxDistancePossibleToCover && courierVehicle != null)
                {
                    _packageService.AddPackageToVehiclePackingList(package, courierVehicle);

                    package.State = Package.PackageState.AddedToPackingList;
                    _packageService.UpdatePackage(package);
                }
                else
                {
                    package.Priority = Package.PackagePriority.High;
                    _packageService.UpdatePackage(package);
                }
            }
        }

        public void ExportCourierPackingListToJsonFile(List<Package> packagesForPackingList, int courierId)
        {
            string systemDrivePath = Path.GetPathRoot(Environment.SystemDirectory);
            string targetPath = $"{systemDrivePath}shipping_lists";
            Directory.CreateDirectory(targetPath);

            var listGenerationDate = _timeCalculator.GetCurrentWarpedTime().ToString("yyyy-MM-dd");
            string fileName = $"{courierId}_{listGenerationDate}.json";
            string filePath = Path.Combine(targetPath, $"{fileName}");

            AddPackingListInfo(courierId, fileName, targetPath, packagesForPackingList.Count);

            var jsonData = JsonConvert.SerializeObject(packagesForPackingList, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

        private void AddPackingListInfo(int courierId, string fileName, string targetPath, int packagesCount)
        {
            using (var context = _dbContextFactoryMethod())
            {
                context.PackingListsInfo
                    .Add(new PackingListInfo
                    {
                        CourierId = courierId,
                        FileName = fileName,
                        FilePath = targetPath,
                        PackagesCount = packagesCount,
                        ManuallyManaged = false
                    });
                context.SaveChanges();
            }
        }

        public void SetVehiclesCurrentLoadToZero()
        {
            using (var context = _dbContextFactoryMethod())
            {
                context.Vehicles
                    .AsQueryable()
                    .Where(x => x.CurrentLoad != 0)
                    .ToList()
                    .ForEach(x => x.CurrentLoad = 0);

                context.SaveChanges();
            }
        }

        public bool CheckIfPackingListIsManuallyManaged(int courierId)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.PackingListsInfo
                    .Any(x => x.CourierId == courierId && x.ManuallyManaged);
            }
        }

        public string SetPackingListToManualManaging(int courierId)
        {
            PackingListInfo packingList;

            using (var context = _dbContextFactoryMethod())
            {
                packingList = context.PackingListsInfo
                    .FirstOrDefault(x => x.CourierId == courierId);

                packingList.ManuallyManaged = true;

                context.PackingListsInfo.Update(packingList);

                var courierVehicle = context.Vehicles
                    .Include(x => x.CourierPackingList)
                    .FirstOrDefault(x => x.Driver.Id == courierId);

                foreach (var package in courierVehicle.CourierPackingList)
                {
                    package.State = Package.PackageState.Shipping;
                }

                context.SaveChanges();
            }

            return packingList.FileName;
        }

        public bool CheckIfPackingListIsAlreadyBeingManaged(int courierId)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.PackingListsInfo
                    .Any(x => x.CourierId == courierId && x.IsManaged == true);
            }
        }

        public void SwitchPackingListToAlreadyBeingManaged(int courierId)
        {
            using (var context = _dbContextFactoryMethod())
            {
                var packingListInfo = context.PackingListsInfo
                    .FirstOrDefault(x => x.CourierId == courierId);

                packingListInfo.IsManaged = true;

                context.PackingListsInfo
                    .Update(packingListInfo);

                context.SaveChanges();
            }
        }

        public void CalculateCourierRatingForPackingList(Vehicle courierVehicle)
        {
            var packingListRating = (float)courierVehicle.CourierPackingList.Average(x => x.CourierRatingForDelivery);

            using (var context = _dbContextFactoryMethod())
            {
                var packingListInfo = context.PackingListsInfo
                    .FirstOrDefault(x => x.CourierId == courierVehicle.Driver.Id);

                packingListInfo.CourierRating = packingListRating;

                context.PackingListsInfo.Update(packingListInfo);
                context.SaveChanges();
            }
        }

        public float CalculateTotalCourierRating(int courierId)
        {
            List<PackingListInfo> courierPackingListsInfo;
            float courierRating = 0;

            using (var context = _dbContextFactoryMethod())
            {
                courierPackingListsInfo = context.PackingListsInfo
                    .AsQueryable()
                    .Where(x => x.CourierId == courierId && x.CourierRating != 0)
                    .ToList();

                if (courierPackingListsInfo.Count == 0)
                {
                    return courierRating;
                }

                courierRating = courierPackingListsInfo.Average(x => x.CourierRating);

                var courier = context.Users
                    .FirstOrDefault(x => x.Id == courierId);

                courier.CourierRating = courierRating;

                context.Users.Update(courier);
                context.SaveChanges();
            }

            return courierRating;
        }

        public List<PackingListInfo> GetCourierPackingListInfos(int id)
        {
            using (var context = _dbContextFactoryMethod())
            {
                return context.PackingListsInfo
                    .AsQueryable()
                    .Where(x => x.CourierId == id)
                    .ToList();
            }
        }
    }
}
