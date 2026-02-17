using System;

// Utility functions for path operations
namespace VortexLocalPCA.Util
{
    // Checks Windows version for PCA compatibility
    public static class WinVersionCheck
    {
        // Minimum build number for Windows 11 22H2
        private const int Windows11MinBuild = 22621;

        // Checks if Windows version is lower than 22H2
        public static bool IsLowerThan22H2(out string windowsVersion)
        {
            try
            {
                var osVersion = Environment.OSVersion.Version;
                windowsVersion = $"{osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";

                if (osVersion.Major < 10)
                {
                    return true;
                }

                if (osVersion.Major == 10 && osVersion.Minor == 0)
                {
                    return osVersion.Build < Windows11MinBuild;
                }

                return false;
            }
            catch (Exception)
            {
                windowsVersion = "Unknown";
                return true;
            }
        }
    }
}
