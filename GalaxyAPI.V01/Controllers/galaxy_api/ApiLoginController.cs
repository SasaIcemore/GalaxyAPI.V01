using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaxyApi;
using GalaxyApi.Model;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GalaxyAPI.V01.Controllers.galaxy_api
{
    [Route("api/[controller]")]
    public class ApiLoginController : Controller
    {
        [HttpPost]
        public IActionResult Login(string user, string pwd)
        {
            IdServerToken idsToken = new IdServerToken("glxApi002", "password", "console.write", user, pwd);
            IdentityServerResult idrs = idsToken.GetIdServerResult();
            string token = idsToken == null ? string.Empty : idrs.access_token;
            return Content(token);
        }
    }
}