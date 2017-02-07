using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MarkdownWiki.Controllers
{
    public class NavigationController : Controller
    {
        private readonly MenuTreeCreator _menuTreeCreator = new MenuTreeCreator();

        public ActionResult GetMenuTree()
        {
            return new JsonResult(_menuTreeCreator.GetMenuTreeForDirectory(Settings.WikiPath));

            // todo: Not supported in AspNetCore.Mvc?
            //return new JsonResult()
            //{
            // Data = _menuTreeCreator.GetMenuTreeForDirectory(Settings.WikiPath),
            //JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //};
        }
    }
}