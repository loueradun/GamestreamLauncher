using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamestreamLauncher.HelperApi
{
    public class AwesomeMinerApi
    {
        public class StatusInfo
        {
            public string statusDisplay { get; set; }
            public string statusLine3 { get; set; }
        }

        public class ProgressInfo
        {
            public string line1 { get; set; }
            public string line2 { get; set; }
            public string line3 { get; set; }
        }

        public class SpeedInfo
        {
            public int logInterval { get; set; }
            public string hashrate { get; set; }
            public string avgHashrate { get; set; }
            public string workUtility { get; set; }
            public object line2 { get; set; }
            public object line3 { get; set; }
        }

        public class CoinInfo
        {
            public string displayName { get; set; }
            public string revenuePerDay { get; set; }
            public double revenuePerDayValue { get; set; }
            public string revenuePerMonth { get; set; }
            public double profitPerDayValue { get; set; }
            public string profitPerDay { get; set; }
            public string profitPerMonth { get; set; }
        }

        public class StatusInfo2
        {
            public string statusDisplay { get; set; }
            public object statusLine3 { get; set; }
        }

        public class AdditionalInfo
        {
            public string displayUrl { get; set; }
            public string worker { get; set; }
        }

        public class PriorityInfo
        {
            public int priority { get; set; }
            public int quota { get; set; }
        }

        public class ProgressInfo2
        {
            public string line1 { get; set; }
            public string line2 { get; set; }
            public string line3 { get; set; }
        }

        public class PoolList
        {
            public int id { get; set; }
            public string name { get; set; }
            public StatusInfo2 statusInfo { get; set; }
            public AdditionalInfo additionalInfo { get; set; }
            public PriorityInfo priorityInfo { get; set; }
            public ProgressInfo2 progressInfo { get; set; }
            public string coinName { get; set; }
            public int minerID { get; set; }
            public string minerName { get; set; }
            public bool canRemove { get; set; }
            public bool canDisable { get; set; }
            public bool canEnable { get; set; }
            public bool canPrioritize { get; set; }
        }

        public class StatusInfo3
        {
            public string statusDisplay { get; set; }
            public string statusLine3 { get; set; }
        }

        public class DeviceInfo
        {
            public string deviceType { get; set; }
            public int gpuActivity { get; set; }
            public string intensity { get; set; }
            public object name { get; set; }
            public int gpuClock { get; set; }
            public int gpuMemoryClock { get; set; }
            public string gpuVoltage { get; set; }
            public int gpuPowertune { get; set; }
            public int fanSpeed { get; set; }
            public int fanPercent { get; set; }
            public int temperature { get; set; }
        }

        public class ProgressInfo3
        {
            public string line1 { get; set; }
            public string line2 { get; set; }
            public string line3 { get; set; }
        }

        public class SpeedInfo2
        {
            public int logInterval { get; set; }
            public string hashrate { get; set; }
            public string avgHashrate { get; set; }
            public string workUtility { get; set; }
            public object line2 { get; set; }
            public object line3 { get; set; }
        }

        public class GpuList
        {
            public string name { get; set; }
            public StatusInfo3 statusInfo { get; set; }
            public DeviceInfo deviceInfo { get; set; }
            public ProgressInfo3 progressInfo { get; set; }
            public SpeedInfo2 speedInfo { get; set; }
        }

        public class MinerList
        {
            public int id { get; set; }
            public string name { get; set; }
            public string hostname { get; set; }
            public int groupId { get; set; }
            public string pool { get; set; }
            public string temperature { get; set; }
            public object hardware { get; set; }
            public StatusInfo statusInfo { get; set; }
            public ProgressInfo progressInfo { get; set; }
            public SpeedInfo speedInfo { get; set; }
            public CoinInfo coinInfo { get; set; }
            public DateTime updatedUtc { get; set; }
            public string updated { get; set; }
            public List<PoolList> poolList { get; set; }
            public List<GpuList> gpuList { get; set; }
            public List<object> pgaList { get; set; }
            public List<object> asicList { get; set; }
            public bool hasPool { get; set; }
            public bool hasGpu { get; set; }
            public bool hasPga { get; set; }
            public bool hasAsic { get; set; }
            public bool canReboot { get; set; }
            public bool canStop { get; set; }
            public bool canRestart { get; set; }
            public bool canStart { get; set; }
            public bool canPool { get; set; }
            public bool hasValidStatus { get; set; }
        }

        public class GroupList
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<MinerList> minerList { get; set; }
        }

        public class MetaData
        {
            public string updated { get; set; }
            public string edition { get; set; }
            public string version { get; set; }
            public List<object> infoList { get; set; }
            public List<object> warningList { get; set; }
        }

        public class AwesomeMinerStatus
        {
            public string totalHashrate5s { get; set; }
            public bool canManualAction { get; set; }
            public bool hasManualAction { get; set; }
            public bool hasPoolOperations { get; set; }
            public List<object> manualActionList { get; set; }
            public List<GroupList> groupList { get; set; }
            public MetaData metaData { get; set; }
        }
    }
}
