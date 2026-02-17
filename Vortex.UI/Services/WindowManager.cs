using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Vortex.UI.Services
{
    public class WindowManager
    {
        private readonly Window _window;
        private bool _isFirstNavigation = true;

        public WindowManager(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void OnStateChanged()
        {
            var mainBorder = _window.Content as Border;
            var closeButton = FindCloseButton();

            if (_window.WindowState == WindowState.Maximized)
            {
                if (mainBorder != null)
                {
                    mainBorder.CornerRadius = new CornerRadius(0);
                }

                UpdateCloseButtonCornerRadius(closeButton, new CornerRadius(0));
            }
            else if (_window.WindowState == WindowState.Normal)
            {
                if (mainBorder != null)
                {
                    mainBorder.CornerRadius = new CornerRadius(8);
                }

                UpdateCloseButtonCornerRadius(closeButton, new CornerRadius(0, 8, 0, 0));
            }
        }

        public void Minimize()
        {
            _window.WindowState = WindowState.Minimized;
        }

        public void ToggleMaximizeRestore()
        {
            if (_window.WindowState == WindowState.Maximized)
            {
                _window.WindowState = WindowState.Normal;
            }
            else
            {
                _window.WindowState = WindowState.Maximized;
            }
        }

        public void Close()
        {
            _window.Close();
        }

        public void HandleTitleBarMouseDown(int clickCount)
        {
            try
            {
                _window.DragMove();
            }
            catch { }
        }

        public void AnimateFrameNavigation(FrameworkElement content)
        {
            if (content == null)
                return;

            if (_isFirstNavigation)
            {
                _isFirstNavigation = false;
                content.Opacity = 1;
                return;
            }

            content.Opacity = 0;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            content.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        public void ResetNavigationState()
        {
            _isFirstNavigation = true;
        }

        private Button FindCloseButton()
        {
            return _window.FindName("CloseButton") as Button;
        }

        private void UpdateCloseButtonCornerRadius(Button closeButton, CornerRadius radius)
        {
            if (closeButton != null)
            {
                closeButton.ApplyTemplate();
                if (closeButton.Template?.FindName("CloseBorder", closeButton) is Border border)
                {
                    border.CornerRadius = radius;
                }
            }
        }
    }
}
