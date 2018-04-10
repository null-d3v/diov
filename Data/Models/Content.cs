using System;

namespace Diov.Data
{
    public class Content
    {
        private string _body;

        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;  // TODO Sanitize on set
            }
        }
        public int Id { get; set; }
        public bool IsIndexed { get; set; }
        public string Name { get; set; }
        public string Path { get; set; } // TODO Path encode on set
        public DateTimeOffset PublishedDateTime { get; set; }
        public string Summary { get; set; }
    }
}