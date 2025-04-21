using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SampleOpenTK.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK.Windows
{
    public class MainWindow : GoViewWindow
    {
        GoTableLayoutPanel tblTitle;
        GoNavigator nav;

        public MainWindow() : base(1024, 768, WindowBorder.Resizable)
        {
            #region Window Setting
            Title = "Going Library Sample";
            TitleIconImage = "logos";

            Debug = false;
            VSync = VSyncMode.On;
            CenterWindow();
            #endregion

            #region Design Setting
            Design.AddImageFolder("D:\\Project\\Going\\library\\src\\Going\\ImageSample");

            Design.UseTitleBar = true;
            Design.UseLeftSideBar = true;
            Design.UseRightSideBar = true;
            Design.UseFooter = true;

            Design.TitleBar.Title = "Going Library Sample";
            Design.TitleBar.TitleImage = "logos";

            Design.OverlaySideBar = true;
            #endregion

            #region Menu / Sidebar / Footer
            tblTitle = new GoTableLayoutPanel { Fill = true, Columns = ["100%", "50px", "50px"], Rows = ["100%"] };
            nav = new GoNavigator { Fill = true };

            string tp = "Top";
            if (tp == "Top")
            {
                nav.Direction = Going.UI.Enums.GoDirectionHV.Horizon;
                nav.Menus.Add(new GoMenuItem { Text = "Main", PageName = "PageMain" });
                nav.Menus.Add(new GoMenuItem { Text = "Control", PageName = "PageControl" });
                nav.Menus.Add(new GoMenuItem { Text = "Gauge", PageName = "PageGauge" });
                nav.Menus.Add(new GoMenuItem { Text = "Input", PageName = "PageInput" });
                nav.Menus.Add(new GoMenuItem { Text = "List", PageName = "PageList" });
                nav.Menus.Add(new GoMenuItem { Text = "Graph", PageName = "PageGraph" });
                nav.Menus.Add(new GoMenuItem { Text = "DataGrid", PageName = "PageDataGrid" });
                tblTitle.Childrens.Add(nav, 0, 0);
            }
            else 
            {
                nav.Direction = Going.UI.Enums.GoDirectionHV.Vertical;
                nav.Menus.Add(new GoMenuItem { Text = "Main", PageName = "PageMain" });
                nav.Menus.Add(new GoMenuItem { Text = "Control", PageName = "PageControl" });
                nav.Menus.Add(new GoMenuItem { Text = "Gauge", PageName = "PageGauge" });
                nav.Menus.Add(new GoMenuItem { Text = "Input", PageName = "PageInput" });
                nav.Menus.Add(new GoMenuItem { Text = "List", PageName = "PageList" });
                nav.Menus.Add(new GoMenuItem { Text = "Graph", PageName = "PageGraph" });
                nav.Menus.Add(new GoMenuItem { Text = "DataGrid", PageName = "PageDataGrid" });
                Design.LeftSideBar.Childrens.Add(nav);
            }

            tblTitle.Childrens.Add(new GoButton { Fill = true, BackgroundDraw = false, Text = "", IconSize = 18, IconString = "fa-play" }, 1, 0);
            tblTitle.Childrens.Add(new GoButton { Fill = true, BackgroundDraw = false, Text = "", IconSize = 18, IconString = "fa-stop" }, 2, 0);
            
            Design.TitleBar.Childrens.Add(tblTitle);

            Design.Footer.Childrens.Add(new GoLabel { Fill = true, Margin = new GoPadding(0), BackgroundDraw = false, FontSize = 11, TextColor = "#999", Text = "서울특별시 영등포구 문래북로 8, 1208호(문래동6가, 에이스엔에스타워)\r\nCOPYRIGHT © GOING. ALL RIGHTS RESERVED." });
            #endregion

            #region Pages
            Design.AddPage(new PageMain());
            Design.AddPage(new PageControl());
            Design.AddPage(new PageGauge());
            Design.AddPage(new PageGraph());
            Design.AddPage(new PageInput());
            Design.AddPage(new PageList());
            Design.AddPage(new PageDataGrid());
            Design.SetPage("PageMain");
            #endregion
        }
    }
}
