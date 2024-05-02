using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Challenge Context")]
public class ChallengeDefinition : ScriptableObject
{
    public string displayName;
    public string[] branches; //Constraint: Branches must be in history of HEAD or MERGE_HEAD

    [Space]
    [SerializeField] [TextArea] private string guidsRequired; //Newline-separated list: These must show up
    [SerializeField] [TextArea] private string guidsBanned; //Newline-separated list: These must not show up

    internal IEnumerable<string> GuidsRequired => guidsRequired.SplitList();
    internal IEnumerable<string> GuidsBanned   => guidsBanned  .SplitList();

    [Space]
    public AssetContentConstraint[] assetContentConstraints; //Constraints: Assets must contain (or not contain) IDs

    internal string MenuPath => $"Learn Unity Merging/Verify {displayName}";
}
