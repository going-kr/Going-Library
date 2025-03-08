using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using SampleOpenTK;
using SampleOpenTK.Windows;
using System.Runtime.CompilerServices;

using var view = new MainWindow();

var s = view.Design.JsonSerialize();
var design = GoDesign.JsonDeserialize(s);

view.Run();