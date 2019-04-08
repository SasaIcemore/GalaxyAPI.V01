using GalaxyApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GalaxyApi
{
    public class IdServerUserInfo
    {
        private string auth = string.Empty;
        public IdServerUserInfo(string auth)
        {
            this.auth = auth;
        }

        public ConnectUserInfo GetConnectUserInfo()
        {
            ConnectUserInfo userInfo = null;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(MyConfig.ConfigManager.AUTHORITY);
                client.DefaultRequestHeaders.Add("Authorization", auth);
                var t = client.GetAsync("/connect/userinfo");
                t.Wait();
                var result = t.Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                userInfo = JsonConvert.DeserializeObject<ConnectUserInfo>(resultContent);
            }
            return userInfo;
        }
    }
}
