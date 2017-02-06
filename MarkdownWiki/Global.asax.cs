using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MarkdownWiki.Controllers;

namespace MarkdownWiki
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BuildDocumentIndex();
        }

        private static void BuildDocumentIndex()
        {
            var luceneDir = Path.Combine(HttpRuntime.AppDomainAppPath, "lucene_index");
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
