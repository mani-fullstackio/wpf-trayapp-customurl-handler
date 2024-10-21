using System;
using System.Diagnostics;
using System.Drawing; // Required for Icon
using System.Windows;
using System.Windows.Forms; // Required for NotifyIcon
using System.Windows.Interop; // Required for WindowInteropHelper
using System.Runtime.InteropServices;
using Application = System.Windows.Application; // Avoid ambiguity

namespace CustomUrlScheme
{
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
        private bool _isExiting = false; // Flag to track exit requests
        private static readonly string UniqueAppId = "YourUniqueAppId"; // Change this to a unique identifier

        // Import the SetForegroundWindow function from user32.dll to bring the window to the front
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check if another instance is already running
            if (IsAnotherInstanceRunning())
            {
                // If another instance is running, we can just bring it to the front
                BringExistingInstanceToFront();
                Application.Current.Shutdown(); // Shutdown the current instance
                return; // Exit this instance
            }

            base.OnStartup(e);

            // Create and configure the system tray icon
            CreateTrayIcon();

            // Show the main window
            ShowMainWindow(e);
        }

        private void CreateTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/icon.ico"), // Add your icon file
                Visible = true,
                Text = "Custom URL Application"
            };

            // Create the context menu for the tray icon
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show", null, ShowApp);
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApp);

            // Handle double-click to show the app
            _notifyIcon.DoubleClick += (sender, args) => ShowApp(sender, args);
        }

        private void ShowMainWindow(StartupEventArgs e)
        {
            MainWindow mainWindow;

            if (MainWindow == null)
            {
                mainWindow = new MainWindow();
                MainWindow = mainWindow;
            }
            else
            {
                mainWindow = (MainWindow)MainWindow;
            }

            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();

            // Handle custom URL arguments (if any)
            if (e.Args.Length > 0)
            {
                string argument = e.Args[0];
                ////  MessageBox.Show("Argument received: " + argument); // For debugging
            }

            // Bring window to the foreground
            var handle = new WindowInteropHelper(mainWindow).Handle;
            SetForegroundWindow(handle);
        }

        private void ShowApp(object sender, EventArgs e)
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
            }

            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();

            // Bring window to the foreground
            var handle = new WindowInteropHelper(MainWindow).Handle;
            SetForegroundWindow(handle);
        }

        private void ExitApp(object sender, EventArgs e)
        {
            _isExiting = true;
            _notifyIcon.Dispose(); // Dispose of the tray icon
            Shutdown(); // Shutdown the application
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!_isExiting)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnExit(e);
        }

        private bool IsAnotherInstanceRunning()
        {
            // Use a unique identifier to check for another instance
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            return processes.Length > 1; // If there's more than one, another instance is running
        }

        private void BringExistingInstanceToFront()
        {
            // Find the existing instance and bring it to the front
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            foreach (var process in processes)
            {
                if (process.Id != Process.GetCurrentProcess().Id)
                {
                    // Bring this window to the foreground
                    IntPtr handle = process.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        SetForegroundWindow(handle);
                        break; // Exit after bringing the first found instance to the foreground
                    }
                }
            }
        }

        // Override OnSessionEnding to handle minimize to tray when closing
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true; // Prevent the session from ending
                MainWindow.Hide(); // Hide the window instead of closing
            }
        }
    }
}
