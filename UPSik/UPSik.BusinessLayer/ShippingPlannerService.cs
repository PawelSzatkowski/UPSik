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
        double CalculateRouteDistance(List<GeoCoords> geoCoordsList);
        bool CheckIfPackageCanBeDeliveredAtAll(Address receiverAddress, Address senderAddress);
        List<GeoCoords> GetCourierRouteCoords(Vehicle courierVehicle);
    }

    public class ShippingPlannerService : IShippingPlannerService
    {
        private readonly Func<IUPSikDbContext> _dbContextFactoryMethod;
        private readonly IGeoMapService _geoMapService;

        public ShippingPlannerService(
            Func<IUPSikDbContext> dbContextFactoryMethod,
            IGeoMapService geoMapService)
        {
            _dbContextFactoryMethod = dbContextFactoryMethod;
            _geoMapService = geoMapService;
        }

        public bool CheckIfPackageCanBeDeliveredAtAll(Address receiverAddress, Address senderAddress)
        {
            List<Vehicle> vehicles;

            using (var context = _dbContextFactoryMethod()) // mam metodę zwracającą wszystkie pojazdy w VehicleService, ale stworzenie instancji VehicleService doprowadzi tu do kołowej zależności - co w tej sytuacji? czy to jest przeciw clean code czy jest ok?
            {
                vehicles = context.Vehicles
                    .AsQueryable()
                    .Include(x => x.Driver)
                       .ThenInclude(x => x.Address)
                    .ToList();
            }

            int workingHours = 10;

            foreach (var vehicle in vehicles)
            {
                var maxTotalDistance = vehicle.AverageVelocity * workingHours;

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

            foreach (var pack in courierVehicle.CourierPackingList)
            {
                coordsList.Add(new GeoCoords
                {
                    Latitude = pack.Receiver.Address.Latitude,
                    Longitude = pack.Receiver.Address.Longitude
                });
            }

            coordsList.Add(courierAddressCoords);

            return coordsList;
        }
    }
}
