using Microsoft.AspNetCore.Mvc;

namespace WikiNetCore.Controllers
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
            return new JsonResult(_menuTreeCreator.GetMenuTreeForDirectory(_settings.WikiContentFullLocalPath));
        }
    }
}