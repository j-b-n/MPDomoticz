using MediaPortal.GUI.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP_Domoticz
{
    public class DeviceFilter
    {
        private DeviceFilterBy currentFilterBy = DeviceFilterBy.All;

        public enum DeviceFilterBy
        {
            Favorites = 1,
            Switches = 2,
            Scenes = 3,
            Temperature = 4,
            Weather = 5,
            Utility = 6,
            All = 7,
        }

        public DeviceFilter(DeviceFilterBy tmp)
        {
            this.currentFilterBy = tmp;         
        }

        public bool Filter(DomoticzServer.Device dev)
        {           
            switch (currentFilterBy)
            {
                case DeviceFilterBy.Favorites:
                    if (dev.Favorite > 0)
                    {
                        return true;
                    }
                    return false;
                case DeviceFilterBy.Switches:
                    if (dev.Type == "Lighting 2" ||
                        dev.Type == "Security")
                    {
                        return true;
                    }
                    return false;
                case DeviceFilterBy.Temperature:
                    if (dev.Type == "Temp" ||
                        dev.Type == "Temp + Humidity" ||
                        dev.Type == "Temp + Baro")
                    {
                        return true;
                    }
                    return false;
                default:
                    return true;
            }
        }
    }
}
