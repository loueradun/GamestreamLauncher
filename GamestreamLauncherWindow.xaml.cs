using GamestreamLauncher.HelperApi;
using System;
using System.ComponentModel;
using System.Drawing;
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

        bool testUI = false;

        string appPath = "";
        string appName = "Gamestream";
        string imageName = null;

        int resX = 1920;
        int resY = 1080;

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

            if (!testUI)
            {
                if (restore)
                    EndWorkFlow();
                else
                {
                    StartWorkflow();
                }
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
            // We will always be doing monitor jobs now as we are going to set the Gamestream resolution...
            if (restore)
            {
                UpdateStatus(multimonitorSwitchEnabled ? "Switching to multiple monitor mode and restoring desktop resolution..." : "Restoring desktop resolution...");
                new Task(() => { launcherApi.SwitchMonitorMode(true); }).Start();
            } else
            {
                UpdateStatus("Saving current monitor config to disk...");
                new Task(() => { launcherApi.LoadMonitorInfo(); }).Start();
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
            UpdateStatus(multimonitorSwitchEnabled ? "Switching to single monitor mode and setting Gamestream resolution..." : "Setting Gamestream resolution...");
            new Task(() => { launcherApi.SwitchMonitorMode(false, multimonitorSwitchEnabled, resX, resY); }).Start();
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
            imageName = Properties.Settings.Default.BackgroundImage;
            resX = Properties.Settings.Default.ResolutionWidth;
            resY = Properties.Settings.Default.ResolutionHeight;
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
            if (imageName != null && imageName.Length > 0)
            {
                bgImage.LoadedBehavior = System.Windows.Controls.MediaState.Play;
                bgImage.Source = new Uri(imageName);
                bgImage.MediaEnded += BgImage_MediaEnded;
            } else
            {
                bgImage.IsEnabled = false;
                bgImage.Visibility = Visibility.Hidden;
            }
        }

        private void BgImage_MediaEnded(object sender, RoutedEventArgs e)
        {
            bgImage.Position = TimeSpan.Zero;
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
