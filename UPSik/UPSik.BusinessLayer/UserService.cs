using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPSik.DataLayer;
using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface IUserService
    {
        Task AddNewUserAsync(User newUser);
        bool CheckIfAddressIsInPickUpRangeOfAnyCourier(Address address);
        bool CheckIfCourierIsInDatabase(User driver);
        bool CheckIfEmailExist(string emailToCheck);
        bool CheckIfSenderIsInDatabase(string emailToCheck);
        bool CheckIfThereIsAtLeastOnePossibleDriver();
        bool CheckIfThereIsAtLeastOnePossibleSender();
        List<User> GetCouriersList();
        User GetSenderByEmail(string senderEmail);
        Task<User> GetLoggedInUser(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly Func<IUPSikDbContext> _UPSikDbContextFactoryMethod;
        private readonly IGeoMapService _geoMapService;
        private readonly IPackageDeliveryConfigValues _packageDeliveryConfigValues;

        public UserService(
            Func<IUPSikDbContext> UPSikDbContextFactoryMethod,
            IGeoMapService geoMapService,
            IPackageDeliveryConfigValues packageDeliveryConfigValues)
        {
            _UPSikDbContextFactoryMethod = UPSikDbContextFactoryMethod;
            _geoMapService = geoMapService;
            _packageDeliveryConfigValues = packageDeliveryConfigValues;
        }

        public async Task AddNewUserAsync(User newUser)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                context.Users.Add(newUser);
                await context.SaveChangesAsync();
            }
        }

        public bool CheckIfEmailExist(string emailToCheck)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                var user = context.Users
                    .AsQueryable()
                    .FirstOrDefault(x => x.Email == emailToCheck);

                return user != null;
            }
        }

        public bool CheckIfSenderIsInDatabase(string emailToCheck)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                var sender = context.Users
                    .AsQueryable()
                    .FirstOrDefault(x => x.Email == emailToCheck && x.Type == User.UserType.Customer);

                return sender != null;
            }
        }

        public  bool CheckIfThereIsAtLeastOnePossibleSender()
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return context.Users
                    .AsQueryable()
                    .Any(x => x.Type == User.UserType.Customer);
            }
        }

        public bool CheckIfCourierIsInDatabase(User driver)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return context.Users.Any(x => x.Email == driver.Email && x.Type == User.UserType.Courier);
            }
        }

        public bool CheckIfThereIsAtLeastOnePossibleDriver()
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return context.Users.Any(x => x.Type == User.UserType.Courier);
            }
        }

        public bool CheckIfAddressIsInPickUpRangeOfAnyCourier(Address packageAddress)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                var couriersAdresses = context.Users
                    .Include(x => x.Address)
                    .Where(x => x.Type == User.UserType.Courier)
                    .Select(x => x.Address)
                    .ToList();

                foreach (var courierAdress in couriersAdresses)
                {
                    var distance = _geoMapService.CalculateDistanceBetweenTwoPoints(courierAdress.Latitude, courierAdress.Longitude, packageAddress.Latitude, packageAddress.Longitude);

                    if (distance < _packageDeliveryConfigValues.MaxDistanceToPickupPackage)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public List<User> GetCouriersList()
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return context.Users
                    .AsQueryable()
                    .Where(x => x.Type == User.UserType.Courier)
                    .Include(x => x.Address)
                    .ToList();
            }
        }

        public User GetSenderByEmail(string senderEmail)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return context.Users
                    .Include(x => x.Address)
                    .FirstOrDefault(x => x.Email == senderEmail);
            }
        }

        public async Task<User> GetLoggedInUser(string email, string password)
        {
            using (var context = _UPSikDbContextFactoryMethod())
            {
                return await context.Users
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Email == email && x.Password == password && x.Type == User.UserType.Courier);
            }
        }
    }
}
