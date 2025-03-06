using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using SampleOpenTK;
using System.Runtime.CompilerServices;

using var view = new GoViewWindow(1024, 768, WindowBorder.Resizable);
view.Debug = false;
view.Title = "Sample OpenTK";
view.TItleIconString = "fa-check";
view.VSync = VSyncMode.On;
view.CenterWindow();

view.Design.AddImageFolder("D:\\Project\\Going\\Library\\Git\\ImageSample");
view.Design.LoadIC("D:\\Project\\Going\\Library\\Git\\ImageCanvasSample");

view.Design.UseTitleBar = true;
view.Design.UseLeftSideBar = true;
view.Design.UseRightSideBar = true;
view.Design.UseFooter = true;

view.Design.AddPage(new PageMain());
view.Design.SetPage("PageMain");

var s = view.Design.JsonSerialize();
var design = GoDesign.JsonDeserialize(s);

view.Run();