using Microsoft.AspNetCore.Mvc;
using UPSik.DataLayer.Models;
using System.Threading.Tasks;
using UPSik.BusinessLayer;
using System;

namespace UPSik.WebApi.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWorkingCouriersService _workingCouriersService;

        public UsersController(
            IUserService userService,
            IWorkingCouriersService workingCouriersService)
        {
            _userService = userService;
            _workingCouriersService = workingCouriersService;
        }

        [HttpPost]
        public async Task PostUser([FromBody] User user)
        {
            await _userService.AddNewUserAsync(user);
        }

        [HttpPut("{courierId}")]
        public void PutFinishWork(int courierId)
        {
            _workingCouriersService.FinishWorkManually(courierId);
        }


        [HttpGet("{email}/{password}")]
        public async Task<User> GetUser(string email, string password)
        {
            try
            {
                var loggedUser = await _userService.GetLoggedInUser(email, password);
                return loggedUser;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
