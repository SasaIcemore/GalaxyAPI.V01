using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyApi.Model
{
    public class ApiInfo
    {
        public int id { get; set; }
        public string api_name { get; set; }
        public string params_str { get; set; }
        public string request_method { get; set; }
        public string descr { get; set; }
        public int apigroup_id { get; set; }
        public int api_module { get; set; }
        public string create_user { get; set; }
        public string create_tm { get; set; }
        public string update_tm { get; set; }
        public bool is_del { get; set; }
        public string api_db_type { get; set; }
        public string api_db_ip { get; set; }
        public string api_db_name { get; set; }
        public string api_db_user { get; set; }
        public string api_db_pwd { get; set; }
        public string id_field { get; set; }
        public string fields { get; set; }
        public string table_name { get; set; }
        public string query_content { get; set; }
        public string api_code { get; set; }

    }
}
