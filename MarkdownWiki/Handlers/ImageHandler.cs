using System.IO;
using System.Web;

namespace MarkdownWiki.Handlers
{
    public class ImageHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var imagePath = context.Request.QueryString["imagePath"];

            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var normalizedPath = imagePath.Replace("../", string.Empty).Replace("/", @"\");
            var fullPath = Path.Combine(Settings.WikiPath, normalizedPath);
            if (!File.Exists(fullPath)) return;

            context.Response.Headers["Content-Type"] = "image/*";
            context.Response.WriteFile(fullPath);
        }

        public bool IsReusable => true;
    }
}