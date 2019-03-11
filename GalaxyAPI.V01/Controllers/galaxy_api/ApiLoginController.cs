using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            string sessionToken = HttpContext.Session.GetString("api_access_token");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                //已登录
                return Content("0");
            }
            string token = string.Empty;
            try
            {
                //请求identityServer服务器，返回token
                using (var tokenClient = new TokenClient(MyConfig.ConfigManager.TOKEN_URL, "glxApi002", "console.write"))
                {
                    var tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync(user, pwd, "api").Result;
                    token = tokenResponse.AccessToken;
                }
                if (string.IsNullOrEmpty(token)) token = "";
            }
            catch
            {
                token = "";
            }
            finally
            {
                //将token记录cookie,设置过期时间2小时
                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("api_access_token", token);
                }
            }
            if (string.IsNullOrEmpty(token))
            {
                //登录失败
                return Content("-1");
            }
            //登录成功
            return Content(token);
        }
    }
}