using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using static Markdig.Markdown;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtensions
    {
        public static string Transform2(this string content, string contentPath)
        {
            var pipeLine = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseGridTables()
                .UseBootstrap()
                .Build();

            var markdownDocument = Parse(content, pipeLine);
            var links = markdownDocument.Descendants().OfType<LinkInline>();
            foreach (var link in links)
            {
                if (link.IsImage) // todo: only rewrite if link is local
                {
                    // todo: shift to content root
                    // todo: unhardcode
                    var wikiContentRoot = "/wikicontent";
                    var imgPath = link.Url;
                    link.Url = $"{wikiContentRoot}/{contentPath}/{imgPath}";
                }
                else
                {
                    Uri resultingUri;
                    if (Uri.TryCreate(link.Url, UriKind.RelativeOrAbsolute, out resultingUri))
                    {
                        if (!resultingUri.IsAbsoluteUri)
                        {
                            // todo: do this better than a hardcoded '/'?
                            var contentLink = UrlEncoder.Default.Encode(contentPath + "/" + resultingUri);
                            var substituteGroupValue = $"ViewPage?entry={contentLink}";
                            link.Url = substituteGroupValue;
                        }
                    }
                }
            }

            var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            htmlRenderer.Render(markdownDocument);
            writer.Flush();

            return writer.ToString();
        }

        private static string fixUpTableOfContents(string content)
        {
            var tableOfContentsParser = new TableOfContentsParser();
            return tableOfContentsParser.Parse(content);
        }

        public static string MarkDownToHtml(string entry)
        {
            var filePath = absoluteLocalFilePath(entry);

            var content = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "File Not Found";

            var contentPath = Path.GetDirectoryName(entry);
            var markedUpContent = content.Transform2(contentPath);

            markedUpContent = fixUpTableOfContents(markedUpContent);
            return markedUpContent;
        }

        private static string absoluteLocalFilePath(string pathRelativeToWikiContent)
        {
            var location = Settings.WikiContentPathUri();
            return new Uri(location, pathRelativeToWikiContent).LocalPath;
        }
    }
}