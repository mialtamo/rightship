namespace RightShip.Core;
public static class IpAllocator
{
 public const int MaxEntriesPerGroup=5000;
 public static AllocationPlan BuildPlan(string upload,IReadOnlyList<ManagedIpGroup> groups,string prefix="IPBAN",int capacity=MaxEntriesPerGroup,int startingNumber=0)
 {
  if(capacity is <1 or >MaxEntriesPerGroup)throw new ArgumentOutOfRangeException(nameof(capacity));
  if(string.IsNullOrWhiteSpace(prefix)||startingNumber<0||startingNumber>999999)throw new ArgumentException("Invalid IP Group naming settings");
  var parsed=IpParser.Parse(upload);
  var existing=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  var usedNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var group in groups){
   if(!usedNames.Add(group.Name))throw new ArgumentException("Duplicate inventory group name");
   if(group.Addresses.Count>capacity)throw new ArgumentException("Group exceeds configured capacity: "+group.Name);
   foreach(var entry in group.Addresses){if(!IpParser.TryCanonicalize(entry,out var ip))throw new ArgumentException("Invalid inventory address: "+entry);existing.Add(ip);}
  }
  var next=parsed.Valid.Where(x=>!existing.Contains(x)).ToArray();
  var changes=new List<GroupChange>();var offset=0;
  foreach(var group in groups.OrderBy(x=>x.Name,StringComparer.OrdinalIgnoreCase)){
   if(offset==next.Length)break;
   var take=Math.Min(capacity-group.Addresses.Count,next.Length-offset);
   if(take==0)continue;
   changes.Add(new GroupChange(group.Name,false,group.Addresses.Count,next.Skip(offset).Take(take).ToArray()));offset+=take;
  }
  var index=startingNumber;
  while(offset<next.Length){
   string name;
   do{name=prefix+(index++).ToString();}while(!usedNames.Add(name));
   var take=Math.Min(capacity,next.Length-offset);
   changes.Add(new GroupChange(name,true,0,next.Skip(offset).Take(take).ToArray()));offset+=take;
  }
  return new AllocationPlan(parsed.TotalRows,parsed.Issues.Count,parsed.Duplicates,parsed.Valid.Count-next.Length,next.Length,parsed.Issues,changes);
 }
}