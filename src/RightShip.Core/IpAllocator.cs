namespace RightShip.Core;

public static class IpAllocator
{
    public const int MaxEntriesPerGroup = 5000;

    public static AllocationPlan BuildPlan(string upload, IReadOnlyList<ManagedIpGroup> groups, string prefix = "rightship-block-", int capacity = MaxEntriesPerGroup)
    {
        if (capacity is < 1 or > MaxEntriesPerGroup) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("Group prefix is required", nameof(prefix));
        var parsed = IpParser.Parse(upload);
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in groups)
        {
            if (!usedNames.Add(group.Name)) throw new ArgumentException("Duplicate group name in inventory");
            if (group.Addresses.Count > capacity) throw new ArgumentException("Existing group exceeds capacity: " + group.Name);
            foreach (var entry in group.Addresses)
            {
                if (!IpParser.TryCanonicalize(entry, out var ip)) throw new ArgumentException("Invalid existing group entry: " + entry);
                existing.Add(ip);
            }
        }
        var newIps = parsed.Valid.Where(x => !existing.Contains(x)).ToArray();
        var already = parsed.Valid.Count - newIps.Length;
        var changes = new List<GroupChange>();
        var offset = 0;
        foreach (var group in groups.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (offset >= newIps.Length) break;
            var take = Math.Min(capacity - group.Addresses.Count, newIps.Length - offset);
            if (take == 0) continue;
            changes.Add(new GroupChange(group.Name, false, group.Addresses.Count, newIps.Skip(offset).Take(take).ToArray()));
            offset += take;
        }
        var index = 1;
        while (offset < newIps.Length)
        {
            string name;
            do { name = prefix + index++.ToString("D4"); } while (!usedNames.Add(name));
            var take = Math.Min(capacity, newIps.Length - offset);
            changes.Add(new GroupChange(name, true, 0, newIps.Skip(offset).Take(take).ToArray()));
            offset += take;
        }
        return new AllocationPlan(parsed.TotalRows, parsed.Issues.Count, parsed.Duplicates, already,
            newIps.Length, parsed.Issues, changes);
    }
}
