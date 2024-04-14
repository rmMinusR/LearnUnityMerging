using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class VerificationFrontend : EditorWindow
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
                () => CreateWindow<VerificationFrontend>().Init(i),
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

        List<ReportedIssue> issues = new List<ReportedIssue>();
        foreach (string guid in AssetDatabase.FindAssets("t:scene" )) issues.AddRange(GeneralChecks.GetConflicts(AssetDatabase.GUIDToAssetPath(guid)));
        foreach (string guid in AssetDatabase.FindAssets("t:prefab")) issues.AddRange(GeneralChecks.GetConflicts(AssetDatabase.GUIDToAssetPath(guid)));
        conflictMarkers.Write(issues);
        if (conflictMarkers.IssueCount != 0) return;

        issues.Clear();
        //TODO implement
        objectPresenceIssues.Write(issues);
        
        issues.Clear();
        //TODO implement
        contentManglingIssues.Write(issues);
    }

    class IssueList : VisualElement
    {
        public IssueList() { }

        private Dictionary<string, IssueGroup> groups = new Dictionary<string, IssueGroup>();

        public int IssueCount => groups.Count;

        public void SetMessage(string msg)
        {
            Clear();
            
            Label lbl = new Label(msg);
            lbl.style.unityFontStyleAndWeight = FontStyle.Italic;
            Add(lbl);
        }

        public void Write(IEnumerable<ReportedIssue> issues)
        {
            Clear();
            groups.Clear();

            foreach (ReportedIssue i in issues) AddIssue(i);

            if (IssueCount == 0) SetMessage("No issues detected");
        }

        private void AddIssue(ReportedIssue issue)
        {
            IssueGroup group;
            if (!groups.TryGetValue(issue.trace, out group))
            {
                group = new IssueGroup(issue.trace);
                groups[issue.trace] = group;
                Add(group);
            }

            group.AddIssue(issue);
        }
    }

    class IssueGroup : VisualElement
    {
        public string Trace { get; private set; }
        public IssueGroup(string trace)
        {
            this.Trace = trace;

            foldout = new Foldout();
            foldout.text = trace;
            foldout.contentContainer.style.paddingLeft = 20;
            Add(foldout);

            if (File.Exists(trace))
            {
                Button revealBtn = new Button(Reveal);
                revealBtn.Add(new Label("Reveal"));
                foldout.Add(revealBtn);
            }
        }

        Foldout foldout;

        public void AddIssue(ReportedIssue issue)
        {
            Label descLabel = new Label(issue.description);
            descLabel.style.flexGrow = 3;
            foldout.Add(descLabel);
        }

        public void Reveal()
        {
            EditorUtility.RevealInFinder(Trace);
        }
    }
}
