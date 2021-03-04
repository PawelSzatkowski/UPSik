using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UPSik.BusinessLayer;
using UPSik.DataLayer.Models;

namespace UPSik.WebApi.Controllers
{
    [Route("api/packages")]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpPost]
        public async Task PostPackage([FromBody] Package package)
        {
            await _packageService.AddNewPackageAsync(package);
        }
    }
}
