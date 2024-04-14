using System;
using UnityEditor;
using UnityEngine;

struct ProgressBarGroup
{
    private float minProgress, maxProgress;
    private int currentTaskId;
    private int taskCount;
    public string title;
    public string prefix;
    public ProgressBarGroup(int taskCount, string title)
    {
        minProgress = 0;
        maxProgress = 1;
        currentTaskId = 0;
        this.taskCount = taskCount;
        this.title = title;
        prefix = "";
    }

    public float Value => Mathf.Lerp(minProgress, maxProgress, currentTaskId/(float)taskCount);

    private string Format(string desc) => $"{prefix}({currentTaskId}/{taskCount}) {desc}";

    /// <summary>
    /// Incompatible with Subtask. Choose one or the other!
    /// </summary>
    public void MarkDone(string itemDesc)
    {
        currentTaskId++;
        EditorUtility.DisplayProgressBar(title, Format(itemDesc), Value);
    }

    /// <summary>
    /// Incompatible with MarkDone. Choose one or the other!
    /// </summary>
    public ProgressBarGroup Subtask(int subtaskCount, string subtaskDesc)
    {
        ProgressBarGroup @out = new ProgressBarGroup(subtaskCount, this.title);
        @out.minProgress = Value;
        currentTaskId++;
        @out.maxProgress = Value;
        @out.prefix = Format(subtaskDesc) + " - ";
        return @out;
    }
}
