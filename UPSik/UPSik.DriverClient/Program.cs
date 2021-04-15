using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UPSik.DriverClient.Models;
using Unity;
using UPSik.DriverClient.TimeWarper;
using UPSik.DriverClient.Services;
using System.Linq;

namespace UPSik.DriverClient
{
    class Program
    {
        private readonly IMenu _loggingMenu;
        private readonly IMenu _courierMenu;
        private readonly IIoHelper _ioHelper;
        private readonly IMenuOptions _menuOptions;
        private readonly ITimeCalculator _timeCalculator;
        private readonly IConnectionService _connectionService;
        private readonly IPackingListService _packingListService;
        private readonly IVehicleService _vehicleService;
        private readonly IRoutePlannerService _routePlannerService;

        private DateTime _logInDate;
        private int _courierVehicleAvgSpeed;
        private int _loggedCourierId;
        private bool _loggedIn = false;
        private List<Package> _currentPackingList;
        private DateTime _startWorkDay;
        private bool _packingListManuallyManaged = false;

        static void Main(string[] args)
        {
            var container = new DiContainerProvider().GetContainer();

            container.Resolve<Program>().Run();
        }

        public Program(
            IMenu loggingMenu,
            IMenu courierMenu,
            IIoHelper ioHelper,
            IMenuOptions menuOptions,
            ITimeCalculator timeCalculator,
            IConnectionService connectionService,
            IPackingListService packingListService,
            IVehicleService vehicleService,
            IRoutePlannerService routePlannerService)
        {
            _loggingMenu = loggingMenu;
            _courierMenu = courierMenu;
            _ioHelper = ioHelper;
            _menuOptions = menuOptions;
            _timeCalculator = timeCalculator;
            _connectionService = connectionService;
            _packingListService = packingListService;
            _vehicleService = vehicleService;
            _routePlannerService = routePlannerService;
        }

        private void Run()
        {
            RegisterLoggingMenuOptions();
            RegisterUserMenuOptions();

            Console.WriteLine("Welcome, our dear worker!" + "\n");

            int userChoice;

            do
            {
                Console.WriteLine();

                _loggingMenu.PrintAvailableOptions();

                userChoice = _ioHelper.GetIntFromUser("\n" + "Select option");
                _loggingMenu.ExecuteOption(userChoice);

                while (_loggedIn == true)
                {
                    ApplyCourierDataUpdate();

                    Console.WriteLine();

                    _courierMenu.PrintAvailableOptions();

                    userChoice = _ioHelper.GetIntFromUser("\n" + "Select option");
                    _courierMenu.ExecuteOption(userChoice);
                }
            }
            while (true);
        }

        private void RegisterLoggingMenuOptions()
        {
            _loggingMenu.AddOption(new MenuItem { Key = 1, Action = LogIn, Description = "Log in" });
            _loggingMenu.AddOption(new MenuItem { Key = 2, Action = ExitApp, Description = "Exit app" });
        }

        private void RegisterUserMenuOptions()
        {
            _courierMenu.AddOption(new MenuItem { Key = 1, Action = ShowLatestPackingList, Description = "Show your latest packing list" });
            _courierMenu.AddOption(new MenuItem { Key = 2, Action = SwitchPackingListToManual, Description = "Switch your current to-do packing list to manual mode" });
            _courierMenu.AddOption(new MenuItem { Key = 3, Action = ShowShippingRoute, Description = "Show shipping route" });
            _courierMenu.AddOption(new MenuItem { Key = 4, Action = MarkPackageAsDeliverd, Description = "Mark package as delivered" });
            _courierMenu.AddOption(new MenuItem { Key = 5, Action = ShowCourierRating, Description = "Show courier rating" });
            _courierMenu.AddOption(new MenuItem { Key = 6, Action = FinishWork, Description = "Finish work for today" });
            _courierMenu.AddOption(new MenuItem { Key = 8, Action = GetCurrentDate, Description = "Get current date" });
            _courierMenu.AddOption(new MenuItem { Key = 9, Action = LogOut, Description = "Log out" });
            _courierMenu.AddOption(new MenuItem { Key = 0, Action = ExitApp, Description = "Exit app" });
        }

        private void LogIn()
        {
            var loggedCourierId = _menuOptions.LogInUser();

            if (loggedCourierId != 0)
            {
                _loggedCourierId = loggedCourierId;
                _loggedIn = true;

                CourierSetup();

                _logInDate = _timeCalculator.GetCurrentWarpedTime();
            }
        }

        private void ShowLatestPackingList()
        {
            if (_currentPackingList.Count == 0)
            {
                _ioHelper.EmptyPackingListNote();
                return;
            }

            if (_currentPackingList != null)
            {
                _ioHelper.PrintPackingList(_currentPackingList);
            }
        }

        private void SwitchPackingListToManual()
        {
            if (_currentPackingList.Count == 0)
            {
                _ioHelper.EmptyPackingListNote();
                return;
            }

            if (_packingListManuallyManaged)
            {
                Console.WriteLine("Your packing list is already manually managed");
                return;
            }

            if (_menuOptions.SwitchPackingListToManual(_loggedCourierId, _currentPackingList))
            {
                _packingListManuallyManaged = true;
            }
        }

        private void ShowShippingRoute()
        {
            if (_currentPackingList.Count == 0)
            {
                _ioHelper.EmptyPackingListNote();
                return;
            }

            _ioHelper.PrintShippingRoute(_currentPackingList);
        }

        private void MarkPackageAsDeliverd()
        {
            if (_currentPackingList.Count == 0)
            {
                _ioHelper.EmptyPackingListNote();
                return;
            }

            if (_packingListManuallyManaged == false)
            {
                Console.WriteLine("\n" + "Your packing list is still in automatic management - proceeding to switch it to manual management");

                SwitchPackingListToManual();
            }

            ShowLatestPackingList();

            _menuOptions.MarkPackageAsDelivered(_currentPackingList);
        }

        private void ShowCourierRating()
        {
            _menuOptions.ShowCourierRating(_loggedCourierId);
        }

        private void FinishWork()
        {
            Console.WriteLine("Are you sure to finish your work now?");
            var choice = Console.ReadLine();

            if (choice.ToUpper() == "Y")
            {
                _menuOptions.FinishWork(_loggedCourierId);
            }
            if (choice.ToUpper() == "N")
            {
                return;
            }
            else
            {
                Console.WriteLine("Error, wrong option");
                return;
            }
        }

        private void GetCurrentDate()
        {
            Console.WriteLine($"Current real time: {_timeCalculator.GetCurrentRealTime()}");
            Console.WriteLine($"Current warped time: {_timeCalculator.GetCurrentWarpedTime()}");
        }

        private void SetStartWorkDay()
        {
            var currentDate = _timeCalculator.GetCurrentWarpedTime();
            _startWorkDay = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 08, 00, 00);
        }

        private void LogOut()
        {
            _loggedIn = false;
            _loggedCourierId = 0;
            _currentPackingList = null;
            _packingListManuallyManaged = false;
        }

        private void CourierSetup()
        {
            SetStartWorkDay();

            if (_currentPackingList == null)
            {
                _currentPackingList = _menuOptions.GetPackingList(_loggedCourierId);
            }

            _courierVehicleAvgSpeed = _vehicleService.GetCourierVehicleAverageSpeed(_loggedCourierId);
            _currentPackingList = _routePlannerService.CalculateEtaForPackages(_currentPackingList, _courierVehicleAvgSpeed, _startWorkDay);
        }

        private void ApplyCourierDataUpdate()
        {
            if (_logInDate.Day != _timeCalculator.GetCurrentWarpedTime().Day)
            {
                CourierSetup();
            }
        }

        private void ExitApp()
        {
            Environment.Exit(0);
        }
    }
}
