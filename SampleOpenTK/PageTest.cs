using Going.UI.Controls;
using Going.UI.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK
{
    public class PageTest:GoPage
    {
        public PageTest()
        {
            Name = "PageTest";

            var btn = new GoButton { Name = "btn", Left = 10, Top = 10, Width = 80, Height = 40, Text = "메인 페이지" };
            Childrens.Add(btn);
            Childrens.Add(new GoButton { Left = 10, Top = 60, Width = 80, Height = 40, Text = "테스트" });

            btn.ButtonClicked += (o, s) =>
            {
                Design?.SetPage("PageMain");
            };

        }
    }
}
