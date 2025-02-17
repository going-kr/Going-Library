using Going.UI.OpenTK;
using OpenTK.Windowing.Common;

using var view = new GoViewWindow(1024, 768, WindowBorder.Resizable) { Debug = true };
view.VSync = VSyncMode.On;
view.CenterWindow();
view.Run();