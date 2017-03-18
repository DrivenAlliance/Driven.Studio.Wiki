﻿using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace WikiNetCore.Parsers
{
    public class MarkdownConverter
    {
        private readonly Settings _settings;
        private readonly TableOfContentsBuilder _tableOfContentsBuilder;

        public MarkdownConverter(Settings settings)
        {
            _settings = settings;
            _tableOfContentsBuilder = new TableOfContentsBuilder();
        }

        public string Convert(string filePathRelativeToWikiContentRoot, string dynamicContentBaseUrl)
        {
            var wikiContentLocalPath = _settings.WikiContentPathUri.LocalPath;
            var localFilePathAbsolute = Path.Combine(wikiContentLocalPath, filePathRelativeToWikiContentRoot);

            var relativeContainerPath = Path.GetDirectoryName(filePathRelativeToWikiContentRoot);

            var content = File.Exists(localFilePathAbsolute)
                ? File.ReadAllText(localFilePathAbsolute)
                : "File Not Found";

            var localLinkConverter = new LocalLinkConverter(relativeContainerPath, dynamicContentBaseUrl, $"/{_settings.WikiContentRelativePath}/");

            return markdownContentToHtml(content, localLinkConverter);
        }

        private string markdownContentToHtml(string content, LocalLinkConverter localLinkConverter)
        {
            var pipeLine = buildMarkdigParserPipeLine();
            var markdownDocument = Markdown.Parse(content, pipeLine);

            rewriteLocalLinks(markdownDocument, localLinkConverter);
            var markedUpContent = render(pipeLine, markdownDocument);

            // todo: may be cleaner (& possibly more performant) to do this on a parsed MarkdownDocument instance rather
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

        private void rewriteLocalLinks(MarkdownDocument markdownDocument, LocalLinkConverter localLinkConverter)
        {
            var links = markdownDocument
                .Descendants()
                .OfType<LinkInline>();

            foreach (var link in links)
                localLinkConverter.Convert(link);
        }
    }
}