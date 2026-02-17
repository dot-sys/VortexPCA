using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using USNEnhancer.Models;

// Core USN journal reading and caching
namespace USNEnhancer.Core
{
    // Reads Windows USN change journal entries
    internal class USNJournalReader
    {
        // FSCTL control code for journal query
        private const uint FSCTL_QUERY_USN_JOURNAL = 0x000900f4;
        // FSCTL control code for journal reading
        private const uint FSCTL_READ_USN_JOURNAL = 0x000900bb;
        // File sharing mode for reading
        private const uint FILE_SHARE_READ = 0x00000001;
        // File sharing mode for writing
        private const uint FILE_SHARE_WRITE = 0x00000002;
        // File creation disposition for existing
        private const uint OPEN_EXISTING = 3;
        // Generic read and write access
        private const uint GENERIC_READ_WRITE = 0xC0000000;

        // USN reason flag for deletion
        private const uint USN_REASON_FILE_DELETE = 0x00000200;
        // USN reason flag for old name
        private const uint USN_REASON_RENAME_OLD_NAME = 0x00001000;
        // USN reason flag for new name
        private const uint USN_REASON_RENAME_NEW_NAME = 0x00002000;
        // Combined relevant reason flags mask
        private const uint RELEVANT_REASONS = USN_REASON_FILE_DELETE | USN_REASON_RENAME_OLD_NAME | USN_REASON_RENAME_NEW_NAME;

        // Win32 API for file handle creation
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        // Win32 API for device control operations
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        // USN journal metadata structure
        [StructLayout(LayoutKind.Sequential)]
        private struct USN_JOURNAL_DATA
        {
            // Unique journal identifier
            public ulong UsnJournalID;
            // First valid USN value
            public long FirstUsn;
            // Next USN to be allocated
            public long NextUsn;
            // Lowest valid USN
            public long LowestValidUsn;
            // Maximum USN value
            public long MaxUsn;
            // Maximum journal size
            public ulong MaximumSize;
            // Journal allocation increment
            public ulong AllocationDelta;
        }

        // USN journal read parameters structure
        [StructLayout(LayoutKind.Sequential)]
        private struct READ_USN_JOURNAL_DATA
        {
            // Starting USN for read operation
            public long StartUsn;
            // Reason code filter mask
            public uint ReasonMask;
            // Return only on file close
            public uint ReturnOnlyOnClose;
            // Wait timeout in milliseconds
            public ulong Timeout;
            // Bytes to wait before returning
            public ulong BytesToWaitFor;
            // Target journal identifier
            public ulong UsnJournalID;
        }

        // USN change record version 2
        [StructLayout(LayoutKind.Sequential)]
        private struct USN_RECORD_V2
        {
            // Total record length in bytes
            public uint RecordLength;
            // Record format major version
            public ushort MajorVersion;
            // Record format minor version
            public ushort MinorVersion;
            // File reference number
            public ulong FileReferenceNumber;
            // Parent directory reference number
            public ulong ParentFileReferenceNumber;
            // Update sequence number value
            public long Usn;
            // Record timestamp as FILETIME
            public long TimeStamp;
            // Change reason flags
            public uint Reason;
            // Change source information flags
            public uint SourceInfo;
            // Security descriptor identifier
            public uint SecurityId;
            // File attribute flags
            public uint FileAttributes;
            // File name length in bytes
            public ushort FileNameLength;
            // File name offset in record
            public ushort FileNameOffset;
        }

        // Reads all USN journal entries
        public List<USNRecord> ReadUSNJournal(string driveLetter)
        {
            var records = new List<USNRecord>();

            SafeFileHandle volumeHandle = OpenVolumeHandle(driveLetter);
            if (volumeHandle == null || volumeHandle.IsInvalid)
            {
                return records;
            }

            try
            {
                IntPtr journalPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USN_JOURNAL_DATA)));
                try
                {
                    bool success = DeviceIoControl(
                        volumeHandle,
                        FSCTL_QUERY_USN_JOURNAL,
                        IntPtr.Zero,
                        0,
                        journalPtr,
                        Marshal.SizeOf(typeof(USN_JOURNAL_DATA)),
                        out uint bytesReturned,
                        IntPtr.Zero);

                    if (!success)
                    {
                        return records;
                    }

                    USN_JOURNAL_DATA journalData = (USN_JOURNAL_DATA)Marshal.PtrToStructure(journalPtr, typeof(USN_JOURNAL_DATA));

                    // Use READ_USN_JOURNAL to read actual change journal entries with reasons
                    READ_USN_JOURNAL_DATA readData = new READ_USN_JOURNAL_DATA
                    {
                        StartUsn = 0,  // Start from the beginning
                        ReasonMask = 0xFFFFFFFF,  // All reasons
                        ReturnOnlyOnClose = 0,
                        Timeout = 0,
                        BytesToWaitFor = 0,
                        UsnJournalID = journalData.UsnJournalID
                    };

                    int readDataSize = Marshal.SizeOf(readData);
                    IntPtr readDataPtr = Marshal.AllocHGlobal(readDataSize);
                    try
                    {
                        Marshal.StructureToPtr(readData, readDataPtr, false);

                        int bufferSize = 65536;
                        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                        try
                        {
                            while (true)
                            {
                                success = DeviceIoControl(
                                    volumeHandle,
                                    FSCTL_READ_USN_JOURNAL,
                                    readDataPtr,
                                    readDataSize,
                                    buffer,
                                    bufferSize,
                                    out bytesReturned,
                                    IntPtr.Zero);

                                if (!success)
                                {
                                    int error = Marshal.GetLastWin32Error();
                                    if (error == 38 || error == 1179)
                                    {
                                        break;
                                    }
                                    throw new Win32Exception(error);
                                }

                                if (bytesReturned <= 8)
                                {
                                    break;
                                }

                                IntPtr recordPtr = new IntPtr(buffer.ToInt64() + 8);
                                uint recordCount = bytesReturned - 8;

                                while (recordCount > 0)
                                {
                                    USN_RECORD_V2 record = (USN_RECORD_V2)Marshal.PtrToStructure(recordPtr, typeof(USN_RECORD_V2));

                                    if (record.RecordLength == 0)
                                    {
                                        break;
                                    }

                                    if ((record.Reason & RELEVANT_REASONS) != 0)
                                    {
                                        IntPtr fileNamePtr = new IntPtr(recordPtr.ToInt64() + record.FileNameOffset);
                                        string fileName = Marshal.PtrToStringUni(fileNamePtr, record.FileNameLength / 2);
                                        DateTime timestamp = DateTime.FromFileTimeUtc(record.TimeStamp);

                                        if ((record.Reason & USN_REASON_FILE_DELETE) != 0)
                                        {
                                            records.Add(CreateUSNRecord(driveLetter, fileName, USNEnhancer.USNReason.Deleted, timestamp));
                                        }

                                        if ((record.Reason & USN_REASON_RENAME_OLD_NAME) != 0)
                                        {
                                            records.Add(CreateUSNRecord(driveLetter, fileName, USNEnhancer.USNReason.RenameOld, timestamp));
                                        }

                                        if ((record.Reason & USN_REASON_RENAME_NEW_NAME) != 0)
                                        {
                                            records.Add(CreateUSNRecord(driveLetter, fileName, USNEnhancer.USNReason.RenameNew, timestamp));
                                        }
                                    }

                                    recordPtr = new IntPtr(recordPtr.ToInt64() + record.RecordLength);
                                    recordCount -= record.RecordLength;
                                }

                                readData.StartUsn = Marshal.ReadInt64(buffer);
                                Marshal.StructureToPtr(readData, readDataPtr, false);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(buffer);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(readDataPtr);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(journalPtr);
                }
            }
            finally
            {
                volumeHandle.Close();
            }

            return records;
        }

        // Checks if drive has USN journal
        public bool HasUSNJournal(string driveLetter)
        {
            SafeFileHandle volumeHandle = OpenVolumeHandle(driveLetter);
            if (volumeHandle == null || volumeHandle.IsInvalid)
            {
                return false;
            }

            try
            {
                IntPtr journalPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(USN_JOURNAL_DATA)));
                try
                {
                    bool success = DeviceIoControl(
                        volumeHandle,
                        FSCTL_QUERY_USN_JOURNAL,
                        IntPtr.Zero,
                        0,
                        journalPtr,
                        Marshal.SizeOf(typeof(USN_JOURNAL_DATA)),
                        out uint bytesReturned,
                        IntPtr.Zero);

                    return success;
                }
                finally
                {
                    Marshal.FreeHGlobal(journalPtr);
                }
            }
            finally
            {
                volumeHandle.Close();
            }
        }

        // Opens volume handle for drive access
        private SafeFileHandle OpenVolumeHandle(string driveLetter)
        {
            var volumePath = $"\\\\.\\{driveLetter}:";
            return CreateFile(
                volumePath,
                GENERIC_READ_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);
        }

        // Creates USN record from parameters
        private USNRecord CreateUSNRecord(string driveLetter, string fileName, USNEnhancer.USNReason reason, DateTime timestamp)
        {
            return new USNRecord
            {
                DriveLetter = driveLetter,
                FileName = fileName,
                Reason = reason,
                Timestamp = timestamp
            };
        }
    }
}
