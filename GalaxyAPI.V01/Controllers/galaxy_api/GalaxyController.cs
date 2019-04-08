using GalaxyApi;
using GalaxyApi.Model;
using GalaxyAPI.V01.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;

namespace GalaxyAPI.V01.Controllers.galaxy_api
{
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("any")]
    public class GalaxyController : Controller
    {
        APIDataHelper dataHelper = new APIDataHelper();
        
        [HttpPost]
        public IActionResult API(string apiCode, string filter="", int pCount=1, int pNum=1)
        {
            //获取参数,json串
            List<APIParamsFilter> paralist = null;
            try
            {
                paralist = JsonConvert.DeserializeObject<List<APIParamsFilter>>(filter);
            }
            catch
            {
                paralist = null;
            }
            //获取令牌，取得角色id
            string auth = HttpContext.Request.Headers["Authorization"];
            IdServerUserInfo idsUserInfo = new IdServerUserInfo(auth);
            ConnectUserInfo userInfo = idsUserInfo.GetConnectUserInfo();
            int role_id = userInfo == null ? 0 : userInfo.role;
            //是否有权调用api
            bool canUse = dataHelper.IsGetApi(role_id, apiCode);
            if (canUse)
            {
                APIDataHelper datahelper = new APIDataHelper();
                DataTable tbl = datahelper.GetGuideAPIByCode(apiCode, pCount, pNum, paralist);
                string jsonStr = string.Empty;
                if (tbl != null)
                {
                    jsonStr = JsonConvert.SerializeObject(tbl);
                }
                else
                {
                    jsonStr = "";
                }
                return Content(jsonStr);
            } 
            else
            {
                return Conflict("没有权限，请联系管理员");
            }
        }
    }
}