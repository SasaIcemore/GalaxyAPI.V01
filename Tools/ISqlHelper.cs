using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Tools.DelegateInfo;

namespace Tools
{
    public interface ISqlHelper
    {
        /// <summary>
        /// 返回数据表
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        DataTable GetDataTbl(string sql, params SqlParameter[] paras);


        /// <summary>
        /// 获取单行
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="ReaderModelFunc"></param>
        /// <param name="sqlParas"></param>
        /// <returns></returns>
        dynamic GetData(string sqlstr, SqlReaderModel ReaderModelFunc, params SqlParameter[] sqlParas);

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="sqlParas"></param>
        /// <returns></returns>
        object GetFristData(string sqlstr, params SqlParameter[] sqlParas);


        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        int DoNonQuery(string sql, params SqlParameter[] paras);

        /// <summary>
        /// ExecuteTransaction执行一组SQL语句
        /// </summary>
        /// <param name="sqlList">要执行的SQL语句集合</param>
        /// <param name="earlyTermination">事务中有数据不满足要求是否提前终止事务</param>
        /// <returns></returns>
        /// <returns></returns>
        bool ExecuteTransaction(List<string> sqlList, bool earlyTermination);
    }
}
