using System.Collections.Generic;

public static class Extensions
{
    public static string StringJoin(this IEnumerable<string> list, string separator)
    {
        string result = string.Join(separator, list);
        return result;
    }
}