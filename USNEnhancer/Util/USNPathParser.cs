using System;
using System.IO;

// Utility functions for USN enhancement
namespace USNEnhancer.Util
{
    // Parses file paths into components
    internal static class USNPathParser
    {
        // Contains parsed path components
        public class ParsedPath
        {
            // Extracted drive letter from path
            public string DriveLetter { get; set; }
            // Extracted file name from path
            public string FileName { get; set; }
        }

        // Extracts drive letter and file name
        public static ParsedPath ParsePath(string fullPath)
        {
            var result = new ParsedPath();

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return result;
            }

            try
            {
                if (fullPath.Length >= 2 && fullPath[1] == ':')
                {
                    result.DriveLetter = fullPath.Substring(0, 1).ToUpper();
                }

                result.FileName = Path.GetFileName(fullPath);
            }
            catch
            {
            }

            return result;
        }
    }
}
