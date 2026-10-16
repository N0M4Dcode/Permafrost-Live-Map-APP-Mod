using System.IO;
using System.Text.Json.Nodes;
namespace PermafrostLiveMap;
public sealed class FeedReader(string path) {
 readonly object gate=new(); JsonObject? cached; string? key; long changed;
 public JsonObject Read(){lock(gate){
  try {
   using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
   var next=JsonNode.Parse(stream)?.AsObject()??throw new InvalidDataException();
   var nextKey=$"{next["session"]}:{next["sequence"]}:{next["timestamp"]}";
   if(nextKey!=key){key=nextKey;cached=next;changed=Environment.TickCount64;}
  }catch(Exception e) when(e is IOException or System.Text.Json.JsonException or InvalidOperationException or UnauthorizedAccessException){}
  if(cached==null)return new JsonObject{["status"]="waiting_for_game"};
  var result=(JsonObject)cached.DeepClone();
  double age=(Environment.TickCount64-changed)/1000d;
  if(result["timestamp"] is JsonValue ts&&ts.TryGetValue<double>(out var seconds))age=Math.Max(age,DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d-seconds-1);
  result["mapMatrix"] ??= new JsonArray(0.013433912758,0.0,1962.694653985,0.0,0.013433912758,1962.694653985);
  result["age"]=Math.Max(0,age);result["receivedAtMs"]=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  if(age>1.5){result["status"]="disconnected";result.Remove("position");}
  return result;
 }}
}


