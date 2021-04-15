using System;
using System.Text.RegularExpressions;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;

namespace UPSik
{
    public interface IIoHelper
    {
        Address GetAddressFromUser(string message);
        string GetEmailFromUser(string message);
        int GetIntFromUser(string message);
        User GetPackageReceiver(Address senderAddress);
        Package.PackageWeight GetPackageWeight();
        string GetRegistrationNumberFromUser();
        string GetTextFromUser(string message);
        User.UserType GetUserTypeFromUser(string message);
        User GetVehicleDriver();
        bool CheckEmailFormat(string email);
        void PrintCouriers();
    }

    public class IoHelper : IIoHelper
    {
        private readonly IUserService _userService;
        private readonly IGeoMapService _geoMapService;
        private readonly IShippingPlannerService _shippingPlannerService;

        public IoHelper(
            IUserService userService,
            IVehicleService vehicleService,
            IGeoMapService geoMapService,
            IShippingPlannerService shippingPlannerService)
        {
            _userService = userService;
            _geoMapService = geoMapService;
            _shippingPlannerService = shippingPlannerService;
        }

        public int GetIntFromUser(string message)
        {
            int number;

            while (!int.TryParse(GetTextFromUser(message), out number))
            {
                Console.WriteLine("Format not correct, try again.");
            }

            return number;
        }

        public string GetTextFromUser(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }

        public string GetEmailFromUser(string message)
        {
            var email = GetTextFromUser(message);
            bool isValid;

            do
            {
                isValid = true;

                if (!CheckEmailFormat(email))
                {
                    Console.WriteLine("Incorrect e-mail format!");
                    email = GetTextFromUser(message);

                    isValid = false;
                }

                if (_userService.CheckIfEmailExist(email) == true)
                {
                    Console.WriteLine("This e-mail already exists!");
                    email = GetTextFromUser(message);

                    isValid = false;
                }
            }
            while (!isValid);

            return email;
        }

        public bool CheckEmailFormat(string email)
        {
            string correctEmailRegex = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            if (Regex.IsMatch(email, correctEmailRegex))
            {
                return true;
            }
            return false;
        }

        public Address GetAddressFromUser(string message)
        {
            var newAdress = new Address()
            {
                Street = GetTextFromUser("Enter street"),
                HouseNumber = GetTextFromUser("Enter house number"),
                City = GetTextFromUser("Enter city"),
                PostCode = GetTextFromUser("Enter postcode"),
                Country = GetTextFromUser("Enter country")
            };

            var geoCoords = _geoMapService.GetCoordinatesForAddress(newAdress.Country, newAdress.City, newAdress.Street, newAdress.HouseNumber);

            if (geoCoords == null)
            {
                Console.WriteLine("The address doesn't exist!");
                return null;
            }

            newAdress.Latitude = geoCoords.Latitude;
            newAdress.Longitude = geoCoords.Longitude;

            return newAdress;
        }

        public User.UserType GetUserTypeFromUser(string message)
        {
            var userType = GetTextFromUser(message);

            while (userType != User.UserType.Customer.ToString() && userType != User.UserType.Courier.ToString())
            {
                Console.WriteLine("Option invalid!");
                userType = GetTextFromUser(message);
            }

            if (userType == User.UserType.Courier.ToString())
            {
                return User.UserType.Courier;
            }

            else
            {
                return User.UserType.Customer;
            }
        }

        public User GetPackageReceiver(Address senderAddress)
        {
            Console.WriteLine("\n" + "Enter receiver's addres:");

            var receiverAddress = GetAddressFromUser(null);

            if (receiverAddress == null)
            {
                return null;
            }

            if (!_shippingPlannerService.CheckIfPackageCanBeDeliveredAtAll(receiverAddress, senderAddress))
            {
                Console.WriteLine("\n" + "The delivery address is out of range for any of our couriers, sorry!" + "\n");
                return null;
            }

            return new User()
            {
                Name = GetTextFromUser("Enter receiver's name"),
                Surname = GetTextFromUser("Enter receiver's surname"),
                Address = receiverAddress,
                Email = GetEmailFromUser("Enter receiver's email"),
            };

        }

        public Package.PackageWeight GetPackageWeight()
        {
            do
            {
                var userChoice = GetTextFromUser("Select package weight - small (S), medium (M) or large (L)");

                if (userChoice.ToUpper() == "S")
                {
                    return Package.PackageWeight.Small;
                }

                if (userChoice.ToUpper() == "M")
                {
                    return Package.PackageWeight.Medium;
                }

                if (userChoice.ToUpper() == "L")
                {
                    return Package.PackageWeight.Large;
                }

                else
                {
                    Console.WriteLine("Incorrect input!");
                }
            }
            while (true);
        }

        public User GetVehicleDriver()
        {
            return new User()
            {
                Email = GetTextFromUser("Enter driver's email")
            };
        }

        public string GetRegistrationNumberFromUser()
        {
            string registrationNumber;
            do
            {
                registrationNumber = GetTextFromUser("Enter vehicle's registration number (8 characters max)");
            }
            while (registrationNumber.Length > 8);

            return registrationNumber;
        }

        public void PrintCouriers()
        {
            var couriers = _userService.GetCouriersList();

            foreach (var courier in couriers)
            {
                Console.WriteLine("\n" + $"{courier.Id} {courier.Name} {courier.Surname}");
            }
        }
    }
}
