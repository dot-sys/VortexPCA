using System;
using System.IO;

// Services for file analysis operations
namespace EntryEnhancer.Services
{
    // PE file digital signature verification
    internal static class SignatureVerifier
    {
        // Minimum valid PE file size
        private const int MinPEFileSize = 64;
        // Offset to PE header pointer
        private const int PEHeaderOffsetPosition = 0x3C;
        // PE signature magic number
        private const uint PESignature = 0x00004550;
        // PE32 optional header magic
        private const ushort PE32Magic = 0x10b;
        // PE32 plus optional header magic
        private const ushort PE32PlusMagic = 0x20b;
        // Certificate table directory index
        private const int CertificateDirectoryIndex = 4;
        // Data directory entry size in bytes
        private const int DataDirectoryEntrySize = 8;

        // Checks if PE file has digital signature
        public static bool IsFileSigned(string filePath)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                using (var reader = new BinaryReader(stream))
                {
                    if (stream.Length < MinPEFileSize)
                        return false;

                    stream.Seek(PEHeaderOffsetPosition, SeekOrigin.Begin);
                    var peHeaderOffset = reader.ReadInt32();

                    if (peHeaderOffset <= 0 || peHeaderOffset + 256 >= stream.Length)
                        return false;

                    stream.Seek(peHeaderOffset, SeekOrigin.Begin);
                    var peSignature = reader.ReadUInt32();
                    if (peSignature != PESignature)
                        return false;

                    SkipCOFFHeader(reader);

                    var optionalHeaderSize = reader.ReadUInt16();
                    reader.ReadUInt16();

                    if (optionalHeaderSize == 0)
                        return false;

                    var magic = reader.ReadUInt16();
                    if (!IsValidPEMagic(magic))
                        return false;

                    int skipBytes = (magic == PE32PlusMagic) ? 88 : 92;
                    stream.Seek(skipBytes, SeekOrigin.Current);

                    var numberOfRvaAndSizes = reader.ReadUInt32();
                    if (numberOfRvaAndSizes < 5)
                        return false;

                    stream.Seek(CertificateDirectoryIndex * DataDirectoryEntrySize, SeekOrigin.Current);

                    var certificateTableRva = reader.ReadUInt32();
                    var certificateTableSize = reader.ReadUInt32();

                    return certificateTableRva != 0 && certificateTableSize != 0;
                }
            }
            catch
            {
                return false;
            }
        }

        // Advances reader past COFF header fields
        private static void SkipCOFFHeader(BinaryReader reader)
        {
            reader.ReadUInt16();
            reader.ReadUInt16();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
        }

        // Validates PE optional header magic number
        private static bool IsValidPEMagic(ushort magic)
        {
            return magic == PE32Magic || magic == PE32PlusMagic;
        }
    }
}

