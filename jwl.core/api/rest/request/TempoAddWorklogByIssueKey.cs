namespace jwl.core.api.rest.request;
using commons = jwl.core.api.rest.common;

public struct TempoAddWorklogByIssueKey
{
    public string IssueKey;
    public int TimeSpentSeconds;
    public int BillableSeconds;
    public string Worker;
    public string Started;
    public string EndDate;
    public bool IncludeNonWorkingDays;
    public Dictionary<string, commons.TempoWorklogAttribute> Attributes;
}
