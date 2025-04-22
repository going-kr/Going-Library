using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using Going.UIEditor.Datas;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Going.UIEditor.Utils
{
    public class Code
    {
        #region ValidCode
        public static bool ValidCode(Project? prj) => prj?.Design != null;
        #endregion

        public static Dictionary<string, Dictionary<string, UICode>>? MakeCode(Project prj)
        {
            Dictionary<string, Dictionary<string, UICode>>? ret = null;
            if (prj != null)
            {
                ret = new Dictionary<string, Dictionary<string, UICode>>
                {
                    { "", [] },
                    { "Pages", [] },
                    { "Windows", [] }
                };

                #region Program.cs
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"using {prj.Name};");
                    sb.AppendLine($"");
                    sb.AppendLine($"using var view = new MainWindow();");
                    sb.AppendLine($"view.Run();");

                    ret[""].Add("Program.cs", new UICode("Program.cs", sb.ToString(), true));
                }
                #endregion
                #region MainWindow.cs
                {
                    var sb = new StringBuilder();

                    sb.AppendLine($"using Going.UI.Containers;");
                    sb.AppendLine($"using Going.UI.Controls;");
                    sb.AppendLine($"using Going.UI.Datas;");
                    sb.AppendLine($"using Going.UI.OpenTK.Windows;");
                    sb.AppendLine($"using OpenTK.Windowing.Common;");
                    if (prj.Design.Pages.Count > 0) sb.AppendLine($"using {prj.Name}.Pages;");
                    if (prj.Design.Windows.Count > 0) sb.AppendLine($"using {prj.Name}.Windows;");
                    sb.AppendLine($"");
                    sb.AppendLine($"namespace {prj.Name}");
                    sb.AppendLine($"{{");
                    sb.AppendLine($"    public partial class MainWindow : GoViewWindow");
                    sb.AppendLine($"    {{");
                    sb.AppendLine($"        public MainWindow() : base({prj.Width}, {prj.Height}, WindowBorder.Hidden)");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            InitializeComponent();");
                    sb.AppendLine($"        }}");
                    sb.AppendLine($"    }}");
                    sb.AppendLine($"}}");

                    ret[""].Add("MainWindow.cs", new UICode("MainWindow.cs", sb.ToString(), true));
                }
                #endregion
                #region MainWindow.Designer.cs
                {
                    var sb = new StringBuilder();

                    sb.AppendLine($"using System.Text;");
                    sb.AppendLine($"using Going.UI.Containers;");
                    sb.AppendLine($"using Going.UI.Controls;");
                    sb.AppendLine($"using Going.UI.Datas;");
                    sb.AppendLine($"using Going.UI.OpenTK.Windows;");
                    sb.AppendLine($"using OpenTK.Windowing.Common;");
                    sb.AppendLine($"using {prj.Name}.Pages;");
                    sb.AppendLine($"");
                    sb.AppendLine($"namespace {prj.Name}");
                    sb.AppendLine($"{{");
                    sb.AppendLine($"    public partial class MainWindow");
                    sb.AppendLine($"    {{");
                    sb.AppendLine($"        private void InitializeComponent()");
                    sb.AppendLine($"        {{");
                    #region Window Setting
                    sb.AppendLine($"            #region Window Setting");
                    sb.AppendLine($"            Title = \"{prj.Name}\";");
                    sb.AppendLine($"            Debug = false;");
                    sb.AppendLine($"            VSync = VSyncMode.On;");
                    sb.AppendLine($"            CenterWindow();");
                    sb.AppendLine($"            #endregion");
                    sb.AppendLine($"");
                    #endregion
                    #region Design Setting
                    sb.AppendLine($"            #region Design Setting");
                    sb.AppendLine($"            Design.UseTitleBar = {(prj.Design.UseTitleBar ? "true" : "false")};");
                    sb.AppendLine($"            Design.UseLeftSideBar = {(prj.Design.UseLeftSideBar ? "true" : "false")};");
                    sb.AppendLine($"            Design.UseRightSideBar = {(prj.Design.UseRightSideBar ? "true" : "false")};");
                    sb.AppendLine($"            Design.UseFooter = {(prj.Design.UseFooter ? "true" : "false")};");
                    sb.AppendLine($"            Design.OverlaySideBar = {(prj.Design.OverlaySideBar ? "true" : "false")};");
                    sb.AppendLine($"            Design.BarColor = \"{prj.Design.BarColor}\";");
                    sb.AppendLine($"            Design.ExpandLeftSideBar = {(prj.Design.ExpandLeftSideBar? "true" : "false")};");
                    sb.AppendLine($"            Design.ExpandRightSideBar = {(prj.Design.ExpandRightSideBar  ? "true" : "false")};");
                    sb.AppendLine($"            #endregion");
                    sb.AppendLine($"");
                    #endregion
                    #region Resources
                    sb.AppendLine($"            #region Resources");
                    var jimgs = JsonSerializer.Serialize(prj.Design.GetImages().ToDictionary(x => x.name, y => y.images), GoJsonConverter.Options);
                    if (!string.IsNullOrWhiteSpace(jimgs)) sb.AppendLine($"            Design.LoadImagesFromJson(Encoding.UTF8.GetString(Convert.FromBase64String(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(jimgs))}\")));");
                    sb.AppendLine($"            #endregion");
                    #endregion


                    sb.AppendLine($"        }}");
                    sb.AppendLine($"    }}");
                    sb.AppendLine($"}}");

                    ret[""].Add("MainWindow.Designer.cs", new UICode("MainWindow.Designer.cs", sb.ToString(), false));
                }
                #endregion
                #region Pages
                {
                    foreach (var page in prj.Design.Pages.Values)
                    {
                        #region Code
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"using System;");
                            sb.AppendLine($"using System.Collections.Generic;");
                            sb.AppendLine($"using System.Linq;");
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Threading.Tasks;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.ImageCanvas;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Pages");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    public partial class {page.Name} : {(page is IcPage ? "IcPage" : "GoPage")}");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        public {page.Name}()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            InitializeComponent();");
                            sb.AppendLine($"        }}");
                            sb.AppendLine($"    }}");
                            sb.AppendLine($"}}");

                            ret["Pages"].Add($"{page.Name}.cs", new UICode($"{page.Name}.cs", sb.ToString(), true));
                        }
                        #endregion
                        #region Designer
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"using System;");
                            sb.AppendLine($"using System.Collections.Generic;");
                            sb.AppendLine($"using System.Linq;");
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Threading.Tasks;");
                            sb.AppendLine($"using Going.UI.Utils;");
                            sb.AppendLine($"using Going.UI.Controls;");
                            sb.AppendLine($"using Going.UI.Containers;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.ImageCanvas;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Pages");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    partial class {page.Name}");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        #region declare");
                            sb.AppendLine($"        #endregion");
                            sb.AppendLine($"");
                            sb.AppendLine($"        public void InitializeComponent()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            #region base");
                            sb.AppendLine($"            #endregion");
                            sb.AppendLine($"");
                            sb.AppendLine($"        }}");
                            sb.AppendLine($"    }}");
                            sb.AppendLine($"}}");

                            ret["Pages"].Add($"{page.Name}.Designer.cs", new UICode($"{page.Name}.Designer.cs", sb.ToString(), false));
                        }
                    }
                    #endregion
                }
                #endregion
                #region Windows
                {
                    foreach (var wnd in prj.Design.Windows.Values)
                    {
                        #region Code
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"using System;");
                            sb.AppendLine($"using System.Collections.Generic;");
                            sb.AppendLine($"using System.Linq;");
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Threading.Tasks;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.ImageCanvas;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Windows");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    public partial class {wnd.Name} : GoWindow");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        public {wnd.Name}()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            InitializeComponent();");
                            sb.AppendLine($"        }}");
                            sb.AppendLine($"    }}");
                            sb.AppendLine($"}}");

                            ret["Windows"].Add($"{wnd.Name}.cs", new UICode($"{wnd.Name}.cs", sb.ToString(), true));
                        }
                        #endregion
                        #region Designer
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"using System;");
                            sb.AppendLine($"using System.Collections.Generic;");
                            sb.AppendLine($"using System.Linq;");
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Threading.Tasks;");
                            sb.AppendLine($"using Going.UI.Utils;");
                            sb.AppendLine($"using Going.UI.Controls;");
                            sb.AppendLine($"using Going.UI.Containers;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.ImageCanvas;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Windows");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    partial class {wnd.Name}");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        #region declare");
                            sb.AppendLine($"        #endregion");
                            sb.AppendLine($"");
                            sb.AppendLine($"        public void InitializeComponent()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            #region base");
                            sb.AppendLine($"            Name = \"{wnd.Name}\";");
                            sb.AppendLine($"            #endregion");
                            sb.AppendLine($"        }}");
                            sb.AppendLine($"    }}");
                            sb.AppendLine($"}}");

                            ret["Windows"].Add($"{wnd.Name}.Designer.cs", new UICode($"{wnd.Name}.Designer.cs", sb.ToString(), false));
                        }
                        #endregion
                    }
                }
                #endregion
            }
            return ret;
        }
    }

    #region class : UICode
    public class UICode(string filename, string code, bool existCheck)
    {
        public string FileName { get; } = filename;
        public string Code { get; } = code;
        public bool ExistsCheck { get; } = existCheck;
    }
    #endregion
    #region class : TreeItem
    class TreeItem(string name, string parent, GoTreeNode node)
    {
        public string Name { get; } = name;
        public string Parent { get; } = parent;
        public GoTreeNode Node { get; } = node;
    }
    #endregion
}
