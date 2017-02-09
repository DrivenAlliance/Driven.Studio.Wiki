using System;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using static Markdig.Markdown;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtentions
    {
        public static string Transform2(this string content, string contentPath)
        {
            var pipeLine = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
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
            }

            var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            htmlRenderer.Render(markdownDocument);
            writer.Flush();

            return writer.ToString();
        }

        public static string Transform(this string content)
        {
            var pipeLine = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .Build();

            var markdownDocument = Parse(content, pipeLine);
            var links = markdownDocument.Descendants().OfType<LinkInline>();
            foreach (var link in links)
            {
                if (link.IsImage) // todo: only rewrite if link is local
                {
                    // todo: shift to content root
                    link.Url = "/wikicontent/test/IMG_8752.PNG";
                }
            }

            var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);
            htmlRenderer.Render(markdownDocument);
            writer.Flush();

            return writer.ToString();
        }
    }
}