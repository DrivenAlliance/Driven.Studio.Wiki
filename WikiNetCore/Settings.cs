using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace WikiNetCore
{
    public class Settings
    {
        public Settings(string webRootPath, IConfiguration config)
        {
            // Wiki content needs to reside under the web root in order to serve static files like images.
            WikiContentRelativePath = config["WikiContentDir"];
            AbsoluteWikiContentPath = buildAbsoluteWikiContentPath(webRootPath);
        }

        public string AbsoluteWikiContentPath { get; }
        public string WikiContentRelativePath { get; }

        public Uri WikiContentPathUri
        {
            get
            {
                var wikiContentUri = new Uri(AbsoluteWikiContentPath);
                // todo: the trailing slash here is important, as it identifies the uri as a container
                return !wikiContentUri.AbsoluteUri.EndsWith("/")
                    ? new Uri(wikiContentUri + "/")
                    : wikiContentUri;
            }
        }

        public string MakeRelativeToWikiContentPath(string fileName)
        {
            var uri = new Uri(fileName);
            return WikiContentPathUri.MakeRelativeUri(uri).ToString();
        }

        private string buildAbsoluteWikiContentPath(string webRootPath)
        {
            return Path.GetFullPath(Path.Combine(webRootPath, WikiContentRelativePath));
        }
    }
}