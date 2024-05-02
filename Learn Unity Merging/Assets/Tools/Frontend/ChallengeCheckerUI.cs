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
    #region Opening checker

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

    #endregion

    private void CreateGUI()
    {
        if (challenge != null && titleLbl != null) titleLbl.text = $"Verify challenge: {challenge.displayName}";

        VisualElement root = rootVisualElement;
        if (root.childCount > 0) return;

        titleLbl = new Label();
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
        List<string> prefabs = GetFilesByFilter("t:prefab").ToList();
        List<string> scenes  = GetFilesByFilter("t:scene" ).ToList();
        List<string> allManaged = new List<string>(); allManaged.AddRange(prefabs); allManaged.AddRange(scenes);

        SetMessages(MSG_CANNOT_EVALUATE);
        using ProgressBarGuard pbg = new ProgressBarGuard();
        List<ReportedIssue> issues = new List<ReportedIssue>();

        ProgressBarGroup testTaskMonitor = new ProgressBarGroup(4, "Checking challenge");

        //Scan for conflict markers
        {
            issues.Clear();
            ExecTests_CheckForConflicts(prefabs, issues, testTaskMonitor.Subtask(prefabs.Count, "Checking prefabs for conflicts"));
            ExecTests_CheckForConflicts(scenes , issues, testTaskMonitor.Subtask(scenes .Count, "Checking scenes for conflicts" ));
            conflictMarkers.Write(issues);

            //Don't go further if we have conflicts in files -- we'll prob just feed bad information to the user
            if (conflictMarkers.IssueCount != 0) return;
        }

        //Check content IDs
        {
            //All GUIDs that should be present are present (and all that shouldn't, isn't)
            issues.Clear();

            ExecTests_CheckGUIDs(challenge.GuidsRequired.ToList(), challenge.GuidsBanned.ToList(), issues, testTaskMonitor.Subtask(1, "Scanning for mismatched files..."));
            objectPresenceIssues.Write(issues);

            //All local file content that should be present is present (and all that shouldn't, isn't)

            ProgressBarGroup contentTaskMonitor = testTaskMonitor.Subtask(challenge.assetContentConstraints.Length, "Scanning for local file content for mismatches...");
            foreach (AssetContentConstraint c in challenge.assetContentConstraints)
            {
                contentTaskMonitor.MarkDone(AssetDatabase.GetAssetPath(c.Asset));
                issues.AddRange(c.Evaluate());
            }

            objectPresenceIssues.Write(issues);
        }

        //All fields expected on the given object are present
        {
            ProgressBarGroup contentTaskMonitor = testTaskMonitor.Subtask(prefabs.Count+scenes.Count, "Scanning content for mangling...");
            issues.Clear();
            foreach (string i in prefabs) ExecTests_CheckMangling(i, issues, ref contentTaskMonitor);
            foreach (string i in scenes ) ExecTests_CheckMangling(i, issues, ref contentTaskMonitor);
            contentManglingIssues.Write(issues);
        }
    }

    private static IEnumerable<string> GetFilesByFilter(string filter)
    {
        foreach (string guid in AssetDatabase.FindAssets(filter))
        {
            yield return AssetDatabase.GUIDToAssetPath(guid);
        }
    }

    private static void ExecTests_CheckForConflicts(IReadOnlyList<string> paths, List<ReportedIssue> issues_out, ProgressBarGroup taskMonitor)
    {
        for (int i = 0; i < paths.Count; ++i)
        {
            taskMonitor.MarkDone(paths[i]);
            issues_out.AddRange(GeneralChecks.GetConflicts(paths[i]));
        }
    }

    private static void ExecTests_CheckGUIDs(IReadOnlyList<string> required, IReadOnlyList<string> banned, List<ReportedIssue> issues_out, ProgressBarGroup taskMonitor)
    {
        taskMonitor.taskCount = required.Count + banned.Count;

        foreach (string i in required)
        {
            taskMonitor.MarkDone(i);
            string assetPath = AssetDatabase.GUIDToAssetPath(i);
            if (assetPath.Length == 0) issues_out.Add(new ReportedIssue("GUIDs", $"Deleted, but should exist: {i}"));
        }

        foreach (string i in banned)
        {
            taskMonitor.MarkDone(i);
            string assetPath = AssetDatabase.GUIDToAssetPath(i);
            if (assetPath.Length != 0) issues_out.Add(new ReportedIssue("GUIDs", $"{assetPath} was deleted on one branch, but still exists"));
        }
    }

    private static void ExecTests_CheckMangling(string path, List<ReportedIssue> issues_out, ref ProgressBarGroup taskMonitor)
    {
        List<ObjectBlock> rawObjects = Utils.GetRawObjects(path).ToList();
        taskMonitor.taskCount = rawObjects.Count;
        foreach (ObjectBlock obj in rawObjects)
        {
            taskMonitor.MarkDone(obj.id);
            //TODO implement
        }
    }
}
