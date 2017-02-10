using System;
using System.IO;

namespace MarkdownWiki.Controllers
{
    public static class FilePathMetaExtractor
    {
        public static string GetRelativePath(this string entry)
        {
            return entry.Contains(@"\") ? entry.Substring(0, entry.LastIndexOf(@"\", StringComparison.InvariantCultureIgnoreCase) + 1) : String.Empty;
        }

        public static string EscapeFilePathEnd(this string filePath)
        {
            if (filePath.LastIndexOf(@"\", StringComparison.InvariantCultureIgnoreCase) + 1 == filePath.Length)
            {
                filePath += @"\";
            }
            return filePath;
        }

        public static string NormalizeFileName(this string fileName)
        {
            //return fileName.Replace(Settings.WikiPath, String.Empty).Replace(".md", String.Empty);
            return NFN(fileName);
        }

        public static string NFN(this string fileName)
        {
            var uri = new Uri(fileName);
            var result = Settings.WikiContentPathUri().MakeRelativeUri(uri);
            return result.ToString();
        }
    }
}