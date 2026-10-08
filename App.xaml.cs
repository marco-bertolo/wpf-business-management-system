using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PapJoaoSimoes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            while (true)
            {
                LoginWindow loginWindow = new LoginWindow();
                bool? autenticado = loginWindow.ShowDialog();

                if (autenticado != true)
                {
                    break;
                }

                MainWindow mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.ShowDialog();

                if (!mainWindow.SessaoTerminada)
                {
                    break;
                }
            }

            Shutdown();
        }
    }
}
