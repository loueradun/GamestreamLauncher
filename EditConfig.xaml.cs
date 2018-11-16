using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GamestreamLauncher
{
    /// <summary>
    /// Interaction logic for EditConfig.xaml
    /// </summary>
    public partial class EditConfig : Window
    {
        public EditConfig()
        {
            InitializeComponent();

            txtAppName.Text = Properties.Settings.Default.AppName;
            txtAppPath.Text = Properties.Settings.Default.AppPath;
            chkMiner.IsChecked = Properties.Settings.Default.AwesomeMinerSwitchEnabled;
            chkMonitor.IsChecked = Properties.Settings.Default.MultiMonSwitchEnabled;
            chkScripts.IsChecked = Properties.Settings.Default.ScriptsEnabled;
            txtMinerIP.Text = Properties.Settings.Default.AwesomeMinerIP;
            txtMinerPort.Text = Properties.Settings.Default.AwesomeMinerPort;
            txtMinerApiKey.Text = Properties.Settings.Default.AwesomeMinerKey;
            txtStartupScriptPath.Text = Properties.Settings.Default.StartupScriptPath;
            txtStartupScriptParams.Text = Properties.Settings.Default.StartupScriptParameters;
            txtShutdownScriptPath.Text = Properties.Settings.Default.ShutdownScriptPath;
            txtShutdownScriptParams.Text = Properties.Settings.Default.ShutdownScriptParameters;

            UpdateUI();
        }

        private void UpdateUI()
        {
            txtMinerIP.IsEnabled = txtMinerPort.IsEnabled = txtMinerApiKey.IsEnabled = chkMiner.IsChecked.GetValueOrDefault();
            txtStartupScriptPath.IsEnabled = txtStartupScriptParams.IsEnabled = txtShutdownScriptPath.IsEnabled = txtShutdownScriptParams.IsEnabled = btnBrowseStartupScriptPath.IsEnabled = btnBrowseShutdownScriptPath.IsEnabled = chkScripts.IsChecked.GetValueOrDefault();
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";

            if (chkMiner.IsChecked.GetValueOrDefault())
            {
                HelperApi.LauncherApi launcherApi = new HelperApi.LauncherApi();

                if (!launcherApi.LoadMinerInfo(txtMinerIP.Text, txtMinerPort.Text, txtMinerApiKey.Text))
                    strError = "The Miner could not be reached at " + String.Format("http://{0}:{1}/api/miners?key={2}", txtMinerIP.Text, txtMinerPort.Text, txtMinerApiKey.Text) + Environment.NewLine + "Please check the Miner IP, Port, and API Key.";

                // call awesome miner web portal and verify connectivity
            }

            if (chkScripts.IsChecked.GetValueOrDefault())
            {
                if (String.IsNullOrEmpty(txtShutdownScriptPath.Text) || !File.Exists(txtShutdownScriptPath.Text))
                    strError = "The Shutdown Script Path does not point to an executable file.";
                if (String.IsNullOrEmpty(txtStartupScriptPath.Text) || !File.Exists(txtStartupScriptPath.Text))
                    strError = "The Startup Script Path does not point to an executable file.";
            }

            if (!File.Exists(txtAppPath.Text))
                strError = "The App Path does not point to an executable file.";

            if (String.IsNullOrEmpty(strError))
            {
                MessageBox.Show("Getting ready to give general users permission to stop a Gamestream session.  Please accept the following UAC prompt or Gamestream sessions will not end gracefully.", "Enabling Access To Gamestream Service", MessageBoxButton.OK, MessageBoxImage.Information);
                GrantAccessGamestreamService(); // Should only really do this if necessary, but it shouldn't hurt anything for the moment
                Properties.Settings.Default.AppName = txtAppName.Text;
                Properties.Settings.Default.AppPath = txtAppPath.Text;
                Properties.Settings.Default.AwesomeMinerSwitchEnabled = chkMiner.IsChecked.GetValueOrDefault();
                Properties.Settings.Default.MultiMonSwitchEnabled = chkMonitor.IsChecked.GetValueOrDefault();
                Properties.Settings.Default.ScriptsEnabled = chkScripts.IsChecked.GetValueOrDefault();
                Properties.Settings.Default.AwesomeMinerIP = txtMinerIP.Text;
                Properties.Settings.Default.AwesomeMinerPort = txtMinerPort.Text;
                Properties.Settings.Default.AwesomeMinerKey = txtMinerApiKey.Text;
                Properties.Settings.Default.StartupScriptPath = txtStartupScriptPath.Text;
                Properties.Settings.Default.StartupScriptParameters = txtStartupScriptParams.Text;
                Properties.Settings.Default.ShutdownScriptParameters = txtShutdownScriptParams.Text;
                Properties.Settings.Default.ShutdownScriptPath = txtShutdownScriptPath.Text;
                Properties.Settings.Default.Save();
                MessageBox.Show("All application settings have been saved.  If you need to edit the config, you can do so by running: " + Environment.NewLine + AppDomain.CurrentDomain.BaseDirectory + "GamestreamLauncherConfig.bat", "Config Updated Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
                ThrowError(strError);
        }

        private void btnBrowseStartupScriptPath_Click(object sender, RoutedEventArgs e)
        {
            txtStartupScriptPath.Text = BrowseForFile(txtStartupScriptPath.Text);
        }

        private void btnBrowseShutdownScriptPath_Click(object sender, RoutedEventArgs e)
        {
            txtShutdownScriptPath.Text = BrowseForFile(txtShutdownScriptPath.Text);
        }

        private void btnBrowseAppPath_Click(object sender, RoutedEventArgs e)
        {
            txtAppPath.Text = BrowseForFile(txtAppPath.Text);
        }

        private string BrowseForFile(string currentVal)
        {
            Microsoft.Win32.OpenFileDialog browseFile = new Microsoft.Win32.OpenFileDialog();

            bool? result = browseFile.ShowDialog();

            if (result == true)
                currentVal = browseFile.FileName;

            return currentVal;
        }

        private void GrantAccessGamestreamService()
        {
            var strNvContainerPermissions = "sc sdset NvContainerLocalSystem D:(A;;RP;;;AU)(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;IU)(A;;CCLCSWLOCRRC;;;SU)(A;;RPWPCR;;;BU)S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)";
            var strUninstallNvContainerRestart = "\"\"" + System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\InstallUtil.exe\"\" /u \"\"" + System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\NvContainerRestart.exe\"\"";
            var strInstallNvContainerRestart = "\"\"" + System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\InstallUtil.exe\"\" \"\"" + System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\NvContainerRestart.exe\"\"";
            var strNvContainerRestartPermissions = "sc sdset NvContainerRestart D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;IU)(A;;CCLCSWLOCRRC;;;SU)(A;;RPWPCR;;;BU)S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)";

            Process process = new Process() {StartInfo = new ProcessStartInfo() {FileName="Binaries\\nircmd.exe", Arguments= "elevate cmd /C \"" + strUninstallNvContainerRestart + " && " + strInstallNvContainerRestart + " && " + strNvContainerPermissions + " && " + strNvContainerRestartPermissions + "\"", UseShellExecute=true, CreateNoWindow=true } };
            process.Start();
            process.WaitForExit();
        }

        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        public void ThrowError(string message)
        {
            MessageBox.Show("Error: " + message, "Config Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void chkSetting_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }
    }
}
