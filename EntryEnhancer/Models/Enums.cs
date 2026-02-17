// Models for entry enhancement data structures
namespace EntryEnhancer.Models
{
    // File existence and deletion state
    public enum FileStatus
    {
        Unknown,
        Deleted,
        Present
    }

    // File presence validation result
    public enum FilePresentStatus
    {
        Error,
        False,
        True
    }

    // Digital signature verification result
    public enum SignatureStatus
    {
        Unknown,
        Unsigned,
        Signed
    }
}
