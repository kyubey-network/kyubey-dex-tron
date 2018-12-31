using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetNewsListResponse
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public bool Pinned { get; set; }

        public DateTime Time { get; set; }
    }
}
