using System;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MarkdownWiki.Controllers;
using MarkdownWiki.Models;
using MarkdownWiki.Parsers;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Hosting;
using Version = Lucene.Net.Util.Version;
using Microsoft.AspNetCore.Mvc;

namespace WikiNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(SearchModel searchModel)
        {
            if (ModelState.IsValid)
            {
                // TODO: Confirm that this is the correct way of finding path
                var appPath = _hostingEnvironment.ContentRootPath;
                //var appPath = HttpRuntime.AppDomainAppPath;

                var luceneDir = Path.Combine(appPath, "lucene_index");
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
                        .Select(document => new DocumentResult { FileName = document.Get("Entry"), DisplayText = document.Get("Entry").CreateDisplayTextFromFileName() })
                        .DistinctBy(result => result.FileName)
                        .ToList();

                    searchModel.Results.AddRange(results);
                }
            }

            return View(searchModel);
        }

        public ActionResult ViewPage(string entry)
        {
            var markedDownContent = TransformExtensions.MarkDownToHtml(entry);

            var result = new MarkdownResult
            {
                Title = entry,
                Content = markedDownContent
            };

            return View(result);
        }
    }
}
