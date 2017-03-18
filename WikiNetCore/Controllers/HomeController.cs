using System;
using Microsoft.AspNetCore.Mvc;
using WikiNetCore.Models;
using WikiNetCore.Parsers;

namespace WikiNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly Settings _settings;
        private readonly IWikiContentSearcher _wikiContentSearcher;

        public HomeController(Settings settings, IWikiContentSearcher wikiContentSearcher)
        {
            _settings = settings;
            _wikiContentSearcher = wikiContentSearcher;
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
                    var results = _wikiContentSearcher.Search(searchModel.SearchTerm);
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
            var wikiContentRelativePath = _settings.WikiContentRelativePath;
            var markdownConverter = new MarkdownConverter(entry, baseUrl, $"/{wikiContentRelativePath}/", _settings);
            var markedDownContent = markdownConverter.Convert();

            return new MarkdownResult
            {
                Title = entry,
                Content = markedDownContent
            };
        }
    }
}
