using GalaxyApi.Model;
using Npgsql;
using System;
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
        public int AddGuideAPI(string api_code, string api_name, int api_group, string api_descr, int[] api_roles, string id_field, string fields, string table_name, string query_content, string apiDbType, string ip, string db_name, string user, string pwd,string create_user, string params_str)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(MyConfig.ConfigManager.strConn))
            {
                connection.Open();
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;  //为命令指定事务
                        try
                        {
                            #region 新增api_info sql
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
                            command.Parameters.Add(apiCodeParam);
                            command.Parameters.Add(queryParam);
                            command.Parameters.Add(apiNameParam);
                            command.Parameters.Add(descrParam);
                            command.CommandText = insert_api_info;
                            #endregion
                            try
                            {
                                int api_info_id = command.ExecuteScalar().ChkDBNullToInt();
                                string sql = string.Empty;
                                bool flag = false;//执行成功的标记
                                if (api_roles != null)
                                {
                                    foreach (int i in api_roles)
                                    {
                                        sql = string.Format(@"insert into public.api_role(api_id, role_id, create_tm, create_user) values({0},{1},now(),'{2}')", api_info_id, i, create_user);
                                        command.CommandText = sql;
                                        try
                                        {
                                            int rs = command.ExecuteNonQuery();
                                            if (rs <= 0)
                                            {
                                                flag = false;
                                                transaction.Rollback();
                                            }
                                            else
                                            {
                                                flag = true;
                                            }
                                        }
                                        catch
                                        {
                                            flag = false;
                                            transaction.Rollback(); 
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(params_str))
                                {
                                    try
                                    {
                                        List<APIParamsInsert> paramList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<APIParamsInsert>>(params_str);
                                        foreach (APIParamsInsert param in paramList)
                                        {
                                            #region 新增api_params
                                            sql = string.Format(@"insert into public.api_params (
                                                                                api_id, 
                                                                                params_name, 
                                                                                params_type, 
                                                                                params_descr, 
                                                                                create_user
                                                                                ) values (
                                                                                {0}, 
                                                                                @params_name, 
                                                                                @params_type, 
                                                                                @params_descr, 
                                                                                '{1}')", api_info_id, create_user);
                                            Npgsql.NpgsqlParameter nameParam = new NpgsqlParameter("@params_name", param.param_name);
                                            Npgsql.NpgsqlParameter typeParam = new NpgsqlParameter("@params_type", param.param_type);
                                            Npgsql.NpgsqlParameter descr_Param = new NpgsqlParameter("@params_descr", param.param_descr);
                                            command.Parameters.Add(nameParam);
                                            command.Parameters.Add(typeParam);
                                            command.Parameters.Add(descr_Param);
                                            command.CommandText = sql;
                                            #endregion
                                            try
                                            {
                                                int rs = command.ExecuteNonQuery();
                                                if (rs <= 0)
                                                {
                                                    flag = false;
                                                    transaction.Rollback();
                                                }
                                                else
                                                {
                                                    flag = true;
                                                }
                                            }
                                            catch
                                            {
                                                flag = false;
                                                transaction.Rollback();
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        transaction.Rollback();
                                    }
                                }
                                //标记值为true，没有执行错误
                                if (flag)
                                {
                                    transaction.Commit();
                                    return 1;
                                }
                                else
                                {
                                    transaction.Rollback();
                                }
                            }
                            catch
                            {
                                transaction.Rollback(); 
                            }
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback(); 
                            throw e;
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 根据api代码查询api
        /// </summary>
        /// <param name="api_code"></param>
        /// <param name="p_count"></param>
        /// <param name="p_num"></param>
        /// <returns></returns>
        public DataTable GetGuideAPIByCode(string api_code, int p_count, int p_num, List<APIParamsFilter> paralist)
        {
            //保存的api_db_pwd是加密字符，使用时需RSACrypto解密
            RSACrypto rsaCrypto = new RSACrypto(RSAHelper.PRIVATE_KEY, RSAHelper.PUBLIC_KEY);
            string sql = @"select * from public.api_info where api_code=@api_code";
            NpgsqlParameter codeParam = new NpgsqlParameter("@api_code", api_code);
            //获取api信息
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
                        api_code = dr["api_code"].ChkDBNullToStr()
                    };
                },codeParam);
            //如果找到了对应的api
            if (apiInfo != null)
            {
                dataHelper = GetSqlHelper(apiInfo.api_db_type, apiInfo.api_db_ip, apiInfo.api_db_name, apiInfo.api_db_user, apiInfo.api_db_pwd);
                string selSql = string.Empty;
                DataTable tbl = null;
                int start = (p_num - 1) * p_count;
                int end = p_num * p_count;

                //处理过滤参数
                string query_filter = string.Empty;
                ParamsArr paraArr = null;
                //如果有参数传入
                if (paralist != null)
                {
                    int length = paralist.Count + 2;
                    if (apiInfo.api_db_type == MyConfig.ConfigManager.DB_TYPE_SQL)
                    {
                        query_filter = GetFilterStr(paralist);
                        paraArr = GetParamsArr(MyConfig.ConfigManager.DB_TYPE_SQL, paralist);
                        SqlParameter startParam = new SqlParameter("@start", start);
                        SqlParameter endParam = new SqlParameter("@end", end);
                        paraArr.sqlParamArr[length - 2] = startParam;
                        paraArr.sqlParamArr[length - 1] = endParam;
                        selSql = string.Format(MyConfig.ConfigManager.sqlServerQuery, apiInfo.id_field, apiInfo.fields, apiInfo.table_name, query_filter);
                        tbl = ((SqlHelper)dataHelper).GetDataTbl(selSql, true, paraArr);
                    }
                    else
                    {
                        query_filter = GetFilterStr(paralist);
                        paraArr = GetParamsArr(MyConfig.ConfigManager.DB_TYPE_PGSQL, paralist);
                        NpgsqlParameter pCountParam = new NpgsqlParameter("@pCount", p_count);
                        NpgsqlParameter startParam = new NpgsqlParameter("@start", start);
                        paraArr.npgsqlParamArr[length - 2] = startParam;
                        paraArr.npgsqlParamArr[length - 1] = pCountParam;
                        selSql = string.Format(MyConfig.ConfigManager.postgreSqlQuery, apiInfo.fields, apiInfo.table_name, query_filter);
                        tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(selSql, true, paraArr);
                    }
                }
                //没有参数传入
                else
                {
                    if (apiInfo.api_db_type == MyConfig.ConfigManager.DB_TYPE_SQL)
                    {
                        selSql = string.Format(MyConfig.ConfigManager.sqlServerQuery, apiInfo.id_field, apiInfo.fields, apiInfo.table_name, apiInfo.query_content);
                        SqlParameter startParam = new SqlParameter("@start", start);
                        SqlParameter endParam = new SqlParameter("@end", end);
                        tbl = ((SqlHelper)dataHelper).GetDataTbl(selSql, startParam, endParam);
                    }
                    else
                    {
                        selSql = string.Format(MyConfig.ConfigManager.postgreSqlQuery, apiInfo.fields, apiInfo.table_name, apiInfo.query_content);
                        NpgsqlParameter pCountParam = new NpgsqlParameter("@pCount", p_count);
                        NpgsqlParameter startParam = new NpgsqlParameter("@start", start);
                        tbl = ((NpgsqlHelper)dataHelper).GetDataTbl(selSql, pCountParam, startParam);
                    }
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

        /// <summary>
        /// 生成过滤字符串
        /// </summary>
        /// <param name="paralist">参数集合</param>
        /// <returns></returns>
        private string GetFilterStr(List<APIParamsFilter> paralist)
        {
            string query_filter = string.Empty;
            foreach (APIParamsFilter filter in paralist)
            {
                query_filter += filter.Logic + " " + filter.ParamName + " " + filter.Operation + " " + "@" + filter.ParamName + " ";
            }
            return query_filter;
        }

        /// <summary>
        /// 生成参数数组
        /// </summary>
        /// <param name="db_type">数据库类型</param>
        /// <param name="paralist">参数集合</param>
        /// <returns></returns>
        private ParamsArr GetParamsArr(string db_type, List<APIParamsFilter> paralist)
        {
            ParamsArr paraArr = null;
            if (paralist != null)
            {
                paraArr = new ParamsArr(paralist.Count + 2, db_type);
                int flag = 0;
                //sqlServer
                if (db_type == MyConfig.ConfigManager.DB_TYPE_SQL)
                {
                    foreach (APIParamsFilter filter in paralist)
                    {
                        SqlParameter para = null;
                        if (filter.Operation == "like")
                        {
                            para = new SqlParameter("@" + filter.ParamName, "%" + FormatFilterValue(filter.ValueType, filter.Value.Trim()) + "%");
                        }
                        else
                        {
                            para = new SqlParameter("@" + filter.ParamName, FormatFilterValue(filter.ValueType, filter.Value.Trim()));
                        }
                        if (para != null)
                        {
                            paraArr.sqlParamArr[flag] = para;
                        }
                        flag++;
                    }
                }
                else
                {
                    foreach (APIParamsFilter filter in paralist)
                    {
                        NpgsqlParameter para = null;
                        if (filter.Operation == "like")
                        {
                            para = new NpgsqlParameter("@" + filter.ParamName, "%" + FormatFilterValue(filter.ValueType, filter.Value.Trim()) + "%" );
                        }
                        else
                        {
                            para = new NpgsqlParameter("@" + filter.ParamName, FormatFilterValue(filter.ValueType, filter.Value.Trim()));
                        }
                        if (para != null)
                        {
                            paraArr.npgsqlParamArr[flag] = para;
                        }
                        flag++;
                    }
                }
            }
            return paraArr;
        }

        /// <summary>
        /// 处理参数类型
        /// </summary>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private dynamic FormatFilterValue(string valueType, string value)
        {
            dynamic val = null;
            switch (valueType)
            {
                case "string":
                    val = value;
                    break;
                case "int":
                    val = Convert.ToInt32(value);
                    break;
                case "bool":
                    val = Convert.ToBoolean(value);
                    break;
                case "double":
                    val = Convert.ToDouble(value);
                    break;
            }
            return val;
        }

     
        /// <summary>
        /// 根据条件查询api
        /// </summary>
        /// <param name="role_id">角色id</param>
        /// <param name="pcode">api代码</param>
        /// <param name="pname">api名称</param>
        /// <param name="pgroup">api分组</param>
        /// <returns></returns>
        public DataTable SelAPIList(int role_id,string pcode,string pname,string pgroup)
        {
            sql = @"select a.id,a.api_code,api_name,a.descr,c.id as group_id,group_name,a.is_del as is_api_del 
                    from public.api_info a
                    left join public.api_role b on a.id=b.api_id
                    left join public.api_group c on a.apigroup_id = c.id
                    where role_id=@role_id ";
            if (!string.IsNullOrEmpty(pcode))
            {
                sql += " and a.api_code like @pcode";
            }
            else
            {
                pcode = "";
            }
            if (!string.IsNullOrEmpty(pname))
            {
                sql += " and api_name like @pname";
            }
            else
            {
                pname = "";
            }
            if (!string.IsNullOrEmpty(pgroup))
            {
                sql += " and group_name like @pgroup";
            }
            else
            {
                pgroup = "";
            }
            sql += " order by a.id desc";
            NpgsqlParameter ridParam = new NpgsqlParameter("@role_id", role_id);
            NpgsqlParameter codeParam = new NpgsqlParameter("@pcode", "%"+ pcode + "%");
            NpgsqlParameter nameParam = new NpgsqlParameter("@pname", "%" + pname + "%");
            NpgsqlParameter groupParam = new NpgsqlParameter("@pgroup", "%" + pgroup + "%");
            DataTable tbl = MyConfig.ConfigManager.dataHelper.GetDataTbl(sql, ridParam, codeParam, nameParam, groupParam);
            return tbl;
        }

        /// <summary>
        /// 根据api的id获取api参数
        /// </summary>
        /// <param name="api_id"></param>
        /// <returns></returns>
        public DataTable GetApiParamsByApiId(int api_id)
        {
            sql = @"select params_name,params_descr,params_type from public.api_params where api_id=@api_id";
            NpgsqlParameter idParam = new NpgsqlParameter("@api_id", api_id);
            DataTable tbl = MyConfig.ConfigManager.dataHelper.GetDataTbl(sql, idParam);
            return tbl;
        }

        public DataTable GetApiRoles(int api_id)
        {
            string sql = @"select * from public.api_role where api_id=@api_id";
            NpgsqlParameter idParam = new NpgsqlParameter("@api_id", api_id);
            DataTable tbl = MyConfig.ConfigManager.dataHelper.GetDataTbl(sql, idParam);
            return tbl;
        }

        /// <summary>
        /// 修改api属性和所属角色
        /// </summary>
        /// <param name="api_id"></param>
        /// <param name="is_del"></param>
        /// <param name="group_id"></param>
        /// <param name="roles"></param>
        /// <param name="create_user"></param>
        /// <returns></returns>
        public int EditApi(int api_id, bool is_del, int group_id, int[] roles, string create_user,int user_role_id)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(MyConfig.ConfigManager.strConn))
            {
                connection.Open();
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;  //为命令指定事务
                        bool flag = false;//执行成功的标记
                        try
                        {
                            #region 修改public.api_info
                            string update_api_info = string.Format(@"update public.api_info set is_del=@is_del,apigroup_id=@group_id where id=@api_id");
                            NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
                            NpgsqlParameter gidParam = new NpgsqlParameter("@group_id", group_id);
                            NpgsqlParameter idParam = new NpgsqlParameter("@api_id", api_id);
                            command.Parameters.Add(delParam);
                            command.Parameters.Add(gidParam);
                            command.Parameters.Add(idParam);
                            command.CommandText = update_api_info;
                            #endregion
                            int api_info_rs = command.ExecuteNonQuery();
                            //修改public.api_info 执行成功
                            if (api_info_rs > 0)
                            {
                                flag = true;
                                if (roles.Length > 0)
                                {
                                    //修改public.api_role
                                    string api_role_sql = @"select * from public.api_role where api_id=@api_id";
                                    NpgsqlParameter apiIdParam = new NpgsqlParameter("@api_id", api_id);
                                    DataTable api_role_tbl = MyConfig.ConfigManager.dataHelper.GetDataTbl(api_role_sql, apiIdParam);
                                    if (api_role_tbl != null)
                                    {
                                        if (api_role_tbl.Rows.Count > 0)
                                        {
                                            bool exist = true;//表里是否存在要配置的角色
                                            List<int> delRoleList = new List<int>();//表里原有的，但现在需要删除的角色
                                            List<int> tblApiRoleList = new List<int>();//记录表里的角色
                                            for (int i = 0; i < roles.Length; i++)
                                            {
                                                //判断角色在表里有无记录
                                                foreach (DataRow dr in api_role_tbl.Rows)
                                                {
                                                    int tblRoleId = dr["role_id"].ChkDBNullToInt();
                                                    //已存在
                                                    if (roles[i] == tblRoleId)
                                                    {
                                                        exist = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        exist = false;
                                                    }
                                                }
                                                //添加授权增加的角色
                                                if (exist == false)
                                                {
                                                    string add_role_sql = string.Format(@"insert into public.api_role (api_id, role_id, create_tm, create_user) 
                                                                                values ({0},{1},now(),'{2}')", api_id, roles[i], create_user);
                                                    command.CommandText = add_role_sql;
                                                    int add_role_rs = command.ExecuteNonQuery();
                                                    if (add_role_rs > 0)
                                                    {
                                                        flag = true;
                                                    }
                                                    else
                                                    {
                                                        flag = false;
                                                        transaction.Rollback();
                                                    }
                                                }
                                            }
                                            foreach (DataRow dr in api_role_tbl.Rows)
                                            {
                                                int tblRoleId = dr["role_id"].ChkDBNullToInt();
                                                tblApiRoleList.Add(tblRoleId);
                                            }
                                            List<int> rolesList = new List<int>(roles);
                                            foreach (int delRole in tblApiRoleList)
                                            {
                                                if (!rolesList.Contains(delRole))
                                                {
                                                    delRoleList.Add(delRole);
                                                }
                                            }
                                            //需要删除的角色id
                                            if (delRoleList.Count > 0)
                                            {
                                                foreach (int role_id in delRoleList)
                                                {
                                                    if (role_id == user_role_id)
                                                    {
                                                        //不能删除自己所属的api
                                                        continue;
                                                    }
                                                    string del_role_sql = string.Format(@"delete from public.api_role where api_id={0} and role_id={1}", api_id, role_id);
                                                    command.CommandText = del_role_sql;
                                                    int del_role_rs = command.ExecuteNonQuery();
                                                    if (del_role_rs > 0)
                                                    {
                                                        flag = true;
                                                    }
                                                    else
                                                    {
                                                        flag = false;
                                                        transaction.Rollback();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            throw e;
                        }
                        //标记值为true，没有执行错误
                        if (flag)
                        {
                            transaction.Commit();
                            return 1;
                        }
                        else
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                }
            }
        }
    }
}
