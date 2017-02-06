using System.IO;
using System.Web;

namespace MarkdownWiki
{
    public class Settings
    {
        public static string WikiPath
        {
            get
            {
                var wikiContentPath = Properties.Settings.Default.WikiFilePath;
                return Path.IsPathRooted(wikiContentPath)
                    ? wikiContentPath
                    : rootInAppPath(wikiContentPath);
            }
        }

        private static string rootInAppPath(string configuredPath)
        {
            return Path.Combine(HttpRuntime.AppDomainAppPath, configuredPath);
        }
    }
}