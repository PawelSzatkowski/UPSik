using System;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;
using Unity;
using UPSik.BusinessLayer.TimeWarper;

namespace UPSik
{
    class Program
    {
        private readonly IMenu _mainMenu;
        private readonly IIoHelper _ioHelper;
        private readonly IUserService _userService;
        private readonly IDatabaseManagementService _databaseManagementService;
        private readonly IPackageService _packageService;
        private readonly IVehicleService _vehicleService;
        private readonly ITimersHandler _timersHandler;
        private readonly ITimeCalculator _timeCalculator;

        private readonly IPackingListService _packingListService;

        static void Main(string[] args)
        {
            var container = new UnityDiContainerProvider().GetContainer();

            container.Resolve<Program>().Run();
        }

        public Program(
            IMenu mainMenu,
            IIoHelper ioHelper,
            IUserService userService,
            IDatabaseManagementService databaseManagementService,
            IPackageService packageService,
            IVehicleService vehicleService,
            ITimersHandler timeHandler,
            ITimeCalculator timeCalculator,
            
            IPackingListService packingListService)
        {
            _mainMenu = mainMenu;
            _ioHelper = ioHelper;
            _userService = userService;
            _databaseManagementService = databaseManagementService;
            _packageService = packageService;
            _vehicleService = vehicleService;
            _timersHandler = timeHandler;
            _timeCalculator = timeCalculator;

            _packingListService = packingListService;
        }


        void Run()
        {
            _timersHandler.SetTimers();

            _databaseManagementService.EnsureDatabaseMigration();
            RegisterMainMenuOptions();

            Console.WriteLine("Welcome to UPSik app - the best, and only app for the best (and only) shipping company in the world - UPSik!");

            int userChoice;

            do
            {
                Console.WriteLine();

                _mainMenu.PrintAvailableOptions();

                userChoice = _ioHelper.GetIntFromUser("\n" + "Select option");
                _mainMenu.ExecuteOption(userChoice);
            }
            while (true);
        }

        private void RegisterMainMenuOptions()
        {
            _mainMenu.AddOption(new MenuItem { Key = 1, Action = AddNewUser, Description = "Create account" });
            _mainMenu.AddOption(new MenuItem { Key = 2, Action = AddNewPackageToSystem, Description = "Add new package to system" });
            _mainMenu.AddOption(new MenuItem { Key = 3, Action = AddNewCourierVehicle, Description = "Add new vehicle to system" });
            _mainMenu.AddOption(new MenuItem { Key = 4, Action = ExitApp, Description = "Exit app" });
            _mainMenu.AddOption(new MenuItem { Key = 5, Action = GetCurrentDate, Description = "Get current date" });
            _mainMenu.AddOption(new MenuItem { Key = 6, Action = ShowSpecificCourierRatings, Description = "Show specific courier ratings" });

            _mainMenu.AddOption(new MenuItem { Key = 6, Action = GeneratePackingList, Description = "paking list" });
        }

        private void GeneratePackingList()
        {
            _packingListService.GeneratePackingLists();
        }

        private void GetCurrentDate()
        {
            Console.WriteLine($"Current real time: {_timeCalculator.GetCurrentRealTime()}");
            Console.WriteLine($"Current warped time: {_timeCalculator.GetCurrentWarpedTime()}");
        }

        private void AddNewUser()
        {
            Console.WriteLine("\n" + "Creating a new user's account");

            var newUserAddress = _ioHelper.GetAddressFromUser("Enter user's address");

            if (newUserAddress == null)
            {
                return;
            }

            var newUserType = _ioHelper.GetUserTypeFromUser("Select user type (Courier/Customer)");

            if (!_userService.CheckIfAddressIsInPickUpRangeOfAnyCourier(newUserAddress) && newUserType == User.UserType.Customer)
            {
                Console.WriteLine("\n" + "Sorry! That's too far from our regular (50km range for each courier) area of operations! Please call us, so we can work this out!" + "\n");
                return;
            }

            var newUser = new User
            {
                Name = _ioHelper.GetTextFromUser("Enter user's name"),
                Surname = _ioHelper.GetTextFromUser("Enter user's surname"),
                Email = _ioHelper.GetEmailFromUser("Enter users's e-mail"),
                Address = newUserAddress,
                Type = newUserType,
            };

            if (newUser.Type == User.UserType.Courier)
            {
                newUser.Password = _ioHelper.GetTextFromUser("Enter user's password");
            }

            _userService.AddNewUserAsync(newUser).Wait();

            Console.WriteLine("\n" + "User added!");
        }

        private void AddNewPackageToSystem()
        {
            if (!_userService.CheckIfThereIsAtLeastOnePossibleSender())
            {
                Console.WriteLine("\n" + "Database empty - there are no possible senders!" + "\n");
                return;
            }

            if (!_vehicleService.CheckIfThereIsAtLeasOneCourierVehicle())
            {
                Console.WriteLine("\n" + "There are no courier vehicles added to the system!" + "\n");
                return;
            }

            Console.WriteLine("\n" + "Adding a new package to the system");

            var newPackage = new Package();
            var senderEmail = _ioHelper.GetTextFromUser("Enter sender's email");

            if (!_userService.CheckIfSenderIsInDatabase(senderEmail))
            {
                Console.WriteLine("Sender is not in database!");
                return;
            }
            newPackage.Sender = _userService.GetSenderByEmail(senderEmail);
            newPackage.Receiver = _ioHelper.GetPackageReceiver(newPackage.Sender.Address);

            if (newPackage.Receiver == null)
            {
                return;
            }

            newPackage.Weight = _ioHelper.GetPackageWeight();

            _packageService.AddNewPackageAsync(newPackage).Wait();

            Console.WriteLine("\n" + "Package added!");
        }

        private void AddNewCourierVehicle()
        {
            if (!_userService.CheckIfThereIsAtLeastOnePossibleDriver())
            {
                Console.WriteLine("\n" + "Database empty - there are no possible drivers!" + "\n");
                return;
            }

            Console.WriteLine("\n" + "Adding a new vehicle to the system" + "\n");

            var driver = _ioHelper.GetVehicleDriver();

            if (!_userService.CheckIfCourierIsInDatabase(driver))
            {
                Console.WriteLine("This courier is not in database!");
                return;
            }

            if (_vehicleService.CheckIfCourierAlreadyHaveACar(driver))
            {
                Console.WriteLine("This courier already have a car!");
                return;
            }

            var newVehicle = new Vehicle
            {
                Brand = _ioHelper.GetTextFromUser("Enter vehicle's brand"),
                Model = _ioHelper.GetTextFromUser("Enter vehicle's model"),
                RegistrationNumber = _ioHelper.GetRegistrationNumberFromUser(),
                LoadCapacity = _ioHelper.GetIntFromUser("Enter vehicle's load capacity"),
                AverageVelocity = _ioHelper.GetIntFromUser("Enter vehicle's average velocity (in kilometers per hour)"),
                Driver = driver
            };

            int metersInKilometer = 1000;

            newVehicle.AverageVelocity = newVehicle.AverageVelocity * metersInKilometer;

            if (_vehicleService.AddNewVehicleAsync(newVehicle).Result)
            {
                Console.WriteLine("\n" + "Vehicle added!");
            }
            else
            {
                Console.WriteLine("\n" + "There has been an error while trying to add a vehicle");
            }
        }

        private void ShowSpecificCourierRatings()
        {
            _ioHelper.PrintCouriers();

            int userChoice = _ioHelper.GetIntFromUser("Select courier id");

            var packingListsInfos = _packingListService.GetCourierPackingListInfos(userChoice);

            int i = 1;

            foreach (var packingListInfo in packingListsInfos)
            {
                Console.WriteLine("\n" + $"{i}. {packingListInfo.FileName} Courier rating: {packingListInfo.CourierRating}");
                i++;
            }
        }

        private void ExitApp()
        {
            Environment.Exit(0);
        }

    }
}
