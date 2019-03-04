using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GalaxyAPI.V01.Controllers
{
    [EnableCors("any")]
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                //已登录
                return View("Views/Manage/Index.cshtml");
            }
            return View();
        }
        
        public IActionResult ChkLogin(string userName, string password)
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                //已登录,跳转到后台管理
                return View("Views/Manage/Index.cshtml");
            }
            string token = string.Empty;
            try
            {
                //请求identityServer服务器，返回token
                using (var tokenClient = new TokenClient(MyConfig.ConfigManager.TOKEN_URL, "glxApi002", "console.write")) {
                    var tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync(userName, password, "api").Result;
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
                    HttpContext.Response.Cookies.Append("token", token, new CookieOptions
                    {
                        Expires = DateTime.Now.AddHours(2)
                    });
                }
            }
            if (string.IsNullOrEmpty(token))
            {
                //登录失败
                return View("Login");
            }
            //登录成功
            HttpContext.Session.SetString("userName", userName);
            HttpContext.Session.SetString("password", password);
            return View("Views/Manage/Index.cshtml");
        }
        
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("token");
            HttpContext.Session.Clear();
            return View("Login");
        }
    }
}