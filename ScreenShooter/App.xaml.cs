using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ScreenShooter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static readonly NotifyIcon icon = new NotifyIcon();
        public App()
        {
            StartupUri = new Uri("MainWindow.xaml", UriKind.RelativeOrAbsolute);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            icon.Dispose();
            base.OnExit(e);
        }

    }

    
}
