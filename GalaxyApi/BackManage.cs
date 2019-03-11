using MyConfig;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tools;
using Tools.pgsql;

namespace GalaxyApi
{
    public class BackManage
    {
        private NpgsqlHelper dataHelper = ConfigManager.dataHelper;
        private string sql = string.Empty;

        /// <summary>
        /// 获取用户id
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public int GetUserId(string userName, string pwd)
        {
            sql = @"select id from public." + "\"" + "user" + "\" where name=@name and pwd=@pwd; ";
            NpgsqlParameter nameParam = new NpgsqlParameter("@name", userName);
            NpgsqlParameter pwdParam = new NpgsqlParameter("@pwd", pwd);
            object rs = dataHelper.GetData(sql,
                delegate(NpgsqlDataReader reader) {
                    return new { Id = reader["id"].ToString() };
                }, nameParam, pwdParam);
            if (rs != null)
            {
                return int.Parse(rs.GetType().GetProperty("Id").GetValue(rs).ToString());
            }
            else
            {
                return -1;
            }
        }
        public int ChangePWD(string pwd ,int userId)
        {
            sql = @"update public."+"\""+"user"+"\" set pwd=@pwd where id=@id; ";
            NpgsqlParameter pwdParam = new NpgsqlParameter("pwd", pwd);
            NpgsqlParameter idParam = new NpgsqlParameter("id", userId);
            int rs = dataHelper.DoNonQuery(sql,pwdParam,idParam);
            return rs;
        }

        public DataTable GetUserInfo(bool all,params int[] user_id)
        {
            DataTable tbl = null;
            if (all)
            {
                sql = @"SELECT a.id,name,role_id,a.create_tm,dept_id,dept_name,a.is_del FROM public.user a
                        left join public.dept b on a.dept_id=b.id
                        ORDER BY a.id ASC";
                tbl = dataHelper.GetDataTbl(sql);
            }
            else
            {
                sql = @"SELECT a.id,name,role_id,a.create_tm,a.is_del FROM public.user a
                        where a.is_del=false and a.id=@user_id
                        ORDER BY a.id ASC";
                NpgsqlParameter uidParam = new NpgsqlParameter("@user_id", user_id[0]);
                tbl = dataHelper.GetDataTbl(sql, uidParam);
            }
            return tbl;
        }

        public int AddUser(string user_name, string dept_name, int role_id, string pwd)
        {
            sql = @"insert into public.user (name, pwd, role_id, is_del) values (@name, @pwd, @role_id, false)";
            NpgsqlParameter nameParam = new NpgsqlParameter("@name", user_name);
            NpgsqlParameter pwdParam = new NpgsqlParameter("@pwd", pwd);
            NpgsqlParameter roleIdParam = new NpgsqlParameter("@role_id", role_id);
            int rs = dataHelper.DoNonQuery(sql,nameParam,pwdParam,roleIdParam);
            return rs;
        }

        public int EditUser(int user_id, string dept_name, int role,string pwd, bool is_del)
        {
            if (string.IsNullOrEmpty(pwd))
            {
                sql = @"update public.user set role_id=@role, is_del=@is_del where id=@id";
            }
            else
            {
                sql = @"update public.user set pwd=@pwd, role_id=@role, is_del=@is_del where id=@id";
            }
             
            NpgsqlParameter pwdParam = new NpgsqlParameter("@pwd", pwd);
            NpgsqlParameter roleParam = new NpgsqlParameter("@role", role);
            NpgsqlParameter idParam = new NpgsqlParameter("@id", user_id);
            NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
            int rs = dataHelper.DoNonQuery(sql, pwdParam, roleParam, idParam, delParam);
            return rs;
        }

        public DataTable GetRoles(bool all_role)
        {
            if (all_role)
            {
                sql = @"select id, role_name, is_del from public.role";
            }
            else
            {
                sql = @"select id, role_name from public.role where is_del=false ";
            }
            DataTable tbl = dataHelper.GetDataTbl(sql);
            return tbl;
        }

        public int AddRoleAct(string role_name)
        {
            sql = @"insert into public.role (role_name,is_del) values (@role_name,false)";
            NpgsqlParameter nameParam = new NpgsqlParameter("@role_name",role_name);
            int rs = dataHelper.DoNonQuery(sql, nameParam);
            return rs;
        }

        public int EditIsDelRole(bool is_del, int role_id)
        {
            sql = @"update public.role set is_del=@is_del where id=@id;";
            NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
            NpgsqlParameter idParam = new NpgsqlParameter("@id", role_id);
            int rs = dataHelper.DoNonQuery(sql, delParam, idParam);
            return rs;
        }

        public DataTable GetModule(bool isAll)
        {
            if (isAll)
            {
                sql = @"select id,module_name,descr,is_del,module_index from public.module";
            }
            else
            {
                sql = @"select id,module_name,descr,is_del,module_index from public.module where is_del=false order by id asc";
            }
            DataTable tbl = dataHelper.GetDataTbl(sql);
            return tbl;
        }

        /// <summary>
        /// 获取所有未删除的菜单
        /// </summary>
        /// <returns></returns>
        public DataTable GetMenu()
        {
            sql = @"select a.id, a.module_id, b.module_name, menu_name, a.descr, a.is_del from public.menu a 
                    left join public.module b on a.module_id = b.id
                    where a.is_del=false and b.is_del=false";
            DataTable tbl = dataHelper.GetDataTbl(sql);
            return tbl;
        }

        /// <summary>
        /// 获取对应模块下当前用户有权限操作的菜单
        /// </summary>
        /// <returns></returns>
        public DataTable GetMenuByModule(int module_id, int role_id)
        {
            sql = @"select module_name, menu_name,a.descr from public.menu a
                    left join public.module b on a.module_id = b.id
                    left join public.menu_role c on a.id = c.menu_id
                    where b.id=@module_id and c.role_id=@role_id and  a.is_del=false and b.is_del=false and c.is_del=false
                    order by a.id asc";
            NpgsqlParameter midParam = new NpgsqlParameter("@module_id", module_id);
            NpgsqlParameter ridParam = new NpgsqlParameter("@role_id", role_id);
            DataTable tbl = dataHelper.GetDataTbl(sql, midParam, ridParam);
            return tbl;
        }

        public DataTable GetMenuRoleByRoleId(int role_id)
        {
            sql = @"select * from public.menu_role where role_id=@role_id";
            NpgsqlParameter idParam = new NpgsqlParameter("@role_id", role_id);
            DataTable tbl = dataHelper.GetDataTbl(sql, idParam);
            return tbl;
        }
        public DataTable GetModuleMenu(int module_id)
        {
            sql = @"select id,menu_name,descr,is_del from public.menu where module_id=@id";
            NpgsqlParameter idParam = new NpgsqlParameter("@id", module_id);
            DataTable tbl = dataHelper.GetDataTbl(sql, idParam);
            return tbl;
        }
        public int AddModuleAct(string module_name,string descr)
        {
            sql = @"insert into public.module (module_name, descr, is_del) values (@module_name, @descr, false)";
            NpgsqlParameter nameParam = new NpgsqlParameter("@module_name", module_name);
            NpgsqlParameter descrParam = new NpgsqlParameter("@descr", descr);
            int rs = dataHelper.DoNonQuery(sql, nameParam, descrParam);
            return rs;
        }

        public int AddMenuAct(int module_id, string menu_name, string descr)
        {
            sql = @"insert into public.menu (module_id, menu_name, descr, is_del) values (@module_id, @menu_name, @descr, false)";
            NpgsqlParameter moduleIdParam = new NpgsqlParameter("module_id", module_id);
            NpgsqlParameter menuNameParam = new NpgsqlParameter("menu_name", menu_name);
            NpgsqlParameter descrParam = new NpgsqlParameter("descr", descr);
            int rs = dataHelper.DoNonQuery(sql, moduleIdParam, menuNameParam, descrParam);
            return rs;
        }
         
        public int SetMenuAct(int menu_id, bool is_del, string descr)
        {
            sql = @"update public.menu set is_del=@is_del, descr=@descr where id=@id ";
            NpgsqlParameter midParam = new NpgsqlParameter("@id", menu_id);
            NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
            NpgsqlParameter descrParam = new NpgsqlParameter("@descr", descr);
            int rs = dataHelper.DoNonQuery(sql, midParam, delParam, descrParam);
            return rs;
        }

        /// <summary>
        /// 修改模块的启用状态和模块首页
        /// </summary>
        /// <param name="module_id"></param>
        /// <param name="is_del"></param>
        /// <param name="module_fir_page"></param>
        /// <returns></returns>
        public int SaveModule(int module_id, bool is_del, string module_fir_page)
        {
            sql = "update public.module set is_del=@is_del,module_index=@module_index where id=@id";
            NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
            NpgsqlParameter idParam = new NpgsqlParameter("@id", module_id);
            NpgsqlParameter indexParam = new NpgsqlParameter("@module_index", module_fir_page);
            int rs = dataHelper.DoNonQuery(sql, delParam, idParam, indexParam);
            return rs;
        }
        
        public int SetMenuRole(bool is_del, int menu_id, int role_id)
        {
            sql = @"select count(*) as row_count from public.menu_role where menu_id=@menu_id and role_id=@role_id";
            NpgsqlParameter midParam = new NpgsqlParameter("@menu_id", menu_id);
            NpgsqlParameter ridParam = new NpgsqlParameter("@role_id", role_id);
            int rowCount = Convert.ToInt32(dataHelper.GetFristData(sql, midParam, ridParam).ToString());
            string updateSql = string.Empty;
            int rs = -1;
            if (rowCount <= 0)
            {
                updateSql = @"insert into public.menu_role (role_id, menu_id, is_del) values (@role_id,@menu_id,false)";
                NpgsqlParameter midParam1 = new NpgsqlParameter("@menu_id", menu_id);
                NpgsqlParameter ridParam1 = new NpgsqlParameter("@role_id", role_id);
                rs = dataHelper.DoNonQuery(updateSql, midParam1, ridParam1);
            }
            else
            {
                updateSql = @"update public.menu_role set is_del=@is_del where menu_id=@menu_id and role_id=@role_id";
                NpgsqlParameter midParam2 = new NpgsqlParameter("@menu_id", menu_id);
                NpgsqlParameter ridParam2 = new NpgsqlParameter("@role_id", role_id);
                NpgsqlParameter delParam = new NpgsqlParameter("@is_del", is_del);
                rs = dataHelper.DoNonQuery(updateSql, delParam, midParam2, ridParam2);
            }
            return rs;
        }

        public int InitMenuRole(int role_id)
        {
            sql = "select id from public.menu";
            DataTable menuTbl = dataHelper.GetDataTbl(sql);
            //所有的menu_id
            List<int> menuList = dataHelper.DataTableToList<int>(menuTbl, delegate (DataRow dr)
            {
                return dr["id"].ChkDBNullToInt();
            });
             
            DataTable menuRoleTbl = GetMenuRoleByRoleId(role_id);
            IEnumerable<int> insertMenuList = null;
            if (menuRoleTbl.Rows.Count > 0)
            {
                //menu_role表中的menu_id
                List<int> menuRoleList = dataHelper.DataTableToList<int>(menuRoleTbl, delegate (DataRow dr)
                {
                    return dr["menu_id"].ChkDBNullToInt();
                });
                //取出menu_role表中缺少的menu_id
                if (menuRoleList != null)
                {
                    insertMenuList = menuList.Except(menuRoleList);
                }
            }
            else
            {
                insertMenuList = menuList;
            }
            //插入menu_role表
            int flag = 0;//记录影响的行数
            if (insertMenuList != null)
            {
                foreach (int i in insertMenuList)
                {
                    sql = @"insert into public.menu_role (menu_id, role_id, is_del) values (@menu_id, @role_id, false)";
                    NpgsqlParameter mparam = new NpgsqlParameter("@menu_id", i);
                    NpgsqlParameter rparam = new NpgsqlParameter("@role_id", role_id);
                    int rs = dataHelper.DoNonQuery(sql, mparam,rparam);
                    flag += rs;
                }
            }

            if (flag == 0)
            {
                return 0;
            }
            else if (flag == insertMenuList.Count())
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
