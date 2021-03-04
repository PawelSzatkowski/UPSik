using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UPSik.DriverClient.Models;

namespace UPSik.DriverClient
{
    class Program
    {
        private int _loggedCourierId;
        private bool _loggedIn = false;

        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            Console.WriteLine("Welcome, our dear worker!" + "\n");
            Console.WriteLine("1. Log in");
            Console.WriteLine("2. Exit app" + "\n");

            while (!_loggedIn)
            {
                var userChoice = GetIntFromUser("Choose option");

                switch (userChoice)
                {
                    case 1:
                        LogIn();
                        break;
                    case 2:
                        Environment.Exit(0);
                        break;
                }
            }

            Console.WriteLine("1. Show your latest shipping list");
            Console.WriteLine("2. Exit app" + "\n");

            while (true)
            {
                var userChoice = GetIntFromUser("Choose option");

                switch (userChoice)
                {
                    case 1:
                        ShowLatestPackingList(_loggedCourierId);
                        break;
                    case 2:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void LogIn()
        {
            var email = GetTextFromUser("Enter your email");
            var password = GetTextFromUser("Enter your password");

            using (var httpClient = new HttpClient())
            {
                var response = new HttpResponseMessage();

                response = httpClient.GetAsync(@$"http://localhost:10500/api/users/{email}/{password}").Result;

                var responseText = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<User>(responseText);

                    if (responseObject == null)
                    {
                        Console.WriteLine("\n" + "Incorrect user//password!" + "\n");
                        return;
                    }

                    Console.WriteLine($"\n" + $"Loggin succesful! Welcome back, {responseObject.Name}!" + "\n");

                    _loggedCourierId = responseObject.Id;
                    _loggedIn = true;
                }
                else
                {
                    Console.WriteLine($"Http query failure. Statuse code: {response.StatusCode}");
                }
            }
        }


        private void ShowLatestPackingList(int id)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(@$"http://localhost:10500/api/vehicles/{id}").Result;
                var responseText = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonConvert.DeserializeObject<List<Package>>(responseText);

                    if (responseObject == null)
                    {
                        Console.WriteLine("You have no packing lists!");
                        return;
                    }

                    PrintPackingList(responseObject);
                }
                else
                {
                    Console.WriteLine($"Http query failure. Status code: {response.StatusCode}");
                }
            }
        }

        private void PrintPackingList(List<Package> packingList)
        {
            int id = 1;

            if (packingList.Count == 0)

            {
                Console.WriteLine();
                Console.WriteLine("Your packing list is empty! Consider having holiday, go for ice-cream, skateboarding maybe? Talk to that that girl you've always liked. Life is short, enjoy it!");
            }

            foreach (var package in packingList)
            {
                Console.WriteLine("\n" + $"{id}.");
                Console.WriteLine($"Package number: {package.Number}");
                Console.WriteLine($"Package weight: {package.Weight}");
                Console.WriteLine($"Package date of adding: {package.DateOfAdding}");
                Console.WriteLine($"Package priority: {package.Priority}" + "\n");

                Console.WriteLine($"Sender's name: {package.Sender.Name} {package.Sender.Surname}");
                Console.WriteLine($"Sender's e-mail: {package.Sender.Email}");
                Console.WriteLine($"Sender's address:");
                Console.WriteLine($"{package.Sender.Address.Street} {package.Sender.Address.HouseNumber}" + "\n" +
                                  $"{package.Sender.Address.PostCode} {package.Sender.Address.City}" + "\n" +
                                  $"{package.Sender.Address.Country}" + "\n");

                Console.WriteLine($"Receiver's name: {package.Receiver.Name} {package.Receiver.Surname}");
                Console.WriteLine($"Receiver's e-mail: {package.Receiver.Email}");
                Console.WriteLine($"Receiver's address:");
                Console.WriteLine($"{package.Receiver.Address.Street} {package.Receiver.Address.HouseNumber}" + "\n" +
                                  $"{package.Receiver.Address.PostCode} {package.Receiver.Address.City}" + "\n" +
                                  $"{package.Receiver.Address.Country}");

                id++;
            }
            Console.WriteLine();
        }

        private string GetTextFromUser(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }

        private int GetIntFromUser(string message)
        {
            int result;

            while (!int.TryParse(GetTextFromUser(message), out result))
            {
                Console.WriteLine("Incorrect. Try again...");
            }

            return result;
        }

    }
}
