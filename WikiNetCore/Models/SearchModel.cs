using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarkdownWiki.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
            Results = new List<DocumentResult>();
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must supply a search term")]
        public string SearchTerm { get; set; }

        public List<DocumentResult> Results { get; set; }
    }
}