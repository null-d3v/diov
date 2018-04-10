using System;
using System.Collections.Generic;

namespace Diov.Data
{
    public class SearchResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}