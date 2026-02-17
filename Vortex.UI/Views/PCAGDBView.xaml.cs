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
    public partial class PCAGDBView : Page
    {
        private ICollectionView _pcaEntriesView;
        private List<GeneralDbEntry> _originalEntries;
        private ObservableCollection<GeneralDbEntry> _groupedEntries;

        public PCAGDBView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += PCAGDBView_Loaded;
        }

        private void PCAGDBView_Loaded(object sender, RoutedEventArgs e)
        {
            PCAViewHelper.PopulateTimeDisplayComboBox(TimeDisplayComboBox);
            PopulateSearchColumnComboBox();

            if (PcaEntriesDataGrid?.ItemsSource != null)
            {
                _originalEntries = (PcaEntriesDataGrid.ItemsSource as System.Collections.IEnumerable)?.Cast<GeneralDbEntry>().ToList();

                _pcaEntriesView = CollectionViewSource.GetDefaultView(PcaEntriesDataGrid.ItemsSource);
                _pcaEntriesView.Filter = FilterPcaEntries;
                _pcaEntriesView.SortDescriptions.Clear();
                _pcaEntriesView.SortDescriptions.Add(new SortDescription("TimestampLocal", ListSortDirection.Descending));
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
                { "Timestamp", "Timestamp" },
                { "FullPath", "Full Path" },
                { "EntryType", "Entry Type" },
                { "ProcessName", "Process Name" },
                { "Publisher", "Publisher" },
                { "Version", "Version" },
                { "Status", "Status" },
                { "Signature", "Signature" },
                { "FoundInUSN", "FoundInUSN" }
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
            PCAViewHelper.UpdateTimeColumnVisibility(PcaEntriesDataGrid, TimeDisplayComboBox, "TimestampUtc", "TimestampLocal");
        }

        private bool FilterPcaEntries(object item)
        {
            if (!(item is GeneralDbEntry entry))
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
                    return entry.TimestampLocal?.ToLower().Contains(searchText) ?? false;
                case 1:
                    return entry.ResolvedPath?.ToLower().Contains(searchText) ?? false;
                case 2:
                    return entry.EntryType?.ToLower().Contains(searchText) ?? false;
                case 3:
                    return entry.ProcessName?.ToLower().Contains(searchText) ?? false;
                case 4:
                    return entry.Publisher?.ToLower().Contains(searchText) ?? false;
                case 5:
                    return entry.Version?.ToLower().Contains(searchText) ?? false;
                case 6:
                    return entry.FileStatus?.ToLower().Contains(searchText) ?? false;
                case 7:
                    return entry.SignatureStatus?.ToLower().Contains(searchText) ?? false;
                case 8:
                    return entry.FoundInUSNDisplay?.ToLower().Contains(searchText) ?? false;
                default:
                    return true;
            }
        }

        private bool ApplySizeFilter(GeneralDbEntry entry)
        {
            PCAViewHelper.ParseSizeFilter(SizeMinBox, SizeMaxBox, out double? minSize, out double? maxSize);
            return PCAViewHelper.ApplySizeFilter(entry.FilesizeInMB, minSize, maxSize);
        }

        private bool ApplyShowOnlyFilter(GeneralDbEntry entry)
        {
            if (TodayCheckBox?.IsChecked == true && !PCAViewHelper.IsTodayFilter(entry.TimestampLocal))
                return false;

            if (NoExeCheckBox?.IsChecked == true && 
                (string.IsNullOrWhiteSpace(entry.ResolvedPath) || entry.ResolvedPath.ToLower().EndsWith(".exe")))
                return false;

            if (NoProcessNameCheckBox?.IsChecked == true && !string.IsNullOrWhiteSpace(entry.ProcessName))
                return false;

            if (NoPublisherCheckBox?.IsChecked == true && !string.IsNullOrWhiteSpace(entry.Publisher))
                return false;

            if (NoVersionCheckBox?.IsChecked == true && !string.IsNullOrWhiteSpace(entry.Version))
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
            _pcaEntriesView?.Refresh();
            UpdateEntryCount();
        }

        private void UpdateEntryCount()
        {
            if (EntryCountRun == null || PcaEntriesDataGrid?.ItemsSource == null)
                return;

            var view = CollectionViewSource.GetDefaultView(PcaEntriesDataGrid.ItemsSource);
            if (view == null)
                return;

            int filteredCount = view.Cast<object>().Count();
            int totalCount = (PcaEntriesDataGrid.ItemsSource as System.Collections.IEnumerable)?.Cast<object>().Count() ?? 0;

            EntryCountRun.Text = $"{filteredCount} / {totalCount}";
        }

        private void SearchColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pcaEntriesView != null && !string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                RefreshFilterAndUpdateCount();
            }
        }

        private void PcaEntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PcaEntriesDataGrid.SelectedItem is GeneralDbEntry selectedEntry)
            {
                var nonTimeDetails = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Full Path", Value = selectedEntry.ResolvedPath ?? "N/A" },
                    new DetailInfo { Info = "MD5 Hash", Value = selectedEntry.MD5Hash ?? "N/A" },
                    new DetailInfo { Info = "USN Journal Entries", Value = selectedEntry.USNEntriesText ?? "N/A" },
                    new DetailInfo { Info = "Run Count", Value = selectedEntry.RunCount.ToString() },
                    new DetailInfo { Info = "Process Name", Value = selectedEntry.ProcessName ?? "N/A" },
                    new DetailInfo { Info = "Publisher", Value = selectedEntry.Publisher ?? "N/A" },
                    new DetailInfo { Info = "Version", Value = selectedEntry.Version ?? "N/A" },
                    new DetailInfo { Info = "Entry Type", Value = selectedEntry.EntryType ?? "N/A" },
                    new DetailInfo { Info = "Program ID", Value = selectedEntry.ProgramId ?? "N/A" },
                    new DetailInfo { Info = "Exit Code", Value = selectedEntry.Exitcode ?? "N/A" },
                    new DetailInfo { Info = "File Path Status", Value = selectedEntry.FilePathStatus ?? "N/A" },
                    new DetailInfo { Info = "Filesize (Raw)", Value = selectedEntry.FilesizeInB ?? "N/A" },
                    new DetailInfo { Info = "Filesize (MB)", Value = selectedEntry.FilesizeInMB ?? "N/A" },
                    new DetailInfo { Info = "Address Of Entry Point", Value = selectedEntry.AddressOfEntryPoint.HasValue ? $"0x{selectedEntry.AddressOfEntryPoint.Value:X}" : "N/A" },
                    new DetailInfo { Info = "Error Message", Value = selectedEntry.ErrorMessage ?? "N/A" },
                    new DetailInfo { Info = "Raw Path", Value = selectedEntry.ExecutablePath ?? "N/A" }
                };

                var timeDetails = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Timestamp (Local)", Value = selectedEntry.TimestampLocal ?? "N/A" },
                    new DetailInfo { Info = "Timestamp (UTC)", Value = selectedEntry.TimestampUtc ?? "N/A" },
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

        private void ShowRunCountToggleButton_Changed(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (toggleButton == null || PcaEntriesDataGrid == null)
                return;

            var runCountColumn = RunCountColumn;
            if (runCountColumn == null)
                return;

            if (toggleButton.IsChecked == true)
            {
                if (_originalEntries != null)
                {
                    var groupedData = _originalEntries
                        .Where(entry => !string.IsNullOrWhiteSpace(entry.ResolvedPath))
                        .GroupBy(entry => entry.ResolvedPath.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(group =>
                        {
                            var first = group.First();
                            var entry = new GeneralDbEntry
                            {
                                TimestampUtc = first.TimestampUtc,
                                TimestampLocal = first.TimestampLocal,
                                EntryType = first.EntryType,
                                ExecutablePath = first.ExecutablePath,
                                ProcessName = first.ProcessName,
                                Publisher = first.Publisher,
                                Version = first.Version,
                                ProgramId = first.ProgramId,
                                Exitcode = first.Exitcode,
                                SourceFilePath = first.SourceFilePath,
                                ResolvedPath = first.ResolvedPath.Trim(),
                                FilePathStatus = first.FilePathStatus,
                                RunCount = group.Count(),
                                FileStatus = first.FileStatus,
                                IsFilePresent = first.IsFilePresent,
                                CreatedDate = first.CreatedDate,
                                ModifiedDate = first.ModifiedDate,
                                AccessedDate = first.AccessedDate,
                                RawFilesize = first.RawFilesize,
                                FilesizeInB = first.FilesizeInB,
                                FilesizeInMB = first.FilesizeInMB,
                                SignatureStatus = first.SignatureStatus,
                                MD5Hash = first.MD5Hash,
                                CompiledTime = first.CompiledTime,
                                IsDebugAllowed = first.IsDebugAllowed,
                                AddressOfEntryPoint = first.AddressOfEntryPoint,
                                ErrorMessage = first.ErrorMessage,
                                FoundInUSN = first.FoundInUSN,
                                USNEntriesText = first.USNEntriesText
                            };
                            return entry;
                        })
                        .OrderByDescending(entry => entry.RunCount)
                        .ToList();

                    _groupedEntries = new ObservableCollection<GeneralDbEntry>(groupedData);
                    PcaEntriesDataGrid.ItemsSource = _groupedEntries;
                }

                runCountColumn.Visibility = Visibility.Visible;

                _pcaEntriesView = CollectionViewSource.GetDefaultView(PcaEntriesDataGrid.ItemsSource);
                _pcaEntriesView.Filter = FilterPcaEntries;
                _pcaEntriesView.SortDescriptions.Clear();
                _pcaEntriesView.SortDescriptions.Add(new SortDescription("RunCount", ListSortDirection.Descending));
            }
            else
            {
                if (_originalEntries != null)
                {
                    PcaEntriesDataGrid.ItemsSource = new ObservableCollection<GeneralDbEntry>(_originalEntries);
                }

                runCountColumn.Visibility = Visibility.Collapsed;

                _pcaEntriesView = CollectionViewSource.GetDefaultView(PcaEntriesDataGrid.ItemsSource);
                _pcaEntriesView.Filter = FilterPcaEntries;
                _pcaEntriesView.SortDescriptions.Clear();
                _pcaEntriesView.SortDescriptions.Add(new SortDescription("TimestampLocal", ListSortDirection.Descending));
            }

            RefreshFilterAndUpdateCount();
        }

        private void UpdateFilterToggleButtonText()
        {
            if (FilterToggleButton == null)
                return;

            int selectedCount = 0;
            if (TodayCheckBox?.IsChecked == true) selectedCount++;
            if (NoExeCheckBox?.IsChecked == true) selectedCount++;
            if (NoProcessNameCheckBox?.IsChecked == true) selectedCount++;
            if (NoPublisherCheckBox?.IsChecked == true) selectedCount++;
            if (NoVersionCheckBox?.IsChecked == true) selectedCount++;
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
                else if (NoExeCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoExe") as string ?? "No exe";
                else if (NoProcessNameCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoProcessName") as string ?? "No Process Name";
                else if (NoPublisherCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoPublisher") as string ?? "No Publisher";
                else if (NoVersionCheckBox?.IsChecked == true) FilterToggleButton.Content = Application.Current.TryFindResource("NoVersionInfo") as string ?? "No Version Info";
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

            if (NoProcessNameCheckBox != null)
                NoProcessNameCheckBox.IsChecked = false;

            if (NoPublisherCheckBox != null)
                NoPublisherCheckBox.IsChecked = false;

            if (NoVersionCheckBox != null)
                NoVersionCheckBox.IsChecked = false;

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
