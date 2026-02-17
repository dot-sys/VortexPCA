using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Vortex.UI.ViewModels;

namespace Vortex.UI.Services
{
    public class WelcomePageManager
    {
        private readonly Page _page;
        private MainWindowViewModel _viewModel;
        private bool _isTraceStarted = false;
        private DispatcherTimer _dotsTimer;
        private int _dotsCount = 0;
        private Storyboard _logoSpinStoryboard;

        private Button _startTraceButton;
        private StackPanel _statusPanel;
        private TextBlock _statusText;
        private TextBlock _dotsText;
        private Grid _mainGrid;
        private Image _logoImage;

        public WelcomePageManager(Page page)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            InitializeDotsTimer();
        }

        public void SetUIElements(Button startTraceButton, StackPanel statusPanel,
            TextBlock statusText, TextBlock dotsText, Grid mainGrid, Image logoImage)
        {
            _startTraceButton = startTraceButton;
            _statusPanel = statusPanel;
            _statusText = statusText;
            _dotsText = dotsText;
            _mainGrid = mainGrid;
            _logoImage = logoImage;
        }

        public async Task StartTraceAsync()
        {
            if (_isTraceStarted) return;

            _isTraceStarted = true;
            _viewModel = _page.DataContext as MainWindowViewModel;

            if (_viewModel == null)
            {
                MessageBox.Show("Unable to access MainWindowViewModel", "Error");
                return;
            }

            _startTraceButton.IsEnabled = false;
            _statusPanel.Visibility = Visibility.Visible;
            _statusText.Text = "Starting traces";

            StartLogoSpin();

            _viewModel.StartAllTraces();

            await Task.Delay(1500);

            await FadeOutStatusTextAsync();

            // Build initial status text from registered framework ViewModels
            var initialViewModels = _viewModel.FrameworkViewModels.Select(vm => vm.Name).ToList();
            _statusText.Text = initialViewModels.Count > 0
                ? $"Parsing: {string.Join(", ", initialViewModels)}"
                : "Parsing...";
            await FadeInStatusTextAsync();

            _dotsCount = 0;
            _dotsText.Text = "";
            _dotsTimer.Start();

            await MonitorTracesAsync();
        }

        private void InitializeDotsTimer()
        {
            _dotsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _dotsTimer.Tick += (sender, e) =>
            {
                _dotsCount = (_dotsCount + 1) % 4;
                if (_dotsText != null)
                {
                    _dotsText.Text = new string('.', _dotsCount);
                }
            };
        }

        private async Task MonitorTracesAsync()
        {
            while (true)
            {
                await Task.Delay(500);

                var statusParts = new System.Collections.Generic.List<string>();

                foreach (var frameworkViewModel in _viewModel.FrameworkViewModels)
                {
                    if (frameworkViewModel.IsLoading)
                    {
                        statusParts.Add(frameworkViewModel.Name);
                    }
                }

                if (statusParts.Count > 0)
                {
                    _statusText.Text = $"Parsing: {string.Join(", ", statusParts)}";
                }
                else
                {
                    _dotsTimer.Stop();
                    _statusText.Text = "All traces complete! Cleaning up memory...";
                    _dotsText.Text = "";

                    StopLogoSpin();

                    _viewModel.IsDataLoaded = true;

                    await Task.Run(() =>
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                        GC.WaitForPendingFinalizers();
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                    });

                    _statusText.Text = "All traces complete!";

                    break;
                }
            }

            await Task.Delay(500);
            await FadeOutAndNavigateAsync();
        }

        private Task FadeOutStatusTextAsync()
        {
            return AnimateOpacityAsync(_statusPanel, 1.0, 0.0, 0.5);
        }

        private Task FadeInStatusTextAsync()
        {
            return AnimateOpacityAsync(_statusPanel, 0.0, 1.0, 0.5);
        }

        private async Task FadeOutAndNavigateAsync()
        {
            await AnimateOpacityAsync(_mainGrid, 1.0, 0.0, 0.5);
            _viewModel?.NavigateToPCALAD();
        }

        private Task AnimateOpacityAsync(UIElement element, double from, double to, double durationSeconds)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new QuadraticEase { EasingMode = from > to ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            var tcs = new TaskCompletionSource<bool>();
            animation.Completed += (s, e) => tcs.SetResult(true);
            element.BeginAnimation(UIElement.OpacityProperty, animation);

            return tcs.Task;
        }

        private void StartLogoSpin()
        {
            if (_logoSpinStoryboard != null || _logoImage == null)
                return;

            var storyboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1.2),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotationAnimation.EasingFunction = new PowerEase
            {
                EasingMode = EasingMode.EaseInOut,
                Power = 2
            };

            Storyboard.SetTarget(rotationAnimation, _logoImage);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotationAnimation);

            _logoSpinStoryboard = storyboard;
            _logoSpinStoryboard.Begin();
        }

        private void StopLogoSpin()
        {
            if (_logoSpinStoryboard == null || _logoImage == null)
                return;

            _logoSpinStoryboard.Stop();

            var resetAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (_logoImage.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, resetAnimation);
            }
            _logoSpinStoryboard = null;
        }

        public void Cleanup()
        {
            _dotsTimer?.Stop();
            _logoSpinStoryboard?.Stop();
        }
    }
}
