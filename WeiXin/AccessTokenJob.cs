using Microsoft.Extensions.Hosting;
using MyConfig;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tools.pgsql;
using WeiXin;
using WeiXin.Models;

namespace WeiXinAccessToken
{
    public class AccessTokenJob : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //如果使用微信公众号，那么获取asscess_token
            if (ConfigManager.MP_IS_USE == "on")
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        bool isToken = false;
                        do
                        {
                            isToken = GetAccessToken();
                        } while (isToken == false);//token获取失败继续执行
                        await Task.Delay(WXAccessToken.expires_in * 1000, stoppingToken); //启动后WXAccessToken.expires_in秒执行一次 
                    }
                }
                catch (Exception ex)
                {
                    bool isToken = false;
                }
            }
        }

        private bool GetAccessToken()
        {
            string resultContent = string.Empty;
            bool isToken = false;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var cliGet = client.GetAsync(ConfigManager.TotalURL);
                    cliGet.Wait();
                    var result = cliGet.Result;
                    resultContent = result.Content.ReadAsStringAsync().Result;

                    //resultContent判断token是否获取成功
                    var accessToken = JsonConvert.DeserializeObject<AccessToken>(resultContent);
                    if (accessToken is AccessToken)
                    {
                        isToken = true;
                        WXAccessToken.Token = accessToken.access_token;
                        WXAccessToken.expires_in = accessToken.expires_in;
                        //测试保存access_token
                        //后面这部分可以注释
                        string sql = "insert into public.wx_token_info (token) values ('" + WXAccessToken.Token + "')";
                        NpgsqlHelper sqlHelper = new NpgsqlHelper(ConfigManager.DbType, ConfigManager.HOST, ConfigManager.DB, ConfigManager.UserName, ConfigManager.PWD);
                        sqlHelper.DoNonQuery(sql);

                    }
                    else
                    {
                        isToken = false;
                    }
                }
                catch (Exception ex)
                {
                    isToken = false;
                }
            }
            return isToken;
        }
    }
}
