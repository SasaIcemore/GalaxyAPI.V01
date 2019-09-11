using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeiXin.Menu;

namespace GalaxyAPI.V01.Controllers
{
    //配置微信功能
    public class WX_ManagerController : Controller
    {
        public IActionResult Manage()
        {
            return View();
        }

        public IActionResult SetMenu()
        {
            string resultStr = Menu.SetMenu();
            return Content(resultStr);

        }
    }
}