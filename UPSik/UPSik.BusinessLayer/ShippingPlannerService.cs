using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IShippingPlannerService
    {
        void AssignDistancesBetweenPackages(List<Package> packingList, Address courierAddress);
        double CalculateRouteDistance(List<GeoCoords> geoCoordsList);
        bool CheckIfPackageCanBeDeliveredAtAll(Address receiverAddress, Address senderAddress);
        List<GeoCoords> GetCourierRouteCoords(Vehicle courierVehicle);
    }

    public class ShippingPlannerService : IShippingPlannerService
    {
        private readonly Func<IUPSikDbContext> _dbContextFactoryMethod;
        private readonly IGeoMapService _geoMapService;
        private readonly IPackageDeliveryConfigValues _packageDeliveryConfigValues;
        private readonly IPackageService _packageService;

        public ShippingPlannerService(
            Func<IUPSikDbContext> dbContextFactoryMethod,
            IGeoMapService geoMapService,
            IPackageDeliveryConfigValues packageDeliveryConfigValues,
            IPackageService packageService)
        {
            _dbContextFactoryMethod = dbContextFactoryMethod;
            _geoMapService = geoMapService;
            _packageDeliveryConfigValues = packageDeliveryConfigValues;
            _packageService = packageService;
        }

        public bool CheckIfPackageCanBeDeliveredAtAll(Address receiverAddress, Address senderAddress)
        {
            List<Vehicle> vehicles;

            using (var context = _dbContextFactoryMethod()) 
            {
                vehicles = context.Vehicles
                    .AsQueryable()
                    .Include(x => x.Driver)
                       .ThenInclude(x => x.Address)
                    .ToList();
            }

            foreach (var vehicle in vehicles)
            {
                var maxTotalDistance = vehicle.AverageVelocity * _packageDeliveryConfigValues.WorkingHours;

                var totalDistanceToCover =
                    _geoMapService.CalculateDistanceBetweenTwoPoints(vehicle.Driver.Address.Latitude, vehicle.Driver.Address.Longitude, senderAddress.Latitude, senderAddress.Longitude)
                    + _geoMapService.CalculateDistanceBetweenTwoPoints(senderAddress.Latitude, senderAddress.Longitude, receiverAddress.Latitude, receiverAddress.Longitude)
                    + _geoMapService.CalculateDistanceBetweenTwoPoints(receiverAddress.Latitude, receiverAddress.Longitude, vehicle.Driver.Address.Latitude, vehicle.Driver.Address.Longitude);

                if (totalDistanceToCover <= maxTotalDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public double CalculateRouteDistance(List<GeoCoords> geoCoordsList)
        {
            if (geoCoordsList.Count == 0)
            {
                return 0;
            }

            double distanceToCover = 0;

            for (var i = 0; i < geoCoordsList.Count - 1; i++)
            {
                distanceToCover += _geoMapService.CalculateDistanceBetweenTwoPoints(
                    geoCoordsList[i].Latitude,
                    geoCoordsList[i].Longitude,
                    geoCoordsList[i + 1].Latitude,
                    geoCoordsList[i + 1].Longitude);
            }

            return distanceToCover;
        }

        public List<GeoCoords> GetCourierRouteCoords(Vehicle courierVehicle)
        {
            if (courierVehicle.CourierPackingList.Count == 0)
            {
                return null;
            }

            var coordsList = new List<GeoCoords>();

            var courierAddressCoords = new GeoCoords
            {
                Latitude = courierVehicle.Driver.Address.Latitude,
                Longitude = courierVehicle.Driver.Address.Longitude
            };
            coordsList.Add(courierAddressCoords);

            foreach (var package in courierVehicle.CourierPackingList)
            {
                coordsList.Add(new GeoCoords
                {
                    Latitude = package.Sender.Address.Latitude,
                    Longitude = package.Sender.Address.Longitude
                });
            }

            foreach (var package in courierVehicle.CourierPackingList)
            {
                coordsList.Add(new GeoCoords
                {
                    Latitude = package.Receiver.Address.Latitude,
                    Longitude = package.Receiver.Address.Longitude
                });
            }

            coordsList.Add(courierAddressCoords);

            return coordsList;
        }

        public void AssignDistancesBetweenPackages(List<Package> packingList, Address courierAddress) // BARDZO BRZYDKIE POMYŚL SOBIE JAK TO UŁADNIĆ
        {
            var previousPackage = new Package();

            foreach (var currentPackage in packingList)
            {
                var fromSender = new GeoCoords();
                var toSender = new GeoCoords();

                var fromReceiver = new GeoCoords();
                var toReceiver = new GeoCoords();

                if (packingList.First() == currentPackage)
                {
                    fromSender.Latitude = courierAddress.Latitude;
                    fromSender.Longitude = courierAddress.Longitude;

                    toSender.Latitude = currentPackage.Sender.Address.Latitude;
                    toSender.Longitude = currentPackage.Sender.Address.Longitude;

                    fromReceiver.Latitude = packingList.Last().Sender.Address.Latitude;
                    fromReceiver.Longitude = packingList.Last().Sender.Address.Longitude;

                    toReceiver.Latitude = currentPackage.Receiver.Address.Latitude;
                    toReceiver.Longitude = currentPackage.Receiver.Address.Longitude;
                }

                else
                {
                    fromSender.Latitude = previousPackage.Sender.Address.Latitude;
                    fromSender.Longitude = previousPackage.Sender.Address.Longitude;

                    toSender.Latitude = currentPackage.Sender.Address.Latitude;
                    toSender.Longitude = currentPackage.Sender.Address.Longitude;

                    fromReceiver.Latitude = previousPackage.Receiver.Address.Latitude;
                    fromReceiver.Longitude = previousPackage.Receiver.Address.Longitude;

                    toReceiver.Latitude = currentPackage.Receiver.Address.Latitude;
                    toReceiver.Longitude = currentPackage.Receiver.Address.Longitude;
                }

                currentPackage.SenderDistanceFromPreviousPoint = _geoMapService.CalculateDistanceBetweenTwoPoints(fromSender.Latitude, fromSender.Longitude, toSender.Latitude, toSender.Longitude);
                currentPackage.ReceiverDistanceFromPreviousPoint = _geoMapService.CalculateDistanceBetweenTwoPoints(fromReceiver.Latitude, fromReceiver.Longitude, toReceiver.Latitude, toReceiver.Longitude);

                previousPackage = currentPackage;
            }

            using (var context = _dbContextFactoryMethod())
            {
                foreach (var package in packingList)
                {
                    context.Packages.Update(package);
                }

                context.SaveChanges();
            }
        }
    }
}
