using System;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WikiNetCore.Controllers;
using WikiNetCore.Parsers;
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

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
                            _ => new Func<IWikiContentSearcher>(() => new LuceneIndexSearcher(Path.Combine(contentRootDir, "lucene_index")))));
                    c.Add(
                        // Ugly injection of factory method. Consider a better DI container that can do this out of the box...
                        ServiceDescriptor.Transient(
                            _ => new Func<MarkdownConverter>(() => new MarkdownConverter(settings))));
                })
                .Build();

            // todo: can this be done on a seperate thread?
            indexWikiContent(luceneIndexPath, settings.AbsoluteWikiContentPath, settings);

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

        private static void indexWikiContent(string indexPath, string wikiContentPath, Settings settings)
        {
            var directory = new SimpleFSDirectory(new DirectoryInfo(indexPath), new NativeFSLockFactory());

            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            {
                // Expire any old indexes
                writer.DeleteAll();

                // Build new index
                var wikiDocs = new DirectoryInfo(wikiContentPath).GetFiles("*.md", SearchOption.AllDirectories);
                foreach (var doc in wikiDocs)
                {
                    string contents;
                    using (var reader = doc.OpenText()) { contents = reader.ReadToEnd(); }

                    var normalizedFileName = settings.MakeRelativeToWikiContentPath(doc.FullName);

                    var luceneDoc = new Document();
                    luceneDoc.Add(new Field("Entry", normalizedFileName, Field.Store.YES, Field.Index.ANALYZED));
                    luceneDoc.Add(new Field("Content", contents, Field.Store.YES, Field.Index.ANALYZED));

                    writer.AddDocument(luceneDoc);
                }

                writer.Optimize();
                writer.Flush(true, true, true);
            }
        }

        private static void log(string message)
        {
            // todo: replace with proper logging framework
            Console.WriteLine(message);
        }
    }
}
