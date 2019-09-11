using System;
using System.Collections.Generic;
using System.Text;

namespace WeiXin.Models.Menu
{
    public class Click_Menu:ChildMenu
    {
        public string type { get; set; }
        public string key { get; set; }
        public Click_Menu() { }
        public Click_Menu(string name, string type, string key)
        {
            this.name = name;
            this.type = type;
            this.key = key;
        }
    }
}
