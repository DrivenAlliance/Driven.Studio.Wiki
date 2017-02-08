using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WikiNetCore.Handlers
{
public class ImageHandler
    {
        public ImageHandler(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "image/*";
            await context.Response.SendFileAsync(
                @"C:\Projects\Driven.Studio.Wiki\WikiNetCore\wikicontent\Attachments\IMG_8752.PNG");
        }
    }

    public static class ImageHandlerExtensions
    {
        public static IApplicationBuilder UseImageHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageHandler>();
        }
    }
}