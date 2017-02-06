using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MarkdownWiki.Controllers
{
    public class NavigationController : Controller
    {
        private readonly MenuTreeCreator _menuTreeCreator = new MenuTreeCreator();

        public ActionResult GetMenuTree()
        {
            return new JsonResult()
            {
                Data = _menuTreeCreator.GetMenuTreeForDirectory(Settings.WikiPath),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}