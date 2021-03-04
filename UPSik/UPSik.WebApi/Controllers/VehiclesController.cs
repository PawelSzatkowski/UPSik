using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;

namespace UPSik.WebApi.Controllers
{
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehiclesService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehiclesService = vehicleService;
        }

        [HttpPost]
        public async Task PostVehicle([FromBody] Vehicle vehicle)
        {
            await _vehiclesService.AddNewVehicleAsync(vehicle);
        }

        [HttpGet("{id}")]
        public async Task<List<Package>> GetPackages(int id)
        {
            return await _vehiclesService.GetLatestCourierPackingListAsync(id);
        }
    }
}
