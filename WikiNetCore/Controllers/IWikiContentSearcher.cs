using System.Collections.Generic;
using WikiNetCore.Models;

namespace WikiNetCore.Controllers
{
    public interface IWikiContentSearcher
    {
        IEnumerable<DocumentResult> Search(string searchTerm);
    }
}