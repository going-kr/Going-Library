using Going.UI.Design;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using SampleOpenTK;

using var view = new GoViewWindow(1024, 768, WindowBorder.Resizable);
view.Debug = false;
view.Title = "Sample OpenTK";
view.TItleIconString = "fa-check";
view.VSync = VSyncMode.On;
view.CenterWindow();

view.Design.AddImageFolder("C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageSample");
view.Design.LoadIC("C:\\Users\\hello\\OneDrive\\Desktop\\RiderProjects\\Going-Library\\ImageCanvasSample");

view.Design.AddPage(new PageMain());
view.Design.AddPage(new PageTest());
view.Design.AddPage(new PageTab());
view.Design.AddPage(new PageIC());
view.Design.SetPage("PageMain");

var s = view.Design.JsonSerialize();
var design = GoDesign.JsonDeserialize(s);

view.Run();