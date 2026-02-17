using System.Windows.Controls;
using Vortex.UI.Helpers;
using VortexAMC.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace Vortex.UI.Views
{
    public partial class AMCDriverPackView : Page
    {
        private string _searchColumn = "KeyLastWriteTimestamp";
        private ICollectionView _collectionView;

        public AMCDriverPackView()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializeCollectionView();
        }

        private void InitializeCollectionView()
        {
            if (DriverPackagesDataGrid?.ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(DriverPackagesDataGrid.ItemsSource);
                _collectionView.Filter = FilterData;
            }
        }

        private bool FilterData(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            if (obj is DriverPackageData entry)
            {
                var searchText = SearchBox.Text.ToLower();
                if (_searchColumn == "KeyLastWriteTimestamp")
                    return entry.KeyLastWriteTimestamp?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Class")
                    return entry.Class?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Provider")
                    return entry.Provider?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Inf")
                    return entry.Inf?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Version")
                    return entry.Version?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Date")
                    return entry.Date?.ToLower().Contains(searchText) ?? false;
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
                    case "Class":
                        _searchColumn = "Class";
                        break;
                    case "Provider":
                        _searchColumn = "Provider";
                        break;
                    case "Inf":
                        _searchColumn = "Inf";
                        break;
                    case "Version":
                        _searchColumn = "Version";
                        break;
                    case "Date":
                        _searchColumn = "Date";
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
            _collectionView = _collectionView ?? (_collectionView = DriverPackagesDataGrid?.ItemsSource != null ? CollectionViewSource.GetDefaultView(DriverPackagesDataGrid.ItemsSource) : null);
            _collectionView?.Refresh();
        }

        private void DriverPackagesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriverPackagesDataGrid.SelectedItem is DriverPackageData selectedEntry)
            {
                var details = new ObservableCollection<DetailInfo>();

                foreach (PropertyInfo prop in typeof(DriverPackageData).GetProperties())
                {
                    var value = prop.GetValue(selectedEntry)?.ToString() ?? "N/A";
                    details.Add(new DetailInfo { Info = prop.Name, Value = value });
                }

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
            // Already on this page
        }
    }
}
