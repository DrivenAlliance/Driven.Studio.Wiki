using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtensions
    {
        public static string MarkDownToHtml(string entry)
        {
            var filePath = absoluteLocalFilePath(entry);

            var content = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "File Not Found";

            var contentPath = Path.GetDirectoryName(entry);
            // todo: could probably build this once only?
            var pipeLine = buildMarkdigParserPipeLine();
            var markdownDocument = Markdown.Parse(content, pipeLine);

            fixupLinks(contentPath, markdownDocument);
            var markedUpContent = render(pipeLine, markdownDocument);

            // todo: may be cleaner (& possibly performant) to do this on a parsed MarkdownDocument instance rather
            markedUpContent = fixUpTableOfContents(markedUpContent);

            return markedUpContent;
        }

        private static MarkdownPipeline buildMarkdigParserPipeLine()
        {
            return new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .Build();
        }

        private static string render(MarkdownPipeline pipeLine, MarkdownDocument markdownDocument)
        {
            var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            pipeLine.Setup(htmlRenderer);
            htmlRenderer.Render(markdownDocument);
            writer.Flush();
            return writer.ToString();
        }

        private static void fixupLinks(string contentPath, MarkdownDocument markdownDocument)
        {
            var links = markdownDocument.Descendants().OfType<LinkInline>()
                .Select(l => new LocalLinkConverter(l));

            foreach (var link in links)
                link.FixLocal(contentPath);
        }

        private static string fixUpTableOfContents(string content)
        {
            var tableOfContentsParser = new TableOfContentsParser();
            return tableOfContentsParser.Parse(content);
        }

        private static string absoluteLocalFilePath(string pathRelativeToWikiContent)
        {
            var location = Settings.WikiContentPathUri();
            return Path.Combine(location.LocalPath, pathRelativeToWikiContent);
        }
    }

    internal class LocalLinkConverter
    {
        private readonly LinkInline _markdownLink;
        private readonly Uri _uri;

        public LocalLinkConverter(LinkInline markdownLink)
        {
            _markdownLink = markdownLink;
            Uri linkUri;
            if (Uri.TryCreate(markdownLink.Url, UriKind.RelativeOrAbsolute, out linkUri))
                _uri = linkUri;
        }

        public void FixLocal(string contentPath)
        {
            if (isValidLocalLink())
            {
                _markdownLink.Url = linksToStaticContent()
                    ? linkToStaticContent(contentPath)
                    : linkToDynamicContent(contentPath);
            }
        }

        private bool isValidLocalLink()
        {
            return _uri != null && !_uri.IsAbsoluteUri;
        }

        private bool linksToStaticContent()
        {
            // todo: consider better way of checking for content to be generated instead of this flag
            return _markdownLink.IsImage;
        }

        private string linkToDynamicContent(string contentPath)
        {
            // todo: do this better than a hardcoded '/'?
            var contentLink = UrlEncoder.Default.Encode(contentPath + "/" + _uri);
            return $"ViewPage?entry={contentLink}";
        }

        private string linkToStaticContent(string contentPath)
        {
            // todo: shift to content root
            // todo: unhardcode
            var wikiContentRoot = "/wikicontent";
            var imgPath = _uri.ToString();
            return $"{wikiContentRoot}/{contentPath}/{imgPath}";
        }
    }
}