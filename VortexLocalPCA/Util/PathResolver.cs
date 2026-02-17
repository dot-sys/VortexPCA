using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VortexLocalPCA.Models;

// Utility functions for path operations
namespace VortexLocalPCA.Util
{
    // Resolves and normalizes executable paths from PCA data
    public static class PathResolver
    {
        // Resolves paths and calculates run counts for entries
        public static void ResolveGdbPaths(List<GeneralDbEntry> gdbEntries, List<LadEntry> ladEntries)
        {
            if (gdbEntries == null || gdbEntries.Count == 0)
            {
                return;
            }

            var availableDrives = GetAvailableDriveLetters();

            foreach (var gdbEntry in gdbEntries)
            {
                ResolvePathForEntry(gdbEntry, ladEntries, availableDrives);
            }

            var runCounts = gdbEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.ResolvedPath))
                .GroupBy(e => e.ResolvedPath, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);


            foreach (var gdbEntry in gdbEntries)
            {
                if (!string.IsNullOrWhiteSpace(gdbEntry.ResolvedPath) && runCounts.TryGetValue(gdbEntry.ResolvedPath, out var count))
                {
                    gdbEntry.RunCount = count;
                }
                else
                {
                    gdbEntry.RunCount = 1;
                }
            }

        }

        // Resolves a single entry path using LAD data
        private static void ResolvePathForEntry(GeneralDbEntry gdbEntry, List<LadEntry> ladEntries, List<string> availableDrives)
        {
            string originalPath = gdbEntry.ExecutablePath;

            if (string.IsNullOrWhiteSpace(originalPath))
            {
                gdbEntry.ResolvedPath = originalPath;
                gdbEntry.FilePathStatus = "Empty";
                return;
            }

            string expandedPath = originalPath;

            bool wasExpanded = false;
            if (originalPath.Contains("%"))
            {
                try
                {
                    expandedPath = Environment.ExpandEnvironmentVariables(originalPath);
                    wasExpanded = !originalPath.Equals(expandedPath, StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception)
                {
                }
            }

            bool fileExists = File.Exists(expandedPath);
            bool isValidAbsolutePath = IsValidAbsolutePath(expandedPath);

            if (fileExists && isValidAbsolutePath)
            {
                gdbEntry.ResolvedPath = expandedPath;
                gdbEntry.FilePathStatus = "Resolved";
                return;
            }

            if (wasExpanded && isValidAbsolutePath)
            {
                gdbEntry.ResolvedPath = expandedPath;
                gdbEntry.FilePathStatus = "Resolved";
                return;
            }

            if (ladEntries == null || ladEntries.Count == 0)
            {
                gdbEntry.ResolvedPath = expandedPath;
                gdbEntry.FilePathStatus = isValidAbsolutePath ? "Resolved" : "Unknown";
                return;
            }

            string pathWithoutDrive = GetPathWithoutDrive(expandedPath);

            if (string.IsNullOrWhiteSpace(pathWithoutDrive))
            {
                gdbEntry.ResolvedPath = expandedPath;
                gdbEntry.FilePathStatus = isValidAbsolutePath ? "Resolved" : "Unknown";
                return;
            }

            var matchingLadEntries = ladEntries
                .Where(lad => !string.IsNullOrWhiteSpace(lad.FullPath))
                .Where(lad => GetPathWithoutDrive(lad.FullPath).Equals(pathWithoutDrive, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingLadEntries.Count == 0)
            {
                string resolvedByDrive = TryResolvingWithAvailableDrives(expandedPath, availableDrives);
                if (resolvedByDrive != null)
                {
                    gdbEntry.ResolvedPath = resolvedByDrive;
                    gdbEntry.FilePathStatus = "Resolved";
                }
                else
                {
                    gdbEntry.ResolvedPath = expandedPath;
                    gdbEntry.FilePathStatus = "Unknown";
                }
            }
            else if (matchingLadEntries.Count == 1)
            {
                gdbEntry.ResolvedPath = matchingLadEntries[0].FullPath;
                gdbEntry.FilePathStatus = "Resolved";

                TryResolveMissingFile(gdbEntry, expandedPath, availableDrives);
            }
            else
            {
                var uniqueDriveLetters = matchingLadEntries
                    .Select(lad => GetDriveLetter(lad.FullPath))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (uniqueDriveLetters.Count > 1)
                {
                    gdbEntry.ResolvedPath = expandedPath;
                    gdbEntry.FilePathStatus = "Duplicate";

                    if (expandedPath.StartsWith(@"\"))
                    {
                        string resolvedByDrive = TryResolvingWithAvailableDrives(expandedPath, availableDrives);
                        if (resolvedByDrive != null)
                        {
                            gdbEntry.ResolvedPath = resolvedByDrive;
                            gdbEntry.FilePathStatus = "Resolved";
                        }
                    }
                }
                else
                {
                    gdbEntry.ResolvedPath = matchingLadEntries[0].FullPath;
                    gdbEntry.FilePathStatus = "Resolved";

                    TryResolveMissingFile(gdbEntry, expandedPath, availableDrives);
                }
            }
        }

        // Attempts to resolve missing file with drive letters
        private static void TryResolveMissingFile(GeneralDbEntry entry, string originalPath, List<string> availableDrives)
        {
            if (!File.Exists(entry.ResolvedPath) && originalPath.StartsWith(@"\"))
            {
                string resolvedByDrive = TryResolvingWithAvailableDrives(originalPath, availableDrives);
                if (resolvedByDrive != null)
                {
                    entry.ResolvedPath = resolvedByDrive;
                }
            }
        }

        // Extracts path without drive letter prefix
        private static string GetPathWithoutDrive(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (path.Length >= 2 && path[1] == ':')
            {
                return path.Substring(2);
            }

            if (path.StartsWith(@"\\"))
            {
                return path;
            }

            return path;
        }

        // Extracts drive letter from path string
        private static string GetDriveLetter(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 2)
                return string.Empty;

            if (path[1] == ':')
            {
                return path.Substring(0, 2).ToUpperInvariant();
            }

            return string.Empty;
        }

        // Validates if path is absolute with drive
        private static bool IsValidAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.Length >= 3 && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
            {
                char driveLetter = char.ToUpperInvariant(path[0]);
                return driveLetter >= 'A' && driveLetter <= 'Z';
            }

            if (path.StartsWith(@"\\") && path.Length > 2)
            {
                return true;
            }

            return false;
        }

        // Gets all available system drive letters
        private static List<string> GetAvailableDriveLetters()
        {
            var driveLetters = new List<string>();
            try
            {
                var drives = DriveInfo.GetDrives();
                var availableDriveSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.DriveType != DriveType.CDRom)
                    {
                        availableDriveSet.Add(drive.Name.Substring(0, 2).ToUpperInvariant());
                    }
                }

                // Return drives in order: C, D, E, ..., Z, then A, B
                // Check C-Z first
                for (char c = 'C'; c <= 'Z'; c++)
                {
                    string driveLetter = c + ":";
                    if (availableDriveSet.Contains(driveLetter))
                    {
                        driveLetters.Add(driveLetter);
                    }
                }

                for (char c = 'A'; c <= 'B'; c++)
                {
                    string driveLetter = c + ":";
                    if (availableDriveSet.Contains(driveLetter))
                    {
                        driveLetters.Add(driveLetter);
                    }
                }
            }
            catch (Exception)
            {
            }
            return driveLetters;
        }

        // Tests path with all available drive letters
        private static string TryResolvingWithAvailableDrives(string path, List<string> availableDrives)
        {
            if (string.IsNullOrWhiteSpace(path) || availableDrives == null || availableDrives.Count == 0)
                return null;

            if (path.Length >= 2 && path[1] == ':')
                return null;

            if (!path.StartsWith(@"\"))
                return null;

            foreach (var driveLetter in availableDrives)
            {
                string fullPath = driveLetter + path;

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }
    }
}
