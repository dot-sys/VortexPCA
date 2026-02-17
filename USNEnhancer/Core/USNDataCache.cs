using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using USNEnhancer.Models;

// Core USN journal reading and caching
namespace USNEnhancer.Core
{
    // Caches USN journal records per drive
    internal class USNDataCache
    {
        // Stores records grouped by drive letter
        private readonly Dictionary<string, List<USNRecord>> _recordsByDrive;
        // Tracks drive journal availability status
        private readonly Dictionary<string, string> _driveStatus;
        // Thread synchronization lock object
        private readonly object _lock = new object();
        // Cache initialization completion flag
        private bool _isInitialized;

        // Initializes empty cache dictionaries
        public USNDataCache()
        {
            _recordsByDrive = new Dictionary<string, List<USNRecord>>(StringComparer.OrdinalIgnoreCase);
            _driveStatus = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        // Reads USN journals from all fixed drives
        public void Initialize()
        {
            lock (_lock)
            {
                if (_isInitialized)
                {
                    return;
                }

                var reader = new USNJournalReader();
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                    .Select(d => d.Name.Substring(0, 1))
                    .ToList();

                foreach (var driveLetter in drives)
                {
                    try
                    {
                        if (reader.HasUSNJournal(driveLetter))
                        {
                            var records = reader.ReadUSNJournal(driveLetter);
                            _recordsByDrive[driveLetter] = records;
                            _driveStatus[driveLetter] = "USN Journal Available";
                        }
                        else
                        {
                            _driveStatus[driveLetter] = $"No USN Journal on Drive {driveLetter}:";
                        }
                    }
                    catch (Exception)
                    {
                        _driveStatus[driveLetter] = $"No USN Journal on Drive {driveLetter}:";
                    }
                }

                _isInitialized = true;
            }
        }

        // Retrieves USN records for specific file
        public List<USNRecord> GetRecordsForFile(string driveLetter, string fileName)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    Initialize();
                }

                if (!_recordsByDrive.TryGetValue(driveLetter, out List<USNRecord> driveRecords))
                {
                    return new List<USNRecord>();
                }

                return driveRecords
                    .Where(r => string.Equals(r.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Timestamp)
                    .ToList();
            }
        }

        // Gets drive USN journal availability status
        public string GetDriveStatus(string driveLetter)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    Initialize();
                }

                if (_driveStatus.TryGetValue(driveLetter, out string status))
                {
                    return status;
                }

                return $"No USN Journal on Drive {driveLetter}:";
            }
        }
    }
}
