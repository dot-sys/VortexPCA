using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VortexLocalPCA.Models;

namespace Vortex.UI.ViewModels
{
    public class PCAGDBViewModel : INotifyPropertyChanged
    {
        private List<GeneralDbEntry> _pcaEntries;

        public List<GeneralDbEntry> PcaEntries
        {
            get => _pcaEntries;
            set
            {
                if (_pcaEntries != value)
                {
                    if (value == null)
                    {
                        _pcaEntries = new List<GeneralDbEntry>();
                    }
                    else
                    {
                        var sortedList = new List<GeneralDbEntry>(value);
                        ViewModelHelper.SortByTimestampDescending(sortedList, e => e.TimestampLocal);
                        _pcaEntries = sortedList;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public PCAGDBViewModel()
        {
            _pcaEntries = new List<GeneralDbEntry>();
        }

        public PCAGDBViewModel(List<GeneralDbEntry> pcaEntries)
        {
            if (pcaEntries == null)
            {
                _pcaEntries = new List<GeneralDbEntry>();
            }
            else
            {
                var sortedList = new List<GeneralDbEntry>(pcaEntries);
                ViewModelHelper.SortByTimestampDescending(sortedList, e => e.TimestampLocal);
                _pcaEntries = sortedList;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
