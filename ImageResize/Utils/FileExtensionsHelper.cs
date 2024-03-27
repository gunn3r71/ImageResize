namespace ImageResize.Utils
{
    public static class FileExtensionsHelper
    {
        private static readonly string[] _validFormats = [".jpg", ".jpeg"];

        public static bool IsAccepted(string filePath) => _validFormats.Contains(GetExtension(filePath));
        public static string GetExtension(string filePath) => Path.GetExtension(filePath);
    }
}