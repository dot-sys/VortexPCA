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
    public partial class AMCContainerView : Page
    {
        private string _searchColumn = "KeyLastWriteTimestamp";
        private ICollectionView _collectionView;

        public AMCContainerView()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializeCollectionView();
        }

        private void InitializeCollectionView()
        {
            if (ContainersDataGrid?.ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ContainersDataGrid.ItemsSource);
                _collectionView.Filter = FilterData;
            }
        }

        private bool FilterData(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            if (obj is DeviceContainerData entry)
            {
                var searchText = SearchBox.Text.ToLower();
                if (_searchColumn == "KeyLastWriteTimestamp")
                    return entry.KeyLastWriteTimestamp?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "FriendlyName")
                    return entry.FriendlyName?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Manufacturer")
                    return entry.Manufacturer?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "ModelName")
                    return entry.ModelName?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "PrimaryCategory")
                    return entry.PrimaryCategory?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "State")
                    return entry.State?.ToLower().Contains(searchText) ?? false;
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
                    case "Friendly Name":
                        _searchColumn = "FriendlyName";
                        break;
                    case "Manufacturer":
                        _searchColumn = "Manufacturer";
                        break;
                    case "Model Name":
                        _searchColumn = "ModelName";
                        break;
                    case "Primary Category":
                        _searchColumn = "PrimaryCategory";
                        break;
                    case "State":
                        _searchColumn = "State";
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
            _collectionView = _collectionView ?? (_collectionView = ContainersDataGrid?.ItemsSource != null ? CollectionViewSource.GetDefaultView(ContainersDataGrid.ItemsSource) : null);
            _collectionView?.Refresh();
        }

        private void ContainersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContainersDataGrid.SelectedItem is DeviceContainerData selectedEntry)
            {
                var details = new ObservableCollection<DetailInfo>();

                foreach (PropertyInfo prop in typeof(DeviceContainerData).GetProperties())
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
            // Already on this page
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
