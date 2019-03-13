using GalaxyApi;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Tools.pgsql;
using Tools.RSA;
using Tools.sql;

namespace GalaxyAPI.V01.Models
{
    [EnableCors("any")]
    public class IndexController : Controller
    {
        private APIDataHelper apiDataHelper = new APIDataHelper();
        private string apiSql = string.Empty;

        [ValidateAntiForgeryToken]
        public int ChkDBConn(string database_type, string ip, string database, string user, string pwd)
        {
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string dcrsa_ip = rsaCrypto.Decrypt(ip);
            string dcrsa_database = rsaCrypto.Decrypt(database);
            string dcrsa_user = rsaCrypto.Decrypt(user);
            string dcrsa_pwd = rsaCrypto.Decrypt(pwd);
            HttpContext.Session.SetString("databaseType", database_type);
            HttpContext.Session.SetString("ip", ip);
            HttpContext.Session.SetString("database", database);
            HttpContext.Session.SetString("user", user);
            HttpContext.Session.SetString("pwd", pwd);
            int rsConn = -2;//0:fail====1:success

            DatabaseAbs dataHelper = apiDataHelper.GetSqlHelper(database_type, dcrsa_ip, dcrsa_database, dcrsa_user, dcrsa_pwd);
            rsConn = dataHelper.ChkConn();
            dataHelper = null;
            return rsConn;
        }

        /// <summary>
        /// 主界面
        /// </summary>
        /// <returns></returns>
        public IActionResult Config()
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (string.IsNullOrEmpty(cookieToken))
            {
                //未登录
                return View("Views/Login/Login.cshtml");
            }
            ViewData["ModuleId"] = 6;
            return View();
        }

        /// <summary>
        /// 数据库配置页面
        /// </summary>
        /// <returns></returns>
        public IActionResult APIConfig()
        {
            string databaseType = string.Empty;
            string ip = string.Empty;
            string database = string.Empty;
            string user = string.Empty;
            string pwd = string.Empty;
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            try
            {
                databaseType = HttpContext.Session.GetString("databaseType");
                ip = rsaCrypto.Decrypt(HttpContext.Session.GetString("ip"));
                database = rsaCrypto.Decrypt(HttpContext.Session.GetString("database"));
                user = rsaCrypto.Decrypt(HttpContext.Session.GetString("user"));
                pwd = rsaCrypto.Decrypt(HttpContext.Session.GetString("pwd"));
            }
            catch
            {

            }
            ViewData["PublicKey"] = RSAHelper.PUBLIC_KEY;
            ViewData["databaseType"] = databaseType;
            ViewData["ip"] = ip;
            ViewData["database"] = database;
            ViewData["user"] = user;
            ViewData["ModuleId"] = 6;
            return View();
        }

        /// <summary>
        /// API列表
        /// </summary>
        /// <returns></returns>
        public IActionResult APIList()
        {
            return View();
        }

        ///// <summary>
        ///// api参数配置视图
        ///// </summary>
        ///// <returns></returns>
        //public IActionResult ApiParams()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// 参数类型视图
        ///// </summary>
        ///// <returns></returns>
        //public IActionResult ApiParamsType()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// 运算符视图
        ///// </summary>
        ///// <returns></returns>
        //public IActionResult ApiOperation()
        //{
        //    return View();
        //}

        /// <summary>
        /// 生成数据库连接串，记录数据库信息
        /// </summary>
        /// <param name="database_type"></param>
        /// <param name="ip"></param>
        /// <param name="database"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult SelAPIModule(string database_type, string ip, string database, string user, string pwd)
        {
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string dcrsa_ip = rsaCrypto.Decrypt(ip);
            string dcrsa_database = rsaCrypto.Decrypt(database);
            string dcrsa_user = rsaCrypto.Decrypt(user);
            string dcrsa_pwd = rsaCrypto.Decrypt(pwd);
            HttpContext.Session.SetString("databaseType", database_type);
            HttpContext.Session.SetString("ip", ip);
            HttpContext.Session.SetString("database", database);
            HttpContext.Session.SetString("user", user);
            HttpContext.Session.SetString("pwd", pwd);
            DatabaseAbs dataHelper = apiDataHelper.GetSqlHelper(database_type, dcrsa_ip, dcrsa_database, dcrsa_user, dcrsa_pwd);
            int rs = dataHelper.ChkConn();
            if (rs == 0)
            {
                return Content("");
            }
            dataHelper = null;
            ViewData["tblNameList"] = GetTblNameList();
            ViewData["dbType"] = database_type;
            return View();
        }

        /// <summary>
        /// 向导模式
        /// </summary>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult GuideModel()
        {
            string databaseType = string.Empty;
            string ip = string.Empty;
            string database = string.Empty;
            string user = string.Empty;
            string pwd = string.Empty;
            try
            {
                databaseType = HttpContext.Session.GetString("databaseType");
                ip = HttpContext.Session.GetString("ip");
                database = HttpContext.Session.GetString("database");
                user = HttpContext.Session.GetString("user");
                pwd = HttpContext.Session.GetString("pwd");
            }
            catch
            {

            }
            ViewData["ip"] = ip;
            ViewData["database"] = database;
            ViewData["user"] = user;
            ViewData["tblNameList"] = GetTblNameList();
            ViewData["dbType"] = databaseType;
            ViewData["pwd"] = pwd;
            return View();
        }

        public IActionResult SaveGuidAPI()
        {
            return View();
        }

        public IActionResult SaveGuidAPIAct(string api_code, string api_name, int api_group, string api_descr, int[] api_roles, string id_field,string fields,string table_name,string query_content, string params_str)
        {
            string apiDbType = HttpContext.Session.GetString("databaseType");
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string ip = rsaCrypto.Decrypt(HttpContext.Session.GetString("ip"));
            string database = rsaCrypto.Decrypt(HttpContext.Session.GetString("database"));
            string user = rsaCrypto.Decrypt(HttpContext.Session.GetString("user"));
            string pwd = HttpContext.Session.GetString("pwd");
            string create_user = HttpContext.Session.GetString("userName");
            query_content = query_content == null ? "" : query_content;
            int rs = apiDataHelper.AddGuideAPI(api_code,api_name, api_group, api_descr, api_roles, id_field, fields, table_name, query_content, apiDbType, ip, database, user, pwd, create_user, params_str);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult AddAPIGroup()
        {
            return View();
        }

        public IActionResult AddAPIGroupAct(string api_group_code, string api_group_name, string api_group_descr)
        {
            int rs = apiDataHelper.AddAPIGroupAct( api_group_code,  api_group_name,  api_group_descr,HttpContext.Session.GetString("userName"));
            return Content(rs.ChkNonQuery());
        }

        public IActionResult GetAPIGroup(bool all)
        {
            DataTable tbl = apiDataHelper.GetAPIGroup(all);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

        [ValidateAntiForgeryToken]
        public IActionResult ScriptModel()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MakeSql(string id_field, string fields, string table_name, string page_count, string page_num, string query_content)
        {
            string database_type = HttpContext.Session.GetString("databaseType");
            int pCount = 0;
            int pNum = 0;
            int start = 0;
            int end = 0;
            try
            {
                pCount = Convert.ToInt32(page_count);
                pNum = Convert.ToInt32(page_num);
                start = (pNum - 1) * pCount ;
                end = pNum * pCount;
            }
            catch
            {

            }
            string sql = string.Empty;
            if (database_type == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                if (string.IsNullOrEmpty(query_content))
                {
                    query_content = "";
                }
                sql = string.Format(MyConfig.ConfigManager.sqlServerQuery, id_field, fields, table_name, query_content);
            }
            else
            {
                sql = string.Format(MyConfig.ConfigManager.postgreSqlQuery, fields, table_name, query_content);
            }
            apiSql = sql;
            string jsonStr = string.Empty;
            try
            {
                DataTable tbl = null;
                RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
                string ip = rsaCrypto.Decrypt(HttpContext.Session.GetString("ip"));
                string database = rsaCrypto.Decrypt(HttpContext.Session.GetString("database"));
                string user = rsaCrypto.Decrypt(HttpContext.Session.GetString("user"));
                string pwd = rsaCrypto.Decrypt(HttpContext.Session.GetString("pwd"));
                DatabaseAbs dataHelper = apiDataHelper.GetSqlHelper(database_type, ip, database, user, pwd);
                if (database_type == MyConfig.ConfigManager.DB_TYPE_SQL)
                {
                    SqlParameter startParam = new SqlParameter("@start", start);
                    SqlParameter endParam = new SqlParameter("@end", end);
                    tbl = ((SqlHelper)dataHelper).GetDataTbl(sql, startParam, endParam);
                }
                else
                {
                    Npgsql.NpgsqlParameter pCountParam = new Npgsql.NpgsqlParameter("@pCount", pCount);
                    Npgsql.NpgsqlParameter startParam = new Npgsql.NpgsqlParameter("@start", start);
                    tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(sql, pCountParam, startParam);
                }
                
                jsonStr = JsonConvert.SerializeObject(tbl);
            }
            catch
            {
                jsonStr = "sql error";
            }
            return Content(jsonStr);
        }

        public List<SelectListItem> GetTblNameList()
        {
            //所有数据表和视图下拉列表---------------------------------------------------------
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string databaseType = HttpContext.Session.GetString("databaseType");
            string ip = rsaCrypto.Decrypt(HttpContext.Session.GetString("ip"));
            string database = rsaCrypto.Decrypt(HttpContext.Session.GetString("database"));
            string user = rsaCrypto.Decrypt(HttpContext.Session.GetString("user"));
            string pwd = rsaCrypto.Decrypt(HttpContext.Session.GetString("pwd"));
            DatabaseAbs dataHelper = apiDataHelper.GetSqlHelper(databaseType, ip, database, user, pwd);
            List<SelectListItem> tblList = null;
            string sql = string.Empty;
            DataTable tbl = null;
            if (databaseType == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                sql = @"select name from sysobjects where xtype='U' or xtype='V';";
                tbl = ((SqlHelper)dataHelper).GetDataTbl(sql);
            }
            else if (databaseType == "postgresql")
            {
                sql = @"SELECT tablename as name FROM pg_tables where schemaname='public'
                        union
                        select viewname as name from pg_views where schemaname='public';";
                tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(sql);
            }
            
            tblList = dataHelper.DataTableToList<SelectListItem>(tbl, delegate (DataRow dr)
            {
                return new SelectListItem
                {
                    Text = dr["name"].ToString(),
                    Value = dr["name"].ToString(),
                };
            });
            return tblList;
        }

        [ValidateAntiForgeryToken]
        public string GetDataFields(string tableName)
        {
            //查询表的所有字段---------------------------------------------------------------------------------------
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY,RSAHelper.PUBLIC_KEY);
            string databaseType = HttpContext.Session.GetString("databaseType");
            string ip = rsaCrypto.Decrypt(HttpContext.Session.GetString("ip"));
            string database = rsaCrypto.Decrypt(HttpContext.Session.GetString("database"));
            string user = rsaCrypto.Decrypt(HttpContext.Session.GetString("user"));
            string pwd = rsaCrypto.Decrypt(HttpContext.Session.GetString("pwd"));
            DatabaseAbs dataHelper = apiDataHelper.GetSqlHelper(databaseType, ip, database, user, pwd);
            string sql = string.Empty;
            DataTable tbl = null;
            if (dataHelper.databaseType == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                sql = string.Format(@"select syscolumns.name from syscolumns where id=object_id('{0}')", tableName);
                tbl = ((SqlHelper)dataHelper).GetDataTbl(sql);
            }
            else
            {
                sql = string.Format(@"select column_name as name from information_schema.columns
                                        where table_schema='public' and table_name='{0}';", tableName);
                tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(sql);
            }
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return jsonStr;
        }

    }
}