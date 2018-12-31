using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class News
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsPinned { get; set; }

        public DateTime PublishedAt { get; set; }
    }
}
