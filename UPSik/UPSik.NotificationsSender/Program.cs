using EventStore.Client;
using Newtonsoft.Json;
using UPSik.NotificationsSender.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Mail;

namespace UPSik.NotificationsSender
{
    class Program
    {
        static void Main(string[] args)
        {
            const string stream = "UPSik-package_delivered-stream";
            const int defaultPort = 2113;

            var settings = EventStoreClientSettings.Create($"esdb://127.0.0.1:{defaultPort}?Tls=false");

            using (var client = new EventStoreClient(settings))
            {
                client.SubscribeToStreamAsync(
                    stream,
                    StreamPosition.End,
                    EventArrived);

                Console.WriteLine("Awaiting delivery confirmation. Press any key + enter to exit app.");
                Console.WriteLine();
                Console.ReadLine();
            }
        }

        private async static Task EventArrived( // DO POPRAWY ŻEBY BYŁO ASYNC
            StreamSubscription subscription,
            ResolvedEvent resolvedEvent,
            CancellationToken cancellationToken)
        {
            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.ToArray());
            var packages = JsonConvert.DeserializeObject<List<Package>>(jsonData);

            int i = 1;

            foreach (var package in packages)
            {
                Console.WriteLine();
                Console.WriteLine($"{i}.");
                Console.WriteLine($"Package (number: {package.Number}) has been delivered");
                Console.WriteLine($"Sender's e-mail: {package.Sender.Email}");
                Console.WriteLine($"Receiver's name: {package.Receiver.Name} {package.Receiver.Surname}");
                Console.WriteLine($"Receiver's e-mail: {package.Receiver.Email}");
                Console.WriteLine($"Receiver's address:");
                Console.WriteLine($"{package.Receiver.Address.Street} {package.Receiver.Address.HouseNumber}" + "\n" +
                                  $"{package.Receiver.Address.PostCode} {package.Receiver.Address.City}" + "\n" +
                                  $"{package.Receiver.Address.Country}");

                i++;
            }

            SendNotificationEmails(packages);
        }

        private static void SendNotificationEmails(List<Package> packages)
        {
            var smtp = new SmtpClient
            {
                Host = "localhost",
                Port = 2500,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true
            };
            
            foreach (var package in packages)
            {
                var receiver = package.Receiver.Email;
                var sender = package.Sender.Email;
                var subject = "Your package has been delivered!";
                var body = $"Hello! Your package (number: {package.Number}) sent by {package.Sender.Name} {package.Sender.Surname} has just arrived at your address: " +
                    $"{package.Receiver.Address.Street} {package.Receiver.Address.HouseNumber}, {package.Receiver.Address.PostCode} {package.Receiver.Address.City}, " +
                    $"{package.Receiver.Address.Country}. Thanks for using UPSik, and have a great day {package.Receiver.Name}!";

                try
                {
                    smtp.Send(sender, receiver, subject, body);
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.WriteLine(e.Message);
                }
            }
            smtp.Dispose();
        }
    }
}
