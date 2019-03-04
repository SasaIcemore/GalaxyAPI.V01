using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseAPI.Models
{
    public class DeliveryRecords
    {
        public string CustCode { get; set; }
        public string LogisticCode { get; set; }
        public string Express { get; set; }
        public string OutNo { get; set; }
        public string Assistant { get; set; }
        public string DanQty { get; set; }
        public string JianQty { get; set; }
        public string SendDate { get; set; }
        public string Remark { get; set; }
    }
}
