using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using System.Xml;

namespace GamestreamLauncher.HelperApi
{
    public class MonitorInfoEventArgs : EventArgs
    {
        public List<string> MonitorsToDisable { get; set; }
    }

    public class MinerInfoEventArgs : EventArgs
    {
        public List<string> MinersToDisable { get; set; }
    }

    class LauncherApi
    {
        string multimonitortoolPath = System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\MultiMonitorTool.exe";
        string nircmdPath = System.AppDomain.CurrentDomain.BaseDirectory + "Binaries\\nircmd.exe";
        string multimonitortoolConfigName = System.AppDomain.CurrentDomain.BaseDirectory + "defaultMonitorSetup.cfg";

        string minerIP = "";
        string minerPort = "";
        string minerKey = "";

        List<string> minersToStop = new List<string>();
        List<string> monitorsToDisable = new List<string>();

        public delegate void MonitorInfoLoadedEvent(object sender, MonitorInfoEventArgs e);
        public delegate void MonitorModeSingleEvent(object sender, MonitorInfoEventArgs e);
        public delegate void MonitorModeMultiEvent(object sender, MonitorInfoEventArgs e);
        public delegate void MinerInfoLoadedEvent(object sender, MinerInfoEventArgs e);
        public delegate void MinerEnabledEvent(object sender, MinerInfoEventArgs e);
        public delegate void MinerDisabledEvent(object sender, MinerInfoEventArgs e);
        public delegate void StartupScriptsFinishedEvent(object sender, EventArgs e);
        public delegate void ShutdownScriptsFinishedEvent(object sender, EventArgs e);
        public delegate void ApplicationStartedEvent(object sender, EventArgs e);
        public delegate void ApplicationStoppedEvent(object sender, EventArgs e);
        public delegate void StreamClosedEvent(object sender, EventArgs e);

        public event MonitorInfoLoadedEvent MonitorInfoLoaded;
        public event MonitorModeSingleEvent MonitorModeSingle;
        public event MonitorModeMultiEvent MonitorModeMulti;
        public event MinerInfoLoadedEvent MinerInfoLoaded;
        public event MinerEnabledEvent MinerEnabled;
        public event MinerDisabledEvent MinerDisabled;
        public event StartupScriptsFinishedEvent StartupScriptsFinished;
        public event ShutdownScriptsFinishedEvent ShutdownScriptsFinished;
        public event ApplicationStartedEvent ApplicationStarted;
        public event ApplicationStoppedEvent ApplicationStopped;
        public event StreamClosedEvent StreamClosed;

        public LauncherApi(List<string> monitorsDisabled = null, List<string> minersDisabled = null)
        {
            if (monitorsDisabled != null)
                monitorsToDisable = monitorsDisabled;

            if (minersDisabled != null)
                minersToStop = minersDisabled;
        }

        #region Monitor Helpers

        public void LoadMonitorInfo()
        {
            var myProcess = new Process { StartInfo = new ProcessStartInfo(multimonitortoolPath, "/sxml monitorinfo.xml") };
            myProcess.Start();
            myProcess.WaitForExit();

            XmlDocument xmlMonitorInfo = new XmlDocument();
            xmlMonitorInfo.Load(System.AppDomain.CurrentDomain.BaseDirectory + "\\monitorinfo.xml");

            string jsonMonitorInfo = JsonConvert.SerializeXmlNode(xmlMonitorInfo).Replace("?xml", "xml").Replace("@version", "version").Replace("left-top", "topleft").Replace("right-bottom", "rightbottom");

            MonitorInfoApi.MonitorInfo monInfo = JsonConvert.DeserializeObject<MonitorInfoApi.MonitorInfo>(jsonMonitorInfo);

            foreach (MonitorInfoApi.Item monitor in monInfo.monitors_list.item)
            {
                if (monitor.active == "Yes" && monitor.primary == "No")
                {
                    monitorsToDisable.Add(monitor.name);
                }
            }

            myProcess = new Process { StartInfo = new ProcessStartInfo(multimonitortoolPath, "/saveConfig \"" + multimonitortoolConfigName + "\"") };
            myProcess.Start();
            myProcess.WaitForExit();

            MonitorInfoLoaded?.Invoke(this, new MonitorInfoEventArgs() { MonitorsToDisable = monitorsToDisable });
        }

        public void SwitchMonitorMode(bool restore = false, bool disableMons = false, int x = 1920, int y = 1080)
        {
            if (restore)
            {
                for (int i = 0; i <= monitorsToDisable.Count; i++) // for some reason loading the original config will only re-enable a single monitor so we must run this for each monitor we disabled
                {
                    RunScript(multimonitortoolPath, "/loadConfig \"" + multimonitortoolConfigName + "\"");
                    Thread.Sleep(3000);
                }
                MonitorModeMulti?.Invoke(this, new MonitorInfoEventArgs() { MonitorsToDisable = monitorsToDisable });
            }
            else
            {
                if (disableMons)
                {
                    RunScript(multimonitortoolPath, "/disable " + String.Join(" ", monitorsToDisable.ToArray()));
                    Thread.Sleep(3000);
                }

                RunScript(nircmdPath, "setdisplay " + x + " " + y + " 32");
                MonitorModeSingle?.Invoke(this, new MonitorInfoEventArgs() { MonitorsToDisable = monitorsToDisable });
            }
        }

        #endregion

        #region Miner Helpers

        public bool LoadMinerInfo(string ip, string port, string key)
        {
            bool success = true;

            try
            {
                string strResponse = "";

                minerIP = ip;
                minerPort = port;
                minerKey = key;

                System.Net.HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}:{1}/api/miners?key={2}", minerIP, minerPort, minerKey));

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    strResponse = reader.ReadToEnd();
                }

                AwesomeMinerApi.AwesomeMinerStatus minerStatus = JsonConvert.DeserializeObject<AwesomeMinerApi.AwesomeMinerStatus>(strResponse);

                foreach (AwesomeMinerApi.GroupList group in minerStatus.groupList)
                {
                    foreach (AwesomeMinerApi.MinerList miner in group.minerList)
                    {
                        if (miner.hostname == "localhost" && miner.hasGpu && miner.canStop)
                        {
                            minersToStop.Add(miner.id.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
            } finally
            {
                MinerInfoLoaded?.Invoke(this, new MinerInfoEventArgs() { MinersToDisable = minersToStop });
            }
            return success;
        }

        public void StartStopMiner(bool restore = false)
        {
            try
            {
                foreach (string minerID in minersToStop)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}:{1}/api/miners/{2}?action={3}&key={4}", minerIP, minerPort, minerID, restore ? "start" : "stop", minerKey));
                    request.ContentLength = 0;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Method = "POST";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        //success = response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (restore)
                    MinerEnabled?.Invoke(this, new MinerInfoEventArgs() { MinersToDisable = minersToStop });
                else
                    MinerDisabled?.Invoke(this, new MinerInfoEventArgs() { MinersToDisable = minersToStop });
            }
        }

        #endregion

        #region Script Helpers

        public void RunStartupScripts(string scriptPath, string scriptParams = "")
        {
            RunScript(scriptPath, scriptParams);

            StartupScriptsFinished?.Invoke(this, EventArgs.Empty);
        }

        public void RunShutdownScripts(string scriptPath, string scriptParams = "")
        {
            RunScript(scriptPath, scriptParams);

            ShutdownScriptsFinished?.Invoke(this, EventArgs.Empty);
        }

        public void RunScript(string scriptPath, string scriptParams = "")
        {
            if (!String.IsNullOrEmpty(scriptPath))
            {
                var processStartInfo = new ProcessStartInfo(scriptPath, scriptParams);
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                
                var myProcess = new Process { StartInfo = processStartInfo };
                myProcess.Start();
                myProcess.WaitForExit();
            }
        }

        #endregion

        #region Application Helpers

        public void LaunchApplication(string appPath)
        {
            var myProcess = new Process { StartInfo = new ProcessStartInfo(appPath) };
            myProcess.Start();
            ApplicationStarted?.Invoke(this, EventArgs.Empty);
            myProcess.WaitForExit();
            ApplicationStopped?.Invoke(this, EventArgs.Empty);
        }

        public void CloseStream()
        {
            RunScript("net", "stop NvContainerLocalSystem");

            StreamClosed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region UI Helpers
        static public System.Windows.Media.Color getMediaColor(System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        static public System.Drawing.Color getDrawingColor(System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        #endregion
    }
}
