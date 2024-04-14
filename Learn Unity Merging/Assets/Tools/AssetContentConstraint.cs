using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssetContentConstraint
{
    [SerializeField] private UnityEngine.Object asset;
    [SerializeField] [TextArea] private string idsRequired; //Newline-separated list: These must show up
    [SerializeField] [TextArea] private string idsBanned; //Newline-separated list: These must not show up

    public List<ReportedIssue> Evaluate()
    {
        List<ReportedIssue> @out = new List<ReportedIssue>();
        
        return @out;
    }
}
