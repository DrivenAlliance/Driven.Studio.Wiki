using System;
using System.Text.Encodings.Web;
using Markdig.Syntax.Inlines;

namespace MarkdownWiki.Parsers
{
    public class LocalLinkConverter
    {
        private readonly LinkInline _markdownLink;
        private readonly Uri _uri;
        private readonly string _parentContentPath;

        public LocalLinkConverter(LinkInline markdownLink, string parentContentPath)
        {
            _markdownLink = markdownLink;
            _parentContentPath = parentContentPath;
            Uri linkUri;
            if (Uri.TryCreate(markdownLink.Url, UriKind.RelativeOrAbsolute, out linkUri))
                _uri = linkUri;
        }

        public void FixLocal()
        {
            if (isValidRelativeLink())
            {
                _markdownLink.Url = linksToStaticContent()
                    ? buildLinkToStaticContent()
                    : buildLinkToDynamicContent();
            }
        }

        private bool isValidRelativeLink()
        {
            return _uri != null && !_uri.IsAbsoluteUri;
        }

        private bool linksToStaticContent()
        {
            // todo: consider better way of checking for content to be generated instead of this flag
            return _markdownLink.IsImage;
        }

        private string buildLinkToDynamicContent()
        {
            // todo: do this better than a hardcoded '/'?
            // todo: necessary to encode url?
            var contentLink = UrlEncoder.Default.Encode(_parentContentPath + "/" + _uri);
            return $"ViewPage?entry={contentLink}";
        }

        private string buildLinkToStaticContent()
        {
            var staticContentUri = _uri.ToString();
            return $"/{Settings.WikiContentRelativePath}/{_parentContentPath}/{staticContentUri}";
        }
    }
}