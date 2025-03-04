using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class PageTest:GoPage
    {
        GoButton btnMain, btn;
        GoOnOff onoff;
        GoSwitch sw;
        GoNumberBox nb;
        GoTableLayoutPanel tpnl;
        GoSwitchPanel mpnl;
        GoScrollablePanel spnl;

        public PageTest()
        {
            Name = "PageTest";

            #region Constrols
            btnMain = new GoButton { Name = "btn", Left = 10, Top = 10, Width = 80, Height = 40, Text = "메인 페이지" };
            btn = new GoButton { Left = 10, Top = 60, Width = 80, Height = 40, Text = "테스트" };
            onoff = new GoOnOff { Left = 10, Top = 110, Width = 150, Height = 40 };
            sw = new GoSwitch { Left = 10, Top = 160, Width = 150, Height = 40 };
            nb = new GoNumberBox { Left = 10, Top = 210, Width = 150, Height = 40 };

            Childrens.Add(btnMain);
            Childrens.Add(btn);
            Childrens.Add(onoff);
            Childrens.Add(sw);
            Childrens.Add(nb);
            #endregion

            #region TableLayout / IconButton
            tpnl = new GoTableLayoutPanel { Left = 170, Top = 10, Width = 240, Height = 240 };
            tpnl.Columns = ["33.3%", "33.4%", "33.3%"];
            tpnl.Rows = ["33.3%", "33.4%", "33.3%"];
            Childrens.Add(tpnl);

            tpnl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 270 }, 1, 0);
            tpnl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 180 }, 0, 1);
            tpnl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-stop", Rotate = 0 }, 1, 1);
            tpnl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 0 }, 2, 1);
            tpnl.Childrens.Add(new GoIconButton { Fill = true, IconString = "fa-play", Rotate = 90 }, 1, 2);
            #endregion

            #region SwitchPanel
            mpnl = new GoSwitchPanel { Left = 510, Top = 10, Width = 400, Height = 240 };
            for (int i = 0; i < 5; i++)
            {
                var vp = new GoSubPage() { Name = $"P{i + 1}" };
                vp.Childrens.Add(new GoLabel { Left = 0, Top = 0, Width = 100, Height = 40, Text = $"페이지 {i + 1}", BorderOnly = true, LabelColor = "base3" });
                mpnl.Pages.Add(vp);
            }
            Childrens.Add(mpnl);

            for (int i = 0; i < 5; i++)
            {
                var vb = new GoButton { Left = 420, Top = 10 + (i * 50), Width = 80, Height = 40, Text = $"페이지 {i + 1}" };
                vb.ButtonClicked += (o, s) => mpnl.SetPage($"P{((GoButton)o!).Text.Substring(4)}");
                Childrens.Add(vb);
            }
            #endregion

            #region ScrollablePanel
            spnl = new GoScrollablePanel { Fill = true, Margin = new Going.UI.Datas.GoPadding(10, 260, 10, 10) };
            for (int i = 0; i < 30; i++) spnl.Childrens.Add(new GoButton { Left = 0, Top = 0 + (i * 50), Width = 100, Height = 40, Text = $"버튼{i + 1}" });
            Childrens.Add(spnl);

            var pic = new GoPicture { Left = 110, Top = 0, Width = 300, Height = 150, Image = "nature", ScaleMode = GoImageScaleMode.Zoom };
            var ani = new GoAnimate { Left = 110, Top = 160, Width = 300, Height = 300, OffImage = "ani_off", OnImage = "ani", ScaleMode = GoImageScaleMode.Zoom };
            spnl.Childrens.Add(pic);
            spnl.Childrens.Add(ani);

            ani.MouseClicked += (o, s) => ani.OnOff = !ani.OnOff;
            #endregion

            btnMain.ButtonClicked += (o, s) => Design?.SetPage("PageMain");
        }
    }
}
