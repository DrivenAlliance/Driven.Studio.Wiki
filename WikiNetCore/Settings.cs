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
            WikiContentFullLocalPath = buildAbsoluteWikiContentPath(webRootPath);
        }

        public string WikiContentFullLocalPath { get; }
        public string WikiContentRelativePath { get; }

        public string MakeRelativeToWikiContentPath(string fileName)
        {
            var wikiContentUri = new Uri(WikiContentFullLocalPath);
            // Note: the trailing slash here is important, as it identifies the uri as a container
            var containerUri = wikiContentUri.AbsoluteUri.EndsWith("/")
                ? wikiContentUri
                : new Uri(wikiContentUri + "/");

            var fileUri = new Uri(fileName);

            return containerUri.MakeRelativeUri(fileUri).ToString();
        }

        private string buildAbsoluteWikiContentPath(string webRootPath)
        {
            return Path.GetFullPath(Path.Combine(webRootPath, WikiContentRelativePath));
        }
    }
}