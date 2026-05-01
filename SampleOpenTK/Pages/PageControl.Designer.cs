using System.Text;
using System.Text.Json;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Json;
using Going.UI.OpenTK.Windows;
using Going.UI.Utils;
using Going.UI.ImageCanvas;
using OpenTK.Windowing.Common;
using SkiaSharp;

namespace Sample.Pages
{
    partial class PageControl
    {
        #region declare
        // 동적 데모 컨트롤 — design.json 외부 (TitleBoxDraw + GoStateLamp 시연)
        GoValueNumber<double> demoValTitleBoxOn;
        GoValueNumber<double> demoValTitleBoxOff;
        GoInputNumber<int> demoInpTitleBoxOn;
        GoInputNumber<int> demoInpTitleBoxOff;
        GoStateLamp demoStateLamp;
        GoLabel demoSectionLabel;
        #endregion

        public void InitializeComponent()
        {
            #region base
            var c = MainWindow.Current.DS.Pages["PageControl"];
            this.BackgroundImage = c.BackgroundImage;
            this.Name = c.Name;
            this.Visible = c.Visible;
            this.Enabled = c.Enabled;
            this.UseLongClick = c.UseLongClick;
            this.LongClickTime = c.LongClickTime;
            this.Bounds = c.Bounds;
            this.Dock = c.Dock;
            this.Margin = c.Margin;
            this.Childrens.AddRange(c.Childrens);

            var dic = Util.AllControls(this).ToDictionary(x => x.Id.ToString(), y => y);
            #endregion

            #region 동적 데모 — TitleBoxDraw + GoStateLamp 시연
            // page 우측 (x=420 부터) 세로 배치. design.json 의 기존 컨트롤과 겹치지 않도록.

            // 섹션 라벨
            demoSectionLabel = new GoLabel
            {
                Name = "demoSectionLabel",
                Text = "── New Controls Demo ──",
                FontStyle = GoFontStyle.Bold, FontSize = 13,
                TextColor = "Highlight",
                Bounds = new SKRect(420, 10, 780, 36),
                ContentAlignment = GoContentAlignment.MiddleLeft,
            };
            this.Childrens.Add(demoSectionLabel);

            // TitleBoxDraw=true (기존 동작) — GoValueNumber
            demoValTitleBoxOn = new GoValueNumber<double>
            {
                Name = "demoValTitleBoxOn",
                Title = "TitleBox ON", TitleSize = 110, TitleBoxDraw = true,
                Value = 48.5, FormatString = "0.0", Unit = "kW", UnitSize = 36,
                FontStyle = GoFontStyle.Bold, FontSize = 14,
                TextColor = "Fore", FillColor = "Base3", ValueColor = "Base1", BorderColor = "Base3",
                Bounds = new SKRect(420, 46, 780, 86),
            };
            this.Childrens.Add(demoValTitleBoxOn);

            // TitleBoxDraw=false (신규) — Title 박스 X, 텍스트만 + Value round 흡수
            demoValTitleBoxOff = new GoValueNumber<double>
            {
                Name = "demoValTitleBoxOff",
                Title = "TitleBox OFF", TitleSize = 110, TitleBoxDraw = false,
                Value = 32.1, FormatString = "0.0", Unit = "kW", UnitSize = 36,
                FontStyle = GoFontStyle.Bold, FontSize = 14,
                TextColor = "Highlight", FillColor = "Base3", ValueColor = "Base1", BorderColor = "Base3",
                Bounds = new SKRect(420, 92, 780, 132),
            };
            this.Childrens.Add(demoValTitleBoxOff);

            // GoInput — TitleBox ON
            demoInpTitleBoxOn = new GoInputNumber<int>
            {
                Name = "demoInpTitleBoxOn",
                Title = "Port (Box ON)", TitleSize = 110, TitleBoxDraw = true,
                Value = 502, FormatString = "0",
                FontSize = 13,
                TextColor = "Fore", FillColor = "Base3", ValueColor = "Base1", BorderColor = "Base3",
                Bounds = new SKRect(420, 142, 780, 182),
            };
            this.Childrens.Add(demoInpTitleBoxOn);

            // GoInput — TitleBox OFF
            demoInpTitleBoxOff = new GoInputNumber<int>
            {
                Name = "demoInpTitleBoxOff",
                Title = "Port (Box OFF)", TitleSize = 110, TitleBoxDraw = false,
                Value = 503, FormatString = "0",
                FontSize = 13,
                TextColor = "Highlight", FillColor = "Base3", ValueColor = "Base1", BorderColor = "Base3",
                Bounds = new SKRect(420, 188, 780, 228),
            };
            this.Childrens.Add(demoInpTitleBoxOff);

            // GoStateLamp — multi-state (STOPPED/HEATING/RUNNING/FAULT). State=2 (RUNNING) 으로 초기화
            demoStateLamp = new GoStateLamp
            {
                Name = "demoStateLamp",
                LampSize = 22, FontSize = 14,
                ContentAlignment = GoContentAlignment.MiddleLeft,
                FontStyle = GoFontStyle.Bold,
                TextColor = "Fore", //OffColor = "Base3",
                Bounds = new SKRect(420, 244, 780, 290),
                State = 2,
            };
            demoStateLamp.States.Add(new StateLamp { State = 0, Color = "Base4", Text = "STOPPED" });
            demoStateLamp.States.Add(new StateLamp { State = 1, Color = "Warning", Text = "HEATING" });
            demoStateLamp.States.Add(new StateLamp { State = 2, Color = "Good", Text = "RUNNING" });
            demoStateLamp.States.Add(new StateLamp { State = 3, Color = "Danger", Text = "FAULT" });
            this.Childrens.Add(demoStateLamp);
            #endregion
        }
    }
}
