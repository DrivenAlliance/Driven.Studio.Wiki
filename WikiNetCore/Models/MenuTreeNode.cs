using System.Collections.Generic;

namespace WikiNetCore.Models
{
    public class MenuTreeNode
    {
        public string text { get; set; }
        public string href { get; set; }
        public IEnumerable<MenuTreeNode> nodes { get; set; }
    }
}