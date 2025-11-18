namespace jwl.Jira.api.rest.request;

public class ICTimeAddWorklogByIssueKey
{
    public enum LogWorkOption
    {
        Summary,
        FromTo
    }

    public enum NoCharge
    {
        On,
        Off
    }

    public enum AdjustEstimate
    {
        New,
        Auto,
        Manual
    }
}
