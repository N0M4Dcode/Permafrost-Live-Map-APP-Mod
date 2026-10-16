using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace PermafrostLiveMap;
public static class Program {
 [STAThread] public static void Main(string[] args) {
  var app=new Application();
  app.Run(new MapWindow(args));
 }
}
public sealed partial class MapWindow:Window {
 readonly WebView2 browser=new(); WebApplication? server;
 WindowState previousState; bool fullscreen;
 public MapWindow(string[] args) {
  Title="N0M4D's Permafrost Live Map"; Width=1440;Height=900;MinWidth=800;MinHeight=500;InitializeShell();
  Loaded+=async (_,_)=>{try {
   var feed=args.Length>0?Path.GetFullPath(args[0]):Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","Permafrost","Binaries","Win64","ue4ss","Mods","N0M4DTelemetry","position.json"));
   var builder=WebApplication.CreateSlimBuilder();builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:8767");
   server=builder.Build(); var reader=new FeedReader(feed);
   server.Use(async(context,next)=>{context.Response.Headers.CacheControl="no-store";context.Response.Headers["X-Content-Type-Options"]="nosniff";context.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https://metabot.gg; connect-src 'self'; object-src 'none'; frame-ancestors 'none'";await next();});
   server.MapGet("/api/position",()=>Results.Json(reader.Read()));
   server.MapGet("/api/health",()=>Results.Json(new {app="PermafrostLiveMap",version=2}));
   server.UseDefaultFiles();server.UseStaticFiles(new StaticFileOptions{FileProvider=new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory,"dist"))});
   await server.StartAsync();var origin=server.Urls.Single();
   var profile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"N0M4D","PermafrostLiveMap","WebView2");
   var environment=await CoreWebView2Environment.CreateAsync(null,profile);await browser.EnsureCoreWebView2Async(environment);
   browser.CoreWebView2.NavigationStarting+=(_,e)=>{if(!e.Uri.StartsWith(origin+"/",StringComparison.Ordinal))e.Cancel=true;};
   browser.CoreWebView2.NewWindowRequested+=(_,e)=>{e.Handled=true;if(Uri.TryCreate(e.Uri,UriKind.Absolute,out var uri)&&uri.Scheme=="https")System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri){UseShellExecute=true});};
   browser.CoreWebView2.WebMessageReceived+=(_,e)=>{if(!e.Source.StartsWith(origin+"/",StringComparison.Ordinal))return;var command=e.TryGetWebMessageAsString();if(command.StartsWith("theme:")){var parts=command.Split(':' );if(parts.Length==3)ApplyShellTheme(parts[1],parts[2]);return;}if(command=="fullscreen"||(command=="escape"&&fullscreen))ToggleFullscreen();};
   browser.Source=new Uri(origin+"/index.html");
  }catch(Exception e){MessageBox.Show(e.Message,"Live Map startup failed");Close();}};
  Closed+=async(_,_)=>{browser.Dispose();if(server!=null)await server.DisposeAsync();};
 }
}





