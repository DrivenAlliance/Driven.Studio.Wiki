using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownWiki.Controllers;
using static Markdig.Markdown;

namespace MarkdownWiki.Parsers
{
    public static class TransformExtensions
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

        public static string FixUpLocalLinks(string content, string relativePath)
        {
            var regex = new Regex(@"\(((?:\w|\s|\d|\b|[-!@#$%^&()_+=`~])+(\.md)?)\)", RegexOptions.Multiline);
            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                var groupValue = match.Groups[0].Value;
                var fileName = match.Groups[1].Value;

                try
                {
                    if (fileName.Substring(fileName.Length - 3, 3) == ".md")
                        fileName = fileName.Substring(0, fileName.Length - 3);

                    var wikiDirectory = new DirectoryInfo(Settings.WikiPath);
                    var files = wikiDirectory.GetFiles($"{fileName}.md", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        var fixedFileName = files[0].FullName.Replace(Settings.WikiPath, String.Empty).Replace(".md", String.Empty);
                        var encodedPath = UrlEncoder.Default.Encode(fixedFileName);
                        var substituteGroupValue = $"(ViewPage?entry={encodedPath})";
                        content = content.Replace(groupValue, substituteGroupValue);
                    }
                }
                catch { /* Do nothing */ }
            }

            return content;
        }

        public static string FixUpLanFileLinks(string content)
        {
            var regex = new Regex(@"(\(|\[)(file:\\\\\\)?((?:\w|\s|\d|[-.!@#$%^&()_+=`~\\])+)(\)|\])", RegexOptions.Multiline);
            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                var leftBracket = match.Groups[1].Value;
                var rightBracket = match.Groups[4].Value;
                var protocol = match.Groups[2].Value;
                var filePath = match.Groups[3].Value;
                var entireMatch = match.Value;

                var encodedFilePath = protocol == @"file:\\\" ? filePath.Replace(" ", "%20") : filePath;
                encodedFilePath = encodedFilePath.EscapeFilePathEnd();
                content = content.Replace(entireMatch, $"{leftBracket}{protocol}{encodedFilePath}{rightBracket}");

            }
            return content;
        }

        public static string FixUpGithubTables(string content)
        {
            var tableParser = new GithubTableParser();
            return tableParser.Parse(content);
        }

        public static string FixUpTableOfContents(string content)
        {
            var tableOfContentsParser = new TableOfContentsParser();
            return tableOfContentsParser.Parse(content);
        }

        public static string markDownToHtml(string entry)
        {
            var fileLocation = $"{Settings.WikiPath}{entry}.md";
            var relativePath = entry.GetRelativePath();

            var content = System.IO.File.Exists(fileLocation)
                ? System.IO.File.ReadAllText(fileLocation)
                : "File Not Found";

            content = TransformExtensions.FixUpLocalLinks(content, relativePath);
            content = TransformExtensions.FixUpLanFileLinks(content);

            // todo: test if github tables are working without this
            //content = FixUpGithubTables(content);

            var y = Path.GetDirectoryName(entry);
            var markedDownContent = content.Transform2(y);

            markedDownContent = TransformExtensions.FixUpTableOfContents(markedDownContent);
            return markedDownContent;
        }
    }
}