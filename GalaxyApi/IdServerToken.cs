using GalaxyApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GalaxyApi
{
    public class IdServerToken
    {
        private string client_id = string.Empty;
        private string grant_type = string.Empty;
        private string client_secret = string.Empty;
        private string user_name = string.Empty;
        private string pwd = string.Empty;

        public IdServerToken(string client_id, string grant_type, string client_secret, string user_name, string pwd)
        {
            this.client_id = client_id;
            this.grant_type = grant_type;
            this.client_secret = client_secret;
            this.user_name = user_name;
            this.pwd = pwd;
        }

        /// <summary>
        /// 获取IdServer的返回数据，包含token
        /// </summary>
        /// <returns></returns>
        public IdentityServerResult GetIdServerResult()
        {
            IdentityServerResult idrs = null;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(MyConfig.ConfigManager.AUTHORITY);
                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("client_id", client_id),
                    new KeyValuePair<string, string>("password", pwd),
                    new KeyValuePair<string, string>("grant_type", grant_type),
                    new KeyValuePair<string, string>("client_secret", client_secret),
                    new KeyValuePair<string, string>("userName", user_name)
                    });
                var t = client.PostAsync("/connect/token", content);
                t.Wait();
                var result = t.Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                idrs = JsonConvert.DeserializeObject<IdentityServerResult>(resultContent);
            }
            return idrs;
        }
    }
}
