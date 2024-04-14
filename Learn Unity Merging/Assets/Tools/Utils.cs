using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

static class Utils
{
    public static IEnumerable<string> SplitList(this string block, string itemDelim = "\n", string commentDelim = "#")
    {
        return block
            .Split(itemDelim)
            .Select(i => i.Trim())
            .Where(i => i.Length > 0 && !i.StartsWith(commentDelim));
    }

    public static IEnumerable<string> ListLocalIDs(string assetPath)
    {
        string[] lines = System.IO.File.ReadAllLines(assetPath);
        return lines.Where(i => i.StartsWith("--- !u!")).Select(i => i.Split("&")[1]);
    }

    public static IEnumerable<ObjectBlock> GetRawObjects(string assetPath)
    {
        string lastSeenHeader = null;
        List<string> content = new List<string>();
        foreach (string line in System.IO.File.ReadAllLines(assetPath))
        {
            if (line.StartsWith("--- !u!"))
            {
                if (lastSeenHeader != null) yield return new ObjectBlock(lastSeenHeader, content.ToArray());
                content.Clear();
                lastSeenHeader = line;
            }
            else content.Add(line);
        }
        if (lastSeenHeader != null) yield return new ObjectBlock(lastSeenHeader, content.ToArray());
    }
}
