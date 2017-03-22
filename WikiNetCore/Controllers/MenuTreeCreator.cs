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
            return
                buildSubDirectoryNodes(parentDir)
                .Union(buildFileNodes(parentDir))
                .ToList();
        }

        private IEnumerable<MenuTreeNode> buildSubDirectoryNodes(string parentDir)
        {
            return Directory.GetDirectories(parentDir)
                .OrderBy(dir => dir)
                .Select(dir => buildDirectoryNode(parentDir, dir));
        }

        private MenuTreeNode buildDirectoryNode(string directory, string dir)
        {
            return new MenuTreeNode()
            {
                text = dir.Replace(directory, ""),
                nodes = GetMenuTreeForDirectory(dir)
            };
        }

        private IEnumerable<MenuTreeNode> buildFileNodes(string dir)
        {
            return Directory.GetFiles(dir, "*.md")
                .Select(fileName => buildFileNode(dir, fileName));
        }

        private MenuTreeNode buildFileNode(string dir, string fileName)
        {
            return new MenuTreeNode()
            {
                text = fileName.Replace(dir, "").Replace(".md", "").Replace("\\", ""),
                href = createLinkFromFileName(fileName)
            };
        }

        private string createLinkFromFileName(string fileName)
        {
            return $"/Home/ViewPage?entry={_settings.MakeRelativeToWikiContentPath(fileName)}";
        }
    }
}