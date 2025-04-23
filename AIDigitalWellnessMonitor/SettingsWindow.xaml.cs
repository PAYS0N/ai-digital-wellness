using System.Collections.Generic;
using System.Windows;

namespace AIDigitalWellnessMonitor
{
    public partial class SettingsWindow : Window
    {
        public int InterventionThreshold { get; private set; }
        public List<string> WatchedApps { get; private set; }

        public SettingsWindow(int currentThreshold, List<string> currentWatchedApps)
        {
            InitializeComponent();

            // Set initial values
            ThresholdSlider.Value = currentThreshold;
            WatchedApps = new List<string>(currentWatchedApps);

            // Populate custom apps list
            foreach (var app in WatchedApps)
            {
                // Skip the standard apps that have checkboxes
                if (!IsStandardApp(app))
                {
                    CustomAppsList.Items.Add(app);
                }
            }
        }

        private void AddCustomAppButton_Click(object sender, RoutedEventArgs e)
        {
            string customApp = CustomAppTextBox.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(customApp) && !CustomAppsList.Items.Contains(customApp))
            {
                CustomAppsList.Items.Add(customApp);
                CustomAppTextBox.Clear();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Get threshold value
            InterventionThreshold = (int)ThresholdSlider.Value;

            // Collect watched apps
            WatchedApps = new List<string>();

            // Add standard apps based on checkboxes
            if (BraveCheckbox.IsChecked == true) WatchedApps.Add("brave");
            if (DiscordCheckbox.IsChecked == true) WatchedApps.Add("discord");
            if (MinecraftCheckbox.IsChecked == true) WatchedApps.Add("minecraft");

            if (OtherBrowsersCheckbox.IsChecked == true)
            {
                WatchedApps.Add("chrome");
                WatchedApps.Add("firefox");
                WatchedApps.Add("edge");
            }

            if (SteamCheckbox.IsChecked == true) WatchedApps.Add("steam");
            if (SpotifyCheckbox.IsChecked == true) WatchedApps.Add("spotify");

            // Add custom apps
            foreach (var item in CustomAppsList.Items)
            {
                WatchedApps.Add(item.ToString());
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsStandardApp(string app)
        {
            string[] standardApps = { "brave", "discord", "minecraft", "chrome", "firefox", "edge", "steam", "spotify" };
            return Array.IndexOf(standardApps, app.ToLower()) >= 0;
        }
    }
}