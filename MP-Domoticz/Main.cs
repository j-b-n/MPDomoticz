using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using System;
using System.IO;
using System.Windows.Forms;

//http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/18_Contribute/6_Plugins/Plugin_Developer's_Guide/1_Develop_a_Plugin
/*
 * 
 * 
 * Get graph data:
 * http://192.168.1.6:8080/json.htm?type=graph&sensor=temp&idx=47&range=day range can be day, month or year
 */

namespace MP_Domoticz
{
    /// <summary>
    /// Domoticz Plugin for MediaPortal
    /// 
    /// 
    /// Domoticz is a Home Automation System that lets you monitor and configure various devices like:
    /// Lights, Switches, various sensors/meters like Temperature, Rain, Wind, UV, Electra, Gas, Water and
    /// much more. 
    /// 
    /// For more information see http://domoticz.com
    /// 
    /// </summary>

    public class Main : GUIWindow, ISetupForm
    {
        #region constants

        private enum Controls
        {
            CONTROL_BACKGROUND = 1,
            CONTROL_LIST = 50,
            CONTROL_LABEL = 411,
            CONTROL_BTNREFRESH = 3,
        }

        private enum DeviceFilter
        {
            Favorites = 1,
            Switches = 2,
            Scenes = 3,
            Temperature = 4,
            Weather = 5,
            Utility = 6,
            All = 7,
        }

        int CurrentFilterBy = (int)DeviceFilter.All;

        DeviceSort.SortMethod CurrentSortBy = DeviceSort.SortMethod.Name;
        bool CurrentSortAsc = true;


        /// <summary>
        /// This plugins unique window ID
        /// </summary>
        private const int WINDOW_ID = 7616;

        #endregion


        #region control handles

        [SkinControlAttribute(50)]
        protected GUIListControl listDevices = null;

        [SkinControlAttribute(402)]
        protected GUILabelControl SelectedDevice = null;

        [SkinControl(2)]
        protected GUISortButtonControl btnSortBy = null;

        [SkinControl(5)]
        protected GUIMenuButton btnFilterBy = null;
        #endregion

        #region private properties
        private DomoticzServer currentDomoticzServer = null;
        private bool IsNetworkAvailable = false;
        private DomoticzServer.DeviceResponse DevResponse = null;
        private DateTime RefreshTime = DateTime.Now.AddHours(-1); //for autorefresh
        private int RefreshInterval = 10; //in seconds
        #endregion


        public Main()
        {

        }

        #region ISetupForm Members
       
        /// <summary>
        ///Returns the name of the plugin which is shown in the plugin menu 
        /// </summary>
        /// <returns></returns>
        public string PluginName()
        {
            return "MP-Domoticz";
        }

        /// <summary>
        /// Returns the description of the plugin is shown in the plugin menu
        /// </summary>
        /// <returns></returns> 
        public string Description()
        {
            return "MP-Domoticz interfaces with a Domiticz server.";
        }

        /// <summary>
        /// Returns the author of the plugin which is shown in the plugin menu
        /// </summary>
        /// <returns></returns>
        public string Author()
        {
            return "J-B-N";
        }

        /// <summary>
        /// show the setup dialog
        /// </summary>
        public void ShowPlugin()
        {
            SetupForm _form = new SetupForm();
            _form.Show();
            _form.FormClosing += new FormClosingEventHandler(SetupFormClosing);            
        }

        /// <summary>
        /// Handle the user input and save to setup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SetupFormClosing (object sender, FormClosingEventArgs e)
        {

        }

        /// <summary>
        /// Indicates whether plugin can be enabled/disabled
        /// </summary>
        /// <returns></returns> 
        public bool CanEnable()
        {
            return true;
        }

        /// <summary>
        /// Get Window-ID
        /// </summary>
        /// <returns>The current WindowId</returns>
 
        public int GetWindowId()
        {           
            return 7616;
        }

        /// <summary>
        /// Indicates if plugin is enabled by default
        /// </summary>
        /// <returns></returns>
 
        public bool DefaultEnabled()
        {            
            return true;
        }

        /// <summary>
        /// Indicates if a plugin has it's own setup screen
        /// </summary>
        /// <returns></returns>
 
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// If the plugin should have it's own button on the main menu of MediaPortal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true : plugin needs it's own button on home
        /// false : plugin does not need it's own button on home</returns>
        /// 

        public bool GetHome(out string strButtonText, out string strButtonImage,
                            out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = PluginName();
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }

        ///<summary>
        /// With GetID it will be an window-plugin / otherwise a process-plugin        
        /// </summary>
        public override int GetID
        {
            get
            {
                return WINDOW_ID;
            }
            set
            {
            }
        }

        #endregion

        #region UI
        /// <summary>
        /// Init the plugin
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            IsNetworkAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
            return Load(GUIGraphicsContext.Skin + @"\MP-Domoticz.xml");
        }

        public override void DeInit()
        {
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;
            base.DeInit();                    
        }

        /// <summary>
        /// Catch network changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NetworkAvailabilityChanged(object sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Log.Info("MPDomoticz is connected to the network");
                IsNetworkAvailable = true;
            }
            else
            {
                Log.Info("MPDomoticz is disconnected from the network");
                IsNetworkAvailable = false;
            }
        } 

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    {
                        //Log.Info("MP-Domoticz: GUI_MSG_WINDOW_INIT");

                        #region Translations

                        Translation.Init();

                        // Push Translated Strings to skin
                        Log.Info("Setting translated strings: ");
                        string propertyName = string.Empty;
                        string propertyValue = string.Empty;

                        foreach (string name in Translation.Strings.Keys)
                        {
                            propertyName = "#MPDomoticz.Translation." + name + ".Label";
                            propertyValue = Translation.Strings[name];

                            Log.Info(propertyName + " = " + propertyValue);
                            GUIPropertyManager.SetProperty(propertyName, propertyValue);
                        }

                        #endregion



                        RefreshTime = DateTime.Now;
                        base.OnMessage(message);
                        GUIPropertyManager.SetProperty("#currentmodule", "MP-Domoticz");
                        GUIPropertyManager.SetProperty("#header.label", "MP-Domoticz");
                        GUIPropertyManager.SetProperty("#MPDomoticz.Desc", "Foo");
                        GUIPropertyManager.SetProperty("#MPDomoticz.ServerTime", "ServerTime: ");

                        Refresh();
                        UpdateButtons();

                        //LoadSettings();
                        // m_pSiteImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGELOGO);

                        return true;
                    }


                case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
                    {
                        Log.Info("MP-Domoticz: GUI_MSG_WINDOW_DEINIT");
                        //SaveSettings();
                    }
                    break;


                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                    {
                        //Log.Info("MP-Domoticz: GUI_MSG_ITEM_FOCUS_CHANGED");
                        int iControl = message.SenderControlId;
                        if (iControl == (int)Controls.CONTROL_LIST)
                        {
                            UpdateDetails();
                        }
                    }
                    break;


                case GUIMessage.MessageType.GUI_MSG_CLICKED:
                    {
                        int iControl = message.SenderControlId;
                        Log.Info("MP-Domoticz: GUI_MSG_CLICKED iControl:" + iControl);
                        if (iControl == btnSortBy.GetID)
                        {
                            OnShowSort();
                            break;
                        }

                        if (iControl == btnFilterBy.GetID)
                        {
                            Log.Info("MP-Domoticz: GUI_MSG_CLICKED OnShowFilter:");
                            OnShowFilter();
                            break;
                        }


                        if (iControl == (int)Controls.CONTROL_BTNREFRESH)
                        {
                            Refresh();
                            break;
                        }


                    }
                    break;
            }
            return base.OnMessage(message);
        }

        private void UpdateButtons()
        {
            string btnStr = "";
            btnSortBy.IsAscending = CurrentSortAsc;
            btnStr = GUILocalizeStrings.Get(96); //Sort: 
            switch (CurrentSortBy)
            {
                case DeviceSort.SortMethod.Name:
                    btnStr += GUILocalizeStrings.Get(103); //Name
                    break;
                case DeviceSort.SortMethod.LastSeen:
                    btnStr += GUILocalizeStrings.Get(104); //Date
                    break;
            }
            btnSortBy.Label = btnStr;

            btnStr = "";
            btnStr = GUILocalizeStrings.Get(97); //View by: 
            switch (CurrentFilterBy)
            {
                case (int)DeviceFilter.Favorites:
                    btnStr += Translation.Favourites;
                    break;
                case (int)DeviceFilter.Scenes:
                    btnStr += Translation.Scenes;
                    break;
                case (int)DeviceFilter.Switches:
                    btnStr += Translation.Switches;
                    break;
                case (int)DeviceFilter.Temperature:
                    btnStr += Translation.Temperature;
                    break;
                case (int)DeviceFilter.Weather:
                    btnStr += Translation.Weather;
                    break;
                case (int)DeviceFilter.Utility:
                    btnStr += Translation.Utility;
                    break;
                case (int)DeviceFilter.All:
                    btnStr += Translation.All;
                    break;
            }            
            btnFilterBy.Label = btnStr;
        }


        private void UpdateDetails()
        {
            GUIListItem item = GetSelectedItem();
            if (item == null)
            {
                return;
            }

            DomoticzServer.Device device = (DomoticzServer.Device)item.MusicTag;

            string desc = "";

            switch (device.Type)
            {
                case "Temp":
                    desc = device.Temp.ToString() + "°";
                    break;

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

                    break;
                case "Wind":
                    desc = device.DirectionStr + " / " + device.Speed + "m/s";
                    if (device.Gust != null)
                    {
                        desc += " " + Translation.Gust + ": " + device.Gust.ToString() + "°";
                    }
                    break;

                case "Lighting 2":
                    desc = Translation.Status + ": " + device.Status;
                    break;

                case "Rain":
                    desc = device.Rain + " mm";
                    break;

                default:
                    desc = "";
                    break;
            }

            GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABEL, device.Name + " " +
                device.HardwareName + " " + device.Image);

            GUIPropertyManager.SetProperty("#MPDomoticz.Desc", desc);
            GUIPropertyManager.SetProperty("#MPDomoticz.DeviceLastUpdate", device.LastUpdate);
        }


        /// <summary>
        /// Refresh the data in the plugin
        /// </summary>
        protected void Refresh()
        {
            if (currentDomoticzServer == null)
            {
                currentDomoticzServer = new DomoticzServer();
                currentDomoticzServer.InitServer("192.168.1.6", "8080");
            }

            DevResponse = null;


            DomoticzServer.SunSetRise sun = currentDomoticzServer.GetSunSet();
            string str = "Servertime:" + sun.ServerTime + " " +
                "SunRise: " + sun.Sunrise + " " +
                "SunSet:" + sun.Sunset;            

            GUIPropertyManager.SetProperty("#MPDomoticz.ServerTime", "ServerTime: " + sun.ServerTime);
            
            if (listDevices != null)
            {
                DevResponse = currentDomoticzServer.GetAllDevices();
                OnFilter();
                OnSort();
                UpdateButtons();
            }

        }
       
        /// <summary>
        ///Handle automatic refresh of content from the Domoticz server 
        /// </summary>
        public override void Process()
        {
            TimeSpan ts = DateTime.Now - RefreshTime;
            if ((ts.TotalSeconds >= RefreshInterval))
            {
                // Reset time
                RefreshTime = DateTime.Now;


                // Check if we may refresh (only when not in the list or on the first item)
                if (GetFocusControlId() != listDevices.GetID)
                {
                    Refresh();
                }
                else
                {
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                                    (int)Controls.CONTROL_LIST, 0, 0, null);
                    OnMessage(msg);
                    if ((int)msg.Param1 < 1)
                    {
                        // Try refresh without warnings
                        Refresh();
                    }
                }
            }
            base.Process();
        }

        /* If you need to add things programmatically
     GUILabelControl mylabel;
     public override void AllocResources()
     {
         base.AllocResources();
         mylabel = new GUILabelControl(7616, 999, 300, 400, 200, 100, string.Empty, 
             "This is a new string to render in a label", 0xFFFFFFFF, GUIControl.Alignment.Left,
             GUIControl.VAlignment.Bottom , false,0,0,0);
         mylabel.AllocResources();
     }

     public override void Render(float timePassed)
     {
         base.Render(timePassed);
         mylabel.Render(timePassed);
     }
        */

        /*
                protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
                {                    
                    if (control == listDevices)
                        ItemSelectionChanged();

                    base.OnClicked(controlId, control, actionType);
                }
                */
        /*
        public override void OnAction(MediaPortal.GUI.Library.Action action)
        {
            //Log.Info("MP-Domoticz: Action ID " + action.wID);         
            //if (cntrlFacade.IsFocused)
            if (listDevices != null)
            {                
                switch (action.wID)
                {
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP:
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                    case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
                        ItemSelectionChanged();
                        break;
                }
            }
            base.OnAction(action);
        }
        */

        private GUIListItem GetSelectedItem()
        {
            int iControl;

            iControl = (int)Controls.CONTROL_LIST;
            GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
            return item;
        }


        private void ItemSelectionChanged()
        {
            if (listDevices != null)
            {
                int index = listDevices.SelectedListItemIndex;

                Log.Info("MP-Domoticz: Index: " + index + " Count: " + DevResponse.result.Count);

                SelectedDevice.Label = "Index: " + index;

                GUIListItem item = GetSelectedItem();
                Log.Info("MP-Domoticz: ItemId:" + item.Label);

                DomoticzServer.Device dev = DevResponse.result[index];
                GUIPropertyManager.SetProperty("#MPDomoticz.Desc", dev.Name);
                GUIPropertyManager.SetProperty("#MPDomoticz.DeviceLastUpdate", dev.LastUpdate);

            }
        }

        /// <summary>
        /// Return a string to the current appropriate temperature icon
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        string GetTempIcon(double temp)
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


        private bool AddListItem(DomoticzServer.Device dev)
        {
            string skinName = MediaPortal.Configuration.Config.SkinName;
            string skinPath = MediaPortal.Configuration.Config.GetSubFolder(MediaPortal.Configuration.Config.Dir.Skin, skinName);

            string poster = skinPath + "\\Media\\Domoticz\\" + dev.TypeImg + "48.png";
            string thumb = skinPath + "\\Media\\Domoticz\\" + dev.TypeImg + ".png";

            GUIListItem item = new GUIListItem(dev.Name);

            switch (dev.Type)
            {
                case "Temp":
                    item.Label2 = dev.Temp.ToString() + "°";
                    poster = GetTempIcon(dev.Temp);
                    break;

                case "Temp + Humidity":
                    item.Label2 = dev.Temp.ToString() + "°";
                    if (dev.Humidity != null)
                    {
                        item.Label2 += " / " + dev.Humidity.ToString() + "%";
                    }
                    poster = GetTempIcon(dev.Temp);
                    break;
                case "Wind":
                    item.Label2 = dev.DirectionStr + " / " + dev.Speed + "m/s";
                    poster = skinPath + "\\Media\\Domoticz\\Wind" + dev.DirectionStr + ".png";
                    break;

                case "Lighting 2":
                    item.Label2 = dev.Status;
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
                    item.Label2 = dev.Rain + " mm";
                    break;

                default:
                    item.Label2 = "";
                    break;

            }

            if (!File.Exists(poster))
            {
                poster = thumb;
            }            

            item.IconImage = thumb;
            item.IconImageBig = thumb;
            item.ThumbnailImage = poster;
            item.MusicTag = dev;
            GUIControl.AddListItemControl(GetID, listDevices.GetID, item);
            return true;
        }

        #region Sort
        /// <summary>
        /// Show Sort dialog
        /// </summary>
        protected void OnShowSort()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return;
            }


            dlg.Reset();
            dlg.SetHeading(495); // Sort options
            int maxCommonSortIndex = -1; // Inrease by 1 when adding new common sort label(sort valid in share and db views))


            // Common sorts - group 1
            dlg.AddLocalizedString(365); // 0 name
            maxCommonSortIndex++;
            dlg.AddLocalizedString(104); // 2 date created (date)
            maxCommonSortIndex++;


            // show dialog and wait for result
            dlg.DoModal(GetID);
            if (dlg.SelectedId == -1)
            {
                return;
            }

            CurrentSortAsc = true;
            switch (dlg.SelectedId)
            {
                case 365:
                    CurrentSortBy = DeviceSort.SortMethod.Name;
                    break;
                case 104:
                    CurrentSortBy = DeviceSort.SortMethod.LastSeen;
                    CurrentSortAsc = false;
                    break;
                default:
                    CurrentSortBy = DeviceSort.SortMethod.Name;
                    break;
            }

            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID, btnSortBy.GetID);
        }

        protected virtual void OnSort()
        {
            listDevices.Sort(new DeviceSort(CurrentSortBy, CurrentSortAsc));
        }

        #endregion

        #region Filter
        /*
         * Filter
         */
        protected void OnShowFilter()
        {
            Log.Info("MP-Domoticz: OnShowFilter");
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return;
            }


            dlg.Reset();
            dlg.SetHeading(97);

            // Common sorts - group 1

            dlg.Add(Translation.Favourites);
            dlg.Add(Translation.Switches);
            dlg.Add(Translation.Temperature);
            dlg.Add(Translation.Weather);
            dlg.Add(Translation.Utility);
            dlg.Add(Translation.All);


            // show dialog and wait for result
            dlg.DoModal(GetID);
            if (dlg.SelectedId == -1)
            {
                return;
            }

            switch (dlg.SelectedId)
            {
                case 1:
                    CurrentFilterBy = (int)DeviceFilter.Favorites;
                    break;
                case 2:
                    CurrentFilterBy = (int)DeviceFilter.Switches;
                    break;
                case 3:
                    CurrentFilterBy = (int)DeviceFilter.Scenes;
                    break;
                case 4:
                    CurrentFilterBy = (int)DeviceFilter.Temperature;
                    break;
                case 5:
                    CurrentFilterBy = (int)DeviceFilter.Weather;
                    break;
                case 6:
                    CurrentFilterBy = (int)DeviceFilter.Utility;
                    break;
                case 7:
                    CurrentFilterBy = (int)DeviceFilter.All;
                    break;
                default:
                    CurrentFilterBy = (int)DeviceFilter.Favorites;
                    break;
            }
            OnFilter();
            UpdateButtons();
            GUIControl.FocusControl(GetID, btnFilterBy.GetID);
        }

        protected virtual void OnFilter()
        {
            listDevices.Clear();
            foreach (DomoticzServer.Device dev in DevResponse.result)
            {
                switch (CurrentFilterBy)
                {
                    case (int)DeviceFilter.Favorites:
                        if (dev.Favorite > 0)
                        {
                            AddListItem(dev);
                        }
                        break;
                    case (int)DeviceFilter.Switches:
                        if (dev.Type == "Lighting 2" ||
                            dev.Type == "Security")
                        {
                            AddListItem(dev);
                        }
                        break;
                    case (int)DeviceFilter.Temperature:
                        if (dev.Type == "Temp" ||
                            dev.Type == "Temp + Humidity" ||
                            dev.Type == "Temp + Baro")
                        {
                            AddListItem(dev);
                        }
                        break;
                    default:
                        AddListItem(dev);
                        break;
                }
            }
            OnSort();
        }

        #endregion

    }

}

        #endregion