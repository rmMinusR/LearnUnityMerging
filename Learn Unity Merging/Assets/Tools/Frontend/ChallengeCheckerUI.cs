using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ChallengeCheckerUI : EditorWindow
{
    [InitializeOnLoadMethod]
    [MenuItem("Learn Unity Merging/Refresh list", priority = 201)]
    static void RefreshList()
    {
        //Remove stale items
        foreach (string i in menuItems) RemoveMenuItem(i);

        //Refresh list
        challenges.Clear();
        challenges.AddRange(Resources.FindObjectsOfTypeAll<ChallengeDefinition>());
        menuItems.Clear();

        //Add items back
        foreach (ChallengeDefinition i in challenges)
        {
            AddMenuItem(i.MenuPath, null, false, 101,
                () => CreateWindow<ChallengeCheckerUI>().Init(i),
                () => true
            );
            menuItems.Add(i.MenuPath);
        }
    }
    private static List<string> menuItems = new List<string>();
    private static List<ChallengeDefinition> challenges = new List<ChallengeDefinition>();


    static void AddMenuItem(string name, string shortcut, bool @checked, int priority, Action execute, Func<bool> validate)
    {
        typeof(Menu).GetMethod("AddMenuItem", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { name, shortcut, @checked, priority, execute, validate });
    }

    static void RemoveMenuItem(string name)
    {
        typeof(Menu).GetMethod("RemoveMenuItem", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { name });
    }


    [SerializeField] ChallengeDefinition challenge;
    private void Init(ChallengeDefinition challenge)
    {
        this.challenge = challenge;
        titleContent = new GUIContent("Challenge checker");

        CreateGUI();

        ExecTests();
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        if (root.childCount > 0) return;

        titleLbl = new Label();
        if (challenge != null) titleLbl.text = $"Verify challenge: {challenge.displayName}";
        titleLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(titleLbl);

        Button btnCheck = new Button(ExecTests);
        btnCheck.Add(new Label("Re-check"));
        root.Add(btnCheck);

        Label conflictMarkersLbl = new Label("Conflicts:");
        conflictMarkersLbl.style.paddingTop = 20;
        conflictMarkersLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(conflictMarkersLbl);
        root.Add(conflictMarkers = new IssueList());

        Label objectPresenceLbl = new Label("Object presence:");
        objectPresenceLbl.style.paddingTop = 20;
        objectPresenceLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(objectPresenceLbl);
        root.Add(objectPresenceIssues = new IssueList());

        Label contentManglingLbl = new Label("Content mangling:");
        contentManglingLbl.style.paddingTop = 20;
        contentManglingLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(contentManglingLbl);
        root.Add(contentManglingIssues = new IssueList());

        SetMessages(MSG_NOT_YET_EVALUATED);
    }

    Label titleLbl;
    IssueList conflictMarkers;
    IssueList objectPresenceIssues;
    IssueList contentManglingIssues;

    private static readonly string MSG_NOT_YET_EVALUATED= "No data. Press the Re-check button!";
    private static readonly string MSG_CANNOT_EVALUATE = "Cannot check. Fix other issues first.";
    private void SetMessages(string msg)
    {   
        conflictMarkers      .SetMessage(msg);
        objectPresenceIssues .SetMessage(msg);
        contentManglingIssues.SetMessage(msg);
    }

    private void ExecTests()
    {
        SetMessages(MSG_CANNOT_EVALUATE);

        using ProgressBarGuard pbg = new ProgressBarGuard();

        List<ReportedIssue> issues = new List<ReportedIssue>();
        ExecTests_CheckForConflicts("t:prefab", issues, "(1/4) Checking prefabs for conflicts...", 0.0f, 0.25f);
        ExecTests_CheckForConflicts("t:scene" , issues, "(2/4) Checking scenes for conflicts..." , 0.25f, 0.5f);
        conflictMarkers.Write(issues);

        if (conflictMarkers.IssueCount != 0) return;

        EditorUtility.DisplayProgressBar("Checking challenge", "(3/4) Scanning scenes for content... {path}", 0.6f);
        issues.Clear();
        //TODO implement
        objectPresenceIssues.Write(issues);
        
        EditorUtility.DisplayProgressBar("Checking challenge", "(4/4) Scanning scenes for content... {path}", 0.8f);
        issues.Clear();
        //TODO implement
        contentManglingIssues.Write(issues);
    }

    private void ExecTests_CheckForConflicts(string filter, List<ReportedIssue> issues_out, string progressPrefix, float progressRangeBase, float progressRangeMax)
    {
        EditorUtility.DisplayProgressBar("Checking challenge", $"{progressPrefix} - looking for files...", progressRangeBase);
        string[] guids = AssetDatabase.FindAssets(filter);
        float progressPerItem = guids.Length/(progressRangeMax-progressRangeBase);
        for (int i = 0; i < guids.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar("Checking challenge", $"{progressPrefix} - {path}", progressRangeBase+i*progressPerItem);
            issues_out.AddRange(GeneralChecks.GetConflicts(path));
        }
    }
}
