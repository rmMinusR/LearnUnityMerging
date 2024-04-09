using System;
using System.Collections;
using System.Collections.Generic;
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


    ChallengeDefinition challenge;
    private void Init(ChallengeDefinition challenge)
    {
        this.challenge = challenge;
        titleContent = new GUIContent("Challenge checker");

        VisualElement root = rootVisualElement;

        Label title = new Label($"Verify challenge: {challenge.displayName}");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(title);

        Button btnCheck = new Button(ExecTests);
        btnCheck.Add(new Label("Re-check"));
        root.Add(btnCheck);
    }

    private void ExecTests()
    {

    }
}
