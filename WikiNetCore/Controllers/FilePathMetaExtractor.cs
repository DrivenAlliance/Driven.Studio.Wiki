using System;
using WikiNetCore;

namespace MarkdownWiki.Controllers
{
    public static class FilePathMetaExtractor
    {
        public static string NormalizeFileName(this string fileName)
        {
            var uri = new Uri(fileName);
            var result = Settings.Instance.WikiContentPathUri.MakeRelativeUri(uri);
            return result.ToString();
        }
    }
}