using System.Windows.Controls;
using Vortex.UI.Helpers;
using VortexAMC.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows.Documents;

namespace Vortex.UI.Views
{
    public partial class AMCFileEntriesView : Page
    {
        private ICollectionView _fileEntriesView;

        public AMCFileEntriesView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += AMCFileEntriesView_Loaded;
        }

        private void AMCFileEntriesView_Loaded(object sender, RoutedEventArgs e)
        {
            if (FileEntriesDataGrid?.ItemsSource != null)
            {
                _fileEntriesView = CollectionViewSource.GetDefaultView(FileEntriesDataGrid.ItemsSource);
                _fileEntriesView.Filter = FilterFileEntries;

                // Sort by Last Write Time (newest first)
                _fileEntriesView.SortDescriptions.Clear();
                _fileEntriesView.SortDescriptions.Add(new SortDescription("FileKeyLastWriteTimestamp", ListSortDirection.Descending));
            }

            // Initialize filter toggle button text
            UpdateFilterToggleButtonText();
        }

        private bool FilterFileEntries(object item)
        {
            var entry = item as FileEntryData;
            if (entry == null)
                return true;

            // Apply "Show only" filter
            if (!ApplyShowOnlyFilter(entry))
                return false;

            // Apply size range filter
            if (!ApplySizeRangeFilter(entry))
                return false;

            // If no search text, return true (already passed all filters)
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            var searchText = SearchBox.Text.ToLower();
            var selectedIndex = SearchColumnComboBox.SelectedIndex;

            switch (selectedIndex)
            {
                case 0: // Last Write Time
                    return entry.FileKeyLastWriteTimestamp?.ToLower().Contains(searchText) ?? false;
                case 1: // Name
                    return entry.Name?.ToLower().Contains(searchText) ?? false;
                case 2: // Full Path
                    return entry.FullPath?.ToLower().Contains(searchText) ?? false;
                case 3: // Size
                    return entry.Size.ToString().Contains(searchText);
                case 4: // Version
                    return entry.BinFileVersion?.ToLower().Contains(searchText) ?? false;
                case 5: // Publisher
                    return entry.Publisher?.ToLower().Contains(searchText) ?? false;
                case 6: // Status
                    return entry.FEStatus?.ToLower().Contains(searchText) ?? false;
                case 7: // Signature
                    return entry.FESignature?.ToLower().Contains(searchText) ?? false;
                case 8: // SHA1
                    return entry.SHA1?.ToLower().Contains(searchText) ?? false;
                case 9: // Binary Type
                    return entry.BinaryType?.ToLower().Contains(searchText) ?? false;
                case 10: // File Version
                    return entry.BinFileVersion?.ToLower().Contains(searchText) ?? false;
                case 11: // Product Version
                    return entry.BinProductVersion?.ToLower().Contains(searchText) ?? false;
                case 12: // Link Date
                    return entry.LinkDate?.ToLower().Contains(searchText) ?? false;
                case 13: // Is OS Component
                    return entry.IsOsComponent.ToString().ToLower().Contains(searchText);
                case 14: // Is PE File
                    return entry.IsPeFile.ToString().ToLower().Contains(searchText);
                case 15: // Language
                    return entry.Language.ToString().Contains(searchText);
                case 16: // Path Hash
                    return entry.LongPathHash?.ToLower().Contains(searchText) ?? false;
                case 17: // Product Name
                    return entry.ProductName?.ToLower().Contains(searchText) ?? false;
                case 18: // Program ID
                    return entry.ProgramId?.ToLower().Contains(searchText) ?? false;
                case 19: // USN
                    return entry.Usn.ToString().Contains(searchText);
                case 20: // Application Name
                    return entry.ApplicationName?.ToLower().Contains(searchText) ?? false;
                case 21: // Description
                    return entry.Description?.ToLower().Contains(searchText) ?? false;
                case 22: // Source Created Date
                    return entry.SourceCreatedDate?.ToLower().Contains(searchText) ?? false;
                case 23: // Source Modified Date
                    return entry.SourceModifiedDate?.ToLower().Contains(searchText) ?? false;
                case 24: // Source Accessed Date
                    return entry.SourceAccessedDate?.ToLower().Contains(searchText) ?? false;
                default:
                    return true;
            }
        }

        private bool ApplyShowOnlyFilter(FileEntryData entry)
        {
            // Check .exe filter
            if (ExeCheckBox?.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(entry.FileExtension) || 
                    !entry.FileExtension.ToLower().Contains("exe"))
                {
                    return false;
                }
            }

            // Check .sys filter
            if (SysCheckBox?.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(entry.FileExtension) || 
                    !entry.FileExtension.ToLower().Contains("sys"))
                {
                    return false;
                }
            }

            // Check Today filter
            if (TodayCheckBox?.IsChecked == true)
            {
                string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
                if (string.IsNullOrWhiteSpace(entry.FileKeyLastWriteTimestamp) || 
                    !entry.FileKeyLastWriteTimestamp.Contains(todayDate))
                {
                    return false;
                }
            }

            // Check No Publisher filter
            if (NoPublisherCheckBox?.IsChecked == true)
            {
                if (!string.IsNullOrWhiteSpace(entry.Publisher))
                {
                    return false;
                }
            }

            // Check No OS Component filter
            if (NoOSComponentCheckBox?.IsChecked == true)
            {
                if (entry.IsOsComponent)
                {
                    return false;
                }
            }

            // Check Unassociated filter
            if (UnassociatedCheckBox?.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(entry.ApplicationName) || 
                    !entry.ApplicationName.ToLower().Contains("unassociated"))
                {
                    return false;
                }
            }

            // If all checked filters pass, show the entry
            return true;
        }

        private bool ApplySizeRangeFilter(FileEntryData entry)
        {
            // Convert bytes to MB for comparison
            double sizeInMB = entry.Size / (1024.0 * 1024.0);

            // Check "Size Above >" filter
            if (!string.IsNullOrWhiteSpace(SizeAboveBox.Text))
            {
                if (double.TryParse(SizeAboveBox.Text, out double sizeAbove))
                {
                    if (sizeInMB < sizeAbove)
                        return false;
                }
            }

            // Check "< Size Below" filter
            if (!string.IsNullOrWhiteSpace(SizeBelowBox.Text))
            {
                if (double.TryParse(SizeBelowBox.Text, out double sizeBelow))
                {
                    if (sizeInMB > sizeBelow)
                        return false;
                }
            }

            return true;
        }

        private void SizeFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _fileEntriesView?.Refresh();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _fileEntriesView?.Refresh();
        }

        private void SearchColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_fileEntriesView != null && !string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                _fileEntriesView.Refresh();
            }
        }

        private void FileEntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileEntriesDataGrid.SelectedItem is FileEntryData selectedEntry)
            {
                // Convert size to MB for display
                double sizeInMB = selectedEntry.Size / (1024.0 * 1024.0);
                string sizeDisplay = sizeInMB.ToString("N2") + " MB";

                var details = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Last Write Time", Value = selectedEntry.FileKeyLastWriteTimestamp ?? "N/A" },
                    new DetailInfo { Info = "SHA1", Value = selectedEntry.SHA1 ?? "N/A" },
                    new DetailInfo { Info = "Full Path", Value = selectedEntry.FullPath ?? "N/A" },
                    new DetailInfo { Info = "Size", Value = sizeDisplay },
                    new DetailInfo { Info = "Signature", Value = selectedEntry.FESignature ?? "N/A" },
                    new DetailInfo { Info = "Status", Value = selectedEntry.FEStatus ?? "N/A" },
                    new DetailInfo { Info = "Source Created Date", Value = selectedEntry.SourceCreatedDate ?? "N/A" },
                    new DetailInfo { Info = "Source Modified Date", Value = selectedEntry.SourceModifiedDate ?? "N/A" },
                    new DetailInfo { Info = "Source Accessed Date", Value = selectedEntry.SourceAccessedDate ?? "N/A" },
                    new DetailInfo { Info = "Path Hash", Value = selectedEntry.LongPathHash ?? "N/A" },
                    new DetailInfo { Info = "File Extension", Value = selectedEntry.FileExtension ?? "N/A" },
                    new DetailInfo { Info = "USN", Value = selectedEntry.Usn.ToString() },
                    new DetailInfo { Info = "Binary Type", Value = selectedEntry.BinaryType ?? "N/A" },
                    new DetailInfo { Info = "Is PE File", Value = selectedEntry.IsPeFile.ToString() },
                    new DetailInfo { Info = "Is OS Component", Value = selectedEntry.IsOsComponent.ToString() },
                    new DetailInfo { Info = "Link Date", Value = selectedEntry.LinkDate ?? "N/A" },
                    new DetailInfo { Info = "Bin File Version", Value = selectedEntry.BinFileVersion ?? "N/A" },
                    new DetailInfo { Info = "Bin Product Version", Value = selectedEntry.BinProductVersion ?? "N/A" },
                    new DetailInfo { Info = "Version", Value = selectedEntry.Version ?? "N/A" },
                    new DetailInfo { Info = "Product Version", Value = selectedEntry.ProductVersion ?? "N/A" },
                    new DetailInfo { Info = "Product Name", Value = selectedEntry.ProductName ?? "N/A" },
                    new DetailInfo { Info = "Application Name", Value = selectedEntry.ApplicationName ?? "N/A" },
                    new DetailInfo { Info = "Program ID", Value = selectedEntry.ProgramId ?? "N/A" },
                    new DetailInfo { Info = "Description", Value = selectedEntry.Description ?? "N/A" },
                    new DetailInfo { Info = "Language", Value = selectedEntry.Language.ToString() },
                    new DetailInfo { Info = "Publisher", Value = selectedEntry.Publisher ?? "N/A" }
                };

                DetailsDataGrid.ItemsSource = details;
            }
            else
            {
                DetailsDataGrid.ItemsSource = null;
            }
        }

        private void CopyValue_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is DataGrid dataGrid)
            {
                DataGridContextMenuHelper.CopyValue(dataGrid);
            }
        }

        private void CopyRow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is DataGrid dataGrid)
            {
                DataGridContextMenuHelper.CopyRow(dataGrid);
            }
        }

        private void NavigateToFileEntries_Click(object sender, RoutedEventArgs e)
        {
            // Already on this page
        }

        private void NavigateToShortcuts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCShortcutView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void NavigateToPnp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCPnpView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void NavigateToContainers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCContainerView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void NavigateToDriverBin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCDriverBinView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void NavigateToDriverPack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCDriverPackView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void ShowOnlyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _fileEntriesView?.Refresh();
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterToggleButtonText();
            _fileEntriesView?.Refresh();
        }

        private void FilterToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // The ToggleButton's IsChecked is bound to the Popup's IsOpen
            // This method can be used for additional logic if needed
        }

        private void UpdateFilterToggleButtonText()
        {
            if (FilterToggleButton == null)
                return;

            int selectedCount = 0;
            if (ExeCheckBox?.IsChecked == true) selectedCount++;
            if (SysCheckBox?.IsChecked == true) selectedCount++;
            if (TodayCheckBox?.IsChecked == true) selectedCount++;
            if (NoPublisherCheckBox?.IsChecked == true) selectedCount++;
            if (NoOSComponentCheckBox?.IsChecked == true) selectedCount++;
            if (UnassociatedCheckBox?.IsChecked == true) selectedCount++;

            if (selectedCount == 0)
            {
                FilterToggleButton.Content = "All Entries";
            }
            else if (selectedCount == 1)
            {
                if (ExeCheckBox?.IsChecked == true) FilterToggleButton.Content = ".exe";
                else if (SysCheckBox?.IsChecked == true) FilterToggleButton.Content = ".sys";
                else if (TodayCheckBox?.IsChecked == true) FilterToggleButton.Content = "Today";
                else if (NoPublisherCheckBox?.IsChecked == true) FilterToggleButton.Content = "No Publisher";
                else if (NoOSComponentCheckBox?.IsChecked == true) FilterToggleButton.Content = "No OS Component";
                else if (UnassociatedCheckBox?.IsChecked == true) FilterToggleButton.Content = "Unassociated";
            }
            else
            {
                FilterToggleButton.Content = $"{selectedCount} filters";
            }
        }

        private void VirusTotalLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            var url = BuildVirusTotalUrl(hyperlink, e?.Uri);

            if (!string.IsNullOrEmpty(url) && TryShellLaunch(url))
            {
                e.Handled = true;
            }
        }

        private string BuildVirusTotalUrl(Hyperlink link, Uri uri)
        {
            var hash = uri?.OriginalString ?? link?.NavigateUri?.OriginalString;
            if (string.IsNullOrWhiteSpace(hash)) return null;

            var vtUrl = "https://www.virustotal.com/gui/file/" + hash.Trim();
            return Uri.TryCreate(vtUrl, UriKind.Absolute, out var valid) ? valid.AbsoluteUri : null;
        }

        private bool TryShellLaunch(string fileName, string arguments = null)
        {
            try
            {
                var psi = new ProcessStartInfo { FileName = fileName, UseShellExecute = true };
                if (!string.IsNullOrWhiteSpace(arguments)) psi.Arguments = arguments;
                Process.Start(psi);
                return true;
            }
            catch { return false; }
        }
    }
}