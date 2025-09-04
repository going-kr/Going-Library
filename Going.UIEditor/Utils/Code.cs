using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.ImageCanvas;
using Going.UI.Json;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using SkiaSharp;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.UI.Input.Inking;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                    var lsT = All(prj.Design.TitleBar).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();
                    var lsL = All(prj.Design.LeftSideBar).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();
                    var lsR = All(prj.Design.RightSideBar).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();
                    var lsB = All(prj.Design.Footer).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();
                    List<IGoControl> lsA = [.. lsT, .. lsL, .. lsR, .. lsB];

                    sb.AppendLine($"using System.Text;");
                    sb.AppendLine($"using System.Text.Json;");
                    sb.AppendLine($"using Going.UI.Containers;");
                    sb.AppendLine($"using Going.UI.Controls;");
                    sb.AppendLine($"using Going.UI.Datas;");
                    sb.AppendLine($"using Going.UI.Design;");
                    sb.AppendLine($"using Going.UI.Json;");
                    sb.AppendLine($"using Going.UI.OpenTK.Windows;");
                    sb.AppendLine($"using Going.UI.Utils;");
                    sb.AppendLine($"using Going.UI.Themes;");
                    sb.AppendLine($"using OpenTK.Windowing.Common;");

                    if (prj.Design.Pages.Count > 0) sb.AppendLine($"using {prj.Name}.Pages;");
                    if (prj.Design.Windows.Count > 0) sb.AppendLine($"using {prj.Name}.Windows;");
                    sb.AppendLine($"");
                    sb.AppendLine($"namespace {prj.Name}");
                    sb.AppendLine($"{{");
                    sb.AppendLine($"    public partial class MainWindow");
                    sb.AppendLine($"    {{");

                    #region Var
                    sb.AppendLine($"        #region declare");
                    foreach (var v in lsA) sb.AppendLine($"        {TypeName(v)} {v.Name};");
                    sb.AppendLine($"");
                    foreach (var v in prj.Design.Pages.Values) sb.AppendLine($"        public {v.Name} {v.Name} {{ get; private set; }}");
                    sb.AppendLine($"");
                    foreach (var v in prj.Design.Windows.Values) sb.AppendLine($"        public {v.Name} {v.Name} {{ get; private set; }}");
                    sb.AppendLine($"        #endregion");
                    sb.AppendLine($"");
                    #endregion
                    #region static
                    sb.AppendLine($"        #region static");
                    sb.AppendLine($"        public static MainWindow Current {{ get; private set; }}");
                    sb.AppendLine($"        #endregion");
                    sb.AppendLine($"");
                    #endregion
                    sb.AppendLine($"        private void InitializeComponent()");
                    sb.AppendLine($"        {{");

                    #region Window Setting
                    sb.AppendLine($"            #region Window Setting");
                    sb.AppendLine($"            Current = this;");
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
                    sb.AppendLine($"            Design.ExpandLeftSideBar = {(prj.Design.ExpandLeftSideBar ? "true" : "false")};");
                    sb.AppendLine($"            Design.ExpandRightSideBar = {(prj.Design.ExpandRightSideBar ? "true" : "false")};");
                    sb.AppendLine($"            #endregion");
                    sb.AppendLine($"");
                    #endregion
                    #region Theme Setting
                    if (prj.Design.CustomTheme != null)
                    {
                        var v = prj.Design.CustomTheme;
                        sb.AppendLine($"            #region Theme Setting");
                        sb.AppendLine($"            Design.CustomTheme = new GoTheme();");
                        sb.AppendLine($"            Design.CustomTheme.Dark = {(v.Dark ? "true" : "false")};");

                        sb.AppendLine($"            Design.CustomTheme.Fore = {color(v.Fore)};");
                        sb.AppendLine($"            Design.CustomTheme.Back = {color(v.Back)};");
                        sb.AppendLine($"            Design.CustomTheme.Window = {color(v.Window)};");
                        sb.AppendLine($"            Design.CustomTheme.WindowBorder = {color(v.WindowBorder)};");
                        sb.AppendLine($"            Design.CustomTheme.Point = {color(v.Point)};");
                        sb.AppendLine($"            Design.CustomTheme.Title = {color(v.Title)};");

                        sb.AppendLine($"            Design.CustomTheme.ScrollBar = {color(v.ScrollBar)};");
                        sb.AppendLine($"            Design.CustomTheme.ScrollCursor = {color(v.ScrollCursor)};");
                        sb.AppendLine($"            Design.CustomTheme.Good = {color(v.Good)};");
                        sb.AppendLine($"            Design.CustomTheme.Warning = {color(v.Warning)};");
                        sb.AppendLine($"            Design.CustomTheme.Danger = {color(v.Danger)};");
                        sb.AppendLine($"            Design.CustomTheme.Error = {color(v.Error)};");
                        sb.AppendLine($"            Design.CustomTheme.Highlight = {color(v.Highlight)};");
                        sb.AppendLine($"            Design.CustomTheme.Select = {color(v.Select)};");

                        sb.AppendLine($"            Design.CustomTheme.Base0 = {color(v.Base0)};");
                        sb.AppendLine($"            Design.CustomTheme.Base1 = {color(v.Base1)};");
                        sb.AppendLine($"            Design.CustomTheme.Base2 = {color(v.Base2)};");
                        sb.AppendLine($"            Design.CustomTheme.Base3 = {color(v.Base3)};");
                        sb.AppendLine($"            Design.CustomTheme.Base4 = {color(v.Base4)};");
                        sb.AppendLine($"            Design.CustomTheme.Base5 = {color(v.Base5)};");

                        sb.AppendLine($"            Design.CustomTheme.User1 = {color(v.User1)};");
                        sb.AppendLine($"            Design.CustomTheme.User2 = {color(v.User2)};");
                        sb.AppendLine($"            Design.CustomTheme.User3 = {color(v.User3)};");
                        sb.AppendLine($"            Design.CustomTheme.User4 = {color(v.User4)};");
                        sb.AppendLine($"            Design.CustomTheme.User5 = {color(v.User5)};");
                        sb.AppendLine($"            Design.CustomTheme.User6 = {color(v.User6)};");
                        sb.AppendLine($"            Design.CustomTheme.User7 = {color(v.User7)};");
                        sb.AppendLine($"            Design.CustomTheme.User8 = {color(v.User8)};");
                        sb.AppendLine($"            Design.CustomTheme.User9 = {color(v.User9)};");

                        sb.AppendLine($"            Design.CustomTheme.Corner = {v.Corner};");
                        sb.AppendLine($"            Design.CustomTheme.DownBrightness = {v.DownBrightness:0.0##}F;");
                        sb.AppendLine($"            Design.CustomTheme.BorderBrightness = {v.BorderBrightness:0.0##}F;");
                        sb.AppendLine($"            Design.CustomTheme.HoverBorderBrightness = {v.HoverBorderBrightness:0.0##}F;");
                        sb.AppendLine($"            Design.CustomTheme.HoverFillBrightness = {v.HoverFillBrightness:0.0##}F;");
                        sb.AppendLine($"            Design.CustomTheme.ShadowAlpha = {v.ShadowAlpha};");

                        sb.AppendLine($"            #endregion");
                        sb.AppendLine($"");
                    }
                    #endregion
                    #region Resources
                    sb.AppendLine($"            #region Resources");
                    foreach (var v in prj.Design.GetImages())
                    {
                        if (v.images.Count > 0)
                        {
                            sb.AppendLine($"            Design.AddImage(\"{v.name}\", [");
                            foreach (var img in v.images)
                            {
                                using var data = img.Encode(SKEncodedImageFormat.Png, 100);
                                sb.AppendLine($"                Util.FromImage64(\"{Convert.ToBase64String(data.ToArray())}\"),");
                            }
                            sb.AppendLine($"            ]);");
                        }
                    }

                    foreach(var v in prj.Design.GetFonts())
                    {
                        foreach(var data in v.fonts)
                        {
                            sb.AppendLine($"            Design.AddFont(\"{v.name}\", Convert.FromBase64String(\"{Convert.ToBase64String(data)}\"));");
                        }
                    }

                    sb.AppendLine($"            #endregion");
                    sb.AppendLine($"");
                    #endregion

                    #region TitleBar
                    {
                        sb.AppendLine($"            #region TitleBar");
                        sb.AppendLine($"            {{");
                        MakeDesignBarCode(sb, "                ", "Design.TitleBar", prj.Design.TitleBar, lsT);
                        sb.AppendLine($"            }}");
                        sb.AppendLine($"            #endregion");
                        sb.AppendLine($"");
                    }
                    #endregion
                    #region LeftSideBar
                    {
                        sb.AppendLine($"            #region LeftSideBar");
                        sb.AppendLine($"            {{");
                        MakeDesignBarCode(sb, "                ", "Design.LeftSideBar", prj.Design.LeftSideBar, lsL);
                        sb.AppendLine($"            }}");
                        sb.AppendLine($"            #endregion");
                        sb.AppendLine($"");
                    }
                    #endregion
                    #region RightSideBar
                    {
                        sb.AppendLine($"            #region RightSideBar");
                        sb.AppendLine($"            {{");
                        MakeDesignBarCode(sb, "                ", "Design.RightSideBar", prj.Design.RightSideBar, lsR);
                        sb.AppendLine($"            }}");
                        sb.AppendLine($"            #endregion");
                        sb.AppendLine($"");
                    }
                    #endregion
                    #region Footer
                    {
                        sb.AppendLine($"            #region Footer");
                        sb.AppendLine($"            {{");
                        MakeDesignBarCode(sb, "                ", "Design.Footer", prj.Design.Footer, lsB);
                        sb.AppendLine($"            }}");
                        sb.AppendLine($"            #endregion");
                        sb.AppendLine($"");
                    }
                    #endregion

                    #region Pages
                    if (prj.Design.Pages.Count > 0)
                    {
                        sb.AppendLine($"            #region Pages");
                        foreach (var v in prj.Design.Pages.Values) sb.AppendLine($"            {v.Name} = new {v.Name}();");
                        sb.AppendLine($"");
                        foreach (var v in prj.Design.Pages.Values) sb.AppendLine($"            Design.AddPage({v.Name});");
                        sb.AppendLine($"");
                        sb.AppendLine($"            Design.SetPage(\"{prj.Design.Pages.First().Value.Name}\");");
                        sb.AppendLine($"            #endregion");
                    }
                    #endregion
                    #region Windows
                    if (prj.Design.Windows.Count > 0)
                    {
                        sb.AppendLine($"            #region Windows");
                        foreach (var v in prj.Design.Windows.Values) sb.AppendLine($"            {v.Name} = new {v.Name}();");
                        sb.AppendLine($"");
                        foreach (var v in prj.Design.Windows.Values) sb.AppendLine($"            Design.Windows.Add({v.Name}.Name, {v.Name});");
                        sb.AppendLine($"            #endregion");
                    }
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
                            var ls = All(page).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();

                            var sb = new StringBuilder();
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Text.Json;");
                            sb.AppendLine($"using Going.UI.Containers;");
                            sb.AppendLine($"using Going.UI.Controls;");
                            sb.AppendLine($"using Going.UI.Datas;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.Json;");
                            sb.AppendLine($"using Going.UI.OpenTK.Windows;");
                            sb.AppendLine($"using Going.UI.Utils;");
                            sb.AppendLine($"using OpenTK.Windowing.Common;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Pages");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    partial class {page.Name}");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        #region declare");
                            foreach (var v in ls) sb.AppendLine($"        {TypeName(v)} {v.Name};");
                            sb.AppendLine($"        #endregion");
                            sb.AppendLine($"");
                            sb.AppendLine($"        public void InitializeComponent()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            #region base");
                            MakeDesignBarCode(sb, "            ", "this", page, ls);
                            sb.AppendLine($"            #endregion");
                            sb.AppendLine($"        }}");
                            sb.AppendLine($"    }}");
                            sb.AppendLine($"}}");

                            ret["Pages"].Add($"{page.Name}.Designer.cs", new UICode($"{page.Name}.Designer.cs", sb.ToString(), false));
                        }
                        #endregion
                    }
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
                            var ls = All(wnd).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();

                            var sb = new StringBuilder();
                            sb.AppendLine($"using System.Text;");
                            sb.AppendLine($"using System.Text.Json;");
                            sb.AppendLine($"using Going.UI.Containers;");
                            sb.AppendLine($"using Going.UI.Controls;");
                            sb.AppendLine($"using Going.UI.Datas;");
                            sb.AppendLine($"using Going.UI.Design;");
                            sb.AppendLine($"using Going.UI.Json;");
                            sb.AppendLine($"using Going.UI.OpenTK.Windows;");
                            sb.AppendLine($"using Going.UI.Utils;");
                            sb.AppendLine($"using OpenTK.Windowing.Common;");
                            sb.AppendLine($"");
                            sb.AppendLine($"namespace {prj.Name}.Windows");
                            sb.AppendLine($"{{");
                            sb.AppendLine($"    partial class {wnd.Name}");
                            sb.AppendLine($"    {{");
                            sb.AppendLine($"        #region declare");
                            foreach (var v in ls) sb.AppendLine($"        {TypeName(v)} {v.Name};");
                            sb.AppendLine($"        #endregion");
                            sb.AppendLine($"");
                            sb.AppendLine($"        public void InitializeComponent()");
                            sb.AppendLine($"        {{");
                            sb.AppendLine($"            #region base");
                            MakeDesignBarCode(sb, "            ", "this", wnd, ls);
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

        #region ValidProp
        static bool ValidProp(PropertyInfo pi)
        {
            return ((pi.CanRead && pi.CanWrite && (pi.SetMethod?.IsPublic ?? false) && (pi.GetMethod?.IsPublic ?? false)) ||
                    (pi.CanRead && (pi.GetMethod?.IsPublic ?? false) && pi.PropertyType.GetInterface("IEnumerable") != null)) &&
                     (Attribute.IsDefined(pi, typeof(GoPropertyAttribute)) || Attribute.IsDefined(pi, typeof(GoSizePropertyAttribute)) || Attribute.IsDefined(pi, typeof(GoSizesPropertyAttribute))) &&
                     !Attribute.IsDefined(pi, typeof(JsonIgnoreAttribute));
        }
        #endregion
        #region MakePropCode
        private static void MakePropCode(StringBuilder sb, string space, string varname, IGoControl c, PropertyInfo pi)
        {
            if (c != null)
            {
                var ls = c.GetType().GetProperties().Where(x => ValidProp(x));
                var val = pi.GetValue(c);
                string s = "";

                #region byte / ushort / uint / ulong
                if (pi.PropertyType == typeof(byte) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(ushort) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(uint) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(ulong) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                #endregion
                #region sbyte / short / int / long
                else if (pi.PropertyType == typeof(sbyte) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(short) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(int) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(long) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                #endregion
                #region float / double / decimal
                else if (pi.PropertyType == typeof(float) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val + "F"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(double) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(decimal) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val + "M"};";
                    sb.AppendLine(s);
                }
                #endregion
                #region byte? / ushort? / uint? / ulong?
                else if (pi.PropertyType == typeof(byte?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(ushort?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(uint?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(ulong?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                #endregion
                #region sbyte? / short? / int? / long?
                else if (pi.PropertyType == typeof(sbyte?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(short?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(int?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(long?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                #endregion
                #region float? / double? / decimal?
                else if (pi.PropertyType == typeof(float?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {(val != null ? val + "F" : "null")};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(double?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {val ?? "null"};";
                    sb.AppendLine(s);
                }
                else if (pi.PropertyType == typeof(decimal?) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {(val != null ? val + "M" : "null")};";
                    sb.AppendLine(s);
                }
                #endregion
                #region bool
                else if (pi.PropertyType == typeof(bool) && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {(val is bool vb && vb ? "true" : "false")};";
                    sb.AppendLine(s);
                }
                #endregion
                #region string
                else if (pi.PropertyType == typeof(string) && pi.CanWrite)
                {
                    var v = (string?)val;
                    s = $"{space}{varname}.{pi.Name} = \"{val ?? ""}\";";
                    sb.AppendLine(s);
                }
                #endregion
                #region GoPadding
                else if (pi.PropertyType == typeof(GoPadding) && pi.CanWrite && val is GoPadding pad)
                {
                    var v = pad;
                    s = $"{space}{varname}.{pi.Name} = new GoPadding({v.Left}, {v.Top}, {v.Right}, {v.Bottom});";
                    sb.AppendLine(s);
                }
                #endregion
                #region SKColor
                else if (pi.PropertyType == typeof(SkiaSharp.SKColor) && pi.CanWrite && val is SKColor color)
                {
                    var v = color;
                    s = $"{space}{varname}.{pi.Name} = Util.FromArgb({v.Alpha}, {v.Red}, {v.Green}, {v.Blue});";
                    sb.AppendLine(s);
                }
                #endregion
                #region Enum
                else if (pi.PropertyType.IsEnum && pi.CanWrite)
                {
                    s = $"{space}{varname}.{pi.Name} = {pi.PropertyType.FullName}.{val?.ToString()};";
                    sb.AppendLine(s);
                }
                #endregion
                #region Enum?
                else if (pi.PropertyType.IsGenericType && pi.PropertyType.GenericTypeArguments[0].IsEnum && pi.CanWrite)
                {
                    if (val != null)
                    {
                        s = $"{space}{varname}.{pi.Name} = {pi.PropertyType.GenericTypeArguments[0].FullName}.{val?.ToString()};";
                        sb.AppendLine(s);
                    }
                    else if (val == null)
                    {
                        s = $"{space}{varname}.{pi.Name} = null;";
                        sb.AppendLine(s);
                    }
                }
                #endregion
                #region List<String>
                else if (pi.PropertyType == typeof(List<string>) && val is List<string> lsstr)
                {
                    var v = lsstr;
                    foreach (var str in v)
                    {
                        s = $"{space}{varname}.{pi.Name}.Add(\"{str}\");";
                        sb.AppendLine(s);
                    }
                }
                #endregion
                #region List<GoGridLayoutPanelRow>
                else if (pi.PropertyType == typeof(List<GoGridLayoutPanelRow>) && val is List<GoGridLayoutPanelRow> lsrows)
                {
                    var v = lsrows;
                    foreach (var vi in v)
                    {
                        var str = $"new GoGridLayoutPanelRow {{ Height = \"{vi.Height}\", Columns = [{vi.Columns.Select(x => $"\"{x}\", ")}] }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoListItem>
                else if (pi.PropertyType == typeof(List<GoListItem>) && val is List<GoListItem> li1)
                {
                    var v = li1;
                    foreach (var ti in v)
                    {
                        var str = $"var v = new GoListItem {{ Text = {strval(ti.Text)}, IconString = {strval(ti.IconString)} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region ObservableList<GoListItem>
                else if (pi.PropertyType == typeof(ObservableList<GoListItem>) && val is ObservableList<GoListItem> li2)
                {
                    var v = li2;
                    foreach (var ti in v)
                    {
                        var str = $"var v = new GoListItem {{ Text = {strval(ti.Text)}, IconString = {strval(ti.IconString)} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region ObservableList<GoMenuItem>
                else if (pi.PropertyType == typeof(ObservableList<GoMenuItem>) && val is ObservableList<GoMenuItem> lsmenu)
                {
                    var v = lsmenu;
                    foreach (var vi in v)
                    {
                        var str = $"new GoMenuItem {{ Text = {strval(vi.Text)}, IconString = {strval(vi.IconString)}, PageName = {strval(vi.PageName)}  }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoButtonItem>
                else if (pi.PropertyType == typeof(List<GoButtonItem>) && val is List<GoButtonItem> lsbi)
                {
                    var v = lsbi;
                    foreach (var vi in v)
                    {
                        var str = $"new GoButtonItem {{ Name = {strval(vi.Name)}, Size = {strval(vi.Size)}, Text = {strval(vi.Text)}, IconString = {strval(vi.IconString)} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoButtonsItem>
                else if (pi.PropertyType == typeof(List<GoButtonsItem>) && val is List<GoButtonsItem> lsbsi)
                {
                    var v = lsbsi;
                    foreach (var vi in v)
                    {
                        var str = $"new GoButtonsItem {{ Name = {strval(vi.Name)}, Size = {strval(vi.Size)}, Text = {strval(vi.Text)}, IconString = {strval(vi.IconString)} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region ObservableList<GoToolCategory>
                else if (pi.PropertyType == typeof(ObservableList<GoToolCategory>) && val is ObservableList<GoToolCategory> tci)
                {
                    var v = tci;
                    foreach (var vi in v)
                    {
                        sb.AppendLine($"{space}{{");
                        sb.AppendLine($"{space}    var tci = new GoToolCategory {{ Text = {strval(vi.Text)}, IconString = {strval(vi.IconString)} }};");
                        foreach (var vi2 in vi.Items)
                            sb.AppendLine($"{space}    tci.Items.Add(new GoToolItem {{ Text = {strval(vi2.Text)}, IconString = {strval(vi2.IconString)} }});");
                        sb.AppendLine($"{space}    {varname}.{pi.Name}.Add(tci);");
                        sb.AppendLine($"{space}}}");
                    }
                }
                #endregion
                #region List<StateImage>
                else if (pi.PropertyType == typeof(List<StateImage>) && val is List<StateImage> lssi)
                {
                    var v = lssi;
                    foreach (var vi in v)
                    {
                        var str = $"new StateImage {{ Image = {strval(vi.Image)}, State = {vi.State} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoGraphSeries>
                else if (pi.PropertyType == typeof(List<GoGraphSeries>) && val is List<GoGraphSeries> lsgs)
                {
                    var v = lsgs;
                    foreach (var vi in v)
                    {
                        var str = $"new GoGraphSeries {{ Name = {strval(vi.Name)}, Alias = {strval(vi.Alias)}, Color = {strval(vi.Color)}, Visible = {(vi.Visible ? "true" : "false")} }};";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoLineGraphSeries>
                else if (pi.PropertyType == typeof(List<GoLineGraphSeries>) && val is List<GoLineGraphSeries> lslgs)
                {
                    var v = lslgs;
                    foreach (var vi in v)
                    {
                        var str = $"new GoLineGraphSeries {{ Name = {strval(vi.Name)}, Alias = {strval(vi.Alias)}, Color = {strval(vi.Color)}, Minimum = {vi.Minimum}, Maximum = {vi.Maximum}, Visible = {(vi.Visible ? "true" : "false")} }};";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoSubPage>
                else if (pi.PropertyType == typeof(List<GoSubPage>) && val is List<GoSubPage> lssp)
                {
                    var v = lssp;
                    foreach (var ti in v)
                    {
                        var str = $"new GoSubPage {{ Name = {strval(ti.Name)}; }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region List<GoTabPage>
                else if (pi.PropertyType == typeof(List<GoTabPage>) && val is List<GoTabPage> lstp)
                {
                    var v = lstp;
                    foreach (var vi in v)
                    {
                        var str = $"new GoTabPage {{ Name = {strval(vi.Name)}, Text = {strval(vi.Text)}, IconString = {strval(vi.IconString)} }}";
                        sb.AppendLine($"{space}{varname}.{pi.Name}.Add({str});");
                    }
                }
                #endregion
                #region TreeViewNodeCollection
                else if (pi.PropertyType == typeof(ObservableList<GoTreeNode>) && val is ObservableList<GoTreeNode> lstn)
                {
                    var v = lstn;
                    var lsv = GetTreeItem(v);

                    sb.AppendLine($"{space}{{");
                    foreach (var vi in lsv) sb.AppendLine($"{space}    var {vi.Name} = new GoTreeNode {{ Text = {strval(vi.Node.Text)}, IconString = {strval(vi.Node.IconString)} }};");
                    sb.AppendLine($"");
                    foreach (var vi in lsv)
                    {
                        if (vi.Parent == "") sb.AppendLine($"{space}    {varname}.{pi.Name}.Add({vi.Name});");
                        else sb.AppendLine($"{space}    {vi.Parent}.Nodes.Add({vi.Name});");
                    }
                    sb.AppendLine($"{space}}}");
                }
                #endregion
            }
        }
        #endregion
        #region MakeDesignBarCode
        static void MakeDesignBarCode<T>(StringBuilder sb, string space, string varname, T con, List<IGoControl> ls)
        {
            sb.AppendLine($"{space}var json = Encoding.UTF8.GetString(Convert.FromBase64String(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(con, GoJsonConverter.Options)))}\"));");
            sb.AppendLine($"{space}var c = JsonSerializer.Deserialize<{TypeName(con)}>(json, GoJsonConverter.Options);");
            var props = con.GetType().GetProperties().Where(x => ValidProp(x));
            foreach (var pi in props) sb.AppendLine($"{space}{varname}.{pi.Name} = c.{pi.Name};");
            sb.AppendLine($"{space}{varname}.Childrens.AddRange(c.Childrens);");
            sb.AppendLine($"");
            sb.AppendLine($"{space}var dic = Util.AllControls({varname}).ToDictionary(x => x.Id.ToString(), y => y);");
            foreach (var c in ls) sb.AppendLine($"{space}{c.Name} = ({TypeName(c)})dic[\"{c.Id}\"];");
        }
        #endregion
        #region MakePageCode
        public static (string page, string design) MakePageCode(string projecjtName, GoPage page)
        {
            string pageCode, designCode;
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
                sb.AppendLine($"namespace {projecjtName}.Pages");
                sb.AppendLine($"{{");
                sb.AppendLine($"    public partial class {page.Name} : {(page is IcPage ? "IcPage" : "GoPage")}");
                sb.AppendLine($"    {{");
                sb.AppendLine($"        public {page.Name}()");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            InitializeComponent();");
                sb.AppendLine($"        }}");
                sb.AppendLine($"    }}");
                sb.AppendLine($"}}");

                pageCode = sb.ToString();
            }
            #endregion
            #region Designer
            {
                var ls = All(page).Where(x => x is IGoControl c && !string.IsNullOrWhiteSpace(c.Name)).Select(x => (IGoControl)x).ToList();

                var sb = new StringBuilder();
                sb.AppendLine($"#region declare");
                foreach (var v in ls) sb.AppendLine($"{TypeName(v)} {v.Name};");
                sb.AppendLine($"#endregion");
                sb.AppendLine($"");
                sb.AppendLine($"#region initialize");
                MakeDesignBarCode(sb, "", "Page", page, ls);
                sb.AppendLine($"#endregion");

                designCode = sb.ToString();
            }
            #endregion

            return (pageCode, designCode);
        }
        #endregion

        #region GetTreeItem
        static List<TreeItem> GetTreeItem(ObservableList<GoTreeNode> collection)
        {
            var ret = new List<TreeItem>();
            for (int i = 0; i < collection.Count; i++)
            {
                ret.AddRange(GetTreeItem(collection[i], $"tvn{i}", ""));
            }
            return ret;
        }

        static List<TreeItem> GetTreeItem(GoTreeNode nd, string name, string parent)
        {
            var ret = new List<TreeItem>
            {
                new(name, parent, nd)
            };
            for (int i = 0; i < nd.Nodes.Count; i++) ret.AddRange(GetTreeItem(nd.Nodes[i], $"{name}_{i}", name));
            return ret;
        }
        #endregion

        #region Control
        #region All
        public static List<object> All(object container)
        {
            List<object> ret = [];

            if (container is GoSwitchPanel sw)
            {
                foreach (var p in sw.Pages)
                {
                    ret.Add(p);
                    ret.AddRange(All(p));
                }
            }
            else if (container is GoTabControl tab)
            {
                foreach (var p in tab.TabPages)
                {
                    ret.Add(p);
                    ret.AddRange(All(p));
                }
            }
            else if (container is GoSubPage sp)
            {
                foreach (var c in sp.Childrens)
                {
                    ret.Add(c);
                    if (c is IGoContainer vcon) ret.AddRange(All(vcon));
                }
            }
            else if (container is GoTabPage tp)
            {
                foreach (var c in tp.Childrens)
                {
                    ret.Add(c);
                    if (c is IGoContainer vcon) ret.AddRange(All(vcon));
                }
            }
            else if (container is IGoContainer con)
            {
                foreach (var c in con.Childrens)
                {
                    ret.Add(c);
                    if (c is IGoContainer vcon) ret.AddRange(All(vcon));
                }
            }

            return ret;
        }
        #endregion
        #region TypeName
        static string TypeName(object c)
        {
            var type = c.GetType();
            if (type.IsGenericType)
            {
                var vs = type.Name.Split('`');
                return $"{vs[0]}<{type.GenericTypeArguments[0].Name}>";
            }
            else return type.Name;
        }
        #endregion
        #endregion

        #region val
        static string strval(string? v) => (v != null ? $"\"{v}\"" : "null");
        #endregion
        #region color
        static string color(SKColor c) => $"Util.FromArgb({c.Alpha}, {c.Red}, {c.Green}, {c.Blue});";
        #endregion
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
