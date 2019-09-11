using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MyConfig
{
    public class ConfigManager
    {
        //=========================================identityServer4验证=========================================
        public static string TOKEN_URL = string.Empty;
        public static string AUTHORITY = string.Empty;
        public static string USER_INFO = string.Empty;
        public static string LOCAL = string.Empty;
        //=============================================配置权限的数据库========================================
        public static string DbType = string.Empty;
        public static string HOST = string.Empty;
        public static string UserName = string.Empty;
        public static string PWD = string.Empty;
        public static string DB = string.Empty;
        public static Tools.pgsql.NpgsqlHelper dataHelper = null;
        public static string strConn = string.Empty;
        //=============================================api数据库查询语句=======================================
        public const string sqlServerQuery = @"select * from ( 
                                               select ROW_NUMBER() over(order by {0})as rid,
                                               (select count(*)as row_count from {2} where 1=1 {3})as row_count, " +
                                               "{1} from {2} where 1=1 {3}) as temp where rid > @start and rid <= @end";

        public const string postgreSqlQuery = @"select (select count(*) as row_count from public.{1} where 1=1 {2}),
                                                {0} from public.{1} where 1=1 {2} limit @pCount  offset  @start ";

        public const string DB_TYPE_SQL = "sqlServer";
        public const string DB_TYPE_PGSQL = "postgresql";

        //=================================================微信公众号==========================================
        //是否启用
        public static string MP_IS_USE = string.Empty;
        //获取微信access_token
        public static string APPID = string.Empty;
        public static string APPSECRET = string.Empty;
        public static string ACCESS_TOKEN_URL = string.Empty;
        public static string TotalURL = string.Empty;

        public static string MP_TOKEN = string.Empty;
        //微信菜单
        public static string WX_MENU_URL = string.Empty;

        //=================================================企业微信=============================================
        //是否启用
        public static string WK_IS_USE = string.Empty;
        //
        public static string WK_CROPID = string.Empty;
        public static string WK_TXSECRET = string.Empty;
        public static string WK_APPSECRET = string.Empty;

        private ConfigManager()
        {
            //=========================================identityServer4验证=========================================
            TOKEN_URL = GetConfigVal("galaxy_token", "val");
            AUTHORITY = GetConfigVal("authority","val");
            USER_INFO = GetConfigVal("userinfo","val");
            LOCAL = GetConfigVal("local","val");
            //=================================================微信公众号==========================================
            MP_IS_USE = GetConfigVal("wx_mp", "is_use"); 

            MP_TOKEN = GetConfigVal("token", "val");

            APPID = GetConfigVal("appid", "val"); 

            APPSECRET = GetConfigVal("appsecret", "val"); 

            ACCESS_TOKEN_URL = GetConfigVal("access_token_url", "val"); 

            TotalURL = ACCESS_TOKEN_URL + "&appid=" + APPID + "&secret=" + APPSECRET;

            WX_MENU_URL = GetConfigVal("wxmenu", "val");

            //=============================================配置权限的数据库========================================
            DbType = GetConfigVal("source", "DbType");

            HOST = GetConfigVal("source", "Host");

            UserName = GetConfigVal("source", "UserName");

            PWD = GetConfigVal("source", "Password"); 

            DB = GetConfigVal("source", "Database");

            dataHelper = new Tools.pgsql.NpgsqlHelper(DbType, HOST, DB, UserName, PWD);
            if (DbType == DB_TYPE_PGSQL)
            {
                strConn = string.Format("Host={0};UserName={1};Password={2};Database={3}", HOST, UserName, PWD, DB);
            }
            else
            {
                strConn = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", HOST, DB, UserName, PWD);
            }
            
            //=================================================企业微信=============================================
            //是否启用
            WK_IS_USE = GetConfigVal("wx_work", "is_use"); 
            //
            WK_CROPID = GetConfigVal("cropid", "val"); 

            WK_TXSECRET = GetConfigVal("tongxun_secret", "val");

            WK_APPSECRET = GetConfigVal("appsecret", "val"); 
        }

        public static ConfigManager ConfigXML = new ConfigManager();

        public static string GetConfigVal(string node, string property)
        {
            var directory = System.IO.Directory.GetCurrentDirectory();
            XDocument configXml = XDocument.Load(directory + "/Config.xml");

            string val = string.Empty;
            val = Convert.ToString(configXml.Descendants(node)
            .Select(p => p.Attribute(property).Value)
            .FirstOrDefault());

            return val;
        }
        
    }
}
