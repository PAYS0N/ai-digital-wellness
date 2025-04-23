using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AIDigitalWellnessMonitor
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private bool _isMonitoring = false;
        private CancellationTokenSource _monitoringCts;
        private Dictionary<string, AppUsageInfo> _appUsageData = new Dictionary<string, AppUsageInfo>();
        private ObservableCollection<AppUsageInfo> _appUsageCollection = new ObservableCollection<AppUsageInfo>();

        // List of applications to monitor closely
        private List<string> _watchedApps = new List<string>
        {
            "brave", "discord", "minecraft", "chrome", "firefox", "edge", "spotify", "steam"
        };

        // Threshold in minutes before suggesting a break
        private int _interventionThreshold = 90;

        public MainWindow()
        {
            InitializeComponent();
            AppListView.ItemsSource = _appUsageCollection;
        }

        private void MonitoringButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isMonitoring)
            {
                StopMonitoring();
                MonitoringButton.Content = "Start Monitoring";
            }
            else
            {
                StartMonitoring();
                MonitoringButton.Content = "Stop Monitoring";
            }

            _isMonitoring = !_isMonitoring;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // We'll implement this in a later step
            MessageBox.Show("Settings functionality will be added soon!");
        }

        private void StartMonitoring()
        {
            _monitoringCts = new CancellationTokenSource();
            var token = _monitoringCts.Token;

            Task.Run(() => MonitorActiveWindow(token), token);
        }

        private void StopMonitoring()
        {
            _monitoringCts?.Cancel();
        }

        private void MonitorActiveWindow(CancellationToken cancellationToken)
        {
            DateTime lastCheckedAt = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get the active window
                    IntPtr hwnd = GetForegroundWindow();
                    GetWindowThreadProcessId(hwnd, out uint processId);

                    string appName = "Unknown";

                    try
                    {
                        // Try to get the process name
                        using (var process = Process.GetProcessById((int)processId))
                        {
                            appName = process.ProcessName.ToLower();
                        }
                    }
                    catch { /* Process might have terminated */ }

                    // update timing
                    DateTime now = DateTime.Now;
                    TimeSpan timeSpent = now - lastCheckedAt;

                    // Update last app usage time if it wasn't empty
                    UpdateAppUsage(appName, timeSpent);
                    lastCheckedAt = now;

                    // Check if current app time exceeds threshold
                    if (_watchedApps.Contains(appName.ToLower()))
                    {
                        if (_appUsageData.TryGetValue(appName, out var usageInfo))
                        {
                            if (usageInfo.MinutesToday >= _interventionThreshold && !usageInfo.InterventionShown)
                            {
                                ShowIntervention(appName, usageInfo.MinutesToday);

                                // Mark that we've shown intervention
                                Dispatcher.Invoke(() =>
                                {
                                    usageInfo.InterventionShown = true;
                                    usageInfo.Status = "Warning Shown";
                                });
                            }
                        }
                    }

                    // Sleep for a short time to avoid excessive CPU usage
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                    Debug.WriteLine($"Error in monitoring: {ex.Message}");
                    Thread.Sleep(5000);  // Wait longer if there's an error
                }
            }
        }

        private void UpdateAppUsage(string appName, TimeSpan timeSpent)
        {
            // Update on UI thread
            Dispatcher.Invoke(() =>
            {
                if (!_appUsageData.TryGetValue(appName, out var usageInfo))
                {
                    usageInfo = new AppUsageInfo
                    {
                        ApplicationName = appName,
                        MinutesToday = 0,
                        Status = "Monitoring"
                    };

                    _appUsageData[appName] = usageInfo;
                    _appUsageCollection.Add(usageInfo);
                }

                // Add the time spent
                usageInfo.MinutesToday += timeSpent.TotalMinutes;

                // Reset flag if enough time has passed
                if (usageInfo.InterventionShown && usageInfo.MinutesToday > _interventionThreshold + 30)
                {
                    usageInfo.InterventionShown = false;
                }
            });
        }

        private void ShowIntervention(string appName, double minutes)
        {
            Dispatcher.Invoke(() =>
            {
                // For now, just show a message box
                // Later we'll replace this with AI-powered intervention
                MessageBox.Show(
                    $"You've been using {appName} for {Math.Round(minutes)} minutes today.\n\nMaybe it's time for a break?",
                    "Digital Wellness Check",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            });
        }
    }

    public class AppUsageInfo
    {
        public string ApplicationName { get; set; }
        public double MinutesToday { get; set; }
        public string Status { get; set; }
        public bool InterventionShown { get; set; }
    }
}