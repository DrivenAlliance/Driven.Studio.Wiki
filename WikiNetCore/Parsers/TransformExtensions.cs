using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtensions
    {
        public static string MarkDownFileToHtml(string filePathRelativeToWikiContentRoot)
        {
            var localFilePath = absoluteLocalFilePath(filePathRelativeToWikiContentRoot);

            var content = File.Exists(localFilePath)
                ? File.ReadAllText(localFilePath)
                : "File Not Found";

            var relativeContainerPath = Path.GetDirectoryName(filePathRelativeToWikiContentRoot);
            return markdownContentToHtml(content, relativeContainerPath);
        }

        private static string markdownContentToHtml(string content, string relativeContainerPath)
        {
            var pipeLine = buildMarkdigParserPipeLine();
            var markdownDocument = Markdown.Parse(content, pipeLine);

            fixUpLocalLinks(relativeContainerPath, markdownDocument);

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

        private static void fixUpLocalLinks(string parentContentPath, MarkdownDocument markdownDocument)
        {
            var converter = new LocalLinkConverter(
                parentContentPath,
                "ViewPage?entry=",
                $"/{Settings.WikiContentRelativePath}/");

            var links = markdownDocument
                .Descendants()
                .OfType<LinkInline>();

            foreach (var link in links)
                converter.Convert(link);
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