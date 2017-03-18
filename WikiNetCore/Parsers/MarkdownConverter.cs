using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace WikiNetCore.Parsers
{
    public class MarkdownConverter
    {
        private readonly LocalLinkConverter _localLinkConverter;
        private readonly TableOfContentsBuilder _tableOfContentsBuilder;
        private readonly string _localFilePathAbsolute;

        public MarkdownConverter(string filePathRelativeToWikiContentRoot, string dynamicContentBaseUrl, string staticContentBaseUrl, Settings settings)
        {
            var wikiContentLocalPath = settings.WikiContentPathUri.LocalPath;
            _localFilePathAbsolute = Path.Combine(wikiContentLocalPath, filePathRelativeToWikiContentRoot);

            _tableOfContentsBuilder = new TableOfContentsBuilder();

            var relativeContainerPath = Path.GetDirectoryName(filePathRelativeToWikiContentRoot);
            _localLinkConverter = new LocalLinkConverter(relativeContainerPath, dynamicContentBaseUrl, staticContentBaseUrl);
        }

        public string Convert()
        {
            var content = File.Exists(_localFilePathAbsolute)
                ? File.ReadAllText(_localFilePathAbsolute)
                : "File Not Found";

            return markdownContentToHtml(content);
        }

        private string markdownContentToHtml(string content)
        {
            var pipeLine = buildMarkdigParserPipeLine();
            var markdownDocument = Markdown.Parse(content, pipeLine);

            rewriteLocalLinks(markdownDocument);
            var markedUpContent = render(pipeLine, markdownDocument);

            // todo: may be cleaner (& possibly performant) to do this on a parsed MarkdownDocument instance rather
            markedUpContent = _tableOfContentsBuilder.Parse(markedUpContent);

            return markedUpContent;
        }

        private MarkdownPipeline buildMarkdigParserPipeLine()
        {
            return new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .Build();
        }

        private string render(MarkdownPipeline pipeLine, MarkdownDocument markdownDocument)
        {
            var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            pipeLine.Setup(htmlRenderer);
            htmlRenderer.Render(markdownDocument);
            writer.Flush();

            return writer.ToString();
        }

        private void rewriteLocalLinks(MarkdownDocument markdownDocument)
        {
            var links = markdownDocument
                .Descendants()
                .OfType<LinkInline>();

            foreach (var link in links)
                _localLinkConverter.Convert(link);
        }
    }
}