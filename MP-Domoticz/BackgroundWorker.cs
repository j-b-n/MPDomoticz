using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Globalization;

namespace MP_Domoticz
{
    class BackgroundWorker
    {
        #region BackgroundWorker
        BackgroundWorker pluginBackgroundWorker;
        void StartBackgroundInitialization()
        {
            Log.Info("PictureOfTheDay: Init!");
            pluginBackgroundWorker = new BackgroundWorker();            
            pluginBackgroundWorker.WorkerReportsProgress = true;
            pluginBackgroundWorker.WorkerSupportsCancellation = false;
            pluginBackgroundWorker.DoWork += DoWork;
            pluginBackgroundWorker.RunWorkerAsync();
        }

        private void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {                        
            //Log.Info("PictureOfTheDay: No refresh!", 1);            
        }

#endregion

    }
}
