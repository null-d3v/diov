using Diov.Data;

namespace Diov.Web
{
    public class ContentIndexModel
    {
        public bool IsIndexed { get; set; }
        public SearchResponse<Content> SearchResponse { get; set; }
    }
}