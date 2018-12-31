using System.Collections.Generic;
namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetPagingResponse<T>
    {
        private GetPagingResponse() { }

        public GetPagingResponse(IEnumerable<T> result, int total, int size)
        {
            Result = result;
            Count = (total + size - 1) / size;
            Size = size;
            Total = total;
        }

        public IEnumerable<T> Result { get; set; }

        public int Count { get; set; }

        public int Size { get; set; }

        public int Total { get; set; }
    }
}
