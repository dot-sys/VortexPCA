using System;

// Models for USN journal data
namespace USNEnhancer.Models
{
    // Single USN journal change record
    internal class USNRecord
    {
        // Drive letter where change occurred
        public string DriveLetter { get; set; }
        // Name of affected file
        public string FileName { get; set; }
        // Type of file system change
        public USNEnhancer.USNReason Reason { get; set; }
        // Timestamp of the change event
        public DateTime Timestamp { get; set; }
    }
}
