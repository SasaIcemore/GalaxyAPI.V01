using System;
using System.Collections.Generic;
using System.Text;

namespace WeiXin
{
    /// <summary>
    /// 此类用于记录token
    /// </summary>
    public static class WXAccessToken
    {
        public static string Token{ get; set; }
        public static int expires_in { get; set; }

        
    }
}
