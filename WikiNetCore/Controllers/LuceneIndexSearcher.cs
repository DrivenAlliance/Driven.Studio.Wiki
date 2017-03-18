using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Ajax.Utilities;
using WikiNetCore.Models;

namespace WikiNetCore.Controllers
{
    public class LuceneIndexSearcher : IWikiContentSearcher
    {
        private readonly string _indexDir;

        public LuceneIndexSearcher(string indexDir)
        {
            _indexDir = indexDir;
        }

        public IEnumerable<DocumentResult> Search(string searchTerm)
        {
            var directory = new SimpleFSDirectory(new DirectoryInfo(_indexDir), new NativeFSLockFactory());

            using (var searcher = new IndexSearcher(directory, true))
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                var parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "Entry", "Content" }, analyzer);
                var query = parser.Parse(searchTerm);
                var hits = searcher.Search(query, 1000).ScoreDocs;

                var documents = hits
                    .Select(hit => searcher.Doc(hit.Doc))
                    .ToList();

                return documents
                    .Select(
                        document =>
                            new DocumentResult
                            {
                                FileName = document.Get("Entry"),
                                DisplayText = document.Get("Entry").CreateDisplayTextFromFileName()
                            })
                    .DistinctBy(result => result.FileName)
                    .ToList();
            }
        }
    }
}