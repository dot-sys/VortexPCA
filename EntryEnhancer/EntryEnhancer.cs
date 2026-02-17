using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EntryEnhancer.Models;

// File metadata extraction and enhancement
namespace EntryEnhancer
{
    // Extracts and enriches file metadata with hashes
    public class EntryEnhancer
    {
        // Validates Windows file path format
        private static readonly Regex WindowsPathRegex = new Regex(
            @"(?:""?[a-zA-Z]\:|\\\\[^\\\/\:\*\?\<\>\|]+\\[^\\\/\:\*\?\<\>\|]*)\\(?:[^\\\/\:\*\?\<\>\|]+\\)*\w([^\\\/\:\*\?\<\>\|])*",
            RegexOptions.Compiled);

        // German culture for number formatting
        private static readonly CultureInfo GermanCulture = new CultureInfo("de-DE");

        // Minimum valid PE file size
        private const int MinPEFileSize = 64;
        // PE header read buffer size
        private const int PEHeaderBufferSize = 1024;
        // Small file size threshold bytes
        private const int SmallFileThreshold = 1048576;
        // Large file read buffer size
        private const int LargeFileBufferSize = 81920;
        // Progress report update interval
        private const int ProgressReportInterval = 50;
        // PE signature magic number
        private const uint PESignature = 0x00004550;
        // PE32 optional header magic
        private const ushort PE32Magic = 0x10b;
        // PE32 plus optional header magic
        private const ushort PE32PlusMagic = 0x20b;

        public EnhancedEntry Enhance(string fullPath)
        {
            return Enhance(fullPath, computeMD5: true, checkSignature: true);
        }

        public EnhancedEntry Enhance(string fullPath, bool computeMD5 = true, bool checkSignature = true)
        {
            var result = new EnhancedEntry
            {
                OriginalPath = fullPath,
                FileStatus = FileStatus.Unknown,
                IsFilePresent = FilePresentStatus.Error
            };

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return result;
            }

            if (!WindowsPathRegex.IsMatch(fullPath))
            {
                return result;
            }

            if (!File.Exists(fullPath))
            {
                result.IsFilePresent = FilePresentStatus.False;
                result.FileStatus = FileStatus.Deleted;
                return result;
            }

            result.IsFilePresent = FilePresentStatus.True;
            result.FileStatus = FileStatus.Present;

            try
            {
                ExtractFileMetadata(fullPath, result);

                ExtractAllPEInformation(fullPath, result, computeMD5, checkSignature);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        // Enhances multiple files sequentially
        public List<EnhancedEntry> EnhanceMultiple(IEnumerable<string> fullPaths)
        {
            return fullPaths.Select(Enhance).ToList();
        }

        // Enhances files in parallel with progress
        public Dictionary<string, EnhancedEntry> EnhanceBulk(IEnumerable<string> fullPaths, bool computeMD5 = false, bool checkSignature = true, IProgress<EnhancementProgress> progress = null)
        {
            var uniquePaths = new HashSet<string>(fullPaths.Where(p => !string.IsNullOrWhiteSpace(p)), StringComparer.OrdinalIgnoreCase);
            var results = new ConcurrentDictionary<string, EnhancedEntry>(StringComparer.OrdinalIgnoreCase);

            int totalPaths = uniquePaths.Count;
            int processedCount = 0;
            int lastReportedCount = 0;
            var lockObj = new object();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount / 2)
            };

            Parallel.ForEach(uniquePaths, parallelOptions, path =>
            {
                var enhanced = Enhance(path, computeMD5, checkSignature);
                results[path] = enhanced;

                if (progress != null)
                {
                    ReportProgress(progress, ref processedCount, ref lastReportedCount, totalPaths, path, lockObj);
                }
                else
                {
                    lock (lockObj)
                    {
                        processedCount++;
                    }
                }
            });

            return new Dictionary<string, EnhancedEntry>(results, StringComparer.OrdinalIgnoreCase);
        }

        // Reports progress at specific intervals
        private void ReportProgress(IProgress<EnhancementProgress> progress, ref int processedCount, ref int lastReportedCount, int totalPaths, string currentPath, object lockObj)
        {
            int currentCount;
            bool shouldReport = false;

            lock (lockObj)
            {
                processedCount++;
                currentCount = processedCount;

                if (currentCount - lastReportedCount >= ProgressReportInterval || currentCount == totalPaths)
                {
                    shouldReport = true;
                    lastReportedCount = currentCount;
                }
            }

            if (shouldReport)
            {
                progress.Report(new EnhancementProgress
                {
                    TotalPaths = totalPaths,
                    ProcessedPaths = currentCount,
                    CurrentPath = currentPath
                });
            }
        }

        // Enhances collection with custom path extractor
        public void EnhanceCollection<TEntry>(
            IEnumerable<TEntry> entries,
            Func<TEntry, string> pathExtractor,
            Action<TEntry, EnhancedEntry> enhancementApplier,
            bool computeMD5 = true,
            bool checkSignature = true,
            IProgress<EnhancementProgress> progress = null)
        {
            if (entries == null) return;

            var entriesList = entries.ToList();
            var paths = entriesList.Select(pathExtractor).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            var enhancementCache = EnhanceBulk(paths, computeMD5, checkSignature, progress);
            foreach (var entry in entriesList)
            {
                var path = pathExtractor(entry);
                if (!string.IsNullOrWhiteSpace(path) && enhancementCache.TryGetValue(path, out var enhanced))
                {
                    enhancementApplier(entry, enhanced);
                }
            }
        }

        // Extracts basic file system metadata
        private void ExtractFileMetadata(string fullPath, EnhancedEntry result)
        {
            var fileInfo = new FileInfo(fullPath);
            result.CreatedDate = fileInfo.CreationTimeUtc;
            result.ModifiedDate = fileInfo.LastWriteTimeUtc;
            result.AccessedDate = fileInfo.LastAccessTimeUtc;
            result.RawFilesize = fileInfo.Length;
            result.FilesizeInB = FormatBytesWithDelimiter(fileInfo.Length) + " B";
            result.FilesizeInMB = FormatFilesizeInMB(fileInfo.Length);
        }

        // Formats file size in megabytes
        private string FormatFilesizeInMB(long bytes)
        {
            double megabytes = bytes / (1024.0 * 1024.0);
            if (megabytes < 1.0)
            {
                return megabytes.ToString("0.00", GermanCulture) + " MB";
            }

            long megabytesWhole = bytes / (1024 * 1024);
            return FormatBytesWithDelimiter(megabytesWhole) + " MB";
        }

        // Formats number with German delimiter style
        private string FormatBytesWithDelimiter(long value)
        {
            return value.ToString("N0", GermanCulture).Replace(",", ".");
        }

        // Extracts PE header and computes hash
        private void ExtractAllPEInformation(string fullPath, EnhancedEntry result, bool computeMD5, bool checkSignature)
        {
            try
            {
                if (!computeMD5)
                {
                    ExtractPEHeaderOnly(fullPath, result, checkSignature);
                    return;
                }

                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, LargeFileBufferSize, FileOptions.SequentialScan))
                using (var md5 = MD5.Create())
                {
                    if (stream.Length < MinPEFileSize)
                    {
                        result.MD5Hash = ComputeHashString(md5.ComputeHash(stream));
                        return;
                    }

                    if (stream.Length <= SmallFileThreshold)
                    {
                        ProcessSmallFile(stream, md5, result, checkSignature);
                    }
                    else
                    {
                        ProcessLargeFile(stream, md5, result, checkSignature);
                    }
                }
            }
            catch
            {
            }
        }

        // Extracts PE header without hash calculation
        private void ExtractPEHeaderOnly(string fullPath, EnhancedEntry result, bool checkSignature)
        {
            var headerBuffer = new byte[PEHeaderBufferSize];

            using (var stream = File.OpenRead(fullPath))
            {
                if (stream.Length < MinPEFileSize)
                    return;

                int bytesRead = stream.Read(headerBuffer, 0, (int)Math.Min(PEHeaderBufferSize, stream.Length));
                if (bytesRead >= MinPEFileSize)
                {
                    ParsePEHeaderFromBuffer(headerBuffer, result, checkSignature);
                }
            }
        }

        // Processes small files in single read
        private void ProcessSmallFile(FileStream stream, MD5 md5, EnhancedEntry result, bool checkSignature)
        {
            var entireFile = new byte[stream.Length];
            stream.Read(entireFile, 0, entireFile.Length);
            result.MD5Hash = ComputeHashString(md5.ComputeHash(entireFile));

            if (entireFile.Length >= MinPEFileSize)
            {
                var headerBuffer = new byte[Math.Min(PEHeaderBufferSize, entireFile.Length)];
                Buffer.BlockCopy(entireFile, 0, headerBuffer, 0, headerBuffer.Length);
                ParsePEHeaderFromBuffer(headerBuffer, result, checkSignature);
            }
        }

        // Processes large files with buffered reads
        private void ProcessLargeFile(FileStream stream, MD5 md5, EnhancedEntry result, bool checkSignature)
        {
            var buffer = new byte[LargeFileBufferSize];
            var headerBuffer = new byte[Math.Min(PEHeaderBufferSize, stream.Length)];
            var headerRead = stream.Read(headerBuffer, 0, headerBuffer.Length);

            md5.TransformBlock(headerBuffer, 0, headerRead, null, 0);

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytesRead == buffer.Length)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                }
                else
                {
                    md5.TransformFinalBlock(buffer, 0, bytesRead);
                }
            }

            if (stream.Position == headerRead)
            {
                md5.TransformFinalBlock(new byte[0], 0, 0);
            }

            result.MD5Hash = ComputeHashString(md5.Hash);

            if (headerBuffer.Length >= MinPEFileSize)
            {
                ParsePEHeaderFromBuffer(headerBuffer, result, checkSignature);
            }
        }

        // Converts hash bytes to hex string
        private string ComputeHashString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // Parses PE header from byte buffer
        private void ParsePEHeaderFromBuffer(byte[] buffer, EnhancedEntry result, bool checkSignature)
        {
            try
            {
                if (buffer.Length < MinPEFileSize)
                    return;

                int peHeaderOffset = BitConverter.ToInt32(buffer, 0x3C);
                if (peHeaderOffset <= 0 || peHeaderOffset + 256 >= buffer.Length)
                    return;

                uint peSignature = BitConverter.ToUInt32(buffer, peHeaderOffset);
                if (peSignature != PESignature)
                    return;

                int offset = peHeaderOffset + 4;
                offset += 4;

                uint timeDateStamp = BitConverter.ToUInt32(buffer, offset);
                result.CompiledTime = DateTimeOffset.FromUnixTimeSeconds(timeDateStamp).UtcDateTime;
                offset += 4;

                offset += 8;
                ushort optionalHeaderSize = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                offset += 2;

                if (optionalHeaderSize == 0)
                    return;

                ushort magic = BitConverter.ToUInt16(buffer, offset);
                bool is64Bit = magic == PE32PlusMagic;
                offset += 2;

                if (magic != PE32Magic && magic != PE32PlusMagic)
                    return;

                // Skip: MajorLinkerVersion(1) + MinorLinkerVersion(1) + SizeOfCode(4) + SizeOfInitializedData(4) + SizeOfUninitializedData(4) = 14 bytes
                offset += 2;  // MajorLinkerVersion + MinorLinkerVersion
                offset += 4;  // SizeOfCode
                offset += 4;  // SizeOfInitializedData
                offset += 4;  // SizeOfUninitializedData
                result.AddressOfEntryPoint = BitConverter.ToUInt32(buffer, offset);
                offset += 4;

                offset += 4;
                if (!is64Bit)
                    offset += 4;

                offset += 8 + 4 + 4 + 2 + 2 + 2 + 2 + 2 + 2 + 4 + 4 + 4 + 4 + 2 + 2;

                if (offset + 4 >= buffer.Length)
                    return;

                uint numberOfRvaAndSizes = BitConverter.ToUInt32(buffer, offset);
                offset += 4;

                if (checkSignature)
                {
                    ExtractSignatureStatus(buffer, offset, numberOfRvaAndSizes, result);
                }
                else
                {
                    result.SignatureStatus = SignatureStatus.Unknown;
                }

                ExtractDebugDirectoryStatus(buffer, offset, numberOfRvaAndSizes, result);
            }
            catch
            {
            }
        }

        // Extracts signature status from PE header
        private void ExtractSignatureStatus(byte[] buffer, int offset, uint numberOfRvaAndSizes, EnhancedEntry result)
        {
            if (numberOfRvaAndSizes >= 5)
            {
                int certTableOffset = offset + (4 * 8);
                if (certTableOffset + 8 <= buffer.Length)
                {
                    uint certificateTableRva = BitConverter.ToUInt32(buffer, certTableOffset);
                    uint certificateTableSize = BitConverter.ToUInt32(buffer, certTableOffset + 4);
                    result.SignatureStatus = (certificateTableRva != 0 && certificateTableSize != 0)
                        ? SignatureStatus.Signed
                        : SignatureStatus.Unsigned;
                }
            }
        }

        // Extracts debug directory presence from PE
        private void ExtractDebugDirectoryStatus(byte[] buffer, int offset, uint numberOfRvaAndSizes, EnhancedEntry result)
        {
            if (numberOfRvaAndSizes >= 7)
            {
                int debugDirOffset = offset + (6 * 8);
                if (debugDirOffset + 8 <= buffer.Length)
                {
                    uint debugDirRva = BitConverter.ToUInt32(buffer, debugDirOffset);
                    uint debugDirSize = BitConverter.ToUInt32(buffer, debugDirOffset + 4);
                    result.IsDebugAllowed = debugDirRva != 0 && debugDirSize != 0;
                }
            }
        }
    }
}
