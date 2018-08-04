using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MediaSearcher.ViewModels
{
    public class SearchViewModel
    {
        [Required]
        public string Keyword { get; set; }

        [Required]
        public int SampleSize { get; set; }

        public System.Collections.Generic.IEnumerable<System.Web.Mvc.ModelError> Errors = null;

        public SearchViewModel()
        {
        }
    }
}