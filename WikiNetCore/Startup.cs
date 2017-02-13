using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using MarkdownWiki;
using MarkdownWiki.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WikiNetCore.Handlers;
using Version = Lucene.Net.Util.Version;

namespace WikiNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // todo: can this be done on a seperate thread?
            BuildDocumentIndex(env);

            //app.MapWhen(
            //    // todo: might not be the greatest way of checking for images
            //    context => context.Request.Path.ToString().EndsWith("/ShowImage.ashx"),
            //    appBranch => { appBranch.UseImageHandler(); });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.MapWhen(
                context => context.Request.Path.Value.EndsWith(".md"),
                appBranch => { appBranch.UseMarkdownRenderer(); });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void BuildDocumentIndex(IHostingEnvironment hostingEnvironment)
        {
            // todo: confirm the following is correct
            var appPath = hostingEnvironment.ContentRootPath;
            var luceneDir = Path.Combine(appPath, "lucene_index");

            //var luceneDir = Path.Combine(HttpRuntime.AppDomainAppPath, "lucene_index");
            var directory = new SimpleFSDirectory(new DirectoryInfo(luceneDir), new NativeFSLockFactory());

            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            {
                // Expire any old indexes
                writer.DeleteAll();

                // Build new index
                var wikiDocs = new DirectoryInfo(Settings.WikiPath).GetFiles("*.md", SearchOption.AllDirectories);
                foreach (var doc in wikiDocs)
                {
                    string contents;
                    using (var reader = doc.OpenText()) { contents = reader.ReadToEnd(); }

                    var normalizedFileName = doc.FullName.NormalizeFileName();

                    var luceneDoc = new Document();
                    luceneDoc.Add(new Field("Entry", normalizedFileName, Field.Store.YES, Field.Index.ANALYZED));
                    luceneDoc.Add(new Field("Content", contents, Field.Store.YES, Field.Index.ANALYZED));

                    writer.AddDocument(luceneDoc);
                }

                writer.Optimize();
                writer.Flush(true, true, true);
            }
        }

    }
}
