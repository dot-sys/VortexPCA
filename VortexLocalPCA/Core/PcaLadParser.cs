using System;
using System.Collections.Generic;
using System.IO;
using VortexLocalPCA.Models;
using VortexLocalPCA.Util;

// Core parsing logic for PCA files
namespace VortexLocalPCA.Core
{
    // Parses PCA AppLaunchDic LAD file entries
    public static class PcaLadParser
    {
        // Parses LAD file and returns entry list
        public static List<LadEntry> ParseLadFile(string folderPath, string fileName, TimeSpan utcOffset)
        {
            var entries = new List<LadEntry>();
            string filePath = Path.Combine(folderPath, fileName);

            if (!FileSystemHelper.FileExists(filePath))
            {
                return entries;
            }

            string[] lines = FileSystemHelper.ReadAllLines(filePath);
            if (lines == null || lines.Length == 0)
            {
                return entries;
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 2)
                {
                    continue;
                }

                try
                {
                    DateTime utcTime = DateTime.Parse(parts[1].Trim());
                    DateTime localTime = utcTime.Add(utcOffset);
                    string fullPath = parts[0].Trim();

                    var entry = new LadEntry
                    {
                        FullPath = fullPath,
                        LastExecutedTimeUtc = utcTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        LastExecutedTimeLocal = localTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        SourceFilePath = filePath
                    };

                    entries.Add(entry);
                }
                catch (Exception)
                {
                }
            }

            return entries;
        }
    }
}
