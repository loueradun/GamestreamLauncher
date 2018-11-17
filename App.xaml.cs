using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GamestreamLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GamestreamLauncherWindow launcherWindow = null;

            if (e.Args.Length > 0)
            {
                foreach(string arg in e.Args)
                {
                    switch (arg)
                    {
                        case "-config":
                            ShowConfig();
                            break;
                        case "-restore":
                            Console.WriteLine("Restoring previous config");
                            launcherWindow = new GamestreamLauncherWindow(true);
                            System.Threading.Thread.Sleep(5000);
                            //launcherWindow.ShowDialog();
                            Shutdown(1);
                            break;
                        default: // this is an application path
                            launcherWindow = new GamestreamLauncherWindow(false, arg);
                            launcherWindow.ShowDialog();
                            Shutdown(1);
                            break;
                    }
                }
            } else if (String.IsNullOrEmpty(GamestreamLauncher.Properties.Settings.Default.AppName) || String.IsNullOrEmpty(GamestreamLauncher.Properties.Settings.Default.AppPath))
            {
                ShowConfig();
            }
            else
            {
                launcherWindow = new GamestreamLauncherWindow();
                launcherWindow.ShowDialog();
                Shutdown(1);
            }
        }

        private void ShowConfig()
        {
            Console.WriteLine("Opening config window");
            EditConfig editConfigWindow = new EditConfig();
            editConfigWindow.ShowDialog();
            Console.WriteLine("Config window closed");
            Shutdown(1);
        }
    }
}
