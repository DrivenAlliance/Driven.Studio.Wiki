using System;
using System.IO;

namespace MarkdownWiki
{
    public class Settings
    {
        public static string WikiContentRelativePath { get; } = "wikicontent";

        public static string WikiPath
        {
            get
            {
                //var wikiContentPath = Properties.Settings.Default.WikiFilePath;
                // TODO: UnHardcode
                var wikiContentPath = WikiContentRelativePath;
                return Path.IsPathRooted(wikiContentPath)
                    ? wikiContentPath
                    : rootInAppPath(wikiContentPath);
            }
        }

        private static string rootInAppPath(string configuredPath)
        {
            // todo: Unhardcode
            var contentRootedPath = Path.Combine(@"c:\Projects\Driven.Studio.Wiki\WikiNetCore\wwwroot", configuredPath);
            return contentRootedPath;
            //return Path.Combine(HttpRuntime.AppDomainAppPath, configuredPath);
        }

        public static Uri WikiContentPathUri()
        {
            var wikiContentUri = new Uri(Settings.WikiPath);
            // todo: the trailing slash here is important, as it identifies the uri as a container
            return !wikiContentUri.AbsoluteUri.EndsWith("/")
                ? new Uri(wikiContentUri + "/")
                : wikiContentUri;
        }
    }
}