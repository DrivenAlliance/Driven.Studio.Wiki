using System.Text;
using CsQuery;

namespace WikiNetCore.Parsers
{
    public class TableOfContentsBuilder
    {
        public string Parse(string content)
        {
            var toc = new StringBuilder();
            toc.AppendLine(@"<div class=""toc"">");
            toc.AppendLine("<ul>");
            var doc = new CQ(content);
            var id = 0;
            doc["h1,h2,h3,h4,h5,h6"].Each(el =>
            {
                var myDepth = int.Parse(el.NodeName.Substring(1, 1));
                if (string.IsNullOrEmpty(el.Id))
                {
                    el.SetAttribute("id", id.ToString());
                    id++;
                }
                if (!string.IsNullOrEmpty(el.Id))
                {
                    toc.AppendFormat("<li class=\"heading{2}\"><a href=\"#{0}\">{1}</a></li>\r\n",
                        el.Id,
                        el.InnerText,
                        myDepth);
                }
            });

            toc.AppendLine("</ul>");
            toc.AppendLine("</div>");

            content = doc.Render(); // Apply the new id attributes to the original source so the TOC links will work

            return content.Replace("[[<em>TOC</em>]]", toc.ToString());
        }
    }
}