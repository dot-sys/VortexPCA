using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VortexAMC.Models;

namespace Vortex.UI.ViewModels
{
    public class AMCFileEntriesViewModel : INotifyPropertyChanged
    {
        private List<FileEntryData> _fileEntries;

        public List<FileEntryData> FileEntries
        {
            get => _fileEntries;
            set
            {
                if (_fileEntries != value)
                {
                    _fileEntries = value == null ? new List<FileEntryData>() :
                        value.OrderByDescending(fe => ParseTimestamp(fe.FileKeyLastWriteTimestamp) ?? DateTime.MinValue).ToList();
                    OnPropertyChanged();
                }
            }
        }

        public AMCFileEntriesViewModel()
        {
            _fileEntries = new List<FileEntryData>();
        }

        public AMCFileEntriesViewModel(List<FileEntryData> fileEntries)
        {
            _fileEntries = fileEntries == null ? new List<FileEntryData>() :
                fileEntries.OrderByDescending(fe => ParseTimestamp(fe.FileKeyLastWriteTimestamp) ?? DateTime.MinValue).ToList();
        }

        private static DateTime? ParseTimestamp(string ts)
        {
            if (string.IsNullOrWhiteSpace(ts))
                return null;

            if (DateTime.TryParse(ts, out var dt))
                return dt;

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}