using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vortex.UI.ViewModels
{
    public abstract class FrameworkViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;
        private string _name;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        protected FrameworkViewModel(string name)
        {
            Name = name;
            IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
