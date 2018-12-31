namespace Andoromeda.Kyubey.Dex.Extensions
{
    public static class StringExtensions
    {
        public static string TrimEnd(this string source, string trimString)
        {
            if (!source.EndsWith(trimString))
                return source;

            return source.Remove(source.LastIndexOf(trimString));
        }
    }
}
