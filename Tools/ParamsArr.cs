using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Data.SqlClient;

namespace Tools
{
    public class ParamsArr
    {
        private int length = 0;
        public NpgsqlParameter[] npgsqlParamArr = null;
        public SqlParameter[] sqlParamArr = null;

        public ParamsArr(){}

        public ParamsArr(int length, string dbType)
        {
            this.length = length;
            if (dbType == MyConfig.ConfigManager.DB_TYPE_SQL)
            {
                sqlParamArr = new SqlParameter[this.length];
            }
            else
            {
                npgsqlParamArr = new NpgsqlParameter[this.length];
            }
        }

        


    }
}
