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
    public partial class AMCDriverBinView : Page
    {
        private string _searchColumn = "KeyLastWriteTimestamp";
        private ICollectionView _collectionView;

        public AMCDriverBinView()
        {
            InitializeComponent();
            Loaded += (s, e) => InitializeCollectionView();
        }

        private void InitializeCollectionView()
        {
            if (DriverBinariesDataGrid?.ItemsSource != null)
            {
                _collectionView = CollectionViewSource.GetDefaultView(DriverBinariesDataGrid.ItemsSource);
                _collectionView.Filter = FilterData;
            }
        }

        private bool FilterData(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            if (obj is DriverBinaryData entry)
            {
                var searchText = SearchBox.Text.ToLower();
                if (_searchColumn == "KeyLastWriteTimestamp")
                    return entry.KeyLastWriteTimestamp?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "DriverName")
                    return entry.DriverName?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "DriverCompany")
                    return entry.DriverCompany?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "DriverVersion")
                    return entry.DriverVersion?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "Product")
                    return entry.Product?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "DriverType")
                    return entry.DriverType?.ToLower().Contains(searchText) ?? false;
                if (_searchColumn == "DriverSigned")
                    return entry.DriverSigned.ToString().ToLower().Contains(searchText);
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
                    case "Driver Name":
                        _searchColumn = "DriverName";
                        break;
                    case "Driver Company":
                        _searchColumn = "DriverCompany";
                        break;
                    case "Driver Version":
                        _searchColumn = "DriverVersion";
                        break;
                    case "Product":
                        _searchColumn = "Product";
                        break;
                    case "Driver Type":
                        _searchColumn = "DriverType";
                        break;
                    case "Driver Signed":
                        _searchColumn = "DriverSigned";
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
            _collectionView = _collectionView ?? (_collectionView = DriverBinariesDataGrid?.ItemsSource != null ? CollectionViewSource.GetDefaultView(DriverBinariesDataGrid.ItemsSource) : null);
            _collectionView?.Refresh();
        }

        private void DriverBinariesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriverBinariesDataGrid.SelectedItem is DriverBinaryData selectedEntry)
            {
                var details = new ObservableCollection<DetailInfo>();

                foreach (PropertyInfo prop in typeof(DriverBinaryData).GetProperties())
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
            // Already on this page
        }

        private void NavigateToDriverPack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AMCDriverPackView { DataContext = Application.Current.Resources["AmCacheResults"] });
        }
    }
}
