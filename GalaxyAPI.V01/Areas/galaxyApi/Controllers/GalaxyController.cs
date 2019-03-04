using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GalaxyAPI.V01.Controllers.galaxy_api
{
    public class GalaxyController : Controller
    {
        [Area("galaxyApi")]
        public IActionResult API(string apiName, string param)
        {
            ViewData["api"] = apiName;
            if (param != null)
            {
                string[] strList = param.Split("&");
                ViewData["params"] = strList;
            }
            return View();
        }
    }
}