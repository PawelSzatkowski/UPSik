using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;

namespace UPSik.WebApi.Controllers
{
    [Route("api/packing_lists")]
    public class PackingListsController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IPackingListService _packingListService;

        public PackingListsController(
            IVehicleService vehicleService,
            IPackingListService packingListService,
            IPackageService packageService)
        {
            _vehicleService = vehicleService;
            _packingListService = packingListService;
        }

        [HttpGet("{id}")]
        public async Task<List<Package>> GetPackagesList(int id)
        {
            var packingList = await _vehicleService.GetLatestCourierPackingListAsync(id);
            return packingList;
        }

        [HttpPut("{id}")]
        public string PutPackingList(int id)
        {
            var packingListName = _packingListService.SetPackingListToManualManaging(id);
            return packingListName;
        }

        [HttpGet("find")]
        public bool GetPackagesInfo([FromQuery] int courierId)
        {
            var answer = _packingListService.CheckIfPackingListIsAlreadyBeingManaged(courierId);
            return answer;
        }

        [HttpGet("get_rating")]
        public float GetRating([FromQuery] int courierId)
        {
            var courierRating = _packingListService.CalculateTotalCourierRating(courierId);
            return courierRating;
        }
    }
}
