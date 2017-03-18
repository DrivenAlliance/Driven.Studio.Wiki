using System.Collections.Generic;
using System.IO;
using System.Linq;
using WikiNetCore.Models;

namespace WikiNetCore.Controllers
{
    public class MenuTreeCreator
    {
        private readonly Settings _settings;

        public MenuTreeCreator(Settings settings)
        {
            _settings = settings;
        }

        public IEnumerable<MenuTreeNode> GetMenuTreeForDirectory(string parentDir)
        {
            return Directory.GetDirectories(parentDir)
                .OrderBy(dir => dir)
                .Select(dir =>
                    new MenuTreeNode()
                    {
                        text = dir.Replace(parentDir, ""),
                        nodes = GetMenuTreeForDirectory(dir).Union(getFiles(dir))
                    })
                .ToList();
        }

        private IEnumerable<MenuTreeNode> getFiles(string dir)
        {
            return Directory.GetFiles(dir,"*.md")
                .Select(fileName =>
                    new MenuTreeNode()
                    {
                        text = fileName.Replace(dir,"").Replace(".md","").Replace("\\",""),
                        href = createLinkFromFileName(fileName)
                    });
        }

        private string createLinkFromFileName(string fileName)
        {
            return $"/Home/ViewPage?entry={_settings.MakeRelativeToWikiContentPath(fileName)}";
        }
    }
}