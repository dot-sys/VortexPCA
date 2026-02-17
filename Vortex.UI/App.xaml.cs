using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Vortex.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

    public static class PCAViewHelper
    {
        public static void PopulateTimeDisplayComboBox(ComboBox comboBox)
        {
            if (comboBox == null) return;

            int selectedIndex = comboBox.SelectedIndex;
            comboBox.Items.Clear();

            var showLocalTime = GetLocalizedString("ShowLocalTime", "Show Local Time");
            var showUTCTime = GetLocalizedString("ShowUTCTime", "Show UTC Time");
            var showBoth = GetLocalizedString("ShowBoth", "Show Both");

            comboBox.Items.Add(new ComboBoxItem { Content = showLocalTime });
            comboBox.Items.Add(new ComboBoxItem { Content = showUTCTime });
            comboBox.Items.Add(new ComboBoxItem { Content = showBoth });

            comboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        public static void UpdateTimeColumnVisibility(DataGrid dataGrid, ComboBox timeDisplayComboBox, string utcBindingPath, string localBindingPath)
        {
            if (dataGrid == null || timeDisplayComboBox == null)
                return;

            int selectedIndex = timeDisplayComboBox.SelectedIndex;

            var utcColumn = FindColumnByBindingPath(dataGrid, utcBindingPath);
            var localColumn = FindColumnByBindingPath(dataGrid, localBindingPath);

            if (utcColumn == null || localColumn == null)
                return;

            switch (selectedIndex)
            {
                case 0: // Show Local Time
                    utcColumn.Visibility = Visibility.Collapsed;
                    localColumn.Visibility = Visibility.Visible;
                    break;
                case 1: // Show UTC Time
                    utcColumn.Visibility = Visibility.Visible;
                    localColumn.Visibility = Visibility.Collapsed;
                    break;
                case 2: // Show Both
                default:
                    utcColumn.Visibility = Visibility.Visible;
                    localColumn.Visibility = Visibility.Visible;
                    break;
            }
        }

        public static bool ParseSizeFilter(TextBox minBox, TextBox maxBox, out double? minSize, out double? maxSize)
        {
            minSize = null;
            maxSize = null;

            if (!string.IsNullOrWhiteSpace(minBox?.Text) && double.TryParse(minBox.Text, out double min))
                minSize = min;

            if (!string.IsNullOrWhiteSpace(maxBox?.Text) && double.TryParse(maxBox.Text, out double max))
                maxSize = max;

            return minSize.HasValue || maxSize.HasValue;
        }

        public static bool ApplySizeFilter(string filesizeInMB, double? minSize, double? maxSize)
        {
            if (!minSize.HasValue && !maxSize.HasValue)
                return true;

            if (string.IsNullOrWhiteSpace(filesizeInMB))
                return false;

            string sizeStr = filesizeInMB.Replace(" MB", "").Trim();
            if (!double.TryParse(sizeStr, out double fileSize))
                return false;

            if (minSize.HasValue && fileSize <= minSize.Value)
                return false;

            if (maxSize.HasValue && fileSize >= maxSize.Value)
                return false;

            return true;
        }

        public static bool IsTodayFilter(string timestampLocal)
        {
            if (string.IsNullOrWhiteSpace(timestampLocal))
                return false;

            if (!DateTime.TryParse(timestampLocal, out DateTime entryTime))
                return false;

            return (DateTime.Now - entryTime).TotalHours <= 24;
        }

        public static void UpdateEntryCount(TextBlock entryCountTextBlock, DataGrid dataGrid)
        {
            if (entryCountTextBlock == null || dataGrid?.ItemsSource == null)
                return;

            var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (view == null)
                return;

            int filteredCount = view.Cast<object>().Count();
            int totalCount = (dataGrid.ItemsSource as System.Collections.IEnumerable)?.Cast<object>().Count() ?? 0;

            var formatString = GetLocalizedString("EntriesFormat", "{0} / {1} Entries");
            entryCountTextBlock.Text = string.Format(formatString, filteredCount, totalCount);
        }

        public static string BuildVirusTotalUrl(Hyperlink link, Uri uri)
        {
            var hash = uri?.OriginalString ?? link?.NavigateUri?.OriginalString;
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var trimmed = hash.Trim();
            var sb = new System.Text.StringBuilder(trimmed.Length);
            foreach (var c in trimmed)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    sb.Append(c);
            }
            var sanitizedHash = sb.ToString();

            if (sanitizedHash.Length != 32 && sanitizedHash.Length != 40 && sanitizedHash.Length != 64)
                return null;

            var vtUrl = "https://www.virustotal.com/gui/file/" + sanitizedHash;
            return Uri.TryCreate(vtUrl, UriKind.Absolute, out var valid) ? valid.AbsoluteUri : null;
        }

        public static bool LaunchBrowserSafely(string url)
        {
            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    return false;

                if (uri.Scheme != Uri.UriSchemeHttps)
                    return false;

                if (!uri.Host.Equals("www.virustotal.com", StringComparison.OrdinalIgnoreCase) &&
                    !uri.Host.Equals("virustotal.com", StringComparison.OrdinalIgnoreCase))
                    return false;

                var psi = new ProcessStartInfo
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                };

                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void HandleVirusTotalLinkNavigation(object sender, RequestNavigateEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            var url = BuildVirusTotalUrl(hyperlink, e?.Uri);

            if (!string.IsNullOrEmpty(url) && LaunchBrowserSafely(url))
                e.Handled = true;
        }

        private static DataGridTextColumn FindColumnByBindingPath(DataGrid dataGrid, string bindingPath)
        {
            return dataGrid.Columns.OfType<DataGridTextColumn>()
                .FirstOrDefault(c => (c.Binding as Binding)?.Path.Path == bindingPath);
        }

        private static string GetLocalizedString(string key, string defaultValue)
        {
            return Application.Current.TryFindResource(key) as string ?? defaultValue;
        }
    }
}