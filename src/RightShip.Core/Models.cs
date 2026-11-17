namespace RightShip.Core;

public sealed record ValidationIssue(int Line, string Value, string Reason);
public sealed record ParsedAddresses(int TotalRows, IReadOnlyList<string> Valid, IReadOnlyList<ValidationIssue> Issues, int Duplicates);
public sealed record ManagedIpGroup(string Name, IReadOnlyList<string> Addresses);
public sealed record GroupChange(string Name, bool Create, int OriginalCount, IReadOnlyList<string> Added)
{
    public int FinalCount => OriginalCount + Added.Count;
}
public sealed record AllocationPlan(int UploadedRows, int InvalidRows, int Duplicates, int AlreadyManaged,
    int UniqueToAdd, IReadOnlyList<ValidationIssue> Issues, IReadOnlyList<GroupChange> Changes)
{
    public int NewGroups => Changes.Count(x => x.Create);
    public int UpdatedGroups => Changes.Count(x => !x.Create && x.Added.Count > 0);
}
