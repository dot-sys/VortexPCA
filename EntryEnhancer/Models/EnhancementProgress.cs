// Models for entry enhancement data structures
namespace EntryEnhancer.Models
{
    // Progress tracking for bulk enhancement operations
    public class EnhancementProgress
    {
        // Total number of paths to process
        public int TotalPaths { get; set; }
        // Number of paths already processed
        public int ProcessedPaths { get; set; }
        // Currently processing path
        public string CurrentPath { get; set; }
        // Completion percentage calculated from processed paths
        public int PercentComplete => TotalPaths > 0 ? (ProcessedPaths * 100 / TotalPaths) : 0;
    }
}
