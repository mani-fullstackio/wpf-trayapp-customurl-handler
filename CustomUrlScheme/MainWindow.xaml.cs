using Microsoft.Win32;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Forms = System.Windows.Forms;
namespace CustomUrlScheme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            createcustomlink();

            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("Resources/icon.ico");
            _notifyIcon.Text = "SingletonSean";
            _notifyIcon.DoubleClick += (sender, args) => ShowApp(sender, args);

            _notifyIcon.Visible = true;
            //  this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the close event
            this.Hide(); // Hide the window instead of closing it
        }
        public void createcustomlink()
        {
            string assemblyApplication = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("Application1"))
            {
                key.SetValue(string.Empty, "URL:CustomApplication");
                key.SetValue("URL Protocol", string.Empty);
                using (RegistryKey shellkey = key.CreateSubKey("shell"))
                using (RegistryKey openkey = shellkey.CreateSubKey("open"))
                using (RegistryKey coomandkey = openkey.CreateSubKey("command"))
                {
                    coomandkey.SetValue(string.Empty, $"\"{assemblyApplication}\" \"%1\"");
                }
            }
        }

        //Minimize the app to tray when closing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the close action
            Hide(); // Hide the window
        }

        // Show the application when the user clicks "Show" in the tray
        private void ShowApp(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate(); // Bring the app to the foreground
        }

        // Exit the application
        private void ExitApp(object sender, EventArgs e)
        {
            _notifyIcon.Dispose(); // Dispose of the tray icon
            System.Windows.Application.Current.Shutdown(); // Use WPF Application for shutdown
        }

        // Optionally minimize to tray when minimized
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                Hide(); // Hide the window instead of minimizing to taskbar
            }
        }
    }
}