using System;
using System.Collections.Generic;
using System.IO;

// Utility functions for path operations
namespace VortexLocalPCA.Util
{
    // Provides file system access and encoding detection
    public static class FileSystemHelper
    {
        private static readonly Dictionary<string, System.Text.Encoding> _encodingCache = new Dictionary<string, System.Text.Encoding>(StringComparer.OrdinalIgnoreCase);

        // Returns Windows PCA folder location path
        public static string GetPcaFolderPath()
        {
            string sysRoot = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(sysRoot))
            {
                sysRoot = @"C:\Windows";
            }

            string pcaPath = Path.Combine(sysRoot, "appcompat", "pca");

            return pcaPath;
        }

        // Checks if file exists at given path
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        // Reads first line from file with encoding detection
        public static string ReadFirstLine(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var encoding = DetectFileEncoding(filePath);

                using (var reader = OpenFileReader(filePath, encoding))
                {
                    string firstLine = reader.ReadLine();
                    if (firstLine != null)
                    {
                        firstLine = firstLine.Trim('\0', '\uFEFF', '\u200B');
                    }
                    return firstLine;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Detects file encoding from byte order marks
        private static System.Text.Encoding DetectFileEncoding(string filePath)
        {
            if (_encodingCache.TryGetValue(filePath, out var cachedEncoding))
                return cachedEncoding;

            byte[] buffer = new byte[100];
            System.Text.Encoding encoding;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192))
            {
                int bytesRead = fs.Read(buffer, 0, Math.Min(100, (int)fs.Length));
                if (bytesRead < 2)
                {
                    encoding = System.Text.Encoding.UTF8;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    encoding = System.Text.Encoding.Unicode;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    encoding = System.Text.Encoding.BigEndianUnicode;
                }
                else if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    encoding = System.Text.Encoding.UTF8;
                }
                else
                {
                    int nullCount = 0;
                    int nonNullCount = 0;
                    for (int i = 0; i < Math.Min(100, bytesRead); i++)
                    {
                        if (buffer[i] == 0)
                            nullCount++;
                        else if (buffer[i] >= 32 && buffer[i] < 127)
                            nonNullCount++;
                    }

                    if (nullCount > 10 && nullCount > nonNullCount / 3)
                    {
                        encoding = System.Text.Encoding.Unicode;
                    }
                    else
                    {
                        encoding = System.Text.Encoding.UTF8;
                    }
                }
            }

            _encodingCache[filePath] = encoding;
            return encoding;
        }

        // Gets file metadata information object
        public static FileInfo GetFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                return new FileInfo(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Reads all lines with proper encoding detection
        public static string[] ReadAllLines(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var encoding = DetectFileEncoding(filePath);

                using (var reader = OpenFileReader(filePath, encoding))
                {
                    var lines = new System.Collections.Generic.List<string>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim('\0', '\uFEFF', '\u200B');
                        lines.Add(line);
                    }
                    return lines.ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Opens file reader with specified encoding
        private static StreamReader OpenFileReader(string filePath, System.Text.Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192);
            return new StreamReader(fs, encoding, false, 8192);
        }
    }
}
