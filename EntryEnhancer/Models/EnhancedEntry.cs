using System;

// Models for entry enhancement data structures
namespace EntryEnhancer.Models
{
    // Complete file metadata and analysis result
    public class EnhancedEntry
    {
        // Full file path as provided
        public string OriginalPath { get; set; }
        // File existence status indicator
        public FileStatus FileStatus { get; set; }
        // File presence check result
        public FilePresentStatus IsFilePresent { get; set; }
        // File creation timestamp in UTC
        public DateTime? CreatedDate { get; set; }
        // Last modification timestamp in UTC
        public DateTime? ModifiedDate { get; set; }
        // Last access timestamp in UTC
        public DateTime? AccessedDate { get; set; }
        // File size in bytes
        public long? RawFilesize { get; set; }
        // Formatted size in bytes with delimiter
        public string FilesizeInB { get; set; }
        // Formatted size in megabytes
        public string FilesizeInMB { get; set; }
        // Digital signature verification status
        public SignatureStatus SignatureStatus { get; set; }
        // MD5 hash of file content
        public string MD5Hash { get; set; }
        // PE header compilation timestamp
        public DateTime? CompiledTime { get; set; }
        // PE debug directory presence flag
        public bool? IsDebugAllowed { get; set; }
        // PE entry point address
        public uint? AddressOfEntryPoint { get; set; }
        // Error message if processing failed
        public string ErrorMessage { get; set; }
    }
}
