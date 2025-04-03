using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleOpenTK.Pages
{
    public class PageControl : GoPage
    {
        GoProgress prgs;
        GoSlider sldh, sldv;
        GoRangeSlider rsldh, rsldv;
        public PageControl()
        {
            Name = "PageControl";
            var pg1 = new GoProgress { Left = 10, Top = 10, Width = 200, Height = 40, Title = "Progress", Direction = ProgressDirection.LeftToRight };            
            var pg2 = new GoProgress { Left = 10, Top = 60, Width = 200, Height = 40, Title = "Progress", Direction = ProgressDirection.RightToLeft };
            var pg3 = new GoProgress { Left = 10, Top = 130, Width = 40, Height = 200, Title = "Progress", Direction = ProgressDirection.BottomToTop };
            var pg4 = new GoProgress { Left = 60, Top = 130, Width = 40, Height = 200, Title = "Progress", Direction = ProgressDirection.TopToBottom };
            var slider = new GoSlider { Left = 10, Top = 400, Width = 200, Height = 30 };
            Childrens.Add(pg1);
            Childrens.Add(pg2);
            Childrens.Add(pg3);
            Childrens.Add(pg4);
            Childrens.Add(slider);

            prgs = new GoProgress { Left = 10, Top = 10, Width = 200, Height = 40 };
            sldh = new GoSlider { Left = 10, Top = 60, Width = 200, Height = 40 };
            sldv = new GoSlider { Left = 220, Top = 0, Width = 40, Height = 200, Direction = GoDirectionHV.Vertical };
            rsldh = new GoRangeSlider { Left = 10, Top = 110, Width = 200, Height = 40 };
            rsldv = new GoRangeSlider { Left = 280, Top = 0, Width = 40, Height = 200, Direction = GoDirectionHV.Vertical };

            Childrens.Add(prgs);
            Childrens.Add(sldh);
            Childrens.Add(sldv);
            Childrens.Add(rsldh);
            Childrens.Add(rsldv);

            sldv.ValueChanged += (o, s) => prgs.Value = sldv.Value;
        }
        #endregion
    }
}
