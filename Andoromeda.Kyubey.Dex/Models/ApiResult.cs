using System;
namespace Andoromeda.Kyubey.Dex.Models
{
    public class ApiResult<T>
    {
        public int code { get; set; }

        public string msg { get; set; }

        public T data { get; set; }

        public object request { get; set; }
    }

    public class ApiResult : ApiResult<object>
    {
    }
}
