using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.DriverClient.Services
{
    public interface IVehicleService
    {
        int GetCourierVehicleAverageSpeed(int _loggedCourierId);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IConnectionService _connectionService;

        public VehicleService(
            IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public int GetCourierVehicleAverageSpeed(int id)
        {
            var response = _connectionService.WebApiConnect($"vehicles/{id}", ConnectionService.ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<int>(responseText);
            }
            else
            {
                Console.WriteLine($"Failed why trying to retrieve vehicle's average speed. Status code: {response.StatusCode}");
                return 0;
            }
        }
    }
}
