using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Tools.DelegateInfo
{
    public delegate dynamic ChangeToModel(DataRow dr);
    public delegate dynamic NpgReaderModel(NpgsqlDataReader reader);
    public delegate dynamic SqlReaderModel(SqlDataReader reader);
}
