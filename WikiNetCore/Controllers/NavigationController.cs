using Microsoft.AspNetCore.Mvc;
using WikiNetCore;

namespace MarkdownWiki.Controllers
{
    public class NavigationController : Controller
    {
        private readonly Settings _settings;
        private readonly MenuTreeCreator _menuTreeCreator;

        public NavigationController(Settings settings)
        {
            _settings = settings;
            _menuTreeCreator = new MenuTreeCreator(_settings);
        }

        public IActionResult GetMenuTree()
        {
            return new JsonResult(_menuTreeCreator.GetMenuTreeForDirectory(_settings.AbsoluteWikiContentPath));

            // todo: Not supported in AspNetCore.Mvc?
            //return new JsonResult()
            //{
            // Data = _menuTreeCreator.GetMenuTreeForDirectory(Settings.WikiPath),
            //JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};
        }
    }
}