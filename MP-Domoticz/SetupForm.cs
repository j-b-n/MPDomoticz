using MediaPortal.Configuration;
using MediaPortal.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP_Domoticz
{
    public partial class SetupForm : Form
    {        
        [PluginIcons("MPDomoticz.Domoticz_icon.png", "MPDomoticz.Domoticz_icon.png")]
        private string _serveradress = "";
        private string _serverport = "";
        private int RefreshInterval = 0;

        public SetupForm()
        {
            InitializeComponent();
            LoadSettings();
            textBox1.Text = _serveradress;
            textBox2.Text = _serverport;
            if (RefreshInterval < 5)
            {
                RefreshInterval = 5;
            }
            numericUpDown1.Value = RefreshInterval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Save settings
            
            Uri uriResult; 
            bool result = Uri.TryCreate(textBox1.Text, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;

            if (result)
            {
                _serveradress = uriResult.ToString();
            }
            else
            {
                MessageBox.Show("Malformed url entered!");
                return;
            }
            _serverport = textBox2.Text;
            RefreshInterval = (int)numericUpDown1.Value;

            SaveSettings();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
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


        private void SaveSettings()
        {
            using (Settings xmlwriter = new MPSettings())
            {
                xmlwriter.SetValue("MPDomoticz", "ServerAdress", _serveradress);
                xmlwriter.SetValue("MPDomoticz", "ServerPort", _serverport);
                xmlwriter.SetValue("MPDomoticz", "RefreshInterval", RefreshInterval);
            }
        }


        #endregion        

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/j-b-n/MPDomoticz");    

        }


    }
}
