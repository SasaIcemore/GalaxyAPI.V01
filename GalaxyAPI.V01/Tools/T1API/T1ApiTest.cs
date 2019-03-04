using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseAPI.Models;

namespace API.Tools.DeliveryRecords
{
    public class T1ApiTest
    {
        //api调用令牌
        private static string Token = "";
        //用户名
        private static string user = "APISERVICE";
        private static string pwd = "gcrW13Qj86bRU";

        /// <summary>
        /// 新增发货记录
        /// </summary>
        /// <param name="param">发货记录参数</param>
        /// <returns></returns>
        public string Add_DeliveryRecords(WarehouseAPI.Models.DeliveryRecords rc)
        {
            //参数解析---------------------------
            //客户代码
            string custCode = rc.CustCode;
            //快递单号
            string logisticCode = rc.LogisticCode;
            //快递公司名：顺丰、速尔、跨越
            string express = rc.Express;
            //出库单号
            string outNo = rc.OutNo;
            //助理名称
            string assistant = rc.Assistant;
            //单数
            string danQty = rc.DanQty;
            //件数
            string jianQty = rc.JianQty;
            //发货日期
            string sendDate = rc.SendDate;

            //备注
            string remark = rc.Remark;

            //登录得到令牌
            string log = T1Api.Logon(user, pwd);
            string[] aw = log.Split(',');
            JObject ret = JObject.Parse(log);
            bool success = ret["success"].Value<bool>();
            Token = ret["token"].Value<String>();
            if (string.IsNullOrEmpty(Token))
            {
                return "Token is null or empty";
            }

            //获取初始化数据包
            string initStr = T1Api.GetCreate(Token, "DeliveryRecords");
            JObject deliveryRecords = JObject.Parse(initStr);

            //表单字段------------------------
            // Customer	客户全称 Lookup
            JObject Customer = new JObject();
            Dictionary<string, string> custDic = getCustomer(custCode);
            Customer.Add(new JProperty("value", custDic["Id"]));
            Customer.Add(new JProperty("text", custDic["ItemName"]));
            deliveryRecords["Customer"] = Customer;
            //	LogisticCode 快递单号 text
            JObject LogisticCode = new JObject();
            LogisticCode.Add(new JProperty("value", logisticCode));
            LogisticCode.Add(new JProperty("text", logisticCode));
            deliveryRecords["LogisticCode"] = LogisticCode;
            //	Express	快递	 Lookup
            JObject Express = new JObject();
            T1MsgInfo ExpressDic = GetExpress(express);
            Express.Add(new JProperty("value", ExpressDic.Id));
            Express.Add(new JProperty("text", ExpressDic.ItemName));
            deliveryRecords["Express"] = Express;
            //OutNo	出库单号	Text
            JObject OutNo = new JObject();
            OutNo.Add(new JProperty("value", outNo));
            OutNo.Add(new JProperty("text", outNo));
            deliveryRecords["OutNo"] = OutNo;
            //Assistant	助理	Text
            JObject Assistant = new JObject();
            Assistant.Add(new JProperty("value", assistant));
            Assistant.Add(new JProperty("text", assistant));
            deliveryRecords["Assistant"] = Assistant;
            //DanQty	单数	Number
            JObject DanQty = new JObject();
            DanQty.Add(new JProperty("value", danQty));
            DanQty.Add(new JProperty("text", danQty));
            deliveryRecords["DanQty"] = DanQty;
            //JianQty	件数	Number
            JObject JianQty = new JObject();
            JianQty.Add(new JProperty("value", jianQty));
            JianQty.Add(new JProperty("text", jianQty));
            deliveryRecords["JianQty"] = JianQty;
            //SendDate	发货日期	Date
            JObject SendDate = new JObject();
            SendDate.Add(new JProperty("value", sendDate));
            SendDate.Add(new JProperty("text", sendDate));
            deliveryRecords["SendDate"] = SendDate;

            //	Remark	备注	
            JObject Remark = new JObject();
            Remark.Add(new JProperty("value", remark));
            Remark.Add(new JProperty("text", remark));
            deliveryRecords["Remark"] = Remark;

            //	CreateDate	制单日期	DateTime
            //JObject CreateDate = new JObject();
            //CreateDate.Add(new JProperty("value", DateTime.Now.ToString()));
            //CreateDate.Add(new JProperty("text", DateTime.Now.ToString()));
            //deliveryRecords["CreateDate"] = CreateDate;

            string dataStr = JsonConvert.SerializeObject(deliveryRecords, Formatting.None);

            //新增保存
            string saveRet = T1Api.Create(Token, "DeliveryRecords", dataStr);

            return saveRet;
        }

        /// <summary>
        /// 根据客户代码查询T1客户信息
        /// </summary>
        /// <param name="custCode">客户代码</param>
        /// <returns></returns>
        public T1MsgInfo GetCustomer(string custCode)
        {
            T1MsgInfo cust = new T1MsgInfo();
            string itemType = "SysCustomer";

            //过滤JSON数据包
            JObject filter = new JObject();
            filter.Add(new JProperty("logic", "and"));
            JArray conds = new JArray();
            JObject cond1 = new JObject();
            cond1.Add(new JProperty("property", "ItemCode"));
            cond1.Add(new JProperty("operators", "doubleEqual"));
            cond1.Add(new JProperty("valueType", "string"));
            cond1.Add(new JProperty("value", custCode));
            conds.Add(cond1);
            filter.Add(new JProperty("condition", conds));
            string filterString = JsonConvert.SerializeObject(filter, Formatting.None);

            string Data = T1Api.Query(Token, itemType, filterString);
            JObject dataRet = JObject.Parse(Data);
            bool ss = dataRet["success"].Value<bool>();
            if (ss)
            {
                JArray data = dataRet["data"].Value<JArray>();
                foreach (var s in data)
                {
                    var row = s as JObject;
                    //客户ID
                    JObject jId = row["Id"].Value<JObject>();
                    string Id = jId["value"].Value<string>();
                    //客户代码
                    JObject jItemCode = row["ItemCode"].Value<JObject>();
                    string ItemCode = jItemCode["value"].Value<string>();
                    //客户全称
                    JObject jItemName = row["ItemName"].Value<JObject>();
                    string ItemName = jItemName["value"].Value<string>();

                    cust.Id = Id;
                    cust.ItemCode = ItemCode;
                    cust.ItemName = ItemName;
                }
            }

            return cust;

        }

        /// <summary>
        /// 根据客户代码查询T1客户信息
        /// </summary>
        /// <param name="custCode">客户代码</param>
        /// <returns>客户字典---Id,ItemCode,ItemName </returns>
        public Dictionary<string, string> getCustomer(string custCode)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Id", null);
            dic.Add("ItemCode", null);
            dic.Add("ItemName", null);
            string itemType = "SysCustomer";

            //过滤JSON数据包
            JObject filter = new JObject();
            filter.Add(new JProperty("logic", "and"));
            JArray conds = new JArray();
            JObject cond1 = new JObject();
            cond1.Add(new JProperty("property", "ItemCode"));
            cond1.Add(new JProperty("operators", "doubleEqual"));
            cond1.Add(new JProperty("valueType", "string"));
            cond1.Add(new JProperty("value", custCode));
            conds.Add(cond1);
            filter.Add(new JProperty("condition", conds));
            string filterString = JsonConvert.SerializeObject(filter, Formatting.None);

            string Data = T1Api.Query(Token, itemType, filterString);
            JObject dataRet = JObject.Parse(Data);
            bool ss = dataRet["success"].Value<bool>();
            if (ss)
            {
                JArray data = dataRet["data"].Value<JArray>();
                foreach (var s in data)
                {
                    var row = s as JObject;
                    //客户ID
                    JObject jId = row["Id"].Value<JObject>();
                    string Id = jId["value"].Value<string>();
                    //客户代码
                    JObject jItemCode = row["ItemCode"].Value<JObject>();
                    string ItemCode = jItemCode["value"].Value<string>();
                    //客户全称
                    JObject jItemName = row["ItemName"].Value<JObject>();
                    string ItemName = jItemName["value"].Value<string>();

                    dic["Id"] = Id;
                    dic["ItemCode"] = ItemCode;
                    dic["ItemName"] = ItemName;
                }
            }

            return dic;
        }

        public T1MsgInfo GetExpress(string expressName)
        {
            T1MsgInfo exp = new T1MsgInfo();
            string itemType = "ExpCompany";//T1表单名称

            //过滤JSON数据包
            JObject filter = new JObject();
            filter.Add(new JProperty("logic", "and"));
            JArray conds = new JArray();
            JObject cond1 = new JObject();
            cond1.Add(new JProperty("property", "ItemName"));
            cond1.Add(new JProperty("operators", "doubleEqual"));
            cond1.Add(new JProperty("valueType", "string"));
            cond1.Add(new JProperty("value", expressName));
            conds.Add(cond1);
            filter.Add(new JProperty("condition", conds));
            string filterString = JsonConvert.SerializeObject(filter, Formatting.None);

            string Data = T1Api.Query(Token, itemType, filterString);
            JObject dataRet = JObject.Parse(Data);
            bool ss = dataRet["success"].Value<bool>();
            if (ss)
            {
                JArray data = dataRet["data"].Value<JArray>();
                foreach (var s in data)
                {
                    var row = s as JObject;
                    //客户ID
                    JObject jId = row["Id"].Value<JObject>();
                    string Id = jId["value"].Value<string>();
                    //客户代码
                    JObject jItemCode = row["ItemCode"].Value<JObject>();
                    string ItemCode = jItemCode["value"].Value<string>();
                    //客户全称
                    JObject jItemName = row["ItemName"].Value<JObject>();
                    string ItemName = jItemName["value"].Value<string>();

                    exp.Id = Id;
                    exp.ItemCode = ItemCode;
                    exp.ItemName = ItemName;
                }
            }

            return exp;
        }

        ///// <summary>
        ///// 根据快递公司名称（顺丰、速尔、跨越）查询T1快递公司表单数据
        ///// </summary>
        ///// <param name="expressName"></param>
        ///// <returns></returns>
        //public Dictionary<string, string> getExpress(string expressName)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    dic.Add("Id", null);
        //    dic.Add("ItemCode", null);
        //    dic.Add("ItemName", null);
        //    string itemType = "ExpCompany";//T1表单名称

        //    //过滤JSON数据包
        //    JObject filter = new JObject();
        //    filter.Add(new JProperty("logic", "and"));
        //    JArray conds = new JArray();
        //    JObject cond1 = new JObject();
        //    cond1.Add(new JProperty("property", "ItemName"));
        //    cond1.Add(new JProperty("operators", "doubleEqual"));
        //    cond1.Add(new JProperty("valueType", "string"));
        //    cond1.Add(new JProperty("value", expressName));
        //    conds.Add(cond1);
        //    filter.Add(new JProperty("condition", conds));
        //    string filterString = JsonConvert.SerializeObject(filter, Formatting.None);

        //    string Data = T1Api.Query(Token, itemType, filterString);
        //    JObject dataRet = JObject.Parse(Data);
        //    bool ss = dataRet["success"].Value<bool>();
        //    if (ss)
        //    {
        //        JArray data = dataRet["data"].Value<JArray>();
        //        foreach (var s in data)
        //        {
        //            var row = s as JObject;
        //            //客户ID
        //            JObject jId = row["Id"].Value<JObject>();
        //            string Id = jId["value"].Value<string>();
        //            //客户代码
        //            JObject jItemCode = row["ItemCode"].Value<JObject>();
        //            string ItemCode = jItemCode["value"].Value<string>();
        //            //客户全称
        //            JObject jItemName = row["ItemName"].Value<JObject>();
        //            string ItemName = jItemName["value"].Value<string>();
                    
        //            dic["Id"] = Id;
        //            dic["ItemCode"] = ItemCode;
        //            dic["ItemName"] = ItemName;
        //        }
        //    }

        //    return dic;
        //}

    }
}
