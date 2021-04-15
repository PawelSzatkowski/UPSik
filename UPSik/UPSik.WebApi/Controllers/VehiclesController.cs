using Microsoft.AspNetCore.Mvc;
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
        public void PostVehicle([FromBody] Vehicle vehicle)
        {
            _vehiclesService.AddNewVehicleAsync(vehicle);
        }

        [HttpGet("{id}")]
        public int GetVehicleSpeed(int id)
        {
            var vehicleAverageSpeed = _vehiclesService.GetVehicleAverageVelocity(id);
            return vehicleAverageSpeed; 
        }
    }
}
