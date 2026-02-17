using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntryEnhancer.Models;
using VortexLocalPCA.Models;

// Core parsing logic for PCA files
namespace VortexLocalPCA.Core
{
    // Enhances entries with file metadata and signatures
    public static class EntryEnhancementService
    {
        // Entry enhancer instance for metadata extraction
        private static readonly EntryEnhancer.EntryEnhancer _enhancer = new EntryEnhancer.EntryEnhancer();

        // Enhances LAD and GDB entries with metadata
        public static void EnhanceAllEntries(List<LadEntry> ladEntries, List<GeneralDbEntry> gdbEntries)
        {
            var allPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (ladEntries != null)
            {
                foreach (var entry in ladEntries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.FullPath))
                        allPaths.Add(entry.FullPath);
                }
            }

            if (gdbEntries != null)
            {
                foreach (var entry in gdbEntries)
                {
                    var path = !string.IsNullOrWhiteSpace(entry.ResolvedPath) ? entry.ResolvedPath : entry.ExecutablePath;
                    if (!string.IsNullOrWhiteSpace(path))
                        allPaths.Add(path);
                }
            }

            var progress = new Progress<EnhancementProgress>(p => { });

            var enhancementCache = _enhancer.EnhanceBulk(allPaths, computeMD5: true, checkSignature: true, progress: progress);

            if (ladEntries != null)
            {
                foreach (var entry in ladEntries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.FullPath) && enhancementCache.TryGetValue(entry.FullPath, out var enhanced))
                    {
                        ApplyToLadEntry(entry, enhanced);
                    }
                }
            }

            if (gdbEntries != null)
            {
                foreach (var entry in gdbEntries)
                {
                    var path = !string.IsNullOrWhiteSpace(entry.ResolvedPath) ? entry.ResolvedPath : entry.ExecutablePath;
                    if (!string.IsNullOrWhiteSpace(path) && enhancementCache.TryGetValue(path, out var enhanced))
                    {
                        ApplyToGdbEntry(entry, enhanced);
                    }
                }
            }

            var usnEnhancer = USNEnhancementService.GetEnhancer();
            var usnCache = usnEnhancer.EnhanceBulk(allPaths);

            if (ladEntries != null)
            {
                foreach (var entry in ladEntries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.FullPath) && usnCache.TryGetValue(entry.FullPath, out var usnResult))
                    {
                        ApplyUSNToEntry(entry, usnResult);
                    }
                }
            }

            if (gdbEntries != null)
            {
                foreach (var entry in gdbEntries)
                {
                    var path = !string.IsNullOrWhiteSpace(entry.ResolvedPath) ? entry.ResolvedPath : entry.ExecutablePath;
                    if (!string.IsNullOrWhiteSpace(path) && usnCache.TryGetValue(path, out var usnResult))
                    {
                        ApplyUSNToEntry(entry, usnResult);
                    }
                }
            }
        }

        // Applies enhancement data to LAD entry object
        private static void ApplyToLadEntry(LadEntry entry, EnhancedEntry enhanced)
        {
            entry.FileStatus = enhanced.FileStatus.ToString();
            entry.IsFilePresent = enhanced.IsFilePresent.ToString();
            entry.CreatedDate = enhanced.CreatedDate;
            entry.ModifiedDate = enhanced.ModifiedDate;
            entry.AccessedDate = enhanced.AccessedDate;
            entry.RawFilesize = enhanced.RawFilesize;
            entry.FilesizeInB = enhanced.FilesizeInB;
            entry.FilesizeInMB = enhanced.FilesizeInMB;
            entry.SignatureStatus = enhanced.SignatureStatus.ToString();
            entry.MD5Hash = enhanced.MD5Hash;
            entry.CompiledTime = enhanced.CompiledTime;
            entry.IsDebugAllowed = enhanced.IsDebugAllowed;
            entry.AddressOfEntryPoint = enhanced.AddressOfEntryPoint;
            entry.ErrorMessage = enhanced.ErrorMessage;
        }

        // Applies enhancement data to GDB entry object
        private static void ApplyToGdbEntry(GeneralDbEntry entry, EnhancedEntry enhanced)
        {
            entry.FileStatus = enhanced.FileStatus.ToString();
            entry.IsFilePresent = enhanced.IsFilePresent.ToString();
            entry.CreatedDate = enhanced.CreatedDate;
            entry.ModifiedDate = enhanced.ModifiedDate;
            entry.AccessedDate = enhanced.AccessedDate;
            entry.RawFilesize = enhanced.RawFilesize;
            entry.FilesizeInB = enhanced.FilesizeInB;
            entry.FilesizeInMB = enhanced.FilesizeInMB;
            entry.SignatureStatus = enhanced.SignatureStatus.ToString();
            entry.MD5Hash = enhanced.MD5Hash;
            entry.CompiledTime = enhanced.CompiledTime;
            entry.IsDebugAllowed = enhanced.IsDebugAllowed;
            entry.AddressOfEntryPoint = enhanced.AddressOfEntryPoint;
            entry.ErrorMessage = enhanced.ErrorMessage;
        }

        // Applies USN journal data to entry object
        private static void ApplyUSNToEntry(dynamic entry, USNEnhancer.USNEnhancementResult usnResult)
        {
            entry.FoundInUSN = usnResult.FoundInUSN;
            if (usnResult.FoundInUSN && usnResult.USNEntries != null && usnResult.USNEntries.Count > 0)
            {
                entry.USNEntriesText = string.Join("; ", usnResult.USNEntries);
            }
        }
    }
}
