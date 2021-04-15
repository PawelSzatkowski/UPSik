using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UPSik.DriverClient.Models;

namespace UPSik.DriverClient.Services
{
    public interface IPackingListService
    {
        bool? CheckIfPackingListIsBeingWorkedOn(int loggedCourierId);
        bool SwitchPackingListToManual(int loggedCourierId);
        float CalculateTotalCourierRating(int loggedCourierId);
        List<PackingListInfo> GetCourierPackingListInfos(int loggedCourierId);
    }

    public class PackingListService : IPackingListService
    {
        private readonly IConnectionService _connectionService;

        public PackingListService(
            IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public bool? CheckIfPackingListIsBeingWorkedOn(int loggedCourierId)
        {
            var response = _connectionService.WebApiConnect($"packing_lists/find?courierId={loggedCourierId}", ConnectionService.ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<bool>(responseText);
            }
            else
            {
                Console.WriteLine($"Failed. Status code: {response.StatusCode}");
                return null;
            }
        }
        
        public bool SwitchPackingListToManual(int loggedCourierId)
        {
            var response = _connectionService.WebApiConnect($"packing_lists/{loggedCourierId}", ConnectionService.ConnectionType.Put);
            var fileName = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Packing list: {fileName} has been successfuly changed to manual managing");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed. Status code: {response.StatusCode}");
                return false;
            }
        }

        public float CalculateTotalCourierRating(int loggedCourierId)
        {
            float totalRating;

            var response = _connectionService.WebApiConnect($"packing_lists/get_rating?courierId={loggedCourierId}", ConnectionService.ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                totalRating = JsonConvert.DeserializeObject<float>(responseText);
            }
            else
            {
                Console.WriteLine($"Failed. Status code: {response.StatusCode}");
                totalRating = 0;
            }

            return totalRating;
        }

        public List<PackingListInfo> GetCourierPackingListInfos(int loggedCourierId)
        {
            List<PackingListInfo> packingListInfos;

            var response = _connectionService.WebApiConnect($"packing_list_infos/{loggedCourierId}", ConnectionService.ConnectionType.Get);
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                packingListInfos = JsonConvert.DeserializeObject<List<PackingListInfo>>(responseText);
            }
            else
            {
                Console.WriteLine($"Failed. Status code: {response.StatusCode}");
                packingListInfos = null;
            }

            return packingListInfos;
        }
    }
}
