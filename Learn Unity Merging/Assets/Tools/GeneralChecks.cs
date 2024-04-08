using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using System.IO;

public class GeneralChecks
{
    [Test]
    public void AllFilesMerged()
    {
        foreach (SceneAsset scene in Resources.FindObjectsOfTypeAll<SceneAsset>()) _CheckConflicts(AssetDatabase.GetAssetPath(scene));
        foreach (string prefabGUID in AssetDatabase.FindAssets("t:prefab")) _CheckConflicts(AssetDatabase.GUIDToAssetPath(prefabGUID));
    }

    void _CheckConflicts(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("<<<<") || lines[i].StartsWith("====") || lines[i].StartsWith(">>>>"))
            {
                Assert.Fail($"Conflict marker on line {i+1}");
            }
        }
    }
}
