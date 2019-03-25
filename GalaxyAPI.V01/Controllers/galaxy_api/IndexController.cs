﻿using GalaxyApi;
using GalaxyApi.Model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
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
        private BackManage manage = new BackManage();
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
        /// API列表视图
        /// </summary>
        /// <returns></returns>
        public IActionResult APIList()
        {
            return View();
        }

        /// <summary>
        /// 获取api列表数据
        /// </summary>
        /// <returns></returns>
        public IActionResult GetApiList()
        {
            //获取用户角色
            int role = GetUserRole();
            //获取对应角色未禁用的api
            DataTable apiTbl = apiDataHelper.SelAPIList(role, null, null, null);
            string jsonStr = "";
            if (apiTbl != null)
            {
                jsonStr = JsonConvert.SerializeObject(apiTbl);
            }
            return Content(jsonStr);
        }

        public IActionResult SelApiList(string pcode, string pname, string pgroup)
        {
            //获取用户角色
            int role = GetUserRole();
            //获取对应角色未禁用的api
            DataTable apiTbl = apiDataHelper.SelAPIList(role, pcode, pname, pgroup);
            string jsonStr = "";
            if (apiTbl != null)
            {
                jsonStr = JsonConvert.SerializeObject(apiTbl);
            }
            return Content(jsonStr);
        }

        /// <summary>
        /// 接口详情视图
        /// </summary>
        /// <param name="api_id"></param>
        /// <param name="api_code"></param>
        /// <returns></returns>
        public IActionResult ApiInfo(string api_id, string api_code, bool is_del)
        {
            if (is_del)
            {
                return Content("您没有权限，请联系管理员");
            }
            else
            {
                ViewData["api_id"] = api_id;
                ViewData["api_code"] = api_code;
                return View();
            }
        }

        /// <summary>
        /// api列表属性视图
        /// </summary>
        /// <returns></returns>
        public IActionResult ApiProp()
        {
            return View();
        }

        public IActionResult ApiPropView(int api_id, string api_code, string api_name, bool is_del, int group_id)
        {
            ViewData["api_id"] = api_id;
            ViewData["api_code"] = api_code;
            ViewData["group_id"] = group_id;
            ViewData["api_name"] = api_name;
            ViewData["is_del"] = is_del;
            return View();
        }

        public IActionResult GetApiRoles(int api_id)
        {
            DataTable tbl = apiDataHelper.GetApiRoles(api_id);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

        public IActionResult EditApi(int api_id, bool is_del,int group_id,int[] roles)
        {
            int userId = manage.GetUserId(HttpContext.Session.GetString("userName"), HttpContext.Session.GetString("password"));
            DataTable tbl = manage.GetUserInfo(false, userId);
            int role_id = -1;
            if (tbl!=null)
            {
                role_id = tbl.Rows[0]["role_id"].ChkDBNullToInt();
            }
            int rs = apiDataHelper.EditApi(api_id,is_del,group_id,roles, HttpContext.Session.GetString("userName"), role_id);
            return Content(rs.ChkNonQuery());
        }
        /// <summary>
        /// 测试通用接口
        /// </summary>
        /// <param name="apiCode"></param>
        /// <param name="pCount"></param>
        /// <param name="pNum"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult TestApiGalaxy(string apiCode, string pCount, string pNum, string filter)
        {
            IdentityServerResult idrs = null;
            string token = string.Empty;
            string apiResult = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(MyConfig.ConfigManager.AUTHORITY);
                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("client_id", "glxApi002"),
                    new KeyValuePair<string, string>("password", HttpContext.Session.GetString("password")),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_secret", "console.write"),
                    new KeyValuePair<string, string>("userName", HttpContext.Session.GetString("userName"))
                    });
                var t = client.PostAsync("/connect/token", content);
                t.Wait();
                var result = t.Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                idrs = JsonConvert.DeserializeObject<IdentityServerResult>(resultContent);
                token = idrs.access_token;
            }
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(MyConfig.ConfigManager.LOCAL);
                client.SetBearerToken(token);

                var content1 = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("apiCode", apiCode),
                    new KeyValuePair<string, string>("pCount", pCount),
                    new KeyValuePair<string, string>("pNum", pNum),
                    new KeyValuePair<string, string>("filter",filter)
                    });
                var t1 = client.PostAsync("/api/galaxy", content1);
                t1.Wait();
                var result1 = t1.Result;
                var resultContent1 = result1.Content.ReadAsStringAsync().Result;
                apiResult = resultContent1;
            }
            return Content(apiResult);
        }

        /// <summary>
        /// 获取api对应的参数
        /// </summary>
        /// <param name="api_info_id"></param>
        /// <returns></returns>
        public IActionResult GetApiParamsByApiId(int api_info_id)
        {
            DataTable tbl = apiDataHelper.GetApiParamsByApiId(api_info_id);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

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

        private int GetUserRole()
        {
            //获取用户id和用户角色
            int userId = manage.GetUserId(HttpContext.Session.GetString("userName"), HttpContext.Session.GetString("password"));
            DataTable userTbl = manage.GetUserInfo(false, userId);
            int role = 0;
            if (userTbl != null)
            {
                if (userTbl.Rows.Count > 0)
                {
                    role = userTbl.Rows[0]["role_id"].ChkDBNullToInt();
                }
            }
            return role;
        }

    }
}