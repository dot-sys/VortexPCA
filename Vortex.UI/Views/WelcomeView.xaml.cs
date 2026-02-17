using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VortexLocalPCA;
using Vortex.UI.ViewModels;

namespace Vortex.UI.Views
{
    public partial class WelcomeView : Page
    {
        private LocalPCAOrchestrator _orchestrator;
        private DispatcherTimer _dotTimer;
        private int _dotCount = 0;
        private Storyboard _logoSpinStoryboard;
        private bool _isAnalysisStarted = false;
        private DateTime _currentStatusStartTime;
        private DateTime _analysisStartTime;

        public WelcomeView()
        {
            InitializeComponent();
            InitializeDotTimer();
        }

        private void InitializeDotTimer()
        {
            _dotTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _dotTimer.Tick += DotTimer_Tick;
        }

        private void DotTimer_Tick(object sender, EventArgs e)
        {
            _dotCount = (_dotCount + 1) % 4;
            DotsText.Text = new string('.', _dotCount);
        }

        private async void StartTrace_Click(object sender, RoutedEventArgs e)
        {
            if (_isAnalysisStarted) return;

            try
            {
                _isAnalysisStarted = true;

                StartTraceButton.IsEnabled = false;
                StatusPanel.Visibility = Visibility.Visible;
                UpdateStatusText("StartingAnalysis");
                DotsText.Text = "";

                StartLogoSpin();
                _dotTimer.Start();

                _analysisStartTime = DateTime.Now;

                await ShowStatusForMinDuration("StartingAnalysis", 2000);

                _orchestrator = new LocalPCAOrchestrator();
                _orchestrator.ProgressChanged += OnProgressChanged;

                var result = await Task.Run(() => _orchestrator.StartAnalysis());

                var totalSeconds = (DateTime.Now - _analysisStartTime).TotalSeconds;
                var formattedSeconds = totalSeconds.ToString("F0");

                if (Application.Current.Resources.Contains("FinishedAnalysis"))
                {
                    var template = Application.Current.Resources["FinishedAnalysis"] as string;
                    StatusText.Text = string.Format(template, formattedSeconds);
                }
                else
                {
                    StatusText.Text = $"Finished Analyzing in {formattedSeconds} Seconds";
                }

                await Task.Delay(1000);

                _dotTimer.Stop();
                DotsText.Text = "";
                StopLogoSpin();

                Application.Current.Resources["PCAResults"] = result;

                if (DataContext is MainWindowViewModel mainVM)
                {
                    mainVM.SetPCAEntries(result.LadEntries, result.GdbEntries);
                    mainVM.IsDataLoaded = true;
                }

                await Task.Delay(1000);

                await AnimateOpacity(MainGrid, 1.0, 0.0, 0.5);

                if (DataContext is MainWindowViewModel vm)
                {
                    vm.NavigateToPCALAD();
                }
            }
            catch (Exception ex)
            {
                _dotTimer.Stop();
                DotsText.Text = "";
                StopLogoSpin();
                StatusText.Text = "Analysis failed!";

                MessageBox.Show(
                    $"Error during analysis:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Analysis Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);

                StartTraceButton.IsEnabled = true;
                StatusPanel.Visibility = Visibility.Collapsed;
                _isAnalysisStarted = false;
            }
        }

        private void StartLogoSpin()
        {
            if (_logoSpinStoryboard != null)
                return;

            var logoImage = (Image)this.FindName("WelcomeLogoImage");
            if (logoImage == null)
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
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new PowerEase
                {
                    EasingMode = EasingMode.EaseInOut,
                    Power = 2
                }
            };

            Storyboard.SetTarget(rotationAnimation, logoImage);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(rotationAnimation);

            _logoSpinStoryboard = storyboard;
            _logoSpinStoryboard.Begin();
        }

        private void StopLogoSpin()
        {
            if (_logoSpinStoryboard == null)
                return;

            var logoImage = (Image)this.FindName("WelcomeLogoImage");
            if (logoImage == null)
                return;

            _logoSpinStoryboard.Stop();

            var resetAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (logoImage.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, resetAnimation);
            }
            _logoSpinStoryboard = null;
        }

        private void UpdateStatusText(string resourceKey)
        {
            if (Application.Current.Resources.Contains(resourceKey))
            {
                StatusText.Text = Application.Current.Resources[resourceKey] as string;
            }
            else
            {
                StatusText.Text = resourceKey;
            }
        }

        private async Task ShowStatusForMinDuration(string resourceKey, int minDurationMs)
        {
            _currentStatusStartTime = DateTime.Now;
            UpdateStatusText(resourceKey);
            await Task.Delay(minDurationMs);
        }

        private async void OnProgressChanged(string statusKey)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var elapsed = (DateTime.Now - _currentStatusStartTime).TotalMilliseconds;
                var remainingTime = Math.Max(0, 2000 - elapsed);

                if (remainingTime > 0)
                {
                    await Task.Delay((int)remainingTime);
                }

                _currentStatusStartTime = DateTime.Now;
                UpdateStatusText(statusKey);
            });
        }

        private async Task AnimateOpacity(UIElement element, double from, double to, double durationSeconds)
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

            await tcs.Task;
        }
    }
}