using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace API.Tools
{
    public static class T1Api
    {
        static string T1WebSite = "http://192.168.0.20:8080"; //T1网站部署地址 注意最后不能有字符 /

        #region 缓存变量
        //api调用令牌
        //public static string Token = "";
        //会话信息
        static CookieContainer cookie = new CookieContainer();
        #endregion



        /// <summary>
        /// 获取T1api调用凭证（获取后缓存）
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="appId">第三方应用识别码（给一个随机的GUID即可）</param>
        /// <param name="clientType">pc或mobile</param>
        /// <returns></returns>
        public static string Logon(string user, string psw)
        {
            string AppId = "7570B9AC-11C7-4047-8D13-D6187419B411";
            string ClientType = "pc";  //pc或mobile
            string url = string.Format("{4}/api/getToken?user={0}&password={1}&appId={2}&clientType={3}", user, psw, AppId, ClientType, T1WebSite);
            return HttpGet(url);
        }
        /// <summary>
        /// 添加单据前获取单据初始化数据包
        /// </summary>
        /// <param name="token">api调用凭证</param>
        /// <param name="itemType">单据名称</param>
        /// <returns></returns>
        public static string GetCreate(string token, string itemType)
        {
            string url = string.Format("{0}/apiData/getCreate?token={1}&itemType={2}", T1WebSite, token, itemType);
            return HttpGet(url);
        }
        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="token">api调用凭证</param>
        /// <param name="itemType">单据名称(Name)</param>
        /// <param name="dataString">单据数据包json串</param>
        /// <returns></returns>
        public static string Create(string token, string itemType, string dataString)
        {
            string param = string.Format("token={0}&itemType={1}&dataString={2}", token, itemType, dataString);
            string url = string.Format("{0}/apiData/create", T1WebSite);
            return HttpPost(url, param);
        }
        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="token">api调用凭证</param>
        /// <param name="itemType">单据名称(Name)</param>
        /// <param name="id">单据内码</param>
        /// <param name="dataString">单据数据包</param>
        /// <returns></returns>
        public static string Update(string token, string itemType, string id, string dataString)
        {
            string param = string.Format("token={0}&itemType={1}&dataString={2}", token, itemType, dataString);
            string url = string.Format("{0}/apiData/update", T1WebSite);
            return HttpPost(url, param);
        }
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        /// <param name="idSet">内码集合</param>
        /// <returns></returns>
        public static string Delete(string token, string itemType, List<string> idSet)
        {
            string idString = string.Join(",", idSet.ToArray());
            string param = string.Format("token={0}&itemType={1}&idString={2}", token, itemType, idString);
            string url = string.Format("{0}/apiData/delete", T1WebSite);
            return HttpPost(url, param);
        }
        /// <summary>
        /// 一次性查询表单所有数据（只适用于少量记录的情况）
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public static string Query(string token, string itemType, string filterString)
        {
            string param = string.Format("token={0}&itemType={1}&filterString={2}&skipAuth=true", token, itemType, filterString);
            string url = string.Format("{0}/apiData/query", T1WebSite);

            return HttpPost(url, param);
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        /// <param name="flage">分页表识（第一次传0）</param>
        /// <param name="pageSize">每一页记录数</param>
        /// <param name="filterString">过滤条件</param>
        /// <returns></returns>
        public static string QueryPage(string token, string itemType, string flage, int pageSize, string filterString)
        {
            string param = string.Format("token={0}&itemType={1}&filterString={2}&flage={3}&pageSize={4}", token, itemType, filterString, flage, pageSize);
            string url = string.Format("{0}/apiData/queryPage", T1WebSite);

            return HttpPost(url, param);
        }

        #region api调用
        public static string HttpGet(string url)
        {
            string result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.KeepAlive = false;
            request.CookieContainer = cookie;//重复使用原来的会话

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)  //登录成功
            {
                var o = response.GetResponseStream();
                StreamReader read = new StreamReader(o);
                string y = read.ReadToEnd();
                response.Close();
                read.Close();
                result = y;
            }
            else
                result = string.Empty;
            //释放资源
            if (response != null) { response.Close(); }
            if (response != null) { response = null; }
            if (request != null) { request.Abort(); }
            if (request != null) { request = null; }
            return result;
        }
        public static string HttpPost(string url, string param)
        {
            string result = string.Empty;

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.CookieContainer = cookie;//重复使用原来的会话
            request.Method = "POST";
            request.KeepAlive = false;
            byte[] b_param = Encoding.GetEncoding("UTF-8").GetBytes(param);
            request.ContentLength = b_param.Length;
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(b_param, 0, b_param.Length);
            reqStream.Close();


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {

                var o = response.GetResponseStream();
                StreamReader read = new StreamReader(o);
                result = read.ReadToEnd();
                response.Close();
                read.Close();
            }
            else
            {
                result = response.StatusDescription;
            }

            //释放资源
            if (response != null) { response.Close(); }
            if (response != null) { response = null; }
            if (request != null) { request.Abort(); }
            if (request != null) { request = null; }
            return result;
        }
        #endregion

    }

}
