using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;
using UPSik.BusinessLayer.TimeWarper;

namespace UPSik.BusinessLayer
{
    public interface IPackingListService
    {
        void AddPackagesToPackingList(List<Package> packages, List<Vehicle> vehicles);
        void ChoosePackagesForPackingList();
        void ExportCourierPackingListToJsonFile(List<Package> packagesForPackingList, int courierId);
        void GeneratePackingLists();
        void SetVehiclesCurrentLoadToZero();
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

            var jsonData = JsonConvert.SerializeObject(packagesForPackingList, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
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
    }
}
