namespace Application.Common.FileValidation
{
    internal static class FileSignatures
    {
        public static readonly Dictionary<AllowedFileType, (string Extension, byte[] Signature)> Map = new()
        {
            [AllowedFileType.Jpg]  = (".jpg",  new byte[] { 0xFF, 0xD8, 0xFF }),
            [AllowedFileType.Png]  = (".png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
            [AllowedFileType.Pdf]  = (".pdf",  new byte[] { 0x25, 0x50, 0x44, 0x46 }),
            [AllowedFileType.Gif]  = (".gif",  new byte[] { 0x47, 0x49, 0x46, 0x38 }),
            [AllowedFileType.Docx] = (".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
            [AllowedFileType.Xlsx] = (".xlsx", new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
        };
    }
}
