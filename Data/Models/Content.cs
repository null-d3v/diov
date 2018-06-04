using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Diov.Data
{
    public class Content : IValidatableObject
    {
        private static IHtmlSanitizer htmlSanitizer;

        private string body;
        private string path;

        static Content()
        {
            htmlSanitizer = new HtmlSanitizer();
            htmlSanitizer.AllowedAttributes.Add("class");
        }

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
        [Required]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    path = value;
                }
                else
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
        }
        [DataType(DataType.Date)]
        [Required]
        public DateTimeOffset? PublishedDateTime { get; set; }
        [Required]
        public string Summary { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (IsIndexed && string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult(
                    $"The Name field is required for indexed content.",
                    new[] { "Name", });
            }
        }
    }
}