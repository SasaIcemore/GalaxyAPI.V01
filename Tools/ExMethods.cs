using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Tools
{
    public static class ExMethods
    {
        /// <summary>
        /// 判断增删改操作结果的扩展方法
        /// </summary>
        /// <param name="nonqueryResult"></param>
        /// <returns></returns>
        public static string ChkNonQuery(this int nonqueryResult)
        {
            if (nonqueryResult < 1)
            {
                return "操作失败";
            }
            else
            {
                return "操作成功";
            }
        }

        public static int ChkDBNullToInt(this object obj)
        {
            if (obj == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj.ToString());
            }
        }

        public static double ChkDBNullToDouble(this object obj)
        {
            if (obj == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToDouble(obj.ToString());
            }
        }

        public static string ChkDBNullToStr(this object obj)
        {
            if (obj == DBNull.Value)
            {
                return "";
            }
            else
            {
                return obj.ToString();
            }
        }

        public static bool ChkDBNullToBool(this object obj)
        {
            if (obj == DBNull.Value)
            {
                return false;
            }
            else
            {
                return Convert.ToBoolean(obj.ToString());
            }
        }
    }
}
