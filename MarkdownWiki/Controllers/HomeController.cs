using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MarkdownWiki.Models;
using MarkdownWiki.Parsers;
using Microsoft.Ajax.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Version = Lucene.Net.Util.Version;

namespace MarkdownWiki.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(SearchModel searchModel)
        {
            if (ModelState.IsValid)
            {
                var luceneDir = Path.Combine(HttpRuntime.AppDomainAppPath, "lucene_index");
                var directory = new SimpleFSDirectory(new DirectoryInfo(luceneDir), new NativeFSLockFactory());

                using (var searcher = new IndexSearcher(directory, true))
                using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
                {
                    var parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "Entry", "Content" }, analyzer);
                    Query query;
                    try
                    {
                        query = parser.Parse(searchModel.SearchTerm);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View(searchModel);
                    }
                    var hits = searcher.Search(query, 1000).ScoreDocs;

                    var documents = hits
                        .Select(hit => searcher.Doc(hit.Doc))
                        .ToList();

                    var results = documents
                        .Select(document => new DocumentResult { FileName = document.Get("Entry"), DisplayText = document.Get("Entry").CreateDisplayTextFromFileName()})
                        .DistinctBy(result => result.FileName)
                        .ToList();

                    searchModel.Results.AddRange(results);
                }
            }

            return View(searchModel);
        }

        public ActionResult ViewPage(string entry)
        {
            var fileLocation = $"{Settings.WikiPath}{entry}.md";
            var relativePath = entry.GetRelativePath();

            var content = System.IO.File.Exists(fileLocation)
                ? System.IO.File.ReadAllText(fileLocation)
                : "File Not Found";

            content = FixUpImages(content);
            content = FixUpLocalLinks(content, relativePath);
            content = FixUpLanFileLinks(content);
            //content = FixUpGithubTables(content);

            var markedDownContent = content.Transform();
            markedDownContent = FixUpTableOfContents(markedDownContent);

            var result = new MarkdownResult
            {
                Title = entry,
                Content = markedDownContent
            };

            return View(result);
        }

        private static string FixUpImages(string content)
        {
            var regex = new Regex(@"\(..\/Attachments\/((?:\w|\s|\d|\b|[-.!@#$%^&()_+=`~\/])*)\)", RegexOptions.Multiline);
            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                content = content.Replace(match.Value, match.Value.Replace(" ", "%20"));
            }

            content = content.Replace("../Attachments", "/ShowImage.ashx?imagePath=../Attachments");
            return content;
        }

        private static string FixUpLocalLinks(string content, string relativePath)
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
                        var fixedFileName = files[0].FullName.Replace(Settings.WikiPath, string.Empty).Replace(".md", string.Empty);
                        var encodedPath = HttpUtility.UrlEncode(fixedFileName);
                        var substituteGroupValue = $"(ViewPage?entry={encodedPath})";
                        content = content.Replace(groupValue, substituteGroupValue);
                    }
                }
                catch { /* Do nothing */ }
            }

            return content;
        }

        private static string FixUpLanFileLinks(string content)
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

        private static string FixUpGithubTables(string content)
        {
            var tableParser = new GithubTableParser();
            return tableParser.Parse(content);
        }

        private static string FixUpTableOfContents(string content)
        {
            var tableOfContentsParser = new TableOfContentsParser();
            return tableOfContentsParser.Parse(content);
        }

    }
}