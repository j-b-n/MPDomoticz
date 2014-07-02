using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

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
            CONTROL_CHECKBTN = 10,
            CONTROL_YESNOBTN = 11,
        }

        DeviceFilter.DeviceFilterBy CurrentFilterBy = DeviceFilter.DeviceFilterBy.All;

        DeviceSort.SortMethod CurrentSortBy = DeviceSort.SortMethod.Name;
        bool CurrentSortAsc = true;


        /// <summary>
        /// This plugins unique window ID
        /// </summary>
        private const int WINDOW_ID = 7616;
        public const int GUIDeviceDetails_WINDOW_ID = 7617;

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

        [SkinControl(10)]
        protected GUICheckButton btnCheckButton = null;

        [SkinControl(11)]
        protected GUIDialogYesNo btnYesNo= null;

        #endregion

        #region private properties
        private DomoticzServer currentDomoticzServer = null;
        private string _serveradress = "";
        private string _serverport = "";

        private bool IsNetworkAvailable = false;        
        DomoticzServer.DeviceResponse DevResponse = null;

        private DateTime RefreshTime = DateTime.Now.AddHours(-1); //for autorefresh
        private int RefreshInterval = 10; //in seconds
        #endregion

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
            return WINDOW_ID;
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

        #region Serialisation


        private void LoadSettings()
        {
            using (Settings xmlreader = new MPSettings())
            {                
                _serveradress = xmlreader.GetValueAsString("MPDomoticz", "ServerAdress", "localhost");
                _serverport = xmlreader.GetValueAsString("MPDomoticz", "ServerPort", "8080");
                RefreshInterval = xmlreader.GetValueAsInt("MPDomoticz", "RefreshInterval", 30);
                
                
                string strTmp = xmlreader.GetValueAsString("MPDomoticz", "FilterBy", "All");                
                if (!Enum.TryParse<DeviceFilter.DeviceFilterBy>(strTmp, out CurrentFilterBy))
                {
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.All;
                }
                               
                //CurrentSortBy = (DeviceSort.SortMethod)xmlreader.GetValueAsInt("MPDomoticz", "SortBy", (int)DeviceSort.SortMethod.Name);
                strTmp = xmlreader.GetValueAsString("MPDomoticz", "SortBy", "Name");
                if (!Enum.TryParse<DeviceSort.SortMethod>(strTmp, out CurrentSortBy))
                {
                    CurrentSortBy = DeviceSort.SortMethod.Name;
                }
                
                int tmp = xmlreader.GetValueAsInt("MPDomoticz", "SortByAsc", 1);
                CurrentSortAsc = true;
                if (tmp == 0)
                {
                    CurrentSortAsc = false;
                }
            }
        }


        private void SaveSettings()
        {
            using (Settings xmlwriter = new MPSettings())
            {
                xmlwriter.SetValue("MPDomoticz", "ServerAdress", _serveradress);
                xmlwriter.SetValue("MPDomoticz", "ServerPort", _serverport);
                xmlwriter.SetValue("MPDomoticz", "RefreshInterval", RefreshInterval);
                xmlwriter.SetValue("MPDomoticz", "SortBy", CurrentSortBy);
                if (CurrentSortAsc)
                {
                    xmlwriter.SetValue("MPDomoticz", "SortByAsc", 1);
                }
                else
                {
                    xmlwriter.SetValue("MPDomoticz", "SortByAsc", 0);
                }
                xmlwriter.SetValue("MPDomoticz", "FilterBy", CurrentFilterBy);
                
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

                        LoadSettings();

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

                        //Log.Info("MP-Domoticz: GUI_MSG_WINDOW_INIT COMPLETE");
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

                        if (iControl == (int)Controls.CONTROL_CHECKBTN)
                        {                            
                            OnToggleSwitchCmd();
                            break;
                        }

                        if (iControl == (int)Controls.CONTROL_LIST)
                        {
                            OnShowDeviceDetails();
                            break;
                        }

                        if (iControl == (int)Controls.CONTROL_YESNOBTN)
                        {
                            Log.Info("MP-Domoticz: CONTROL_YESNOBTN");
                            GUIWindowManager.ShowPreviousWindow();
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
                case DeviceFilter.DeviceFilterBy.Favorites:
                    btnStr += Translation.Favourites;
                    break;
                case DeviceFilter.DeviceFilterBy.Scenes:
                    btnStr += Translation.Scenes;
                    break;
                case DeviceFilter.DeviceFilterBy.Switches:
                    btnStr += Translation.Switches;
                    break;
                case DeviceFilter.DeviceFilterBy.Temperature:
                    btnStr += Translation.Temperature;
                    break;
                case DeviceFilter.DeviceFilterBy.Weather:
                    btnStr += Translation.Weather;
                    break;
                case DeviceFilter.DeviceFilterBy.Utility:
                    btnStr += Translation.Utility;
                    break;
                case DeviceFilter.DeviceFilterBy.All:
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

            btnCheckButton.Visible = false;
            DomoticzServer.Device device = (DomoticzServer.Device)item.MusicTag;

            if(device == null)
            {
                return;
            }

            string desc = "";
            listDevices.NavigateLeft = 5;

            desc = currentDomoticzServer.GetDeviceDescription(device);

            switch (device.Type)
            {                
                case "Lighting 2":                    
                    btnCheckButton.Label = Translation.Status;
                    btnCheckButton.Visible = true;
                    if (device.Status == "On")
                    {
                        btnCheckButton.Selected = true;
                    } else
                    {
                        btnCheckButton.Selected = false;
                    }
                    listDevices.NavigateLeft = 10;
                    break;
                
                default:                
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
            int ServerStatus = -1;
            if (currentDomoticzServer == null)
            {
                currentDomoticzServer = new DomoticzServer();
                ServerStatus = currentDomoticzServer.InitServer(_serveradress,_serverport);                
            }

            if(ServerStatus == 0 )
            {
                Log.Info("No connection to server "+_serveradress);
                GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow(
                   (int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dlgOK.SetHeading("No connection");
                dlgOK.SetLine(1, "No connection to server " + _serveradress);
                dlgOK.SetLine(2, String.Empty);
                dlgOK.SetLine(3, String.Empty);
                dlgOK.DoModal(WINDOW_ID);                
                currentDomoticzServer = null;
                return;
            }
            
            DevResponse = null;

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

            if (listDevices != null)
            {                                
                DevResponse = currentDomoticzServer.GetAllDevices();
                                               
                if (DevResponse != null)
                {          
                    OnFilter();                    
                    UpdateButtons();
                }
                else
                {                    
                    currentDomoticzServer = null;
                }
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
                    poster = currentDomoticzServer.GetTempIcon(dev.Temp);
                    break;

                case "Temp + Humidity":
                    item.Label2 = dev.Temp.ToString() + "°";
                    if (dev.Humidity != null)
                    {
                        item.Label2 += " / " + dev.Humidity.ToString() + "%";
                    }
                    poster = currentDomoticzServer.GetTempIcon(dev.Temp);
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

        /// <summary>
        /// Toggle the checkbutton!
        /// </summary>
        protected void OnToggleSwitchCmd()
        {
            DomoticzServer.Response response = null;

            GUIListItem item = GetSelectedItem();
            if (item == null)
            {
                return;
            }

            btnCheckButton.Visible = false;
            DomoticzServer.Device device = (DomoticzServer.Device)item.MusicTag;
            
            if(device.Type != "Lighting 2")
            {
                Log.Info("OnToggleSwitch - not possible to toggle: "+device.Name);
                return;
            }

            if (device.Status == "On")
            {
                response = currentDomoticzServer.SwitchLight(device.idx, "Off");
            }
            else
            {
                response = currentDomoticzServer.SwitchLight(device.idx, "On");
            }
            if (response == null)
            {
                Log.Info("OnToggleSwitch " + device.idx + " RESPONSE NULL!");
            }
            else
            {
                Log.Info("OnToggleSwitch " + device.idx + " " + response.title + " " + response.status);
            }
        }

        #region OnShowDeviceDetails
        private void OnShowDeviceDetails()
        {            
            GUIDeviceDetails dlg = (GUIDeviceDetails)GUIWindowManager.GetWindow(GUIDeviceDetails_WINDOW_ID);
            if (dlg == null)
            {
                return;
            }
            dlg.ResetAllControls();
            
            GUIListItem item = GetSelectedItem();
            if (item == null)
            {
                return;
            }
            DomoticzServer.Device device = (DomoticzServer.Device)item.MusicTag;
            
            Log.Info("Open Device info for " + device.idx);
            GUIPropertyManager.SetProperty("#MP-DomoticzDeviceDetials", device.idx.ToString());
            GUIWindowManager.ActivateWindow((int)GUIDeviceDetails_WINDOW_ID,device.idx.ToString());
        }

        #endregion

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
            SaveSettings();
        }
        
        protected virtual void OnSort()
        {            
            DevResponse.result.Sort(new DeviceSort(CurrentSortBy, CurrentSortAsc));            
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
            dlg.Add(Translation.Scenes);
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
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Favorites;
                    break;
                case 2:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Switches;
                    break;
                case 3:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Scenes;
                    break;
                case 4:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Temperature;
                    break;
                case 5:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Weather;
                    break;
                case 6:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Utility;
                    break;
                case 7:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.All;
                    break;
                default:
                    CurrentFilterBy = DeviceFilter.DeviceFilterBy.Favorites;
                    break;
            }            
            OnFilter();
            UpdateButtons();
            GUIControl.FocusControl(GetID, btnFilterBy.GetID);
            SaveSettings();
        }

        protected virtual void OnFilter()
        {
            OnSort();
            listDevices.Clear();

            List<DomoticzServer.Device> res = null;            
            DeviceFilter F = new DeviceFilter(CurrentFilterBy);
            res = DevResponse.result.Where(x => F.Filter(x)).ToList();
            
            if(DevResponse.result==null)
            {

                Log.Info("OnFilter() DevResponse.result NULL"); 
                return;
            }

            foreach (DomoticzServer.Device dev in res)
            {
                AddListItem(dev);                               
            }            
        }

        #endregion

    }

}

        #endregion