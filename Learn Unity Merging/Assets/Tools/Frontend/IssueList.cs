using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal class IssueList : VisualElement
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
