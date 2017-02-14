using System.Net.Mime;
using System.Threading.Tasks;
using MarkdownWiki.Parsers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebGrease;

namespace WikiNetCore.Handlers
{
    public class MarkdownRenderer
    {
        public MarkdownRenderer(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            var content = TransformExtensions.MarkDownFileToHtml(context.Request.Path);
            await context.Response.WriteAsync(content);
            //context.Response.ContentType = "image/*";
            //await context.Response.SendFileAsync(
            //    @"C:\Projects\Driven.Studio.Wiki\WikiNetCore\wikicontent\Attachments\IMG_8752.PNG");
        }
    }

    public static class MarkdownRendererExtentions
    {
        public static IApplicationBuilder UseMarkdownRenderer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MarkdownRenderer>();
        }
    }

}