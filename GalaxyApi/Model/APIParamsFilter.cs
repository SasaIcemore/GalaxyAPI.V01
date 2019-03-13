using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyApi.Model
{
    public class APIParamsFilter
    {
        //拼接逻辑 and/or
        public string Logic { get; set; }
        //参数名称
        public string ParamName { get; set; }
        //操作符
        public string Operation { get; set; }
        //public string ValueType { get; set; }
        //参数值
        public string Value { get; set; }
    }
}
