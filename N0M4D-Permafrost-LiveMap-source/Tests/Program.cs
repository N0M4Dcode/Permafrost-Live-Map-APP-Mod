using PermafrostLiveMap;
using System.Text.Json;
var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");
void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS: "+name);}
try{
var reader=new FeedReader(path);
Check((string?)reader.Read()["status"]=="waiting_for_game","missing feed");
File.WriteAllText(path,JsonSerializer.Serialize(new{status="live",sequence=1,timestamp=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),position=new{x=1,y=2,z=3}}));
Check((string?)reader.Read()["status"]=="live","valid feed");
File.WriteAllText(path,"{\"status\":");
Check((string?)reader.Read()["status"]=="live","partial write retains last complete sample");
Thread.Sleep(1600);
Check((string?)reader.Read()["status"]=="disconnected"&&reader.Read()["position"]==null,"stale sample disconnected");
File.WriteAllText(path,JsonSerializer.Serialize(new{status="live",sequence=2,timestamp=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),position=new{x=3,y=4,z=5}}));
Check((string?)reader.Read()["status"]=="live","reconnect");
}finally{File.Delete(path);}
