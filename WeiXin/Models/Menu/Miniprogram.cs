using System;
using System.Collections.Generic;
using System.Text;

namespace WeiXin.Models.Menu
{
    public class Miniprogram:ChildMenu
    {
        public string type { get; set; }
        public string url { get; set; }
        public string appid { get; set; }
        public string pagepath { get; set; }

        public Miniprogram() { }
        public Miniprogram(string name, string type, string url, string appid, string pagepath)
        {
            this.name = name;
            this.type = type;
            this.url = url;
            this.appid = appid;
            this.pagepath = pagepath;
        }
    }
}
