using Microsoft.AspNetCore.Mvc;
using UPSik.DataLayer.Models;
using System.Threading.Tasks;
using UPSik.BusinessLayer;

namespace UPSik.WebApi.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task PostUser([FromBody] User user)
        {
            await _userService.AddNewUserAsync(user);
        }

        [HttpGet("{email}/{password}")]
        public async Task<User> GetUser(string email, string password)
        {
            var loggedUser = await _userService.GetLoggedInUser(email, password);
            return loggedUser;
        }
    }
}
