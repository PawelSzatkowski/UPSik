using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using UPSik.BusinessLayer;
using Moq;
using FluentAssertions;
using UPSik.BusinessLayer.TimeWarper;

namespace UPSik.Tests.BussinessLayer
{
    public class PackingListServiceTests
    {
        private readonly Func<IUPSikDbContext> _contextFactoryMethod = () => new UPSikInMemoryDbContext();

        [SetUp]
        public void SetUp()
        {
            using (var context = _contextFactoryMethod())
            {
                var packages = context.Packages.ToList();
                context.Packages.RemoveRange(packages);

                var vehicles = context.Vehicles.ToList();
                context.Vehicles.RemoveRange(vehicles);

                var addresses = context.Addresses.ToList();
                context.Addresses.RemoveRange(addresses);

                var users = context.Users.ToList();
                context.Users.RemoveRange(users);
                context.SaveChanges();

                var address1 = new Address // 1 kurier1
                {
                    Id = 1,
                    Latitude = 54.3542418,
                    Longitude = 18.5136349312495,
                    Country = "Poland",
                    City = "Gdansk",
                    Street = "Rumiankowa",
                    HouseNumber = "4"
                };
                context.Addresses.Add(address1);

                var address2 = new Address // 2 kurier2
                {
                    Id = 2,
                    Latitude = 54.0929346,
                    Longitude = 18.7787318,
                    Country = "Poland",
                    City = "Tczew",
                    Street = "Aleja Solidarnosci",
                    HouseNumber = "6"
                };
                context.Addresses.Add(address2);

                var address3 = new Address // 3 nadawca1
                {
                    Id = 3,
                    Latitude = 54.353753,
                    Longitude = 18.6467207,
                    Country = "Poland",
                    City = "Gdansk",
                    Street = "Elzbietanska",
                    HouseNumber = "4/8"
                };
                context.Addresses.Add(address3);

                var address4 = new Address // 4 nadawca2
                {
                    Id = 4,
                    Latitude = 54.27103735,
                    Longitude = 18.6092722435826,
                    Country = "Poland",
                    City = "Rotmanka",
                    Street = "Sosnowa",
                    HouseNumber = "49"
                };
                context.Addresses.Add(address4);

                var address5 = new Address // 5 nadawca3
                {
                    Id = 5,
                    Latitude = 54.07801,
                    Longitude = 18.79381,
                    Country = "Poland",
                    City = "Tczew",
                    Street = "Starowiejska",
                    HouseNumber = "15"
                };
                context.Addresses.Add(address5);

                var address6 = new Address // 6 nadawca4
                {
                    Id = 6,
                    Latitude = 54.5390846,
                    Longitude = 18.4666061,
                    Country = "Poland",
                    City = "Gdynia",
                    Street = "Rozewska",
                    HouseNumber = "14"
                };
                context.Addresses.Add(address6);

                var address7 = new Address // 7 nadawca5
                {
                    Id = 7,
                    Latitude = 54.26176545,
                    Longitude = 18.6339619710675,
                    Country = "Poland",
                    City = "Pruszcz Gdanski",
                    Street = "Obroncow Pokoju",
                    HouseNumber = "2"
                };
                context.Addresses.Add(address7);

                var address8 = new Address // 8 nadawca6
                {
                    Id = 8,
                    Latitude = 54.2620083,
                    Longitude = 18.6331964180257,
                    Country = "Poland",
                    City = "Pruszcz Gdanski",
                    Street = "Obroncow Pokoju",
                    HouseNumber = "4"
                };
                context.Addresses.Add(address8);

                var address9 = new Address // 9 odbiorca1
                {
                    Id = 9,
                    Latitude = 54.5417356,
                    Longitude = 18.4996978,
                    Country = "Poland",
                    City = "Gdynia",
                    Street = "Golebia",
                    HouseNumber = "1"
                };
                context.Addresses.Add(address9);

                var address10 = new Address // 10 odbiorca2 
                {
                    Id = 10,
                    Latitude = 54.03468185,
                    Longitude = 19.0259193864728,
                    Country = "Poland",
                    City = "Malbork",
                    Street = "Plac Slowianski",
                    HouseNumber = "18"
                };
                context.Addresses.Add(address10);

                var address11 = new Address // 11 odbiorca3
                {
                    Id = 11,
                    Latitude = 53.9291725,
                    Longitude = 18.6969304,
                    Country = "Poland",
                    City = "Pelplin",
                    Street = "Sambora",
                    HouseNumber = "4A"
                };
                context.Addresses.Add(address11);

                var address12 = new Address // 12 odbiorca4
                {
                    Id = 12,
                    Latitude = 54.53575005,
                    Longitude = 17.7481330361574,
                    Country = "Poland",
                    City = "Lebork",
                    Street = "Bohaterow Monte Cassino",
                    HouseNumber = "9"
                };
                context.Addresses.Add(address12);

                var address13 = new Address // 13 odbiorca5
                {
                    Id = 13,
                    Latitude = 54.51946525,
                    Longitude = 18.5364458224583,
                    Country = "Poland",
                    City = "Gdynia",
                    Street = "10 Lutego",
                    HouseNumber = "29"
                };
                context.Addresses.Add(address13);

                var address14 = new Address // 14 odbiorca6
                {
                    Id = 14,
                    Latitude = 54.3550759,
                    Longitude = 18.6464568,
                    Country = "Poland",
                    City = "Gdansk",
                    Street = "Karmelicka",
                    HouseNumber = "1"
                };
                context.Addresses.Add(address14);

                var address15 = new Address // 15 odbiorca7
                {
                    Id = 15,
                    Latitude = 52.408785,
                    Longitude = 16.9317439174727,
                    Country = "Poland",
                    City = "Poznan",
                    Street = "Gora Przemysla",
                    HouseNumber = "4"
                };
                context.Addresses.Add(address15);


                var address16 = new Address // 15 kurier3
                {
                    Id = 16,
                    Latitude = 51.755922,
                    Longitude = 19.471076,
                    Country = "Poland",
                    City = "Lodz",
                    Street = "Jana Kilinskiego",
                    HouseNumber = "168"
                };
                context.Addresses.Add(address16);



                var courier1 = new User
                {
                    Id = 1,
                    Type = User.UserType.Courier,
                    Name = "KurierZGdanska",
                    Address = address1
                };
                context.Users.Add(courier1);

                var courier2 = new User
                {
                    Id = 2,
                    Type = User.UserType.Courier,
                    Name = "KurierZTczewa",
                    Address = address2
                };
                context.Users.Add(courier2);

                var courier3 = new User
                {
                    Id = 16,
                    Type = User.UserType.Courier,
                    Name = "KurierZLodzi",
                    Address = address16
                };
                context.Users.Add(courier3);



                var sender1 = new User // nadawca 1
                {
                    Id = 3,
                    Address = address3
                };
                context.Users.Add(sender1);

                var sender2 = new User // nadawca 2
                {
                    Id = 4,
                    Address = address4
                };
                context.Users.Add(sender2);

                var sender3 = new User // nadawca 3
                {
                    Id = 5,
                    Address = address5
                };
                context.Users.Add(sender3);

                var sender4 = new User // nadawca 4
                {
                    Id = 6,
                    Address = address6
                };
                context.Users.Add(sender4);

                var sender5 = new User // nadawca 5
                {
                    Id = 7,
                    Address = address7
                };
                context.Users.Add(sender5);

                var sender6 = new User // nadawca 6
                {
                    Id = 8,
                    Address = address8
                };
                context.Users.Add(sender6);

                var receiver1 = new User // odbiorca 1
                {
                    Id = 9,
                    Address = address9
                };
                context.Users.Add(receiver1);

                var receiver2 = new User // odbiorca 2
                {
                    Id = 10,
                    Address = address10
                };
                context.Users.Add(receiver2);

                var receiver3 = new User // odbiorca 3
                {
                    Id = 11,
                    Address = address11
                };
                context.Users.Add(receiver3);

                var receiver4 = new User // odbiorca 4
                {
                    Id = 12,
                    Address = address12
                };
                context.Users.Add(receiver4);

                var receiver5 = new User // odbiorca 5
                {
                    Id = 13,
                    Address = address13
                };
                context.Users.Add(receiver5);

                var receiver6 = new User // odbiorca 6
                {
                    Id = 14,
                    Address = address14
                };
                context.Users.Add(receiver6);

                var receiver7 = new User // odbiorca 7
                {
                    Id = 15,
                    Address = address15
                };
                context.Users.Add(receiver7);



                var courierVehicleGdansk = new Vehicle // pojazd kuriera z gdanska
                {
                    Id = 1,
                    CurrentLoad = 0,
                    LoadCapacity = 500,
                    AverageVelocity = 50000,
                    Driver = courier1,
                    RegistrationNumber = "GDN",
                    CourierPackingList = new List<Package>()
                };
                context.Vehicles.Add(courierVehicleGdansk);

                var courierVehicleTczew = new Vehicle // pojazd kuriera z tczewa
                {
                    Id = 2,
                    CurrentLoad = 0,
                    LoadCapacity = 500,
                    AverageVelocity = 50000,
                    Driver = courier2,
                    RegistrationNumber = "TCZW",
                    CourierPackingList = new List<Package>()
                };
                context.Vehicles.Add(courierVehicleTczew);

                var courierVehicleLodz = new Vehicle // pojazd kuriera z lodzi
                {
                    Id = 3,
                    CurrentLoad = 0,
                    LoadCapacity = 500,
                    AverageVelocity = 50000,
                    Driver = courier3,
                    RegistrationNumber = "LDZ",
                    CourierPackingList = new List<Package>()
                };
                context.Vehicles.Add(courierVehicleLodz);



                var package1 = new Package // paczka 1
                {
                    Id = 1,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender1,
                    Receiver = receiver1,
                    Number = new Guid("3f295076-108b-46e0-8052-8149290823d8")
                };
                context.Packages.Add(package1);

                var package2 = new Package // paczka 2
                {
                    Id = 2,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender2,
                    Receiver = receiver2,
                    Number = new Guid("b89ff9a8-93a2-46a0-8d80-d03251eec969")
                };
                context.Packages.Add(package2);

                var package3 = new Package // paczka 3
                {
                    Id = 3,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender3,
                    Receiver = receiver3,
                    Number = new Guid("08c26f3b-ad1a-42dd-8788-8fbf848cbdf5")
                };
                context.Packages.Add(package3);

                var package4 = new Package // paczka 4
                {
                    Id = 4,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender4,
                    Receiver = receiver4,
                    Number = new Guid("049b1f1e-aa83-403a-98c5-2931cd3b2554")
                };
                context.Packages.Add(package4);

                var package5 = new Package // paczka 5
                {
                    Id = 5,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender5,
                    Receiver = receiver5,
                    Number = new Guid("c5691f03-04f1-4e2b-ad92-fd5d5052b284")
                };
                context.Packages.Add(package5);

                var package6 = new Package // paczka 6 
                {
                    Id = 6,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender6,
                    Receiver = receiver6,
                    Number = new Guid("078ead52-8d1a-4016-ae20-b029bb592baf")
                };
                context.Packages.Add(package6);

                var package7 = new Package // paczka 7
                {
                    Id = 7,
                    Priority = 0,
                    State = 0,
                    Weight = Package.PackageWeight.Large,
                    Sender = sender3,
                    Receiver = receiver7,
                    Number = new Guid("8a7f2222-51ab-417b-933c-eb9abb4bf204")
                };
                context.Packages.Add(package7);

                context.SaveChanges();
            }
        }



        [Test]
        public void ChoosePackagesForPackingList_TwoPackagesOneCourier_BothPackagesAdded()
        {
            var packages = new List<Package>();
            var vehicles = new List<Vehicle>();

            using (var context = _contextFactoryMethod())
            {
                var packagesToRemove = context.Packages
                    .AsQueryable()
                    .Where(x => x.Id >= 3)
                    .ToList();

                context.Packages.RemoveRange(packagesToRemove);
                context.SaveChanges();

                var vehiclesToRemove = context.Vehicles
                    .AsQueryable()
                    .Where(x => x.Id >= 2)
                    .ToList();

                context.Vehicles.RemoveRange(vehiclesToRemove);
                context.SaveChanges();

                packages = context.Packages
                    .Include(x => x.Receiver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.Sender)
                      .ThenInclude(x => x.Address)
                    .ToList();

                vehicles = context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                    .ToList();
            }

            var sut = new PackingListService(
                new VehicleService(_contextFactoryMethod),
                new PackageService(_contextFactoryMethod, new Mock<ITimeCalculator>().Object),
                new Mock<IUserService>().Object,
                new Mock<ITimeCalculator>().Object,
                new Mock<Func<IUPSikDbContext>>().Object,
                new PackageDeliveryConfigValues(),
                new GeoMapService(),
                new ShippingPlannerService(_contextFactoryMethod, new GeoMapService()));


            sut.AddPackagesToPackingList(packages, vehicles);


            packages[0].State.Should().Be(1);
            packages[1].State.Should().Be(1);
        }

        [Test]
        public void ChoosePackagesForPackingList_FourPackagesOneCourier_ThreePackagesAddedFourthSetOnHighPriority()
        {
            var packages = new List<Package>();
            var vehicles = new List<Vehicle>();

            using (var context = _contextFactoryMethod())
            {
                var packagesToRemove = context.Packages
                    .AsQueryable()
                    .Where(x => x.Id >= 5)
                    .ToList();

                context.Packages.RemoveRange(packagesToRemove);
                context.SaveChanges();

                var vehiclesToRemove = context.Vehicles
                    .AsQueryable()
                    .Where(x => x.Id >= 2)
                    .ToList();

                context.Vehicles.RemoveRange(vehiclesToRemove);
                context.SaveChanges();

                packages = context.Packages
                    .Include(x => x.Receiver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.Sender)
                      .ThenInclude(x => x.Address)
                    .ToList();

                vehicles = context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                    .ToList();
            }


            var sut = new PackingListService(
                new VehicleService(_contextFactoryMethod),
                new PackageService(_contextFactoryMethod, new Mock<ITimeCalculator>().Object),
                new Mock<IUserService>().Object,
                new Mock<ITimeCalculator>().Object,
                new Mock<Func<IUPSikDbContext>>().Object,
                new PackageDeliveryConfigValues(),
                new GeoMapService(),
                new ShippingPlannerService(_contextFactoryMethod, new GeoMapService()));

            sut.AddPackagesToPackingList(packages, vehicles);

            packages[0].State.Should().Be(1);
            packages[1].State.Should().Be(1);
            packages[2].State.Should().Be(1);
            packages[3].Priority.Should().Be(1);
        }

        [Test]
        public void ChoosePackagesForPackingList_FourPackagesTwoCouriers_AllPackagesAssignedToNearestCouriers()
        {
            var packages = new List<Package>();
            var vehicles = new List<Vehicle>();

            using (var context = _contextFactoryMethod())
            {
                var packagesToRemove = context.Packages
                    .AsQueryable()
                    .Where(x => x.Id >= 5)
                    .ToList();

                context.Packages.RemoveRange(packagesToRemove);
                context.SaveChanges();

                var vehiclesToRemove = context.Vehicles
                    .AsQueryable()
                    .Where(x => x.Id >= 3)
                    .ToList();

                context.Vehicles.RemoveRange(vehiclesToRemove);
                context.SaveChanges();

                var packagesToSort = context.Packages
                    .Include(x => x.Receiver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.Sender)
                      .ThenInclude(x => x.Address)
                    .ToList();

                var vehiclesToSort = context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                    .ToList();


                packages = packagesToSort.OrderBy(x => x.Id).ToList();
                vehicles = vehiclesToSort.OrderBy(x => x.Id).ToList();
            }


            var sut = new PackingListService(
                new VehicleService(_contextFactoryMethod),
                new PackageService(_contextFactoryMethod, new Mock<ITimeCalculator>().Object),
                new Mock<IUserService>().Object,
                new Mock<ITimeCalculator>().Object,
                new Mock<Func<IUPSikDbContext>>().Object,
                new PackageDeliveryConfigValues(),
                new GeoMapService(),
                new ShippingPlannerService(_contextFactoryMethod, new GeoMapService()));

            sut.AddPackagesToPackingList(packages, vehicles);

            packages[0].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[0]);
            packages[1].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[1]);

            packages[2].State.Should().Be(1);
            vehicles[1].CourierPackingList.Should().Contain(packages[2]);
            packages[3].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[3]);
        }

        [Test]
        public void ChoosePackagesForPackingList_FourPackagesTwoCouriersSecondOneFarAway_ThreePackagesAssignedToFirstCourierFourthPutOnHighPriorityAndNotGivenToTheSecondCourier()
        {
            var packages = new List<Package>();
            var vehicles = new List<Vehicle>();

            using (var context = _contextFactoryMethod())
            {
                var packagesToRemove = context.Packages
                    .AsQueryable()
                    .Where(x => x.Id >= 5)
                    .ToList();

                context.Packages.RemoveRange(packagesToRemove);
                context.SaveChanges();

                var vehiclesToRemove = context.Vehicles
                    .AsQueryable()
                    .Where(x => x.Id == 2 || x.Id >= 4)
                    .ToList();

                context.Vehicles.RemoveRange(vehiclesToRemove);
                context.SaveChanges();

                var packagesToSort = context.Packages
                    .Include(x => x.Receiver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.Sender)
                      .ThenInclude(x => x.Address)
                    .ToList();

                var vehiclesToSort = context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                    .ToList();


                packages = packagesToSort.OrderBy(x => x.Id).ToList();
                vehicles = vehiclesToSort.OrderBy(x => x.Id).ToList();
            }


            var sut = new PackingListService(
                new VehicleService(_contextFactoryMethod),
                new PackageService(_contextFactoryMethod, new Mock<ITimeCalculator>().Object),
                new Mock<IUserService>().Object,
                new Mock<ITimeCalculator>().Object,
                new Mock<Func<IUPSikDbContext>>().Object,
                new PackageDeliveryConfigValues(),
                new GeoMapService(),
                new ShippingPlannerService(_contextFactoryMethod, new GeoMapService()));

            sut.AddPackagesToPackingList(packages, vehicles);

            packages[0].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[0]);
            packages[1].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[1]);
            packages[2].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[2]);

            packages[3].State.Should().Be(0);
            packages[3].Priority.Should().Be(1);
            vehicles[1].CourierPackingList.Should().NotContain(packages[3]);
        }

        [Test]
        public void ChoosePackagesForPackingList_SevenPackagesTwoCouriers_SixPackagesAssignedToNearestCouriersSeventhPackagePutOnHighPriority()
        {
            var packages = new List<Package>();
            var vehicles = new List<Vehicle>();

            using (var context = _contextFactoryMethod())
            {
                var packagesToRemove = context.Packages
                    .AsQueryable()
                    .Where(x => x.Id >= 8)
                    .ToList();

                context.Packages.RemoveRange(packagesToRemove);
                context.SaveChanges();

                var vehiclesToRemove = context.Vehicles
                    .AsQueryable()
                    .Where(x => x.Id >= 3)
                    .ToList();

                context.Vehicles.RemoveRange(vehiclesToRemove);
                context.SaveChanges();

                var packagesToSort = context.Packages
                    .Include(x => x.Receiver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.Sender)
                      .ThenInclude(x => x.Address)
                    .ToList();

                var vehiclesToSort = context.Vehicles
                    .Include(x => x.Driver)
                      .ThenInclude(x => x.Address)
                    .Include(x => x.CourierPackingList)
                    .ToList();


                packages = packagesToSort.OrderBy(x => x.Id).ToList();
                vehicles = vehiclesToSort.OrderBy(x => x.Id).ToList();
            }

            var sut = new PackingListService(
                new VehicleService(_contextFactoryMethod),
                new PackageService(_contextFactoryMethod, new Mock<ITimeCalculator>().Object),
                new Mock<IUserService>().Object,
                new Mock<ITimeCalculator>().Object,
                new Mock<Func<IUPSikDbContext>>().Object,
                new PackageDeliveryConfigValues(),
                new GeoMapService(),
                new ShippingPlannerService(_contextFactoryMethod, new GeoMapService()));

            sut.AddPackagesToPackingList(packages, vehicles);


            packages[0].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[0]);
            packages[1].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[1]);
            packages[2].State.Should().Be(1);
            vehicles[0].CourierPackingList.Should().Contain(packages[3]);

            packages[3].State.Should().Be(1);
            vehicles[1].CourierPackingList.Should().Contain(packages[2]);
            packages[4].State.Should().Be(0);
            packages[4].Priority.Should().Be(1);
            vehicles[1].CourierPackingList.Should().NotContain(packages[4]);
            packages[5].State.Should().Be(0);
            packages[5].Priority.Should().Be(1);
            vehicles[1].CourierPackingList.Should().NotContain(packages[5]);

            packages[6].State.Should().Be(1);
            vehicles[1].CourierPackingList.Should().Contain(packages[6]);
        }
    }
}
