using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Dialogs;
using System.IO;

namespace MP_Domoticz
{
    public class GUIDeviceDetails : GUIWindow
    {

        /// <summary>
        /// GUIDeviceDetails unique window ID
        /// </summary>        
        public const int GUIDeviceDetails_WINDOW_ID = 7617;


        private DomoticzServer currentDomoticzServer = null;
        private string _serveradress = "";
        private string _serverport = "";
        private DateTime RefreshTime = DateTime.Now.AddHours(-1); //for autorefresh
        private int RefreshInterval = 10; //in seconds
        int DeviceIdx = -1;
        private int ServerStatus = -1;
        private DomoticzServer.DeviceResponse DevResponse = null;

        [SkinControlAttribute(100)]
        protected GUILabelControl SelectedDevice = null;


        /// <summary>
        /// Get Window-ID
        /// </summary>
        /// <returns>The current WindowId</returns>

        public int GetWindowId()
        {
            return GUIDeviceDetails_WINDOW_ID;
        }


        public override int GetID
        {
            get
            {
                return GUIDeviceDetails_WINDOW_ID;
            }
        }


        public GUIDeviceDetails()
        {
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.GetThemedSkinFile(@"\MP-Domoticz.DeviceDetails.xml"));
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    {
                        //Log.Info("GUIDeviceDetails: GUI_MSG_WINDOW_INIT");
                        LoadSettings();
                        DeviceIdx = Convert.ToInt32(GUIPropertyManager.GetProperty("#MP-DomoticzDeviceDetials"));                                           
                        base.OnMessage(message);
                        Refresh();
        
                        return true;
                    }


                case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
                    {
                        //Log.Info("GUIDeviceDetails: GUI_MSG_WINDOW_DEINIT");                        
                        return true;
                    }

                case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
                    {
                        Refresh();
                        return true;
                    }

                default:
                    Log.Info("DEFAULT: " + message.Message.ToString());
                    return true;
            }
        }

        private void Refresh()
        {
            if (currentDomoticzServer == null)
            {
                currentDomoticzServer = new DomoticzServer();
                ServerStatus = currentDomoticzServer.InitServer(_serveradress, _serverport);
            }

            if (ServerStatus == 0)
            {
                Log.Info("No connection to server " + _serveradress);
                GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(
                   (int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dlgOK.SetHeading("No connection");
                dlgOK.SetLine(1, "No connection to server " + _serveradress);
                dlgOK.SetLine(2, String.Empty);
                dlgOK.SetLine(3, String.Empty);
                dlgOK.DoModal(GUIDeviceDetails_WINDOW_ID);                
                currentDomoticzServer = null;
                return;
            }

            DevResponse = currentDomoticzServer.GetSingleDevice(DeviceIdx);
            DomoticzServer.Device dev = DevResponse.result[0];
            SelectedDevice.Label = ": " + dev.LastUpdate;

            DomoticzServer.SunSetRise sun = currentDomoticzServer.GetSunSet();

            if (sun != null)
            {
                string str = Translation.Servertime + ": " + sun.ServerTime + " " +
                    Translation.Sunrise + ": " + sun.Sunrise + " " +
                    Translation.Sunset + ": " + sun.Sunset;

                GUIPropertyManager.SetProperty("#MPDomoticz.ServerTime", str);
            }
            else
            {
                currentDomoticzServer = null;
            }

            string fileDir = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\MPDomoticz";
            Log.Info("Check: "+fileDir);
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            
            string fileName = MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Thumbs) + "\\MPDomoticz\\"+DeviceIdx+".png";
            Graph.GenerateGraph(currentDomoticzServer, DeviceIdx, fileName);
            Log.Info("Filename:" + fileName);
            GUIPropertyManager.SetProperty("#MPDomoticz.WeekThumb", fileName); 
        }

        // <summary>
        ///Handle automatic refresh of content from the Domoticz server 
        /// </summary>
        public override void Process()
        {
            TimeSpan ts = DateTime.Now - RefreshTime;
            if ((ts.TotalSeconds >= RefreshInterval))
            {
                // Reset time
                RefreshTime = DateTime.Now;
                Refresh();
            }
            base.Process();
        }

        #region Serialisation
        private void LoadSettings()
        {
            using (Settings xmlreader = new MPSettings())
            {
                _serveradress = xmlreader.GetValueAsString("MPDomoticz", "ServerAdress", "localhost");
                _serverport = xmlreader.GetValueAsString("MPDomoticz", "ServerPort", "8080");
                RefreshInterval = xmlreader.GetValueAsInt("MPDomoticz", "RefreshInterval", 30);
            }
        }
        #endregion

    }
}
