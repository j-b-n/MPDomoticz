using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomoticzLib;

namespace Console_Domoticz
{
    class Program
    {
        static void Main(string[] args)
        {            
            Console.WriteLine("Domoticz test!");
            DomoticzLib.Domoticz.SunSetRise sun = DomoticzLib.Domoticz.GetSunSet();
            Console.WriteLine("Servertime:" + sun.ServerTime + " "+
                "SunRise: " + sun.Sunrise + " " +
                "SunSet:" + sun.Sunset);

            DomoticzLib.Domoticz.DeviceResponse DevR = DomoticzLib.Domoticz.GetAllDevices();
            foreach(DomoticzLib.Domoticz.Device dev in DevR.result)
            {
                
                Console.WriteLine("------------");
                Console.WriteLine(dev.CustomImage);
                Console.WriteLine("IdX: " + dev.idx);
                Console.WriteLine("Name: " + dev.Name);
                Console.WriteLine("SwitchType: " + dev.SwitchType);
                Console.WriteLine("SwitchTypeVal: " + dev.SwitchTypeVal);
                Console.WriteLine("Type: " + dev.Type);
                Console.WriteLine("TypeImg: " + dev.TypeImg);
                Console.WriteLine("Hardwarename: " + dev.HardwareName);
                Console.WriteLine("Image: " + dev.Image);
                Console.WriteLine("LastUpdate: "+dev.LastUpdate);
            }
            Console.WriteLine("PRESS ANY <ENTER>!");
            Console.ReadLine();
        }
    }
}
