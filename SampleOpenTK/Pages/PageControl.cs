using Going.UI.Controls;
using Going.UI.Design;
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
        GoSlider sld;
        GoRangeSlider rsld;
        public PageControl()
        {
            Name = "PageControl";

            prgs = new GoProgress { Left = 10, Top = 10, Width = 200, Height = 40 };
            sld = new GoSlider { Left = 10, Top = 60, Width = 200, Height = 40 };
            rsld = new GoRangeSlider { Left = 10, Top = 110, Width = 200, Height = 40 };

            Childrens.Add(prgs);
            Childrens.Add(sld);
            Childrens.Add(rsld);

            sld.ValueChanged += (o, s) => prgs.Value = sld.Value; 
        }
    }
}
