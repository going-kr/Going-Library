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
        GoRangeSlider rsld;
        public PageControl()
        {
            Name = "PageControl";

            prgs = new GoProgress { Left = 10, Top = 10, Width = 200, Height = 40 };
            sldh = new GoSlider { Left = 10, Top = 60, Width = 200, Height = 40 };
            sldv = new GoSlider { Left = 210, Top = 10, Width = 40, Height = 200, Direction = GoDirectionHV.Vertical };
            rsld = new GoRangeSlider { Left = 10, Top = 110, Width = 200, Height = 40 };

            Childrens.Add(prgs);
            Childrens.Add(sldv);
            Childrens.Add(sldh);
            Childrens.Add(rsld);

            sldv.ValueChanged += (o, s) => prgs.Value = sldv.Value;
        }
    }
}
