using USNEnhancer;

// Core parsing logic for PCA files
namespace VortexLocalPCA.Core
{
    // Manages USN journal initialization and access
    public static class USNEnhancementService
    {
        // USN enhancer instance for journal operations
        private static readonly USNEnhance _usnEnhancer = new USNEnhance();

        // Initializes USN journal parsing for all volumes
        public static void Initialize()
        {
            _usnEnhancer.InitializeJournals();
        }

        // Returns the shared USN enhancer instance
        public static USNEnhance GetEnhancer()
        {
            return _usnEnhancer;
        }
    }
}
