using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaxyApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using Tools;

namespace GalaxyAPI.V01.Controllers
{
    public class ManageController : Controller
    {
        BackManage manage = new BackManage();
        DataTable tbl = null;

        public IActionResult Index()
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (string.IsNullOrEmpty(cookieToken))
            {
                //未登录
                return View("Views/Login/Login.cshtml");
            }
            ViewData["ModuleId"] = 5;
            return View();
        }

        public IActionResult MenuBar()
        {
            return View();
        }

        public IActionResult GetMenuByModule(int module_id)
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (string.IsNullOrEmpty(cookieToken))
            {
                //未登录
                return View("Views/Login/Login.cshtml");
            }
            int userId = manage.GetUserId(HttpContext.Session.GetString("userName"), HttpContext.Session.GetString("password"));
            DataTable userTbl = manage.GetUserInfo(false, userId);
            DataTable menuTbl = null;
            if (userTbl != null)
            {
                if (userTbl.Rows.Count>0)
                {
                    int roleId = userTbl.Rows[0]["role_id"].ChkDBNullToInt();
                    menuTbl = manage.GetMenuByModule(module_id, roleId);
                }
            }
            if (menuTbl != null)
            {
                return Content(JsonConvert.SerializeObject(menuTbl));
            }
            return Content("");
        }

        public IActionResult UserHome()
        {
            return View();
        }

        public IActionResult ChangePWD(string new_pwd, string comfirm_pwd)
        {

            if (string.IsNullOrEmpty(new_pwd) || string.IsNullOrEmpty(comfirm_pwd))
            {
                return Content("请填写密码");
            }
            if (new_pwd != comfirm_pwd)
            {
                return Content("新密码与确认密码不一致");
            }
            int userId = manage.GetUserId(HttpContext.Session.GetString("userName"), HttpContext.Session.GetString("password"));
            if (userId > 0)
            {
                int rs = manage.ChangePWD(new_pwd,userId);
                return Content(rs.ChkNonQuery());
            }
            return Content("更新失败");
        }

        public IActionResult ModuleManage()
        {
            return View();
        }

        public IActionResult UserManage()
        {
            return View();
        }

        public IActionResult GetUser()
        {
            tbl = manage.GetUserInfo(true);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

        public IActionResult AddUser()
        {
            return View();
        }

        public IActionResult AddUserAct(string user_name, string dept_name, int role_id, string pwd)
        {
            int rs = manage.AddUser(user_name, dept_name, role_id, pwd);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult EditUser(string user_id, string user_name, bool is_del, int role_id)
        {
            ViewData["UserId"] = user_id;
            ViewData["UserName"] = user_name;
            ViewData["IsDel"] = is_del;
            ViewData["RoleId"] = role_id;
            return View();
        }

        public IActionResult EditUser1(int user_id, string dept_name, int role, string pwd, bool is_del)
        {
            int rs = manage.EditUser(user_id, dept_name, role, pwd, is_del);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult RoleManage()
        {
            return View();
        }

        public IActionResult GetRoles(bool all_role)
        {
            tbl = manage.GetRoles(all_role);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

        public IActionResult AddRole()
        {
            return View();
        }

        public IActionResult AddRoleAct(string role_name)
        {
            int rs = manage.AddRoleAct(role_name);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult RoleInfo(int role_id, bool is_del)
        {
            string moduleTbl = JsonConvert.SerializeObject(manage.GetModule(false));
            string menuTbl = JsonConvert.SerializeObject(manage.GetMenu());
            string menuRoleTbl = JsonConvert.SerializeObject(manage.GetMenuRoleByRoleId(role_id));
            ViewData["moduleJson"] = moduleTbl;
            ViewData["menuJson"] = menuTbl;
            ViewData["menuRoleJson"] = menuRoleTbl;
            ViewData["roleId"] = role_id;
            ViewData["is_del"] = is_del;
            return View();
        }

        public IActionResult SetRoleDel(bool is_del, int role_id)
        {
            int rs = manage.EditIsDelRole(is_del, role_id);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult GetModule(bool all)
        {
            tbl = manage.GetModule(all);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            return Content(jsonStr);
        }

        public IActionResult AddModule()
        {
            return View();
        }

        public IActionResult AddModuleAct(string module_name, string descr)
        {
            int rs = manage.AddModuleAct(module_name, descr);
            return Content(rs.ChkNonQuery());
        }

        [HttpPost]
        public IActionResult SaveModule(int module_id, bool is_del, string module_fir_page)
        {
            int rs = manage.SaveModule(module_id, is_del, module_fir_page);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult SetModuleMenu(int module_id, bool is_del, string module_index)
        {
            tbl = manage.GetModuleMenu(module_id);
            string jsonStr = JsonConvert.SerializeObject(tbl);
            ViewData["module_menu_json"] = jsonStr;
            ViewData["module_id"] = module_id;
            ViewData["is_del"] = is_del;
            ViewData["module_index"] = module_index;
            return View();
        }

        /// <summary>
        /// 添加菜单页面
        /// </summary>
        /// <param name="module_id">模块id</param>
        /// <returns></returns>
        public IActionResult AddMenu(int module_id)
        {
            ViewData["module_id"] = module_id;
            return View();
        }

        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <param name="module_id"></param>
        /// <param name="menu_name"></param>
        /// <param name="descr"></param>
        /// <returns></returns>
        public IActionResult AddMenuAct(int module_id, string menu_name, string descr)
        {
            int rs = manage.AddMenuAct(module_id, menu_name, descr);
            return Content(rs.ChkNonQuery());
        }

        /// <summary>
        /// 修改菜单
        /// </summary>
        /// <param name="menu_id">菜单id</param>
        /// <returns></returns>
        public IActionResult EditMenu(int menu_id, string menu_name, bool is_del, string descr)
        {
            ViewData["MenuId"] = menu_id;
            ViewData["MenuName"] = menu_name;
            ViewData["IsDel"] = is_del;
            ViewData["Descr"] = descr;
            return View();
        }

        public IActionResult SetMenuAct(int menu_id, bool is_del, string descr)
        {
            int rs = manage.SetMenuAct(menu_id, is_del, descr);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult SetMenuRole(bool is_del, int menu_id, int role_id)
        {
            int rs = manage.SetMenuRole(is_del, menu_id, role_id);
            return Content(rs.ChkNonQuery());
        }

        public IActionResult InitMenuRole(int role_id)
        {
            int rs = manage.InitMenuRole(role_id);
            return Content(rs.ChkNonQuery());
        }
    }
}