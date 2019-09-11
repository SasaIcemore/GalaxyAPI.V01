using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GalaxyApi
{
    public class PostFormUrl
    {
        private string baseAddr = string.Empty;
        private string auth = string.Empty;//"Bearer "+token
        private FormUrlEncodedContent content = null;
        private string postPath = string.Empty;

        public PostFormUrl(string baseAddr, string auth, FormUrlEncodedContent content, string postPath)
        {
            this.baseAddr = baseAddr;
            this.auth = auth;
            this.content = content;
            this.postPath = postPath;
        }

        public dynamic GetResult()
        {
            String resultContent = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddr);
                client.DefaultRequestHeaders.Add("Authorization", auth);
                var t = client.PostAsync(postPath, content);
                t.Wait();
                var result = t.Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }
            return resultContent;
        }
    }
}
