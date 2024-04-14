using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AssetContentConstraint
{
    [SerializeField] private UnityEngine.Object asset;
    [SerializeField] [TextArea] private string localIdsRequired; //Newline-separated list: These must show up
    [SerializeField] [TextArea] private string localIdsBanned; //Newline-separated list: These must not show up

    public UnityEngine.Object Asset => asset;

    public List<ReportedIssue> Evaluate()
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string[] lines = System.IO.File.ReadAllLines(assetPath);
        string[] ids = lines.Where(i => i.StartsWith("--- !u!")).Select(i => i.Split("&")[1]).ToArray();
        
        string[] idsRequired = localIdsRequired.Split("\n").Select(i => i.Trim()).Where(i => i.Length>0 && !i.StartsWith("#")).ToArray();
        string[] idsBanned   = localIdsBanned  .Split("\n").Select(i => i.Trim()).Where(i => i.Length>0 && !i.StartsWith("#")).ToArray();
        
        List<ReportedIssue> @out = new List<ReportedIssue>();
        foreach (string id in idsBanned  .Intersect(ids)) @out.Add(new ReportedIssue(assetPath, $"Object {id} exists, but was deleted on one branch"));
        foreach (string id in idsRequired.Except   (ids)) @out.Add(new ReportedIssue(assetPath, $"Object {id} does not exist, but was added on one branch"));
        foreach (string id in new HashSet<string>(ids)) if (ids.Count(i => i==id) > 1) @out.Add(new ReportedIssue(assetPath, $"Multiple instances of {id} exist"));
        return @out;
    }
}
