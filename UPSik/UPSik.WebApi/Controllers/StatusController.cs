using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace UPSik.WebApi.Controllers
{
    [Route("api/status")]
    public class StatusController : ControllerBase
    {
        /*
         * Method: GET
         * URI: http://localhost:10500/api/status
         * Body: no body
         */
        [HttpGet]
        public string GetStatus()
        {
            return "Status OK";
        }
    }
}
