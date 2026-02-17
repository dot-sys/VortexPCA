using System;
using System.IO;
using VortexLocalPCA.Models;
using VortexLocalPCA.Util;

// Core parsing logic for PCA files
namespace VortexLocalPCA.Core
{
    // Analyzes PCA files and extracts metadata
    public static class PcaFileParser
    {
        // Standard PCA AppLaunchDic filename
        private const string PcaAppLaunchDicFileName = "PcaAppLaunchDic.txt";
        // Standard PCA GeneralDb0 filename
        private const string PcaGeneralDb0FileName = "PcaGeneralDb0.txt";
        // Standard PCA GeneralDb1 filename
        private const string PcaGeneralDb1FileName = "PcaGeneralDb1.txt";

        // Analyzes all PCA files in given folder
        public static void AnalyzePcaFiles(string pcaFolderPath, AnalysisResult result)
        {
            result.PcaAppLaunchDic = AnalyzeFile(pcaFolderPath, PcaAppLaunchDicFileName);
            result.IsLADPresent = result.PcaAppLaunchDic.IsPresent;

            result.PcaGeneralDb0 = AnalyzeFile(pcaFolderPath, PcaGeneralDb0FileName);
            result.IsGDB0Present = result.PcaGeneralDb0.IsPresent;

            result.PcaGeneralDb1 = AnalyzeFile(pcaFolderPath, PcaGeneralDb1FileName);
            result.IsGDB1Present = result.PcaGeneralDb1.IsPresent;
        }

        // Analyzes single PCA file metadata and properties
        private static PcaFileInfo AnalyzeFile(string folderPath, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName);
            var fileInfo = new PcaFileInfo
            {
                FileName = fileName,
                IsPresent = FileSystemHelper.FileExists(filePath)
            };

            if (!fileInfo.IsPresent)
            {
                return fileInfo;
            }

            var sysFileInfo = FileSystemHelper.GetFileInfo(filePath);
            if (sysFileInfo == null)
            {
                return fileInfo;
            }

            fileInfo.CreatedDate = sysFileInfo.CreationTime;
            fileInfo.ModifiedDate = sysFileInfo.LastWriteTime;
            fileInfo.AccessedDate = sysFileInfo.LastAccessTime;
            fileInfo.SizeInKB = sysFileInfo.Length / 1024;

            if (sysFileInfo.Length > 0)
            {
                fileInfo.FirstLine = FileSystemHelper.ReadFirstLine(filePath);
                fileInfo.ReadSuccessfully = !string.IsNullOrEmpty(fileInfo.FirstLine);
            }
            else
            {
                fileInfo.ReadSuccessfully = false;
            }

            return fileInfo;
        }
    }
}
