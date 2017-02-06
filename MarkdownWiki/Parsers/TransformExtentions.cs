using Markdig;
using static Markdig.Markdown;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtentions
    {
        public static string Transform(this string content)
        {
            var pipeLine = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .Build();

            return ToHtml(content, pipeLine);
        }
    }
}