
public struct ReportedIssue
{
    public string trace;
    public string description;

    public ReportedIssue(string trace, string description)
    {
        this.trace = trace;
        this.description = description;
    }
}
