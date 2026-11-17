using RightShip.Core;
using Xunit;

public class AllocationTests
{
    [Fact] public void CanonicalizesNetworkAddress() {
        Assert.True(IpParser.TryCanonicalize("192.168.1.35/24", out var ip));
        Assert.Equal("192.168.1.0/24", ip);
    }

    [Fact] public void RejectsInvalidPrefixesAndMalformedIp() {
        Assert.False(IpParser.TryCanonicalize("192.168.1.1/33", out _));
        Assert.False(IpParser.TryCanonicalize("300.1.1.1", out _));
        Assert.False(IpParser.TryCanonicalize("127.1", out _));
    }

    [Fact] public void DeduplicatesWithinUploadAndInventory() {
        var plan = IpAllocator.BuildPlan("10.0.0.1\n10.0.0.1\n10.0.0.2\ninvalid", [new ManagedIpGroup("rightship-block-0001", ["10.0.0.2"])]);
        Assert.Equal(1, plan.Duplicates);
        Assert.Equal(1, plan.InvalidRows);
        Assert.Equal(1, plan.AlreadyManaged);
        Assert.Equal(1, plan.UniqueToAdd);
        Assert.Single(plan.Changes);
    }

    [Fact] public void FillsExistingGroupAndCreatesAdditionalGroups() {
        var existing = Enumerable.Range(0, 4998).Select(i => $"10.{i / 256}.{i % 256}.1").ToArray();
        var incoming = string.Join("\n", Enumerable.Range(0, 5001).Select(i => $"11.{i / 256}.{i % 256}.2"));
        var plan = IpAllocator.BuildPlan(incoming, [new ManagedIpGroup("rightship-block-0001", existing)]);
        Assert.Equal(2, plan.NewGroups);
        Assert.Equal(5000, plan.Changes[0].FinalCount);
        Assert.Equal(5000, plan.Changes[1].FinalCount);
        Assert.Equal(1, plan.Changes[2].FinalCount);
    }

    [Fact] public void RetryWithAlreadyManagedAddressesDoesNothing() {
        var plan = IpAllocator.BuildPlan("10.0.0.1", [new ManagedIpGroup("rightship-block-0001", ["10.0.0.1"])]);
        Assert.Equal(0, plan.UniqueToAdd);
        Assert.Empty(plan.Changes);
    }
}
