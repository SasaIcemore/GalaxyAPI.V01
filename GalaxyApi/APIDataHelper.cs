using GalaxyApi.Model;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tools;
using Tools.pgsql;
using Tools.RSA;
using Tools.sql;

namespace GalaxyApi
{
    public class APIDataHelper
    {
        public DatabaseAbs dataHelper = null;
        private string sql = string.Empty;
        /// <summary>
        /// 生成连接对象
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="ip"></param>
        /// <param name="database"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public DatabaseAbs GetSqlHelper(string databaseType, string ip, string database, string user, string pwd)
        {
            if (databaseType == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                dataHelper = new SqlHelper(databaseType, ip, database, user, pwd);
            }
            else if (databaseType == MyConfig.ConfigManager.DB_TYPE_PGSQL)
            {
                dataHelper = new NpgsqlHelper(databaseType, ip, database, user, pwd);
            }
            return dataHelper;
        }

        /// <summary>
        /// 添加api分组
        /// </summary>
        /// <param name="api_group_code">分组代码</param>
        /// <param name="api_group_name">分组名称</param>
        /// <param name="api_group_descr">描述</param>
        /// <param name="create_user">创建人</param>
        /// <returns>int 受影响行数</returns>
        public int AddAPIGroupAct(string api_group_code, string api_group_name, string api_group_descr, string create_user)
        {
            sql = @"insert into public.api_group (
                                                        group_code, 
                                                        group_name, 
                                                        descr, 
                                                        create_user, 
                                                        create_tm, 
                                                        is_del) 
                                                        values (
                                                        @group_code, 
                                                        @group_name, 
                                                        @descr, 
                                                        @create_user, 
                                                        now(), 
                                                        false)";

            NpgsqlParameter codeParam = new NpgsqlParameter("@group_code", api_group_code);
            NpgsqlParameter nameParam = new NpgsqlParameter("@group_name", api_group_name);
            NpgsqlParameter descrParam = new NpgsqlParameter("@descr", api_group_descr);
            NpgsqlParameter userParam = new NpgsqlParameter("@create_user", create_user);
            int rs = MyConfig.ConfigManager.dataHelper.DoNonQuery(sql, codeParam, nameParam, descrParam, userParam);
            return rs;
        }

        /// <summary>
        /// 获取API分组
        /// </summary>
        /// <param name="all">是否显示所有分组，all==false 不显示已删除的</param>
        /// <returns></returns>
        public DataTable GetAPIGroup(bool all)
        {
            if (all)
            {
                sql = @"select * from public.api_group";
            }
            else
            {
                sql = @"select * from public.api_group where is_del=false";
            }
            return MyConfig.ConfigManager.dataHelper.GetDataTbl(sql);
        }

        /// <summary>
        /// 添加api
        /// </summary>
        /// <param name="api_name"></param>
        /// <param name="api_group"></param>
        /// <param name="api_descr"></param>
        /// <param name="api_roles"></param>
        /// <param name="id_field"></param>
        /// <param name="fields"></param>
        /// <param name="table_name"></param>
        /// <param name="query_content"></param>
        /// <param name="apiDbType"></param>
        /// <param name="ip"></param>
        /// <param name="db_name"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="create_user"></param>
        /// <returns></returns>
        public int AddGuideAPI(string api_code, string api_name, int api_group, string api_descr, int[] api_roles, string id_field, string fields, string table_name, string query_content, string apiDbType, string ip, string db_name, string user, string pwd,string create_user)
        {
            try
            {
                string insert_api_info = string.Format(@"insert into public.api_info (
                                                                    api_code,
                                                                    api_name,
                                                                    descr,
                                                                    apigroup_id,
                                                                    api_module,
                                                                    create_user,
                                                                    create_tm,
                                                                    is_del,
                                                                    api_db_type,
                                                                    api_db_ip,
                                                                    api_db_name,
                                                                    api_db_user,
                                                                    api_db_pwd,
                                                                    id_field,
                                                                    fields,
                                                                    table_name,
                                                                    query_content
                                                        ) values (
                                                                    @api_code, 
                                                                    @api_name, 
                                                                    @api_descr,
                                                                    {0},'0','{1}',now(),false,'{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',
                                                                    @query_content) RETURNING id",
                                                                    api_group,
                                                                    create_user,
                                                                    apiDbType,
                                                                    ip,
                                                                    db_name,
                                                                    user,
                                                                    pwd,
                                                                    id_field,
                                                                    fields,
                                                                    table_name
                                                                    );
                NpgsqlParameter apiCodeParam = new NpgsqlParameter("@api_code", api_code);
                NpgsqlParameter queryParam = new NpgsqlParameter("@query_content", query_content);
                NpgsqlParameter apiNameParam = new NpgsqlParameter("@api_name", api_name);
                NpgsqlParameter descrParam = new NpgsqlParameter("@api_descr", api_descr);
                int api_info_id = MyConfig.ConfigManager.dataHelper.GetFristData(insert_api_info, apiCodeParam, apiNameParam, descrParam, queryParam).ChkDBNullToInt();//获取刚插入的id

                List<string> sqlList = new List<string>();
                if (api_roles != null && api_roles.Length > 0)
                {
                    string sql = string.Empty;
                    foreach (int i in api_roles)
                    {
                        sql = string.Format(@"insert into public.api_role(api_id, role_id, create_tm, is_del) values({0},{1},now(),false)", api_info_id, i);
                        sqlList.Add(sql);
                    }
                }
                MyConfig.ConfigManager.dataHelper.ExecuteTransaction(sqlList, true);
                if (api_info_id > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据api代码查询api
        /// </summary>
        /// <param name="api_code"></param>
        /// <param name="p_count"></param>
        /// <param name="p_num"></param>
        /// <returns></returns>
        public DataTable GetGuideAPIByCode(string api_code, int p_count, int p_num)
        {
            //保存的api_db_pwd是加密字符，使用时需RSACrypto解密
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string sql = @"select * from public.api_info where api_code=@api_code";
            NpgsqlParameter codeParam = new NpgsqlParameter("@api_code", api_code);
            ApiInfo apiInfo = MyConfig.ConfigManager.dataHelper.GetData(sql, 
                delegate (NpgsqlDataReader dr) {
                    return new ApiInfo
                    {
                        id = dr["id"].ChkDBNullToInt(),
                        api_name = dr["api_name"].ChkDBNullToStr(),
                        descr = dr["descr"].ChkDBNullToStr(),
                        apigroup_id = dr["apigroup_id"].ChkDBNullToInt(),
                        create_user = dr["create_user"].ChkDBNullToStr(),
                        is_del = dr["is_del"].ChkDBNullToBool(),
                        api_db_type = dr["api_db_type"].ChkDBNullToStr(),
                        api_db_ip = dr["api_db_ip"].ChkDBNullToStr(),
                        api_db_name = dr["api_db_name"].ChkDBNullToStr(),
                        api_db_user = dr["api_db_user"].ChkDBNullToStr(),
                        api_db_pwd = rsaCrypto.Decrypt(dr["api_db_pwd"].ChkDBNullToStr()),
                        id_field = dr["id_field"].ChkDBNullToStr(),
                        fields = dr["fields"].ChkDBNullToStr(),
                        table_name = dr["table_name"].ChkDBNullToStr(),
                        query_content = dr["query_content"].ChkDBNullToStr(),
                        api_code = dr["api_code"].ChkDBNullToStr()
                    };
                },codeParam);
            if (apiInfo != null)
            {
                dataHelper = GetSqlHelper(apiInfo.api_db_type, apiInfo.api_db_ip, apiInfo.api_db_name, apiInfo.api_db_user, apiInfo.api_db_pwd);
                string selSql = string.Empty;
                DataTable tbl = null;
                int start = (p_num - 1) * p_count;
                int end = p_num * p_count;
                if (apiInfo.api_db_type == MyConfig.ConfigManager.DB_TYPE_SQL)
                {
                    selSql = string.Format(MyConfig.ConfigManager.sqlServerQuery, apiInfo.id_field, apiInfo.fields, apiInfo.table_name, apiInfo.query_content);
                    SqlParameter startParam = new SqlParameter("@start", start);
                    SqlParameter endParam = new SqlParameter("@end", end);
                    ParamsArr paraArr = new ParamsArr(2, MyConfig.ConfigManager.DB_TYPE_SQL);
                    paraArr.sqlParamArr[0] = startParam;
                    paraArr.sqlParamArr[1] = endParam;
                    tbl = ((SqlHelper)dataHelper).GetDataTbl(selSql, startParam, endParam);
                    //tbl = ((SqlHelper)dataHelper).GetDataTbl(selSql,true,paraArr);
                }
                else
                {
                    selSql = string.Format(MyConfig.ConfigManager.postgreSqlQuery, apiInfo.fields, apiInfo.table_name, apiInfo.query_content);
                    NpgsqlParameter pCountParam = new NpgsqlParameter("@pCount", p_count);
                    NpgsqlParameter startParam = new NpgsqlParameter("@start", start);

                    ParamsArr paraArr = new ParamsArr(2, MyConfig.ConfigManager.DB_TYPE_PGSQL);
                    paraArr.npgsqlParamArr[0] = startParam;
                    paraArr.npgsqlParamArr[1] = pCountParam;
                    tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(selSql, pCountParam, startParam);
                    //tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(selSql, true, paraArr);
                }
                return tbl;
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// 判断是否有权限使用api
        /// </summary>
        /// <param name="user_role_id">用户的角色id</param>
        /// <param name="api_code">api代码</param>
        /// <returns></returns>
        public bool IsGetApi(int user_role_id, string api_code)
        {
            bool canUse = false;
            //查询可使用api的角色,api须是启用状态，is_del=false
            string sql = @"select role_id from public.api_role a
                            left join public.api_info b on a.api_id = b.id
                            where b.api_code=@api_code and b.is_del = false";
            NpgsqlParameter codeParam = new NpgsqlParameter("@api_code", api_code);
            DataTable tbl = MyConfig.ConfigManager.dataHelper.GetDataTbl(sql,codeParam);
            if (tbl != null && tbl.Rows.Count > 0)
            {
                //匹配role_id,匹配一个就return
                foreach (DataRow dr in tbl.Rows)
                {
                    if (dr["role_id"].ChkDBNullToInt() == user_role_id)
                    {
                        canUse = true;
                        return canUse;
                    }
                }
            }
            return canUse;
        }
    }
}
