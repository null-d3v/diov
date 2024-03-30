namespace Diov.Data;

public class SearchResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [ ];

    public int Skip { get; set; }

    public int Take { get; set; }

    public int TotalCount { get; set; }
}
