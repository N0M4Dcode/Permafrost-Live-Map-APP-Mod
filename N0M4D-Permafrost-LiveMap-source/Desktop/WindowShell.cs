using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Automation;

namespace PermafrostLiveMap;
public sealed partial class MapWindow {
 readonly RowDefinition captionRow=new(){Height=new GridLength(42)};
 readonly Border caption=new();
 readonly TextBlock captionTitle=new();
 Button maximize=null!;
 readonly WindowChrome chrome=new(){CaptionHeight=42,ResizeBorderThickness=new Thickness(6),GlassFrameThickness=new Thickness(0),CornerRadius=new CornerRadius(0),UseAeroCaptionButtons=false};
 void InitializeShell(){
  WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResize;
  WindowChrome.SetWindowChrome(this,chrome);
  Background=new SolidColorBrush(Color.FromRgb(16,28,37));
  var layout=new Grid();layout.RowDefinitions.Add(captionRow);layout.RowDefinitions.Add(new RowDefinition());
  var bar=new Grid();bar.ColumnDefinitions.Add(new ColumnDefinition());bar.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
  captionTitle.Text="◈  N0M4D’S PERMAFROST LIVE MAP";captionTitle.FontSize=12;captionTitle.FontWeight=FontWeights.SemiBold;captionTitle.VerticalAlignment=VerticalAlignment.Center;captionTitle.Margin=new Thickness(16,0,0,0);
  bar.Children.Add(captionTitle);
  var buttons=new StackPanel{Orientation=Orientation.Horizontal};Grid.SetColumn(buttons,1);bar.Children.Add(buttons);
  buttons.Children.Add(CaptionButton("─","Minimize",()=>WindowState=WindowState.Minimized));
  maximize=CaptionButton("□","Maximize",()=>{if(fullscreen)ToggleFullscreen();else WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;});buttons.Children.Add(maximize);
  buttons.Children.Add(CaptionButton("×","Close",Close,true));
  caption.Child=bar;caption.BorderThickness=new Thickness(0,0,0,1);layout.Children.Add(caption);
  Grid.SetRow(browser,1);layout.Children.Add(browser);Content=layout;
  StateChanged+=(_,_)=>{maximize.Content=WindowState==WindowState.Maximized?"❐":"□";maximize.ToolTip=WindowState==WindowState.Maximized?"Restore":"Maximize";AutomationProperties.SetName(maximize,(string)maximize.ToolTip);};
  ApplyShellTheme("#69dec9","#101c25");
 }
 Button CaptionButton(string glyph,string name,Action action,bool close=false){
  var button=new Button{Content=glyph,Width=48,Height=42,FontSize=19,Foreground=Brushes.White,Background=Brushes.Transparent,BorderThickness=new Thickness(0),ToolTip=name,Focusable=true};
  AutomationProperties.SetName(button,name);WindowChrome.SetIsHitTestVisibleInChrome(button,true);
  var template=new ControlTemplate(typeof(Button));
  var border=new FrameworkElementFactory(typeof(Border));border.SetValue(Border.BackgroundProperty,new TemplateBindingExtension(Control.BackgroundProperty));
  var content=new FrameworkElementFactory(typeof(ContentPresenter));content.SetValue(ContentPresenter.HorizontalAlignmentProperty,HorizontalAlignment.Center);content.SetValue(ContentPresenter.VerticalAlignmentProperty,VerticalAlignment.Center);border.AppendChild(content);template.VisualTree=border;
  var hover=new Trigger{Property=IsMouseOverProperty,Value=true};hover.Setters.Add(new Setter(Control.BackgroundProperty,new SolidColorBrush(close?Color.FromRgb(196,43,65):Color.FromArgb(55,255,255,255))));template.Triggers.Add(hover);
  var focus=new Trigger{Property=IsKeyboardFocusedProperty,Value=true};focus.Setters.Add(new Setter(Control.BackgroundProperty,new SolidColorBrush(Color.FromArgb(75,255,255,255))));template.Triggers.Add(focus);
  button.Template=template;button.Click+=(_,_)=>action();return button;
 }
 void ApplyShellTheme(string accent,string background){
  if(!System.Text.RegularExpressions.Regex.IsMatch(accent,"^#[0-9a-fA-F]{6}$")||!System.Text.RegularExpressions.Regex.IsMatch(background,"^#[0-9a-fA-F]{6}$"))return;
  var a=(Color)ColorConverter.ConvertFromString(accent);var b=(Color)ColorConverter.ConvertFromString(background);
  caption.Background=new SolidColorBrush(b);caption.BorderBrush=new SolidColorBrush(a);
  var foreground=b.R*.299+b.G*.587+b.B*.114>150?Brushes.Black:Brushes.White;
  captionTitle.Foreground=foreground;
  if(caption.Child is Grid bar&&bar.Children[1] is StackPanel buttons)foreach(Button button in buttons.Children)button.Foreground=foreground;
 }
 void ToggleFullscreen(){
  if(!fullscreen){previousState=WindowState;WindowState=WindowState.Normal;captionRow.Height=new GridLength(0);caption.Visibility=Visibility.Collapsed;WindowChrome.SetWindowChrome(this,null);ResizeMode=ResizeMode.NoResize;WindowState=WindowState.Maximized;}
  else{WindowState=WindowState.Normal;captionRow.Height=new GridLength(42);caption.Visibility=Visibility.Visible;WindowChrome.SetWindowChrome(this,chrome);ResizeMode=ResizeMode.CanResize;WindowState=previousState;}
  fullscreen=!fullscreen;
 }
}
