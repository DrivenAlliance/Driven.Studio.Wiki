using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace WikiNetCore
{
    public class WikiContentIndexer
    {
        private readonly Settings _settings;

        public WikiContentIndexer(Settings settings)
        {
            _settings = settings;
        }

        public void Index(string indexPath, string contentPath)
        {
            var directory = new SimpleFSDirectory(new DirectoryInfo(indexPath), new NativeFSLockFactory());

            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            {
                // Expire any old indexes
                writer.DeleteAll();

                // Build new index
                var wikiDocs = new DirectoryInfo(contentPath).GetFiles("*.md", SearchOption.AllDirectories);
                foreach (var doc in wikiDocs)
                {
                    string contents;
                    using (var reader = doc.OpenText()) { contents = reader.ReadToEnd(); }

                    var normalizedFileName = _settings.MakeRelativeToWikiContentPath(doc.FullName);

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