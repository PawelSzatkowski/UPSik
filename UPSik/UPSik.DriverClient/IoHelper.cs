using System;
using System.Collections.Generic;
using System.Text;
using UPSik.DriverClient.Models;
using UPSik.DriverClient.Services;

namespace UPSik.DriverClient
{
    public interface IIoHelper
    {
        int GetIntFromUser(string message);
        string GetTextFromUser(string message);
        void PrintPackingList(List<Package> packingList);
        void PrintShippingRoute(List<Package> currentPackingList);
        void EmptyPackingListNote();
    }

    public class IoHelper : IIoHelper
    {
        public string GetTextFromUser(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }

        public int GetIntFromUser(string message)
        {
            int result;

            while (!int.TryParse(GetTextFromUser(message), out result))
            {
                Console.WriteLine("Incorrect. Try again...");
            }

            return result;
        }

        public void PrintPackingList(List<Package> packingList)
        {
            int id = 1;

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

        public void PrintShippingRoute(List<Package> packingList)
        {
            int id = 1;

            Console.WriteLine("\n" + "Pick up packages from senders: " + "\n");

            foreach (var package in packingList)
            {
                Console.WriteLine($"{id}. Package no.: {package.Number}");
                Console.WriteLine($"ETA: {package.EtaToSender}");
                Console.WriteLine($"Distance from previous point: {String.Format("{0:0.00}", Math.Round(package.SenderDistanceFromPreviousPoint / 1000, 2, MidpointRounding.ToZero))} km");
                Console.WriteLine($"Sender's address:");
                Console.WriteLine($"{package.Sender.Address.Street} {package.Sender.Address.HouseNumber}" + "\n" +
                                  $"{package.Sender.Address.PostCode} {package.Sender.Address.City}" + "\n" +
                                  $"{package.Sender.Address.Country}" + "\n");

                id++;
            }

            id = 1;

            Console.WriteLine("\n" + "Deliver packages to receivers: " + "\n");

            foreach (var package in packingList)
            {
                Console.WriteLine($"{id}. {package.Number}");
                Console.WriteLine($"ETA: {package.EtaToReceiver}");
                Console.WriteLine($"Distance from previous point: {String.Format("{0:0.00}", Math.Round(package.ReceiverDistanceFromPreviousPoint / 1000, 2, MidpointRounding.ToZero))} km");
                Console.WriteLine($"Receiver's address:");
                Console.WriteLine($"{package.Receiver.Address.Street} {package.Receiver.Address.HouseNumber}" + "\n" +
                                  $"{package.Receiver.Address.PostCode} {package.Receiver.Address.City}" + "\n" +
                                  $"{package.Receiver.Address.Country}" + "\n");

                id++;
            }
        }

        public void EmptyPackingListNote()
        {
            {
                Console.WriteLine();
                Console.WriteLine("Your packing list is empty! Consider having holiday, go for ice-cream, skateboarding maybe? Talk to that that girl you've always liked. Life is short, enjoy it!");
            }
        }
    }
}
