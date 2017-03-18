using System;
using Microsoft.AspNetCore.Mvc;
using WikiNetCore.Models;
using WikiNetCore.Parsers;

namespace WikiNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly Func<IWikiContentSearcher> _searcherFactory;
        private readonly Func<MarkdownConverter> _markdownConverterFactory;

        public HomeController(Func<IWikiContentSearcher> searcherFactory, Func<MarkdownConverter> markdownConverterFactory)
        {
            _searcherFactory = searcherFactory;
            _markdownConverterFactory = markdownConverterFactory;
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
                try
                {
                    var searcher = _searcherFactory();
                    var results = searcher.Search(searchModel.SearchTerm);
                    searchModel.Results.AddRange(results);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(searchModel);
                }
            }

            return View(searchModel);
        }

        public ActionResult ViewPage(string entry)
        {
            return View(renderEntryFromMarkdown(entry, "ViewPage?entry="));
        }

        public ActionResult ViewContentOnly(string entry)
        {
            return View(renderEntryFromMarkdown(entry, "ViewContentOnly?entry="));
        }

        private MarkdownResult renderEntryFromMarkdown(string entry, string baseUrl)
        {
            var markdownConverter = _markdownConverterFactory();
            var markedDownContent = markdownConverter.Convert(entry, baseUrl);

            return new MarkdownResult
            {
                Title = entry,
                Content = markedDownContent
            };
        }
    }
}
