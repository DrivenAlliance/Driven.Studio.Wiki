using System;
using Markdig.Syntax.Inlines;

namespace MarkdownWiki.Parsers
{
    public class LocalLinkConverter
    {
        private readonly string _parentContentPath;
        private readonly string _dynamicContentBaseUrl;
        private readonly string _staticContentBaseUrl;

        public LocalLinkConverter(string parentContentPath, string dynamicContentBaseUrl, string staticContentBaseUrl)
        {
            _parentContentPath = parentContentPath;
            _dynamicContentBaseUrl = dynamicContentBaseUrl;
            _staticContentBaseUrl = staticContentBaseUrl;
        }

        public void Convert(LinkInline markdownLink)
        {
            Uri linkUri;
            if (!Uri.TryCreate(markdownLink.Url, UriKind.RelativeOrAbsolute, out linkUri))
                // Link not parseable as URI, leave as is
                return;

            if (isValidRelativeLink(linkUri))
            {
                markdownLink.Url = linksToStaticContent(markdownLink)
                    ? buildUrl(_staticContentBaseUrl, linkUri.ToString())
                    : buildUrl(_dynamicContentBaseUrl, linkUri.ToString());
            }
        }

        private string buildUrl(string baseUrl, string targetLink)
        {
            return $"{baseUrl}{_parentContentPath}/{targetLink}";
        }

        private static bool isValidRelativeLink(Uri uri)
        {
            return uri != null && !uri.IsAbsoluteUri;
        }

        private static bool linksToStaticContent(LinkInline markdownLink)
        {
            // todo: consider better way of checking for content to be generated instead of this flag
            return markdownLink.IsImage;
        }
    }
}