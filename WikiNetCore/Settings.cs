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
                // TODO : Allow this to be configured outside the code
                var wikiContentPath = WikiContentRelativePath;
                return Path.IsPathRooted(wikiContentPath)
                    ? wikiContentPath
                    : rootInAppPath(wikiContentPath);
            }
        }

        private static string rootInAppPath(string configuredPath)
        {
            return Path.GetFullPath(Path.Combine(@".\wwwroot", configuredPath));
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