using System.IO;
using UnityEditor;
using UnityEngine;
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
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            foldout.Add(row);

            Button unityRevealBtn = new Button(RevealInUnity);
            unityRevealBtn.Add(new Label("Show in Unity"));
            row.Add(unityRevealBtn);

            Button nativeRevealBtn = new Button(RevealNative);
            nativeRevealBtn.Add(new Label("Show in folder"));
            row.Add(nativeRevealBtn);
        }
    }

    Foldout foldout;

    public void AddIssue(ReportedIssue issue)
    {
        Label descLabel = new Label(issue.description);
        descLabel.style.flexGrow = 3;
        foldout.Add(descLabel);
    }

    public void RevealNative()
    {
        EditorUtility.RevealInFinder(Trace);
    }

    public void RevealInUnity()
    {
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(Trace));
    }
}
