using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Vortex.UI.Services;
using Vortex.UI.ViewModels;

namespace Vortex.UI
{
    public partial class MainWindow : Window
    {
        private readonly WindowManager _windowManager;
        private readonly LanguageManager _languageManager;

        public MainWindow()
        {
            InitializeComponent();

            _windowManager = new WindowManager(this);
            _languageManager = new LanguageManager();

            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;

            foreach (MenuItem item in LanguageButton.ContextMenu.Items)
            {
                item.Click += LanguageMenuItem_Click;
            }
        }

        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.SetFrame(MainFrame);
            CloseButton.ApplyTemplate();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e) => _windowManager.OnStateChanged();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => 
            _windowManager.HandleTitleBarMouseDown(e.ClickCount);

        private void Minimize_Click(object sender, RoutedEventArgs e) => _windowManager.Minimize();

        private void Refresh_Click(object sender, RoutedEventArgs e) => ViewModel?.RefreshCurrentView();

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => _windowManager.Close();

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var content = MainFrame.Content as FrameworkElement;
            _windowManager.AnimateFrameNavigation(content);

            bool isWelcomeView = content?.GetType().Name == "WelcomeView";
            LeftNavigationButtons.IsEnabled = !isWelcomeView;
            RightNavigationButtons.IsEnabled = !isWelcomeView;
        }

        private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Header is string languageCode)
            {
                _languageManager.ChangeLanguage(languageCode);
                LanguageButton.Content = languageCode;
                ViewModel?.ReloadCurrentView();
            }
        }

        private void LAD_Click(object sender, RoutedEventArgs e) => ViewModel?.NavigateToPCALAD();

        private void GDB_Click(object sender, RoutedEventArgs e) => ViewModel?.NavigateToPCAGDB();
    }
}
