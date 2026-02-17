using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VortexLocalPCA.Models;

namespace Vortex.UI.ViewModels
{
    public class PCALADViewModel : INotifyPropertyChanged
    {
        private List<LadEntry> _ladEntries;

        public List<LadEntry> LadEntries
        {
            get => _ladEntries;
            set
            {
                if (_ladEntries != value)
                {
                    if (value == null)
                    {
                        _ladEntries = new List<LadEntry>();
                    }
                    else
                    {
                        var sortedList = new List<LadEntry>(value);
                        ViewModelHelper.SortByTimestampDescending(sortedList, e => e.LastExecutedTimeLocal);
                        _ladEntries = sortedList;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public PCALADViewModel()
        {
            _ladEntries = new List<LadEntry>();
        }

        public PCALADViewModel(List<LadEntry> ladEntries)
        {
            if (ladEntries == null)
            {
                _ladEntries = new List<LadEntry>();
            }
            else
            {
                var sortedList = new List<LadEntry>(ladEntries);
                ViewModelHelper.SortByTimestampDescending(sortedList, e => e.LastExecutedTimeLocal);
                _ladEntries = sortedList;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
