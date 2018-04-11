using System;
using System.Web;
using Ganss.XSS;

namespace Diov.Data
{
    public class Content
    {
        private static IHtmlSanitizer htmlSanitizer =
            new HtmlSanitizer();

        private string body;
        private string path;
        private string summary;

        public string Body
        {
            get
            {
                return body;
            }
            set
            {
                body = htmlSanitizer.Sanitize(value);
            }
        }
        public int Id { get; set; }
        public bool IsIndexed { get; set; }
        public string Name { get; set; }
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                value = value.ToLowerInvariant();
                var encodedPath = HttpUtility.UrlPathEncode(value);
                if (!string.Equals(
                    value,
                    encodedPath,
                    StringComparison.InvariantCulture))
                {
                    throw new ArgumentException();
                }
                path = encodedPath;
            }
        }
        public DateTimeOffset PublishedDateTime { get; set; }
        public string Summary { get; set; }
    }
}