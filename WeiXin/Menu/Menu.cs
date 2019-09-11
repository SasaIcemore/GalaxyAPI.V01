using Microsoft.Extensions.Configuration;
using MyConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using WeiXin.Models.Menu;

namespace WeiXin.Menu
{
    public static class Menu
    {
        public static string SetMenu()
        {
            string resultContent = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string postUrl = ConfigManager.WX_MENU_URL + WXAccessToken.Token;
                    var directory = System.IO.Directory.GetCurrentDirectory();
                    string strContent = string.Empty;

                    //直接传json不行，下面通过序列化可以，我也不知道为什么

                    //using (FileStream readFile = new FileStream(directory + "/Json/WX_Menu.json", FileMode.Open)) {
                    //    byte[] readByte = new byte[readFile.Length];
                    //    readFile.Read(readByte, 0, readByte.Length);//读取
                    //    strContent = Encoding.UTF8.GetString(readByte);
                    //}
                    //strContent = strContent.Trim().Replace("\r", "").Replace("\n","").Replace(" ", "");

                    ViewMenu vmenu = new ViewMenu("搜索", "view", "http://www.baidu.com/");
                    Miniprogram minipro = new Miniprogram("小程序","miniprogram", "http://mp.weixin.qq.com", "wx286b93c14bbf93aa", "pages/lunar/index");
                    List<object> objList = new List<object>();
                    objList.Add(vmenu);
                    objList.Add(minipro);
                    Caidan caidan = new Caidan();
                    caidan.name = "菜单";
                    caidan.sub_button = objList;

                    Click_Menu climenu = new Click_Menu("点击获取", "click", "V1001_TODAY_MUSIC");
                    List<ChildMenu> menuList = new List<ChildMenu>();
                    menuList.Add(climenu);
                    menuList.Add(caidan);
                    MenuModel menuModel = new MenuModel();
                    menuModel.button = menuList;

                    strContent = JsonConvert.SerializeObject(menuModel);
                    StringContent cont = new StringContent(strContent, Encoding.UTF8, "application/json");
                    var t = client.PostAsync(postUrl, cont);
                    t.Wait();
                    var result = t.Result;
                    resultContent = result.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {

                }
                return resultContent;
            }
        }
    }
}
