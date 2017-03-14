using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarkdownWiki.Models;
using WikiNetCore;

namespace MarkdownWiki.Controllers
{
    public class MenuTreeCreator
    {
        public IEnumerable<MenuTreeNode> GetMenuTreeForDirectory(string parentDir)
        {
            return Directory.GetDirectories(parentDir)
                .OrderBy(dir => dir)
                .Select(dir =>
                    new MenuTreeNode()
                    {
                        text = dir.Replace(Settings.Instance.AbsoluteWikiContentPath, ""),
                        nodes = GetMenuTreeForDirectory(dir).Union(GetFiles(dir))
                    })
                .ToList();
        }

        private IEnumerable<MenuTreeNode> GetFiles(string dir)
        {
            return Directory.GetFiles(dir,"*.md")
                .Select(fileName =>
                    new MenuTreeNode()
                    {
                        text = fileName.Replace(dir,"").Replace(".md","").Replace("\\",""),
                        href = CreateLinkFromFileName(fileName)
                    });
        }

        private static string CreateLinkFromFileName(string fileName)
        {
            return $"/Home/ViewPage?entry={fileName.NormalizeFileName()}";
        }
    }
}