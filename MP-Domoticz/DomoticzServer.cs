﻿using Newtonsoft.Json;
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

        /*
         * Graph data
         */
        public class GraphResult
        {
            public string d { get; set; }
            public string hu { get; set; }
            public double ta { get; set; }
            public double te { get; set; }
            public double tm { get; set; }
            public double ch { get; set; }
            public double cm { get; set; }
            public double dp { get; set; }
            public string di { get; set; }
            public string gu { get; set; }
            public string sp { get; set; }
            public string v { get; set; }
            public string v_min { get; set; }
            public string v_max { get; set; }
            public string uvi { get; set; }
            public string dig { get; set; }
            public string div { get; set; }
            public string mm { get; set; }
            public string ba { get; set; }
            public string co2 { get; set; }
            public string co2_min { get; set; }
            public string co2_max { get; set; }
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
        /// Allowed values are: temp,wind,winddir,counter,rain,uv
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
