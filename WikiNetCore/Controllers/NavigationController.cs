using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WikiNetCore;

namespace MarkdownWiki.Controllers
{
    public class NavigationController : Controller
    {
        private readonly MenuTreeCreator _menuTreeCreator = new MenuTreeCreator();

        public IActionResult GetMenuTree()
        {
            return new JsonResult(_menuTreeCreator.GetMenuTreeForDirectory(Settings.Instance.AbsoluteWikiContentPath));

            // todo: Not supported in AspNetCore.Mvc?
            //return new JsonResult()
            //{
            // Data = _menuTreeCreator.GetMenuTreeForDirectory(Settings.WikiPath),
            //JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};
        }
    }
}