using EventStore.Client;
using System.Collections.Generic;
using System.Text.Json;
using UPSik.DataLayer.Models;

namespace UPSik.BusinessLayer
{
    public interface INotificationService
    {
        void NotifyPackageDelivery(List<Package> packages);
    }

    public class NotificationService : INotificationService
    {
        public void NotifyPackageDelivery(List<Package> packages)
        {
            const string stream = "UPSik-package_delivered-stream";
            const int defaultPort = 2113;

            var settings = EventStoreClientSettings.Create($"esdb://127.0.0.1:{defaultPort}?Tls=false");

            using (var client = new EventStoreClient(settings))
            {
                client.AppendToStreamAsync(
                    stream,
                    StreamState.Any,
                    new[] { GetEventDataFor(packages) }).Wait();
            }
        }

        private static EventData GetEventDataFor(List<Package> data)
        {
            return new EventData(
                Uuid.NewUuid(),
                "package-delivered",
                JsonSerializer.SerializeToUtf8Bytes(data));
        }
    }
}
