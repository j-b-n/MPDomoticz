using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MP_Domoticz
{
    class DomoticzServer
    {        
        static string _ServerAddress;
        
        public static string ServerAddress
        {
            get
            {
                return _ServerAddress;
            }
            set
            {
                _ServerAddress = value;
            }
        }

        static string _ServerPort;
        public static string ServerPort
        {
            get
            {
                return _ServerPort;
            }
            set
            {
                _ServerPort = value;
            }
        }

        public int InitServer(string server, string port)
        {
            ServerAddress = server;
            ServerPort = port;
            /*if(Dns.GetHostEntry(ServerAddress).AddressList.Length < 1)
            {
                return 0;
            }
             */
            return 1;
        }

        public class SunSetRise
        {
            public string ServerTime { get; set; }
            public string Sunrise { get; set; }
            public string Sunset { get; set; }
            public string status { get; set; }
            public string title { get; set; }
        }

        public  SunSetRise GetSunSet()
        {            
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?type=command&param=getSunRiseSet";
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception) { }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (SunSetRise)JsonConvert.DeserializeObject(json_data, typeof(SunSetRise))
                : new SunSetRise();
        }


        /****
         * 
         * 
         * ***/
        public class Device
        {
            public double AddjMulti { get; set; }
            public double AddjValue { get; set; }
            public int BatteryLevel { get; set; }
            public int CustomImage { get; set; }
            public string Data { get; set; }
            public int Favorite { get; set; }
            public int HardwareID { get; set; }
            public string HardwareName { get; set; }
            public bool HaveTimeout { get; set; }
            public string ID { get; set; }
            public string LastUpdate { get; set; }
            public string Name { get; set; }
            public string Notifications { get; set; }
            public bool Protected { get; set; }
            public int SignalLevel { get; set; }
            public string SubType { get; set; }
            public double Temp { get; set; }
            public string Timers { get; set; }
            public string Type { get; set; }
            public string TypeImg { get; set; }
            public int Unit { get; set; }
            public int Used { get; set; }
            public string idx { get; set; }
            public double? AddjMulti2 { get; set; }
            public double? AddjValue2 { get; set; }
            public string Rain { get; set; }
            public string RainRate { get; set; }
            public bool? HaveDimmer { get; set; }
            public bool? HaveGroupCmd { get; set; }
            public string Image { get; set; }
            public bool? IsSubDevice { get; set; }
            public int? Level { get; set; }
            public int? LevelInt { get; set; }
            public int? MaxDimLevel { get; set; }
            public string Status { get; set; }
            public string StrParam1 { get; set; }
            public string StrParam2 { get; set; }
            public string SwitchType { get; set; }
            public int? SwitchTypeVal { get; set; }
            public bool? UsedByCamera { get; set; }
            public string DewPoint { get; set; }
            public int? Humidity { get; set; }
            public string HumidityStatus { get; set; }
            public double? Barometer { get; set; }
            public int? Forecast { get; set; }
            public string ForecastStr { get; set; }
            public string forecast_url { get; set; }
            public double? Chill { get; set; }
            public double? Direction { get; set; }
            public string DirectionStr { get; set; }
            public string Gust { get; set; }
            public string Speed { get; set; }
            public double? Visibility { get; set; }
            public double? Radiation { get; set; }
        }

        public class DeviceResponse
        {
            public int __invalid_name__5MinuteHistoryDays { get; set; }
            public bool AllowWidgetOrdering { get; set; }
            public int DashboardType { get; set; }
            public int MobileType { get; set; }
            public double TempScale { get; set; }
            public string TempSign { get; set; }
            public double WindScale { get; set; }
            public string WindSign { get; set; }
            public List<Device> result { get; set; }
            public string status { get; set; }
            public string title { get; set; }
        }


        public DeviceResponse GetAllDevices()
        {           
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?type=devices&used=true&order=Name";
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception) { }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (DeviceResponse)JsonConvert.DeserializeObject(json_data, typeof(DeviceResponse))
                : new DeviceResponse();
        }


    }
}
