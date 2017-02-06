using Lucene.Net.Documents;

namespace MarkdownWiki.Controllers
{
    public static class DisplayResultFormatter
    {
        public static string CreateDisplayTextFromFileName(this string fileName)
        {
            var displayText = fileName;
            displayText = displayText.StartsWith("\\") ? displayText.Substring(1) : displayText;
            displayText = displayText.Replace("\\"," --> ");
            return displayText;
        }
    }
}