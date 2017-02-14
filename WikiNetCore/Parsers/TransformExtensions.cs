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
            var links = markdownDocument.Descendants().OfType<LinkInline>();
            foreach (var link in links)
            {
                Uri linkUri;
                if (Uri.TryCreate(link.Url, UriKind.RelativeOrAbsolute, out linkUri))
                {
                    if (!linkUri.IsAbsoluteUri) // only rewrite relative links to other local content
                    {
                        link.Url = link.IsImage
                            ? linkToStaticContent(contentPath, linkUri)
                            : linkToDynamicContent(contentPath, linkUri);
                    }
                }
            }
        }

        private static string linkToDynamicContent(string contentPath, Uri linkUri)
        {
            // todo: do this better than a hardcoded '/'?
            var contentLink = UrlEncoder.Default.Encode(contentPath + "/" + linkUri);
            return $"ViewPage?entry={contentLink}";
        }

        private static string linkToStaticContent(string contentPath, Uri linkUri)
        {
            // todo: shift to content root
            // todo: unhardcode
            var wikiContentRoot = "/wikicontent";
            var imgPath = linkUri.ToString();
            return $"{wikiContentRoot}/{contentPath}/{imgPath}";
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
}