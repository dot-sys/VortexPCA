using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Vortex.UI.Views;
using VortexLocalPCA.Models;

namespace Vortex.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Frame _mainFrame;
        private PCALADViewModel _pcaLadViewModel;
        private PCAGDBViewModel _pcaGdbViewModel;
        private readonly List<FrameworkViewModel> _frameworkViewModels;
        private string _currentView = "Welcome";
        private bool _isDataLoaded = false;

        public bool IsDataLoaded
        {
            get => _isDataLoaded;
            set
            {
                if (_isDataLoaded != value)
                {
                    _isDataLoaded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanNavigateToPCALAD));
                    OnPropertyChanged(nameof(CanNavigateToPCAGDB));
                }
            }
        }

        public bool CanNavigateToPCALAD => IsDataLoaded;
        public bool CanNavigateToPCAGDB => IsDataLoaded;

        public MainWindowViewModel()
        {
            _frameworkViewModels = new List<FrameworkViewModel>();
        }

        public PCALADViewModel PCALADViewModel => _pcaLadViewModel;
        public PCAGDBViewModel PCAGDBViewModel => _pcaGdbViewModel;

        public IReadOnlyList<FrameworkViewModel> FrameworkViewModels => _frameworkViewModels.AsReadOnly();

        public void RegisterFrameworkViewModel(FrameworkViewModel viewModel)
        {
            if (viewModel != null && !_frameworkViewModels.Contains(viewModel))
            {
                _frameworkViewModels.Add(viewModel);
            }
        }

        public FrameworkViewModel GetFrameworkViewModel(string name)
        {
            return _frameworkViewModels.FirstOrDefault(vm => vm.Name == name);
        }

        public void ClearFrameworkViewModels()
        {
            _frameworkViewModels.Clear();
        }

        public void SetFrame(Frame frame)
        {
            _mainFrame = frame;
            NavigateToWelcome();
        }

        public void NavigateToWelcome()
        {
            if (_mainFrame != null)
            {
                var welcomeView = new WelcomeView
                {
                    DataContext = this
                };
                _mainFrame.Navigate(welcomeView);
                _currentView = "Welcome";
            }
        }

        public void NavigateToPCALAD()
        {
            if (_mainFrame != null)
            {
                if (!IsDataLoaded || _pcaLadViewModel == null)
                {
                    NavigateToWelcome();
                    return;
                }

                var pcaLadView = new PCALADView
                {
                    DataContext = _pcaLadViewModel
                };
                _mainFrame.Navigate(pcaLadView);
                _currentView = "PCALAD";
            }
        }

        public void NavigateToPCAGDB()
        {
            if (_mainFrame != null)
            {
                if (!IsDataLoaded || _pcaGdbViewModel == null)
                {
                    NavigateToWelcome();
                    return;
                }

                var pcaGdbView = new PCAGDBView
                {
                    DataContext = _pcaGdbViewModel
                };
                _mainFrame.Navigate(pcaGdbView);
                _currentView = "PCAGDB";
            }
        }

        public void SetPCAEntries(List<LadEntry> ladEntries, List<GeneralDbEntry> gdbEntries)
        {
            _pcaLadViewModel = new PCALADViewModel(ladEntries);
            _pcaGdbViewModel = new PCAGDBViewModel(gdbEntries);
        }

        public virtual void StartAllTraces()
        {
        }

        public void RefreshCurrentView()
        {
            IsDataLoaded = false;

            if (_pcaLadViewModel != null)
            {
                _pcaLadViewModel = null;
            }

            if (_pcaGdbViewModel != null)
            {
                _pcaGdbViewModel = null;
            }

            ClearFrameworkViewModels();

            NavigateToWelcome();
        }

        public void ReloadCurrentView()
        {
            switch (_currentView)
            {
                case "Welcome":
                    NavigateToWelcome();
                    break;
                case "PCALAD":
                    NavigateToPCALAD();
                    break;
                case "PCAGDB":
                    NavigateToPCAGDB();
                    break;
                default:
                    NavigateToWelcome();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}