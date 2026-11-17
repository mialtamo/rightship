using RightShip.Core;
using Xunit;
public class NamingTests
{
 [Fact]public void NamingStartsAtZeroAndAdvances(){var plan=IpAllocator.BuildPlan("10.0.0.1\n10.0.0.2",[],"IPBAN",1,0);Assert.Equal(new[]{"IPBAN0","IPBAN1"},plan.Changes.Select(x=>x.Name));}
 [Fact]public void NamingSkipsExistingNames(){var plan=IpAllocator.BuildPlan("10.0.0.1",[new ManagedIpGroup("IPBAN0",["192.0.2.1"])],"IPBAN",1,0);Assert.Equal("IPBAN1",plan.Changes.Single().Name);}
 [Fact]public void NamingUsesConfiguredStart(){var plan=IpAllocator.BuildPlan("10.0.0.1",[],"IPBAN",5000,42);Assert.Equal("IPBAN42",plan.Changes.Single().Name);}
}