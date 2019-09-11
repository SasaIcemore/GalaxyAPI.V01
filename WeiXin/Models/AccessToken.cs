using System;
using System.Collections.Generic;
using System.Text;

namespace WeiXin.Models
{
    /// <summary>
    /// 这个类是用于url请求后返回的resultContent能否反序列化为此类对象
    /// 从而判断token是否获取成功
    /// </summary>
    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}
