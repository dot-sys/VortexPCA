using System;

// Utility functions for path operations
namespace VortexLocalPCA.Util
{
    // Converts time values between UTC and local
    public static class TimeConverter
    {
        // Gets current system UTC time offset
        public static TimeSpan GetUTCOffset()
        {
            return DateTimeOffset.Now.Offset;
        }
    }
}
