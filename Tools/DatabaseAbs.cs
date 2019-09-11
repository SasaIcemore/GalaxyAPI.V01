using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Tools.DelegateInfo;

namespace Tools
{
    public abstract class DatabaseAbs
    {
        public string strConn = string.Empty;
        public string databaseType = string.Empty;

        public DatabaseAbs(string databaseType, string ip, string database, string user, string pwd)
        {
            this.databaseType = databaseType;
            if (databaseType == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                strConn = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", ip, database, user, pwd);
            }
            else if (databaseType == MyConfig.ConfigManager.DB_TYPE_PGSQL)
            {
                strConn = string.Format("Host={0};UserName={1};Password={2};Database={3}", ip, user, pwd, database);
            }
        }
        /// <summary>
        /// 是否连接成功
        /// </summary>
        /// <returns></returns>
        public abstract int ChkConn();

        public abstract List<T> DataTableToList<T>(DataTable table, ChangeToModel Func);
    }
}
