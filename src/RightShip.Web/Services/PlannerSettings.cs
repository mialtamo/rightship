namespace RightShip.Web.Services;
public sealed class PlannerSettings
{
 public string Cloud {get;set;}="AzureCloud";
 public string SubscriptionId {get;set;}="";
 public string IpGroupResourceGroup {get;set;}="";
 public string IpGroupRegion {get;set;}="westus2";
 public string IpGroupPrefix {get;set;}="IPBAN";
 public int StartingNumber {get;set;}=0;
 public string FirewallPolicyResourceId {get;set;}="";
 public string RuleCollectionGroup {get;set;}="";
 public string RuleCollection {get;set;}="";
 public string DenyRule {get;set;}="";
 public int Capacity {get;set;}=5000;
 public bool RequireApproval {get;set;}=true;
}