using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class VerificationCommon
{
    private static TestRunnerApi __testApi;
    public static TestRunnerApi TestApi => __testApi != null ? __testApi : (__testApi = ScriptableObject.CreateInstance<TestRunnerApi>());

    [MenuItem("Learn Git Branching/Verify")]
    static void Verify()
    {
        //Show test runner
        TestRunnerWindow.ShowWindow();
        TestRunnerWindow testRunnerWindow = EditorWindow.GetWindow<TestRunnerWindow>();
        testRunnerWindow.Focus();

        //Open Play-mode tests
        typeof(TestRunnerWindow)
            .GetMethod("SelectTestListGUI", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(testRunnerWindow, new object[] { 0 });

        //Run tests
        Invoke_RunTests();
    }

    static void Invoke_RunTests()
    {
        EditorApplication.delayCall -= _RunTests;
        EditorApplication.delayCall += _RunTests;
    }

    static void _RunTests()
    {
        EditorApplication.delayCall -= _RunTests;

        //Run tests
        Filter filter = new Filter() { testMode = TestMode.PlayMode };
        TestApi.Execute(new ExecutionSettings(filter));
    }
}
