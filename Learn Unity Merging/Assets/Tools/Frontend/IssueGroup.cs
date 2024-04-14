using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

internal class IssueGroup : VisualElement
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
