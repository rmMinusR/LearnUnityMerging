using System.Collections.Generic;
using System.Linq;

static class Utils
{
    public static IEnumerable<string> SplitList(this string block, string itemDelim = "\n", string commentDelim = "#")
    {
        return block
            .Split(itemDelim)
            .Select(i => i.Trim())
            .Where(i => i.Length > 0 && !i.StartsWith(commentDelim));
    }
}
