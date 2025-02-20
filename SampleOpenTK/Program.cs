using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;

using var view = new GoViewWindow(1024, 768, WindowBorder.Resizable);
view.Debug = false;
view.Title = "Sample OpenTK";
view.TItleIconString = "fa-check";
view.VSync = VSyncMode.On;
view.CenterWindow();

view.Design.AddPage(new PageMain());
view.Design.AddPage(new PageTest());
view.Design.SetPage("PageMain");

view.Run();