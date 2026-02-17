using System;
using System.Collections.Generic;
using System.IO;
using VortexLocalPCA.Models;
using VortexLocalPCA.Util;

// Core parsing logic for PCA files
namespace VortexLocalPCA.Core
{
    // Parses PCA GeneralDb GDB file entries
    public static class PcaGdbParser
    {
        // Parses GDB file and returns entry list
        public static List<GeneralDbEntry> ParseGdbFile(string folderPath, string fileName, TimeSpan utcOffset)
        {
            var entries = new List<GeneralDbEntry>();
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

                if (!line.Contains("|"))
                {
                    continue;
                }

                var parts = line.Split('|');
                if (parts.Length < 8)
                {
                    continue;
                }

                if (!DateTime.TryParse(parts[0].Trim(), out DateTime utcTime))
                {
                    continue;
                }

                DateTime localTime = utcTime.Add(utcOffset);
                string executablePath = parts[2].Trim();

                var entry = new GeneralDbEntry
                {
                    TimestampUtc = utcTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimestampLocal = localTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EntryType = parts[1].Trim(),
                    ExecutablePath = executablePath,
                    ProcessName = parts[3].Trim(),
                    Publisher = parts[4].Trim(),
                    Version = parts[5].Trim(),
                    ProgramId = parts[6].Trim(),
                    Exitcode = parts.Length > 8 ? string.Join("|", parts, 7, parts.Length - 7) : parts[7].Trim(),
                    SourceFilePath = filePath
                };

                entries.Add(entry);
            }

            return entries;
        }
    }
}
