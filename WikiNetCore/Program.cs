using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WikiNetCore.Controllers;
using WikiNetCore.Parsers;

namespace WikiNetCore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var contentRootDir = Directory.GetCurrentDirectory();
            var webRootDir = Path.Combine(contentRootDir, "wwwroot");
            var luceneIndexPath = Path.Combine(contentRootDir, "lucene_index");
            var configBasePath = Directory.GetCurrentDirectory();
            var configuration = buildConfiguration(args, configBasePath);
            var settings = new Settings(webRootDir, configuration.GetSection("CanaryWiki"));

            log($"Wiki content path is [{settings.AbsoluteWikiContentPath}]");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRootDir)
                .UseWebRoot(webRootDir)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .ConfigureServices(c =>
                {
                    c.Add(ServiceDescriptor.Singleton(configuration));
                    c.Add(ServiceDescriptor.Singleton(settings));
                    c.Add(
                        // Ugly injection of factory method. Consider a better DI container that can do this out of the box...
                        ServiceDescriptor.Transient(
                            _ => new Func<IWikiContentSearcher>(() => new LuceneIndexSearcher(luceneIndexPath))));
                    c.Add(
                        // Ugly injection of factory method. Consider a better DI container that can do this out of the box...
                        ServiceDescriptor.Transient(
                            _ => new Func<MarkdownConverter>(() => new MarkdownConverter(settings))));
                })
                .Build();

            // todo: can this be done on a seperate thread?
            new WikiContentIndexer(settings).Index(luceneIndexPath, settings.AbsoluteWikiContentPath);

            host.Run();
        }

        private static IConfigurationRoot buildConfiguration(string[] args, string contentRootDir)
        {
            return new ConfigurationBuilder()
                .SetBasePath(contentRootDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                // todo: figure out how to get at current environment without an IHostingEnvironment?
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("hosting.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void log(string message)
        {
            // todo: replace with proper logging framework
            Console.WriteLine(message);
        }
    }
}
