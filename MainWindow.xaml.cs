using GamestreamLauncher.HelperApi;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GamestreamLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Config

        string appPath = "";
        string appName = "Gamestream";
        string appConfigFile = "GamestreamLauncher.exe.config";

        string adminUser;
        string adminPassword;

        bool multimonitorSwitchEnabled = false;
        bool minerSwitchEnabled = false;
        bool scriptsEnabled = false;

        string minerIP;
        string minerPort;
        string minerKey;

        string startupScriptPath;
        string startupScriptParameters;
        bool startupScriptElevated;
        string shutdownScriptPath;
        string shutdownScriptParameters;
        bool shutdownScriptElevated;

        LauncherApi launcherApi;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            LoadConfig();

            launcherApi = new LauncherApi(appPath);

            // configure callbacks
            launcherApi.MonitorInfoLoaded += MonitorInfoLoaded;
            launcherApi.MonitorModeSingle += MonitorModeSingle;
            launcherApi.MonitorModeMulti += MonitorModeMulti;
            launcherApi.MinerInfoLoaded += MinerInfoLoaded;
            launcherApi.MinerDisabled += MinerDisabled;
            launcherApi.MinerEnabled += MinerEnabled;
            launcherApi.StartupScriptsFinished += StartupScriptsFinished;
            launcherApi.ShutdownScriptsFinished += ShutdownScriptsFinished;
            launcherApi.ApplicationStarted += ApplicationStarted;
            launcherApi.ApplicationStopped += ApplicationStopped;
            launcherApi.StreamClosed += StreamClosed;

            lblHeader.Content = appName + " Launcher";

            StartWorkflow();
        }

        #region Workflow

        public void StartWorkflow()
        {
            HandleMinerJobs();
        }

        public void EndWorkFlow()
        {
            HandleScriptJobs(true);
        }

        public void HandleMinerJobs(bool restore = false)
        {
            if (minerSwitchEnabled)
            {
                if (restore)
                {
                    UpdateStatus("Re-starting Miner...");
                    new Task(() => { launcherApi.StartStopMiner(true); }).Start();
                } else
                {
                    UpdateStatus("Loading current miner config...");
                    new Task(() => { launcherApi.LoadMinerInfo(minerIP, minerPort, minerKey); }).Start();
                }
            }
            else
            {
                if (restore)
                    QuitGracefully();
                else
                    HandleMonitorJobs(restore);
            }
        }

        public void HandleMonitorJobs(bool restore = false)
        {
            if (multimonitorSwitchEnabled)
            {
                if (restore)
                {
                    UpdateStatus("Switching to multiple monitor mode...");
                    new Task(() => { launcherApi.SwitchMonitorMode(true); }).Start();
                } else
                {
                    UpdateStatus("Saving current monitor config to disk...");
                    new Task(() => { launcherApi.LoadMonitorInfo(); }).Start();
                }
            }
            else
            {
                if (restore)
                    HandleMinerJobs(restore);
                else
                    HandleScriptJobs(restore);
            }
        }

        public void HandleScriptJobs(bool restore = false)
        {
            if (scriptsEnabled)
            {
                if (restore)
                {
                    UpdateStatus("Running shutdown scripts...");
                    new Task(() => { launcherApi.RunShutdownScripts(shutdownScriptPath, shutdownScriptParameters, shutdownScriptElevated ? adminUser : "", shutdownScriptElevated ? adminPassword : ""); }).Start();
                } else
                {
                    UpdateStatus("Running startup scripts...");
                    new Task(() => { launcherApi.RunStartupScripts(startupScriptPath, startupScriptParameters, startupScriptElevated ? adminUser : "", startupScriptElevated ? adminPassword : ""); }).Start();
                }
            }
            else
            {
                if (restore)
                    HandleMonitorJobs(restore);
                else
                    HandleApplicationJobs();
            }
        }

        public void HandleApplicationJobs(bool restore = false)
        {
            if (restore)
            {
                QuitGracefully();
            }
            else
            {
                UpdateStatus("Launching " + appName + "...");
                new Task(() => { launcherApi.LaunchApplication(); }).Start();
            }
        }

        public void QuitGracefully(bool closeApp = false)
        {
            if (closeApp)
            {
                Thread.Sleep(10000);
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    Application.Current.Shutdown();
                }));
            } else
            {
                UpdateStatus("Terminating Gamestream (Ignore error shown, this is expected)...");
                new Task(() => { launcherApi.CloseStream(adminUser, adminPassword); }).Start();
            }
        }

        #endregion

        public void MinerInfoLoaded(object sender, MinerInfoEventArgs e)
        {
            if (e.MinersToDisable.Count > 0)
            {
                UpdateStatus("Miner is currently running, attempting to stop...");
                new Task(() => { launcherApi.StartStopMiner(); }).Start();
            }
        }

        public void MonitorInfoLoaded(object sender, MonitorInfoEventArgs e)
        {
            if (e.MonitorsToDisable.Count > 0)
            {
                UpdateStatus("Switching to single monitor mode...");
                new Task(() => { launcherApi.SwitchMonitorMode(); }).Start();
            }
        }

        public void MinerDisabled(object sender, MinerInfoEventArgs e)
        {
            HandleMonitorJobs();
        }

        public void MonitorModeSingle(object sender, MonitorInfoEventArgs e)
        {
            HandleScriptJobs();
        }

        public void StartupScriptsFinished(object sender, EventArgs e)
        {
            HandleApplicationJobs();
        }

        public void ApplicationStarted(object sender, EventArgs e)
        {
            UpdateStatus("Waiting for " + appName + " to close...");
        }

        public void ApplicationStopped(object sender, EventArgs e)
        {
            EndWorkFlow();
        }

        public void MinerEnabled(object sender, MinerInfoEventArgs e)
        {
            QuitGracefully();
        }

        public void MonitorModeMulti(object sender, MonitorInfoEventArgs e)
        {
            HandleMinerJobs(true);
        }

        public void ShutdownScriptsFinished(object sender, EventArgs e)
        {
            HandleMonitorJobs(true);
        }

        public void StreamClosed(object sender, EventArgs e)
        {
            QuitGracefully(true);
        }

        #region UI Helpers

        public string GetConfigValue(string valueName)
        {
            string value = null;
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[valueName]))
            {
                ThrowError(String.Format("Missing Appsetting \"{0}\" in {1}", valueName, appConfigFile), true);
            } else
            {
                value = ConfigurationManager.AppSettings[valueName];
            }
            return value;
        }

        public void LoadConfig()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "GamestreamLauncher.exe.config"))
            {
                appName = GetConfigValue("AppName");
                appPath = GetConfigValue("AppPath");
                adminUser = GetConfigValue("AdminAccount");
                adminPassword = GetConfigValue("AdminPassword");
                multimonitorSwitchEnabled = Boolean.Parse(GetConfigValue("MultiMonSwitchEnabled"));
                minerSwitchEnabled = Boolean.Parse(GetConfigValue("AwesomeMinerSwitchEnabled"));
                scriptsEnabled = Boolean.Parse(GetConfigValue("ScriptsEnabled"));
                if (minerSwitchEnabled)
                {
                    minerIP = GetConfigValue("AwesomeMinerUrl");
                    minerPort = GetConfigValue("AwesomeMinerPort");
                    minerKey = GetConfigValue("AwesomeMinerKey");
                }
                if (scriptsEnabled)
                {
                    startupScriptPath = GetConfigValue("StartupScriptPath");
                    startupScriptParameters = GetConfigValue("StartupScriptParameters");
                    startupScriptElevated = Boolean.Parse(GetConfigValue("StartupScriptElevated"));
                    shutdownScriptPath = GetConfigValue("ShutdownScriptPath");
                    shutdownScriptParameters = GetConfigValue("StartupScriptParameters");
                    shutdownScriptElevated = Boolean.Parse(GetConfigValue("ShutdownScriptElevated"));
                }
            }
            else
                ThrowError("GamestreamLauncher.exe.config file is missing.  Can not run without config.", true);
        }

        public void ThrowError(string message, bool critical = false)
        {
            MessageBox.Show("Error: " + message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (critical)
                QuitGracefully(true);
        }

        private void UpdateStatus(string status)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                lblStatus.Content = status;
            }));
        }

        #endregion
    }
}
