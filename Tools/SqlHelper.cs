﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Npgsql;
using Tools.DelegateInfo;

namespace Tools.sql
{
    public class SqlHelper : DatabaseAbs,ISqlHelper
    {
        public SqlHelper(string databaseType, string ip, string database, string user, string pwd) : base(databaseType, ip, database, user, pwd)
        {

        }
        public override int ChkConn()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    conn.Open();
                }
                catch
                {
                    return 0;
                }
                if (conn.State == ConnectionState.Open)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// 查询多条记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetDataTbl(string sql, params SqlParameter[] paras)
        {
            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, strConn);
            adapter.SelectCommand.Parameters.AddRange(paras);
            adapter.Fill(table);
            adapter.SelectCommand.Parameters.Clear();
            adapter.Dispose();
            return table;
        }

        /// <summary>
        /// 查询多条记录
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="flag">为了与上一条区分，加了参数，参数值理论上true 或false都行，但这里统一为true</param>
        /// <param name="paramArr">调用时可变参可以传多个参数，但只取第一个</param>
        /// <returns></returns>
        public DataTable GetDataTbl(string sql, bool flag, params ParamsArr[] paramArr)
        {
            DataTable table = new DataTable();
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, strConn);
            adapter.SelectCommand.Parameters.AddRange(paramArr[0].sqlParamArr);
            adapter.Fill(table);
            adapter.SelectCommand.Parameters.Clear();
            adapter.Dispose();
            return table;
        }

        /// <summary>
        /// 获取单行
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="sqlParas"></param>
        /// <returns></returns>
        public dynamic GetData(string sqlstr, SqlReaderModel ReaderModelFunc, params SqlParameter[] sqlParas)
        {
            dynamic model = null;
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(sqlstr, conn);
                command.Parameters.AddRange(sqlParas);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            model = ReaderModelFunc(reader);
                        }
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="sqlParas"></param>
        /// <returns></returns>
        public object GetFristData(string sqlstr, params SqlParameter[] sqlParas)
        {
            object model = null;
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(sqlstr, conn);
                command.Parameters.AddRange(sqlParas);
                model = command.ExecuteScalar();
            }
            return model;
        }


        /// <summary>
        /// 增删改数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int DoNonQuery(string sql, params SqlParameter[] paras)
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(paras);
                int flag = 0;
                try
                {
                    flag = command.ExecuteNonQuery();
                }
                catch
                {
                    flag = -1;
                }
                return flag;
            }
        }

        /// <summary>
        /// 将表转换为数据集合
        /// </summary>
        /// <param name="table"></param>
        /// <param name="changeModelFunc">将表中的行转为对象的委托方法</param>
        /// <returns></returns>
        public override List<T> DataTableToList<T>(DataTable table, ChangeToModel Func)
        {
            List<T> Models = null;
            if (table.Rows.Count > 0)
            {
                Models = new List<T>();
                foreach (DataRow dr in table.Rows)
                {
                    T model = Func(dr);
                    Models.Add(model);
                }
                return Models;
            }
            else
            {
                return null;
            }
        }

        #region 执行事务

        /// <summary>
        /// ExecuteTransaction执行一组SQL语句
        /// </summary>
        /// <param name="sqlList">要执行的SQL语句集合</param>
        /// <param name="earlyTermination">事务中有数据不满足要求是否提前终止事务</param>
        /// <returns></returns>
        public bool ExecuteTransaction(List<string> sqlList, bool earlyTermination)
        {
            return ExecuteTransaction(sqlList, null, earlyTermination);
        }

        /// <summary>
        /// ExecuteTransaction执行一组SQL语句
        /// </summary>
        /// <param name="sqlList">要执行的SQL语句集合 </param>
        /// <param name="parameters">参数数组 </param>
        /// <param name="earlyTermination">事务中有数据不满足要求是否提前终止事务</param>
        /// <returns></returns>
        public bool ExecuteTransaction(List<string> sqlList, SqlParameter[] parameters, bool earlyTermination)
        {
            using (SqlConnection connection = new SqlConnection(strConn))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;  //为命令指定事务

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        try
                        {
                            bool mark = true;//标记值，记录是否有操作失败的
                            foreach (string str in sqlList)
                            {
                                command.CommandText = str;
                                if (earlyTermination)
                                {
                                    if (command.ExecuteNonQuery() <= 0)
                                    {
                                        mark = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    command.ExecuteNonQuery();
                                }
                            }

                            if (!mark)//如果有某一条执行失败，就回滚
                            {
                                transaction.Rollback(); //事务回滚
                                return false;
                            }
                            else
                            {
                                transaction.Commit();   //事务提交
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback(); //事务回滚

                            throw e;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ExecuteTransaction执行一组SQL语句
        /// </summary>
        /// <param name="sqlAndPara">SQL语句和参数的键值对集合</param>
        /// <param name="earlyTermination">事务中有数据不满足要求是否提前终止事务</param>
        /// <returns></returns>
        public bool ExecuteTransaction(Dictionary<string, SqlParameter[]> sqlAndPara, bool earlyTermination)
        {
            using (SqlConnection connection = new SqlConnection(strConn))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;  //为命令指定事务

                        try
                        {
                            if (sqlAndPara != null)
                            {
                                bool mark = true;//标记值，记录是否有操作失败的
                                foreach (KeyValuePair<string, SqlParameter[]> kvp in sqlAndPara)
                                {
                                    command.CommandText = kvp.Key;//取SQL语句
                                    command.Parameters.Clear();//清理多余的参数
                                    if (kvp.Value != null)//添加参数
                                    {
                                        foreach (SqlParameter parameter in kvp.Value)
                                        {
                                            command.Parameters.Add(parameter);
                                        }
                                    }

                                    if (earlyTermination)
                                    {
                                        if (command.ExecuteNonQuery() <= 0)
                                        {
                                            mark = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                }

                                if (!mark)//如果有某一条执行失败，就回滚
                                {
                                    transaction.Rollback(); //回滚
                                    return false;
                                }
                                else
                                {
                                    transaction.Commit();   //提交
                                    return true;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback(); //回滚
                            throw e;
                        }
                    }
                }
            }
        }

        #endregion 执行事务
    }
}
