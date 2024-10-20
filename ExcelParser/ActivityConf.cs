using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser
{
    public class ActivityConf
    {
        public uint activityId;
        public uint activityType;
        public uint scheduleId;
        public uint[] meetCondList;
        public string beginTime;
        public string endTime;
    }
    public class NewActivityExcelConfigData
    {
        public uint activityId;
    }

   
}
