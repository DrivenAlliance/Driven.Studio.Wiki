using System;
using System.Linq;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace WikiNetCore.Parsers
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

        public void RewriteLocalLinks(MarkdownDocument markdownDocument)
        {
            var links = markdownDocument
                .Descendants()
                .OfType<LinkInline>();

            foreach (var link in links)
                convert(link);
        }

        private void convert(LinkInline markdownLink)
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
            // todo: consider better way of checking for static content. Maybe if link ends in .md / .markdown etc?
            // todo: what about other kinds of static content, like .pdf?
            return markdownLink.IsImage;
        }
    }
}