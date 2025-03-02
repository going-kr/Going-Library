using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.ImageCanvas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class PageIC : IcPage
    {
        IcLabel lblTime;
        List<Box> boxes = [];
        DateTime prev = DateTime.Now;

        public PageIC()
        {
            Name = "main";
            BackgroundColor = "#f2f2f2";

            var btnStart = new IcButton { Left = 0, Top = 704, Width = 256, Height = 64, Text = "전체 운전", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "white" };
            var btnStop = new IcButton { Left = 256, Top = 704, Width = 256, Height = 64, Text = "전체 정지", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "white" };
            var btnSchedule = new IcButton { Left = 512, Top = 704, Width = 256, Height = 64, Text = "스케쥴 확인", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "white" };
            var btnAlarm = new IcButton { Left = 768, Top = 704, Width = 256, Height = 64, Text = "알람 이력", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "white" };
            var lblTitle = new IcLabel { Left = 20, Top = 0, Width = 256, Height = 60, Text = "A 구역 난방기 운전 화면", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "black", ContentAlignment = GoContentAlignment.MiddleLeft };
            lblTime = new IcLabel { Left = 748, Top = 0, Width = 256, Height = 60, Text = "A 구역 난방기 운전 화면", FontName = "나눔바른고딕", FontSize = 16, FontStyle = GoFontStyle.Bold, TextColor = "black", ContentAlignment = GoContentAlignment.MiddleRight };

            var prgs = new IcProgress { Left = 290, Top = 480, Width = 440, Height = 60, BarImage = "prgs", FontName = "나눔바른고딕", FontSize = 14, FontStyle = GoFontStyle.Bold, TextColor = "black" };
            var sld = new IcSlider { Left = 290, Top = 550, Width = 440, Height = 60, BarImage = "bar", CursorImage = "sld", FontName = "나눔바른고딕", FontSize = 14, FontStyle = GoFontStyle.Bold, TextColor = "black", Tick = 5 };

            for (int i = 0; i < 10; i++)
            {
                var box = new Box { Left = 20 + ((i % 5) * 200), Top = 70 + ((i / 5) * 200), Width = 187, Height = 178, Title = $"A-{i + 1}" };
                boxes.Add(box);
                Childrens.Add(box);
            }

            Childrens.Add(btnStart);
            Childrens.Add(btnStop);
            Childrens.Add(btnSchedule);
            Childrens.Add(btnAlarm);
            Childrens.Add(lblTitle);
            Childrens.Add(lblTime);
            Childrens.Add(prgs);
            Childrens.Add(sld);

            sld.ValueChanged += (o, s) => prgs.Value = sld.Value;
            btnAlarm.ButtonClicked += (o, s) => Design?.SetPage("PageMain");
        }

        protected override void OnUpdate()
        {
            var sec = Convert.ToInt32((DateTime.Now - prev).TotalSeconds);
            lblTime.Text = DateTime.Now.ToString("yyyy'/'MM'/'dd (ddd) HH:mm:ss", new CultureInfo("ko-KR"));
            
            for (int i = 1; i <= 10; i++)
            {
                var box = boxes[i - 1];
                if (box != null)
                {
                    box.IconState = sec % 3;
                    box.Alarm = sec % 2 == 0;
                    box.Run = sec % 2 == 1;
                }
            }

            base.OnUpdate();
        }
    }
}
