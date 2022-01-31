namespace Diov.Data;

public class SearchResponse<T>
{
    public IEnumerable<T> Items { get; set; } =
        Enumerable.Empty<T>();

    public int Skip { get; set; }

    public int Take { get; set; }

    public int TotalCount { get; set; }
}
