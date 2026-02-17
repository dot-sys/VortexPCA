using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Vortex.UI.Helpers;
using VortexLocalPCA.Models;

namespace Vortex.UI.Views
{
    public partial class PCALADView : Page
    {
        private ICollectionView _ladEntriesView;

        public PCALADView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += PCALADView_Loaded;
        }

        private void PCALADView_Loaded(object sender, RoutedEventArgs e)
        {
            PCAViewHelper.PopulateTimeDisplayComboBox(TimeDisplayComboBox);
            PopulateSearchColumnComboBox();

            if (LadEntriesDataGrid?.ItemsSource != null)
            {
                _ladEntriesView = CollectionViewSource.GetDefaultView(LadEntriesDataGrid.ItemsSource);
                _ladEntriesView.Filter = FilterLadEntries;
                _ladEntriesView.SortDescriptions.Clear();
                _ladEntriesView.SortDescriptions.Add(new SortDescription("LastExecutedTimeLocal", ListSortDirection.Descending));
            }

            if (EntriesLabelRun != null)
                EntriesLabelRun.Text = Application.Current.TryFindResource("Entries") as string ?? "Entries";

            if (ClearFiltersRun != null)
                ClearFiltersRun.Text = Application.Current.TryFindResource("ClearFilters") as string ?? "Clear Filters";

            UpdateFilterToggleButtonText();
            UpdateTimeColumnVisibility();
            UpdateEntryCount();
        }

        private void PopulateSearchColumnComboBox()
        {
            if (SearchColumnComboBox == null) return;

            int selectedIndex = SearchColumnComboBox.SelectedIndex;
            SearchColumnComboBox.Items.Clear();

            var localizedStrings = new Dictionary<string, string>
            {
                { "ExecutedTime", "Executed Time" },
                { "FullPath", "Full Path" },
                { "Status", "Status" },
                { "Signature", "Signature" },
                { "FoundInUSN", "FoundInUSN" },
                { "AllowDebug", "AllowDebug" }
            };

            foreach (var kvp in localizedStrings)
            {
                var text = Application.Current.TryFindResource(kvp.Key) as string ?? kvp.Value;
                SearchColumnComboBox.Items.Add(new ComboBoxItem { Content = text });
            }

            SearchColumnComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 1;
        }

        private void TimeDisplayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeColumnVisibility();
        }

        private void UpdateTimeColumnVisibility()
        {
            PCAViewHelper.UpdateTimeColumnVisibility(LadEntriesDataGrid, TimeDisplayComboBox, "LastExecutedTimeUtc", "LastExecutedTimeLocal");
        }

        private bool FilterLadEntries(object item)
        {
            if (!(item is LadEntry entry))
                return true;

            if (!ApplyShowOnlyFilter(entry))
                return false;

            if (!ApplySizeFilter(entry))
                return false;

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            var searchText = SearchBox.Text.ToLower();
            var selectedIndex = SearchColumnComboBox.SelectedIndex;

            switch (selectedIndex)
            {
                case 0:
                    return entry.LastExecutedTimeLocal?.ToLower().Contains(searchText) ?? false;
                case 1:
                    return entry.FullPath?.ToLower().Contains(searchText) ?? false;
                case 2:
                    return entry.FileStatus?.ToLower().Contains(searchText) ?? false;
                case 3:
                    return entry.SignatureStatus?.ToLower().Contains(searchText) ?? false;
                case 4:
                    return entry.MD5Hash?.ToLower().Contains(searchText) ?? false;
                case 5:
                    return entry.FoundInUSNDisplay?.ToLower().Contains(searchText) ?? false;
                case 6:
                    return entry.IsDebugAllowed?.ToString().ToLower().Contains(searchText) ?? false;
                default:
                    return true;
            }
        }

        private bool ApplyShowOnlyFilter(LadEntry entry)
        {
            if (TodayCheckBox?.IsChecked == true && !PCAViewHelper.IsTodayFilter(entry.LastExecutedTimeLocal))
                return false;

            if (NoExeCheckBox?.IsChecked == true && 
                (string.IsNullOrWhiteSpace(entry.FullPath) || entry.FullPath.ToLower().EndsWith(".exe")))
                return false;

            if (NoDebugAllowedCheckBox?.IsChecked == true && entry.IsDebugAllowed != false)
                return false;

            if (FilePresentCheckBox?.IsChecked == true && entry.FileStatus != "Present")
                return false;

            if (DeletedCheckBox?.IsChecked == true && entry.FileStatus != "Deleted")
                return false;

            if (UnsignedCheckBox?.IsChecked == true && entry.SignatureStatus != "Unsigned")
                return false;

            if (FoundInUSNCheckBox?.IsChecked == true && !entry.FoundInUSN)
                return false;

            return true;
        }

        private bool ApplySizeFilter(LadEntry entry)
        {
            PCAViewHelper.ParseSizeFilter(SizeMinBox, SizeMaxBox, out double? minSize, out double? maxSize);
            return PCAViewHelper.ApplySizeFilter(entry.FilesizeInMB, minSize, maxSize);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilterAndUpdateCount();
        }

        private void SizeFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilterAndUpdateCount();
        }

        private void RefreshFilterAndUpdateCount()
        {
            _ladEntriesView?.Refresh();
            UpdateEntryCount();
        }

        private void UpdateEntryCount()
        {
            if (EntryCountRun == null || LadEntriesDataGrid?.ItemsSource == null)
                return;

            var view = CollectionViewSource.GetDefaultView(LadEntriesDataGrid.ItemsSource);
            if (view == null)
                return;

            int filteredCount = view.Cast<object>().Count();
            int totalCount = (LadEntriesDataGrid.ItemsSource as System.Collections.IEnumerable)?.Cast<object>().Count() ?? 0;

            EntryCountRun.Text = $"{filteredCount} / {totalCount}";
        }

        private void SearchColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ladEntriesView != null && !string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                RefreshFilterAndUpdateCount();
            }
        }

        private void LadEntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LadEntriesDataGrid.SelectedItem is LadEntry selectedEntry)
            {
                var nonTimeDetails = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Full Path", Value = selectedEntry.FullPath ?? "N/A" },
                    new DetailInfo { Info = "MD5 Hash", Value = selectedEntry.MD5Hash ?? "N/A" },
                    new DetailInfo { Info = "USN Journal Entries", Value = selectedEntry.USNEntriesText ?? "N/A" },
                    new DetailInfo { Info = "File Status", Value = selectedEntry.FileStatus ?? "N/A" },
                    new DetailInfo { Info = "File Size (Raw)", Value = selectedEntry.FilesizeInB ?? "N/A" },
                    new DetailInfo { Info = "Address of Entry Point", Value = selectedEntry.AddressOfEntryPoint.HasValue ? $"0x{selectedEntry.AddressOfEntryPoint.Value:X}" : "N/A" }
                };

                var timeDetails = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Last Executed Time (Local)", Value = selectedEntry.LastExecutedTimeLocal ?? "N/A" },
                    new DetailInfo { Info = "Last Executed Time (UTC)", Value = selectedEntry.LastExecutedTimeUtc ?? "N/A" },
                    new DetailInfo { Info = "Created Date", Value = selectedEntry.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A" },
                    new DetailInfo { Info = "Modified Date", Value = selectedEntry.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A" },
                    new DetailInfo { Info = "Accessed Date", Value = selectedEntry.AccessedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A" },
                    new DetailInfo { Info = "Compiled Time", Value = selectedEntry.CompiledTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A" }
                };

                DetailsDataGrid.ItemsSource = nonTimeDetails;
                TimestampsDataGrid.ItemsSource = timeDetails;
            }
            else
            {
                DetailsDataGrid.ItemsSource = null;
                TimestampsDataGrid.ItemsSource = null;
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

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterToggleButtonText();
            RefreshFilterAndUpdateCount();
        }

        private void FilterToggleButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UpdateFilterToggleButtonText()
        {
            if (FilterToggleButton == null)
                return;

            int selectedCount = 0;
            if (TodayCheckBox?.IsChecked == true) selectedCount++;
            if (NoExeCheckBox?.IsChecked == true) selectedCount++;
            if (NoDebugAllowedCheckBox?.IsChecked == true) selectedCount++;
            if (FilePresentCheckBox?.IsChecked == true) selectedCount++;
            if (DeletedCheckBox?.IsChecked == true) selectedCount++;
            if (UnsignedCheckBox?.IsChecked == true) selectedCount++;
            if (FoundInUSNCheckBox?.IsChecked == true) selectedCount++;

            if (selectedCount == 0)
            {
                FilterToggleButton.Content = Application.Current.TryFindResource("ShowAllEntries") as string ?? "Show all Entries";
            }
            else if (selectedCount == 1)
            {
                if (TodayCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("Today") as string ?? "Today";
                else if (NoExeCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoExe") as string ?? "No .exe";
                else if (NoDebugAllowedCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoDebugAllowed") as string ?? "No Debug Allowed";
                else if (FilePresentCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("FilePresent") as string ?? "File Present";
                else if (DeletedCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("Deleted") as string ?? "Deleted";
                else if (UnsignedCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("Unsigned") as string ?? "Unsigned";
                else if (FoundInUSNCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("FoundInUSNFilter") as string ?? "Found in USN";
            }
            else
            {
                var formatString = Application.Current.TryFindResource("FiltersFormat") as string ?? "{0} filters";
                FilterToggleButton.Content = string.Format(formatString, selectedCount);
            }
        }

        private void VirusTotalLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            PCAViewHelper.HandleVirusTotalLinkNavigation(sender, e);
        }

        private void ClearFiltersLink_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox != null)
                SearchBox.Text = string.Empty;

            if (SizeMinBox != null)
                SizeMinBox.Text = string.Empty;

            if (SizeMaxBox != null)
                SizeMaxBox.Text = string.Empty;

            if (TodayCheckBox != null)
                TodayCheckBox.IsChecked = false;

            if (NoExeCheckBox != null)
                NoExeCheckBox.IsChecked = false;

            if (NoDebugAllowedCheckBox != null)
                NoDebugAllowedCheckBox.IsChecked = false;

            if (FilePresentCheckBox != null)
                FilePresentCheckBox.IsChecked = false;

            if (DeletedCheckBox != null)
                DeletedCheckBox.IsChecked = false;

            if (UnsignedCheckBox != null)
                UnsignedCheckBox.IsChecked = false;

            if (FoundInUSNCheckBox != null)
                FoundInUSNCheckBox.IsChecked = false;

            UpdateFilterToggleButtonText();
            RefreshFilterAndUpdateCount();
        }
    }
}
