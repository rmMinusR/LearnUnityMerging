using UnityEngine;

[CreateAssetMenu(menuName = "Challenge Context")]
public class ChallengeDefinition : ScriptableObject
{
    public string displayName;
    public string[] branches; //Constraint: Branches must be in history of HEAD or MERGE_HEAD

    [Space]
    public AssetContentConstraint[] assetContentConstraints; //Constraints: Assets must contain (or not contain) IDs

    internal string MenuPath => $"Learn Unity Merging/Verify {displayName}";
}
