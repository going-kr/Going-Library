using Going.UI.Icons;
using OpenTK.Windowing.Common;
using Sample;


using (var view = new ViewWindow(1024,768) { Debug=true })
{
    view.VSync = VSyncMode.On;
    view.CenterWindow();
    view.Run();
}
