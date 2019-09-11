using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace WeiXin
{
    public class MsgHelper
    {
        //文本消息
        private string TextMsg = "<xml> <ToUserName>< ![CDATA[{0}] ]></ToUserName> <FromUserName>< ![CDATA[{1}] ]></FromUserName> <CreateTime>{2}</CreateTime> <MsgType>< ![CDATA[text] ]></MsgType> <Content>< ![CDATA[{3}] ]></Content> </xml>".Replace(" ","");

        //图片消息
        //public string PicMsg = "<xml><ToUserName>< ![CDATA[{0}] ]></ToUserName><FromUserName>< ![CDATA[{1}] ]></FromUserName><CreateTime>{2}</CreateTime><MsgType>< ![CDATA[image] ]></MsgType><Image><MediaId>< ![CDATA[{3}] ]></MediaId></Image></xml>".Replace(" ", "");

        public string SendMsg(MemoryStream stream)
        {
            XDocument doc = XDocument.Load(stream);
            TextMessage tmsg = new TextMessage();
            tmsg.FromUserName = doc.Element("xml").Element("FromUserName").Value;
            tmsg.ToUserName = doc.Element("xml").Element("ToUserName").Value;
            tmsg.Content = doc.Element("xml").Element("Content").Value;
            tmsg.MsgType = doc.Element("xml").Element("MsgType").Value;

            //准备回复的数据包
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string millis = Convert.ToInt32(ts.TotalSeconds).ToString();
            switch (tmsg.MsgType)
            {
                case "text":
                    TextMsg = string.Format(TextMsg, tmsg.FromUserName, tmsg.ToUserName, millis, tmsg.Content);
                    return TextMsg;
                default:
                    return "success";
            }
        }
        
    }
}
