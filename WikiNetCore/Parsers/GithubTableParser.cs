using System.IO;
using System.Linq;
using System.Text;

namespace MarkdownWiki.Parsers
{
    public class GithubTableParser
    {
        public string Parse(string content)
        {
            var reader = new StringReader(content);
            var line = "";
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.Contains(" | "))
                {
                    sb.AppendLine(line);
                    continue;
                }

                var firstLine = line;
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    sb.AppendLine(firstLine);
                    continue;
                }

                if (!line.All(x => x == ' ' || x == '|' || x == '-'))
                {
                    sb.AppendLine(line);
                    continue;
                }

                //got a table.
                sb.AppendLine(@"<table class=""table table-striped table-bordered""><thead><tr><th>");
                sb.AppendLine(firstLine.Replace(" | ", "</th><th>"));
                sb.AppendLine("</th></tr></thead><tbody>");
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine("</tbody></table>");
                        sb.AppendLine("<br>\r\n");
                        break;
                    }
                    line = line.Trim('|');
                    sb.Append("<tr>");

                    var oldPos = 0;
                    while (true)
                    {
                        var pos = line.IndexOf('|', oldPos);
                        if (pos == -1)
                        {
                            pos = line.Length;
                        }

                        var cell = line.Substring(oldPos, pos - oldPos);
                        sb.Append("<td> ");
                        var data = cell.Transform().Replace("<p>", "").Replace("</p>", "");
                        sb.Append(data);
                        sb.Append(" </td>");
                        oldPos = pos + 1;
                        if (oldPos >= line.Length)
                        {
                            sb.AppendLine("</tr>");
                            break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}