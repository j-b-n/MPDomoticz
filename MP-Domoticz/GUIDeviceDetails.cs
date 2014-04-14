using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;


namespace MP_Domoticz
{
    class GUIDeviceDetails: GUIWindow
    {
        public override int GetID
        {
            get
            {
                return (int)7617;
            }
        }

        public GUIDeviceDetails()
        {            
        }

        public override bool Init()
        {
            Log.Info("Init GUIDeviceDetials!");
            return Load(GUIGraphicsContext.GetThemedSkinFile(@"\MP-Domoticz.DeviceDetails.xml"));
        }

    }
}
