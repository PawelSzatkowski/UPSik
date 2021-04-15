using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DriverClient.Services
{
    public interface IPackagesService
    {
        int DeliverPackage(int packageId, DateTime etaToReceiver, DateTime deliveryDate);
    }

    public class PackagesService : IPackagesService
    {
        private readonly IConnectionService _connectionService;

        public PackagesService(
            IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public int DeliverPackage(int packageId, DateTime etaToReceiver, DateTime deliveryDate)
        {
            int score;

            var response = _connectionService.WebApiConnect($"packages/deliver_package?packageId={packageId}&etaToReceiver={etaToReceiver}&deliveryDate={deliveryDate}", ConnectionService.ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                score = JsonConvert.DeserializeObject<int>(responseText);
            }
            else
            {
                Console.WriteLine($"Failed. Status code: {response.StatusCode}");
                score = 0;
            }

            return score;
        }
    }
}
