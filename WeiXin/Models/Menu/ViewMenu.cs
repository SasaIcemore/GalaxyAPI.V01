using System;
using System.Collections.Generic;
using System.Text;

namespace WeiXin.Models.Menu
{
    public class ViewMenu:ChildMenu
    {
        public string type { get; set; }
        public string url { get; set; }
        public ViewMenu() { }
        public ViewMenu(string name, string type, string url)
        {
            this.name = name;
            this.type = type;
            this.url = url;
        }
    }
}
