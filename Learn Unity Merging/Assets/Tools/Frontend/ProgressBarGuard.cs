using System;
using UnityEditor;

public class ProgressBarGuard : IDisposable
{
    public void Dispose()
    {
        EditorUtility.ClearProgressBar();
    }
}
