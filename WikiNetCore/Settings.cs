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
                //var wikiContentPath = Properties.Settings.Default.WikiFilePath;
                // TODO: UnHardcode
                var wikiContentPath = "wikicontent";
                return Path.IsPathRooted(wikiContentPath)
                    ? wikiContentPath
                    : rootInAppPath(wikiContentPath);
            }
        }

        private static string rootInAppPath(string configuredPath)
        {
            // todo: fix
            return Path.Combine(@"c:\Projects\Driven.Studio.Wiki\WikiNetCore\wwwroot", configuredPath);
            //return Path.Combine(HttpRuntime.AppDomainAppPath, configuredPath);
        }
    }
}