using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using System.IO;
using System.Linq;

public class GeneralChecks
{
    [Test]
    public void AllFilesMerged()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:scene" )) _CheckConflicts(AssetDatabase.GUIDToAssetPath(guid));
        foreach (string guid in AssetDatabase.FindAssets("t:prefab")) _CheckConflicts(AssetDatabase.GUIDToAssetPath(guid));
    }

    void _CheckConflicts(string filePath)
    {
        foreach (var i in GetConflicts(filePath)) Assert.Fail($"{i.trace}: {i.description}");
    }

    public IEnumerable<ReportedIssue> GetConflicts(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (CONFLICT_MARKERS.Any(m => lines[i].StartsWith(m)))
            {
                yield return new ReportedIssue(filePath, $"Conflict marker on line {i+1}");
            }
        }
    }
    private static readonly string[] CONFLICT_MARKERS = { "<<<<", "====", "||||", ">>>>" };
}
