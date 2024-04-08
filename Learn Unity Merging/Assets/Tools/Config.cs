using LibGit2Sharp;
using System.IO;
using UnityEngine;

static class Config
{
    public static string RepoRoot { get; } = Path.GetFullPath(Path.Combine(Application.dataPath, "../../"));
    public static Repository MakeRepoView() => new Repository(RepoRoot);
}
