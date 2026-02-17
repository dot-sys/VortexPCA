using System;
using System.Collections.Generic;

// Data models for PCA parsing results
namespace VortexLocalPCA.Models
{
    // Stores metadata for individual PCA file
    public class PcaFileInfo
    {
        // Name of the PCA file
        public string FileName { get; set; }
        // Indicates if file exists on disk
        public bool IsPresent { get; set; }
        // File creation timestamp
        public DateTime CreatedDate { get; set; }
        // File last modified timestamp
        public DateTime ModifiedDate { get; set; }
        // File last accessed timestamp
        public DateTime AccessedDate { get; set; }
        // File size in kilobytes
        public long SizeInKB { get; set; }
        // First line content from file
        public string FirstLine { get; set; }
        // Indicates successful file read operation
        public bool ReadSuccessfully { get; set; }
    }

    // Represents PCA AppLaunchDic LAD entry data
    public class LadEntry
    {
        // Full executable path from LAD
        public string FullPath { get; set; }
        // Last execution time in UTC
        public string LastExecutedTimeUtc { get; set; }
        // Last execution time in local timezone
        public string LastExecutedTimeLocal { get; set; }
        // Source PCA file path
        public string SourceFilePath { get; set; }

        // Current file existence status
        public string FileStatus { get; set; }
        // Indicates if file currently exists
        public string IsFilePresent { get; set; }
        // File creation date and time
        public DateTime? CreatedDate { get; set; }
        // File last modification date and time
        public DateTime? ModifiedDate { get; set; }
        // File last access date and time
        public DateTime? AccessedDate { get; set; }
        // Raw file size in bytes
        public long? RawFilesize { get; set; }
        // Formatted file size in bytes
        public string FilesizeInB { get; set; }
        // Formatted file size in megabytes
        public string FilesizeInMB { get; set; }
        // Digital signature verification status
        public string SignatureStatus { get; set; }
        // MD5 hash of file
        public string MD5Hash { get; set; }
        // PE compiled timestamp
        public DateTime? CompiledTime { get; set; }
        // Indicates if debugging is allowed
        public bool? IsDebugAllowed { get; set; }
        // PE entry point address
        public uint? AddressOfEntryPoint { get; set; }
        // Enhancement error message if any
        public string ErrorMessage { get; set; }

        // Indicates if found in USN journal
        public bool FoundInUSN { get; set; }
        // USN journal entries as text
        public string USNEntriesText { get; set; }

        // Display text for USN status
        public string FoundInUSNDisplay
        {
            get
            {
                if (!FoundInUSN || string.IsNullOrWhiteSpace(USNEntriesText))
                    return "";

                var words = USNEntriesText.Split(new[] { ' ', ',', '\r', '\n', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (word.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                        return "Deleted";

                    if (word.StartsWith("Rename", StringComparison.OrdinalIgnoreCase))
                        return "Renamed";
                }

                return "";
            }
        }
    }

    // Represents PCA GeneralDb GDB entry data
    public class GeneralDbEntry
    {
        // Entry timestamp in UTC
        public string TimestampUtc { get; set; }
        // Entry timestamp in local timezone
        public string TimestampLocal { get; set; }
        // Type of GDB entry
        public string EntryType { get; set; }
        // Original executable path from GDB
        public string ExecutablePath { get; set; }
        // Process name from GDB
        public string ProcessName { get; set; }
        // Publisher name from GDB
        public string Publisher { get; set; }
        // Version string from GDB
        public string Version { get; set; }
        // Program identifier from GDB
        public string ProgramId { get; set; }
        // Exit code from GDB
        public string Exitcode { get; set; }
        // Source PCA file path
        public string SourceFilePath { get; set; }
        // Resolved full executable path
        public string ResolvedPath { get; set; }
        // Path resolution status indicator
        public string FilePathStatus { get; set; }

        // Number of times path executed
        public int RunCount { get; set; }

        // Current file existence status
        public string FileStatus { get; set; }
        // Indicates if file currently exists
        public string IsFilePresent { get; set; }
        // File creation date and time
        public DateTime? CreatedDate { get; set; }
        // File last modification date and time
        public DateTime? ModifiedDate { get; set; }
        // File last access date and time
        public DateTime? AccessedDate { get; set; }
        // Raw file size in bytes
        public long? RawFilesize { get; set; }
        // Formatted file size in bytes
        public string FilesizeInB { get; set; }
        // Formatted file size in megabytes
        public string FilesizeInMB { get; set; }
        // Digital signature verification status
        public string SignatureStatus { get; set; }
        // MD5 hash of file
        public string MD5Hash { get; set; }
        // PE compiled timestamp
        public DateTime? CompiledTime { get; set; }
        // Indicates if debugging is allowed
        public bool? IsDebugAllowed { get; set; }
        // PE entry point address
        public uint? AddressOfEntryPoint { get; set; }
        // Enhancement error message if any
        public string ErrorMessage { get; set; }

        // Indicates if found in USN journal
        public bool FoundInUSN { get; set; }
        // USN journal entries as text
        public string USNEntriesText { get; set; }

        // Display text for USN status
        public string FoundInUSNDisplay
        {
            get
            {
                if (!FoundInUSN || string.IsNullOrWhiteSpace(USNEntriesText))
                    return "";

                var words = USNEntriesText.Split(new[] { ' ', ',', '\r', '\n', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (word.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                        return "Deleted";

                    if (word.StartsWith("Rename", StringComparison.OrdinalIgnoreCase))
                        return "Renamed";
                }

                return "";
            }
        }
    }

    // Contains complete PCA analysis results and data
    public class AnalysisResult
    {
        // Windows version string
        public string WindowsVersion { get; set; }
        // Indicates if version is below 22H2
        public bool IsLowerThan22H2 { get; set; }
        // Path to PCA folder location
        public string PcaFolderPath { get; set; }
        // Indicates LAD file presence
        public bool IsLADPresent { get; set; }
        // Indicates GeneralDb0 file presence
        public bool IsGDB0Present { get; set; }
        // Indicates GeneralDb1 file presence
        public bool IsGDB1Present { get; set; }
        // Metadata for AppLaunchDic file
        public PcaFileInfo PcaAppLaunchDic { get; set; }
        // Metadata for GeneralDb0 file
        public PcaFileInfo PcaGeneralDb0 { get; set; }
        // Metadata for GeneralDb1 file
        public PcaFileInfo PcaGeneralDb1 { get; set; }
        // System UTC time offset
        public TimeSpan UTCOffset { get; set; }
        // Collection of all LAD entries
        public List<LadEntry> LadEntries { get; set; }
        // Collection of all GDB entries
        public List<GeneralDbEntry> GdbEntries { get; set; }
    }
}
