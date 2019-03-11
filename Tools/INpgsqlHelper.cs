using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Tools.DelegateInfo;

namespace Tools
{
    public interface INpgsqlHelper
    {
         DataTable GetDataTbl(string sql, params NpgsqlParameter[] paras);

         dynamic GetData(string sqlstr, NpgReaderModel ReaderModelFunc, params NpgsqlParameter[] sqlParas);

        object GetFristData(string sqlstr, params NpgsqlParameter[] sqlParas);

        int DoNonQuery(string sql, params NpgsqlParameter[] paras);
    }
}
