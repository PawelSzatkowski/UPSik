using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UPSik.DriverClient.Models;
using UPSik.DriverClient.Services;
using UPSik.DriverClient.TimeWarper;
using static UPSik.DriverClient.Services.ConnectionService;

namespace UPSik.DriverClient
{
    public interface IMenuOptions
    {
        int LogInUser();
        List<Package> GetPackingList(int courierId);
        bool SwitchPackingListToManual(int courierId, List<Package> currentPackingList);
        void MarkPackageAsDelivered(List<Package> currentPackingList);
        void ShowCourierRating(int courierId);
        void FinishWork(int courierId);
    }

    public class MenuOptions : IMenuOptions
    {
        private readonly IIoHelper _ioHelper;
        private readonly IConnectionService _connectionService;
        private readonly IPackingListService _packingListService;
        private readonly ITimeCalculator _timeCalculator;
        private readonly IPackagesService _packagesService;

        public MenuOptions(
            IIoHelper ioHelper,
            IConnectionService connectionService,
            IPackingListService packingListService,
            ITimeCalculator timeCalculator,
            IPackagesService packagesService)
        {
            _ioHelper = ioHelper;
            _connectionService = connectionService;
            _packingListService = packingListService;
            _timeCalculator = timeCalculator;
            _packagesService = packagesService;
        }

        public int LogInUser()
        {
            var email = _ioHelper.GetTextFromUser("Enter your email");
            var password = _ioHelper.GetTextFromUser("Enter your password");

            var response = _connectionService.WebApiConnect($"users/{email}/{password}", ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JsonConvert.DeserializeObject<User>(responseText);

                if (responseObject == null)
                {
                    Console.WriteLine("\n" + "Incorrect user//password!" + "\n");
                    return 0;
                }

                Console.WriteLine($"\n" + $"Logging in succesful! Welcome back, {responseObject.Name}!" + "\n");

                return responseObject.Id;
            }
            else
            {
                Console.WriteLine($"Http query failure. Statuse code: {response.StatusCode}");
                return 0;
            }
        }

        public List<Package> GetPackingList(int id)
        {
            var response = _connectionService.WebApiConnect($"packing_lists/{id}", ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JsonConvert.DeserializeObject<List<Package>>(responseText);

                if (responseObject == null)
                {
                    Console.WriteLine("You have no packing lists!");
                    return null;
                }

                return responseObject;
            }
            else
            {
                Console.WriteLine($"Http query failure. Status code: {response.StatusCode}");
                return null;
            }
        }

        public bool SwitchPackingListToManual(int loggedCourierId, List<Package> currentPackingList)
        {
            var listAutomaticallyManaged = _packingListService.CheckIfPackingListIsBeingWorkedOn(loggedCourierId);

            if (listAutomaticallyManaged == null)
            {
                return false;
            }

            if (listAutomaticallyManaged == true)
            {
                Console.WriteLine("Sorry, your packing list is already being processed automatically by the system!");
                return false;
            }

            if (listAutomaticallyManaged == false)
            {
                Console.WriteLine("\n" + "You will have to manually manage every package you deliver, are you sure to proceed? [Y/N]");

                var choice = Console.ReadLine();

                if (choice.ToUpper() == "Y")
                {
                    if (_packingListService.SwitchPackingListToManual(loggedCourierId))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (choice.ToUpper() == "N")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Error, wrong option");
                    return false;
                }
            }

            return false;
        }

        public void MarkPackageAsDelivered(List<Package> currentPackingList)
        {
            int userChoice = _ioHelper.GetIntFromUser("Choose package");
            int score;

            if (userChoice <= 0 || userChoice > currentPackingList.Count)
            {
                Console.WriteLine("There is no package with that id");
                return;
            }

            score = _packagesService.DeliverPackage(currentPackingList[userChoice - 1].Id,
                                                    currentPackingList[userChoice - 1].EtaToReceiver,
                                                    _timeCalculator.GetCurrentWarpedTime());

            if (score == 0)
            {
                Console.WriteLine("You have already marked this package as delivered");
                return;
            }

            Console.WriteLine($"Your score for delivering this package is: {score}");
        }

        public void ShowCourierRating(int loggedCourierId)
        {
            float courierRating = _packingListService.CalculateTotalCourierRating(loggedCourierId);

            if (courierRating == 0)
            {
                Console.WriteLine("There has been a connection issue or you have no packing lists to calculate average from");
                return;
            }
            
            Console.WriteLine($"Your average courier rating is: {courierRating}");

            var packingListInfos = _packingListService.GetCourierPackingListInfos(loggedCourierId);

            int i = 1;

            foreach (var packingListInfo in packingListInfos)
            {
                Console.WriteLine($"{i}. {packingListInfo.FileName}, Rating: {packingListInfo.CourierRating}");
                i++;
            }
        }

        public void FinishWork(int courierId)
        {
            var response = _connectionService.WebApiConnect($"users/{courierId}", ConnectionType.Put);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Succesfully finished work");
            }
            else
            {
                Console.WriteLine($"Http query failure. Status code: {response.StatusCode}");
            }
        }
    }
}
