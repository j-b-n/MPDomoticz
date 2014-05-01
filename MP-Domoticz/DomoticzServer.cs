using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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


            string url = "http://" + ServerAddress + ":" + ServerPort + "/";
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception)
            {
                return 0;
            }

            return 1;            
        }

        #region helper functions

        /// <summary>
        /// Return a string to the current appropriate temperature icon
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public string  GetTempIcon(double temp)
        {
            string skinName = MediaPortal.Configuration.Config.SkinName;
            string skinPath = MediaPortal.Configuration.Config.GetSubFolder(MediaPortal.Configuration.Config.Dir.Skin, skinName);
            if (temp < 5)
            {
                return skinPath + "\\Media\\Domoticz\\temp-0-5.png";
            }
            if (temp > 5 && temp <= 10)
            {
                return skinPath + "\\Media\\Domoticz\\temp-5-10.png";
            }
            if (temp > 10 && temp <= 15)
            {
                return skinPath + "\\Media\\Domoticz\\temp-10-15.png";
            }
            if (temp > 15 && temp <= 20)
            {
                return skinPath + "\\Media\\Domoticz\\temp-15-20.png";
            }
            if (temp > 20 && temp <= 25)
            {
                return skinPath + "\\Media\\Domoticz\\temp-20-25.png";
            }
            if (temp > 25 && temp <= 30)
            {
                return skinPath + "\\Media\\Domoticz\\temp-25-30.png";
            }

            return skinPath + "\\Media\\Domoticz\\temp-gt-30.png";
        }

        /// <summary>
        /// Return what type of Graph should be generated for this device
        /// </summary>
        /// <param name="dev"></param>
        /// <returns></returns>
        public string GetGraphType(Device dev)
        {
            switch (dev.Type)
            {
                case "Temp":
                    return "Temp";
                
                case "Temp + Humidity":
                    return "TempHum";
                
                case "Barometer":
                case "Temp + Baro":
                    return "Barometer";
                
                case "Rain":
                    return "Rain";
                case "Wind":
                    return "Wind";
                
                case "General":                
                    if (dev.SubType == "Percentage")
                    {
                        return "Percentage";
                    }
                    return "counter";
                
                case "Security":
                case "Lighting 2":
                    return "lightlog";
            }
            return "None";
        }

        /// <summary>
        /// Return a string to the current appropriate icon
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public string GetIcon(Device dev)
        {
            string skinName = MediaPortal.Configuration.Config.SkinName;
            string skinPath = MediaPortal.Configuration.Config.GetSubFolder(MediaPortal.Configuration.Config.Dir.Skin, skinName);

            string poster = skinPath + "\\Media\\Domoticz\\" + dev.TypeImg + "48.png";
            string thumb = skinPath + "\\Media\\Domoticz\\" + dev.TypeImg + ".png";

            switch (dev.Type)
            {
                case "Temp":
                    poster = GetTempIcon(dev.Temp);
                    break;

                case "Temp + Humidity":
                    poster = GetTempIcon(dev.Temp);
                    break;

                case "Wind":
                    poster = skinPath + "\\Media\\Domoticz\\Wind" + dev.DirectionStr + ".png";
                    break;

                case "Lighting 2":
                    if (dev.Status == "On")
                    {
                        poster = skinPath + "\\Media\\Domoticz\\Light48_On.png";
                    }
                    else
                    {
                        poster = skinPath + "\\Media\\Domoticz\\Light48_Off.png";
                    }
                    break;

                case "Rain":                    
                    break;

                default:                    
                    break;
            }

            if (!File.Exists(poster))
            {
                poster = thumb;
            }
            return poster;
        }

        public string GetDeviceDescription(Device device)
        {
            string desc = "";
            switch (device.Type)
            {
                case "Temp":
                    return device.Temp.ToString() + "°";                    

                case "Temp + Humidity":
                    desc = device.Temp.ToString() + "°";
                    if (device.Humidity != null)
                    {
                        desc += " / " + device.Humidity.ToString() + "%";
                    }
                    if (device.DewPoint != null)
                    {
                        desc += " " + Translation.Dewpoint + ": " + device.DewPoint.ToString() + "°";
                    }

                    return desc;
                case "Wind":
                    desc = device.DirectionStr + " / " + device.Speed + "m/s";
                    if (device.Gust != null)
                    {
                        desc += " " + Translation.Gust + ": " + device.Gust.ToString() + "°";
                    }
                    return desc;

                case "Lighting 2":
                    desc = Translation.Status + ": " + device.Status;
                    return desc;

                case "Rain":
                    desc = device.Rain + " mm";
                    return desc;

                case "General":
                    desc = Translation.Status + ": " + device.Data;
                    return desc;

                default:
                    desc = device.Name + " " + device.Type;
                    return desc;
            }
        }

        #endregion

        #region SunRise & SunSet
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
            catch (Exception) {                
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (SunSetRise)JsonConvert.DeserializeObject(json_data, typeof(SunSetRise))
                : new SunSetRise();
        }
        #endregion


        /// <summary>
        /// Information about device 
        /// </summary>
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
            public int idx { get; set; }
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
            catch (Exception) {
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (DeviceResponse)JsonConvert.DeserializeObject(json_data, typeof(DeviceResponse))
                : new DeviceResponse();
        }


        public DeviceResponse GetSingleDevice(int idx)
        {
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?type=devices&rid="+idx;
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception)
            {
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (DeviceResponse)JsonConvert.DeserializeObject(json_data, typeof(DeviceResponse))
                : new DeviceResponse();
        }

        public class Response
        {
            public string status { get; set; }
            public string title { get; set; }
        }

        public Response SwitchLight(int idx, string command)
        {
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?"+
            "type=command&param=switchlight&idx="+idx+"&switchcmd="+command+"&level=0";
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception) {
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (Response)JsonConvert.DeserializeObject(json_data, typeof(Response))
                : new Response();
        }


        /*
         *  Get LightLog
         * 
         * http://192.168.1.6:8080/json.htm?type=lightlog&idx=17
         */
        public class LightLogResult
        {
            public string Data { get; set; }
            public string Date { get; set; }
            public int Level { get; set; }
            public int MaxDimLevel { get; set; }
            public string Status { get; set; }
            public string idx { get; set; }
        }

        public class LightLogResponse
        {
            public bool HaveDimmer { get; set; }
            public bool HaveGroupCmd { get; set; }
            public List<LightLogResult> result { get; set; }
            public string status { get; set; }
            public string title { get; set; }
        }

        /// <summary>
        /// Get the lightlog for a device
        /// </summary>
        /// <param name="idx">The specified device ID</param>
        /// <returns></returns>
        public LightLogResponse GetLightLog(int idx)
        {
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?type=lightlog&idx=" + idx;
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception)
            {
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (LightLogResponse)JsonConvert.DeserializeObject(json_data, typeof(LightLogResponse))
                : new LightLogResponse();
        }


        /*
         * Graph data
         */
        public class GraphResult
        {
            public string d { get; set; }
            public double hu { get; set; }
            public double ta { get; set; }
            public double te { get; set; }
            public double tm { get; set; }
            public double ch { get; set; }
            public double cm { get; set; }
            public double dp { get; set; }
            public double di { get; set; }
            public double gu { get; set; }
            public double sp { get; set; }
            public double v { get; set; }
            public double v_avg { get; set; }
            public double v_min { get; set; }
            public double v_max { get; set; }
            public double uvi { get; set; }
            public double dig { get; set; }
            public double div { get; set; }
            public double mm { get; set; }
            public double ba { get; set; }
            public double co2 { get; set; }
            public double co2_min { get; set; }
            public double co2_max { get; set; }
        }

        public class GraphResponse
        {
            public List<GraphResult> result { get; set; }
            public string status { get; set; }
            public string title { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="sensortype">
        /// Allowed values are: temp,wind,winddir,counter,rain,uv, Percentage
        /// </param>
        /// <param name="range"></param>
        /// <returns></returns>

        public GraphResponse GetGraphData(int idx, string sensortype, string range)
        {
            string url = "http://" + ServerAddress + ":" + ServerPort + "/json.htm?type=graph&" +
                "sensor=" + sensortype + "&idx=" + idx + "&range=" + range;
            WebClient w = new WebClient();
            string json_data = "";
            try
            {
                w.Encoding = Encoding.UTF8;
                json_data = w.DownloadString(url);
            }
            catch (Exception)
            {
                return null;
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            return !string.IsNullOrEmpty(json_data) ?
                (GraphResponse)JsonConvert.DeserializeObject(json_data, typeof(GraphResponse))
                : new GraphResponse();
        }

    }
}
