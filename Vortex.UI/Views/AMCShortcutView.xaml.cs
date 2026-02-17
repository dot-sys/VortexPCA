using System.Windows.Controls;
using Vortex.UI.Helpers;
using VortexAMC.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace Vortex.UI.Views
{
    public partial class AMCShortcutView : Page
    {
        private string _searchColumn = "KeyLastWriteTimestamp";
        private ICollectionView _collectionView;

        public AMCShortcutView()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializeCollectionView();
        }

        private void InitializeCollectionView()
        {
            if (ShortcutsDataGrid?.ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ShortcutsDataGrid.ItemsSource);
                _collectionView.Filter = FilterData;
            }
        }

        private bool FilterData(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            if (obj is ShortcutData entry)
            {
                var searchText = SearchBox.Text.ToLower();
                if (_searchColumn == "KeyLastWriteTimestamp")
                    return entry.KeyLastWriteTimestamp?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "KeyName")
                    return entry.KeyName?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "LnkName")
                    return entry.LnkName?.ToLower().Contains(searchText) ?? false;
                return false;
            }
            return false;
        }

        private void SearchColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchColumnComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var content = selectedItem.Content.ToString();
                switch (content)
                {
                    case "Last Write Time":
                        _searchColumn = "KeyLastWriteTimestamp";
                        break;
                    case "Key Name":
                        _searchColumn = "KeyName";
                        break;
                    case "Lnk Name":
                        _searchColumn = "LnkName";
                        break;
                    default:
                        _searchColumn = content.Replace(" ", "");
                        break;
                }
                _collectionView?.Refresh();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _collectionView = _collectionView ?? (_collectionView = ShortcutsDataGrid?.ItemsSource != null ? CollectionViewSource.GetDefaultView(ShortcutsDataGrid.ItemsSource) : null);
            _collectionView?.Refresh();
        }

        private void ShortcutsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShortcutsDataGrid.SelectedItem is ShortcutData selectedEntry)
            {
                var details = new ObservableCollection<DetailInfo>
                {
                    new DetailInfo { Info = "Key Name", Value = selectedEntry.KeyName ?? "N/A" },
                    new DetailInfo { Info = "Last Write Time", Value = selectedEntry.KeyLastWriteTimestamp ?? "N/A" },
                    new DetailInfo { Info = "Lnk Name", Value = selectedEntry.LnkName ?? "N/A" }
                };

                DetailsDataGrid.ItemsSource = details;
            }
            else
            {
                DetailsDataGrid.ItemsSource = null;
            }
        }

        private void CopyValue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is DataGrid dataGrid)
            {
                DataGridContextMenuHelper.CopyValue(dataGrid);
            }
        }

        private void CopyRow_Click(object sender, RoutedEventArgs e)
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
            NavigationService?.Navigate(new AMCFileEntriesView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }

        private void NavigateToShortcuts_Click(object sender, RoutedEventArgs e)
        {
            // Already on this page
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
    }
}
