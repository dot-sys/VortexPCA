using System;
using System.Collections.Generic;
using System.Linq;
using USNEnhancer.Core;
using USNEnhancer.Models;
using USNEnhancer.Util;

// USN journal enhancement and analysis
namespace USNEnhancer
{
    // File system change reason codes
    [Flags]
    internal enum USNReason : uint
    {
        // File was deleted
        Deleted = 0x00000200,
        // File rename old name
        RenameOld = 0x00001000,
        // File rename new name
        RenameNew = 0x00002000
    }

    // Enhancement result with USN entries
    public class USNEnhancementResult
    {
        // File found in USN journal
        public bool FoundInUSN { get; set; }
        // List of formatted USN entries
        public List<string> USNEntries { get; set; }

        // Initializes empty result with list
        public USNEnhancementResult()
        {
            USNEntries = new List<string>();
        }
    }

    // Enhances file paths with USN data
    public class USNEnhance
    {
        // Cached USN journal data
        private readonly USNDataCache _cache;

        // Initializes cache for USN enhancement
        public USNEnhance()
        {
            _cache = new USNDataCache();
        }

        // Loads all USN journals into cache
        public void InitializeJournals()
        {
            _cache.Initialize();
        }

        // Enhances single file path with USN
        public USNEnhancementResult Enhance(string fullPath)
        {
            var result = new USNEnhancementResult
            {
                FoundInUSN = false
            };

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return result;
            }

            var parsedPath = USNPathParser.ParsePath(fullPath);

            if (string.IsNullOrEmpty(parsedPath.DriveLetter) || string.IsNullOrEmpty(parsedPath.FileName))
            {
                return result;
            }

            var records = _cache.GetRecordsForFile(parsedPath.DriveLetter, parsedPath.FileName);

            if (records.Count == 0)
            {
                return result;
            }

            result.FoundInUSN = true;

            foreach (var record in records)
            {
                string reasonText = GetReasonText(record.Reason);
                string timestamp = record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                result.USNEntries.Add(string.Format("{0} ({1})", reasonText, timestamp));
            }

            return result;
        }

        // Enhances multiple file paths sequentially
        public List<USNEnhancementResult> EnhanceMultiple(IEnumerable<string> fullPaths)
        {
            return fullPaths.Select(Enhance).ToList();
        }

        // Enhances files in bulk with caching
        public Dictionary<string, USNEnhancementResult> EnhanceBulk(IEnumerable<string> fullPaths)
        {
            var results = new Dictionary<string, USNEnhancementResult>(StringComparer.OrdinalIgnoreCase);
            int foundInUSN = 0;

            foreach (var path in fullPaths)
            {
                if (!string.IsNullOrWhiteSpace(path) && !results.ContainsKey(path))
                {
                    var result = Enhance(path);
                    results[path] = result;
                    if (result.FoundInUSN)
                    {
                        foundInUSN++;
                    }
                }
            }

            return results;
        }

        // Converts reason enum to display text
        private string GetReasonText(USNReason reason)
        {
            switch (reason)
            {
                case USNReason.Deleted:
                    return "Deleted";
                case USNReason.RenameOld:
                    return "RenameOld";
                case USNReason.RenameNew:
                    return "RenameNew";
                default:
                    return "Unknown";
            }
        }
    }
}
