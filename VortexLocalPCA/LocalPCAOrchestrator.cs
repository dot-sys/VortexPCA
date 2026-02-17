using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VortexLocalPCA.Core;
using VortexLocalPCA.Models;
using VortexLocalPCA.Util;

// Main orchestration logic for PCA analysis
namespace VortexLocalPCA
{
    // Coordinates PCA file parsing and enhancement workflow
    public class LocalPCAOrchestrator
    {
        // Event fired when analysis progress changes
        public event Action<string> ProgressChanged;

        // Starts complete PCA analysis and enhancement process
        public AnalysisResult StartAnalysis()
        {
            USNEnhancementService.Initialize();

            var utcOffset = TimeConverter.GetUTCOffset();

            var result = new AnalysisResult
            {
                UTCOffset = utcOffset
            };

            result.IsLowerThan22H2 = WinVersionCheck.IsLowerThan22H2(out string windowsVersion);
            result.WindowsVersion = windowsVersion;

            if (result.IsLowerThan22H2)
            {
                return result;
            }

            result.PcaFolderPath = FileSystemHelper.GetPcaFolderPath();

            ProgressChanged?.Invoke("ParsingLocalPCAEntries");

            PcaFileParser.AnalyzePcaFiles(result.PcaFolderPath, result);

            if (result.IsLADPresent)
            {
                result.LadEntries = PcaLadParser.ParseLadFile(result.PcaFolderPath, "PcaAppLaunchDic.txt", result.UTCOffset);
            }

            result.GdbEntries = new List<GeneralDbEntry>();

            if (result.IsGDB0Present)
            {
                var gdb0Entries = PcaGdbParser.ParseGdbFile(result.PcaFolderPath, "PcaGeneralDb0.txt", result.UTCOffset);
                result.GdbEntries.AddRange(gdb0Entries);
            }

            if (result.IsGDB1Present)
            {
                var gdb1Entries = PcaGdbParser.ParseGdbFile(result.PcaFolderPath, "PcaGeneralDb1.txt", result.UTCOffset);
                result.GdbEntries.AddRange(gdb1Entries);
            }

            if (result.GdbEntries != null && result.GdbEntries.Count > 0)
            {
                PathResolver.ResolveGdbPaths(result.GdbEntries, result.LadEntries);
            }

            ProgressChanged?.Invoke("SearchingUSNJournal");

            ProgressChanged?.Invoke("FetchingMetadata");


            EntryEnhancementService.EnhanceAllEntries(result.LadEntries, result.GdbEntries);

            return result;
        }
    }
}
