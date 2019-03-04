using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using WeiXin;

namespace GalaxyAPI.V01.Controllers
{
    public class WX_MPController : Controller
    {
        //接入
        [HttpGet]
        [ActionName("Index")]
        public String Index(string signature, string timestamp, string nonce, string echostr)
        {
            String rs = WeiXinBaseInfo.ChkConnWX(signature, timestamp, nonce, echostr);
            return rs;
        }

        [HttpPost]
        [ActionName("Index")]
        public String Post()
        {
            //接收消息
            byte[] bStream = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Request.Body.CopyTo(memoryStream);
                bStream = memoryStream.ToArray();
            }
            MemoryStream stream = new MemoryStream(bStream);
            MsgHelper msg = new MsgHelper();
            string rs = msg.SendMsg(stream);
            return rs;
        }

    }
}