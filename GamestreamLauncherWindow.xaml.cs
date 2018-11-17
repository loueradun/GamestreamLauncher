using GamestreamLauncher.HelperApi;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace GamestreamLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GamestreamLauncherWindow : Window
    {
        #region Config

        string appPath = "";
        string appName = "Gamestream";

        bool multimonitorSwitchEnabled = false;
        bool minerSwitchEnabled = false;
        bool scriptsEnabled = false;

        string minerIP;
        string minerPort;
        string minerKey;

        string startupScriptPath;
        string startupScriptParameters;
        string shutdownScriptPath;
        string shutdownScriptParameters;

        bool isRestore = false;

        LauncherApi launcherApi;

        #endregion

        public GamestreamLauncherWindow(bool restore = false, string applicationToLaunch = null)
        {
            InitializeComponent();

            var oVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var version = "v" + oVersion.Major + "." + oVersion.Minor;

            isRestore = restore;

            //this.Topmost = true;

            LoadConfig(applicationToLaunch);

            lblVersion.Content = version;
            lblHeader.Content = appName + " Launcher";

            launcherApi = restore ? new LauncherApi(Properties.Settings.Default.MonitorsDisabled, Properties.Settings.Default.MinersDisabled) : new LauncherApi();

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

            if (restore)
                EndWorkFlow();
            else
            {
                StartWorkflow();
            }
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
                    new Task(() => { launcherApi.RunShutdownScripts(shutdownScriptPath, shutdownScriptParameters); }).Start();
                } else
                {
                    UpdateStatus("Running startup scripts...");
                    new Task(() => { launcherApi.RunStartupScripts(startupScriptPath, startupScriptParameters); }).Start();
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
                launcherApi.RunScript("net", "start NvContainerRestart");
                //this.Topmost = false;
                new Task(() => { launcherApi.LaunchApplication(appPath); }).Start();
            }
        }

        public void QuitGracefully(bool closeApp = false)
        {
            if (closeApp)
            {
                if (Application.Current != null)
                {
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        this.Close();
                    }
                    else
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            this.Close();
                        }));
                }
            }
            else if (!isRestore)
            {
                UpdateStatus("Terminating Gamestream Session...");
                new Task(() => { launcherApi.CloseStream(); }).Start();
            }
            else
                QuitGracefully(true);
        }

        #endregion

        #region Callbacks

        public void MinerInfoLoaded(object sender, MinerInfoEventArgs e)
        {
            if (e.MinersToDisable.Count > 0)
            {
                UpdateStatus("Miner is currently running, attempting to stop...");
                new Task(() => { launcherApi.StartStopMiner(); }).Start();
            } else
            {
                HandleMonitorJobs();
            }
        }

        public void MonitorInfoLoaded(object sender, MonitorInfoEventArgs e)
        {
            if (e.MonitorsToDisable.Count > 0)
            {
                UpdateStatus("Switching to single monitor mode...");
                new Task(() => { launcherApi.SwitchMonitorMode(); }).Start();
            } else
            {
                HandleScriptJobs();
            }
        }

        public void MinerDisabled(object sender, MinerInfoEventArgs e)
        {
            Properties.Settings.Default.MinersDisabled = e.MinersToDisable;
            Properties.Settings.Default.Save();
            HandleMonitorJobs();
        }

        public void MonitorModeSingle(object sender, MonitorInfoEventArgs e)
        {
            Properties.Settings.Default.MonitorsDisabled = e.MonitorsToDisable;
            Properties.Settings.Default.Save();
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
            //if (Application.Current.Dispatcher.CheckAccess())
            //{
            //    this.Topmost = true;
            //}
            //else
            //    Application.Current.Dispatcher.Invoke(new Action(() =>
            //    {
            //        this.Topmost = true;
            //    }));
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

        #endregion Callbacks

        #region UI Helpers

        public void LoadConfig(string applicationPath = null)
        {
            appName = Properties.Settings.Default.AppName;
            appPath = applicationPath == null ? Properties.Settings.Default.AppPath : applicationPath;
            multimonitorSwitchEnabled = Properties.Settings.Default.MultiMonSwitchEnabled;
            minerSwitchEnabled = Properties.Settings.Default.AwesomeMinerSwitchEnabled;
            scriptsEnabled = Properties.Settings.Default.ScriptsEnabled;
            if (minerSwitchEnabled)
            {
                minerIP = Properties.Settings.Default.AwesomeMinerIP;
                minerPort = Properties.Settings.Default.AwesomeMinerPort;
                minerKey = Properties.Settings.Default.AwesomeMinerKey;
            }
            if (scriptsEnabled)
            {
                startupScriptPath = Properties.Settings.Default.StartupScriptPath;
                startupScriptParameters = Properties.Settings.Default.StartupScriptParameters;
                shutdownScriptPath = Properties.Settings.Default.ShutdownScriptPath;
                shutdownScriptParameters = Properties.Settings.Default.ShutdownScriptParameters;
            }
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
