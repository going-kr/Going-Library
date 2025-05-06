using Going.UI.Controls;
using Going.UI.Datas;
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
        public PageControl()
        {
            Name = "PageControl";
            var pg1 = new GoProgress { Left = 10, Top = 10, Width = 200, Height = 40, Direction = ProgressDirection.LeftToRight };            
            var pg2 = new GoProgress { Left = 10, Top = 60, Width = 200, Height = 40, Direction = ProgressDirection.RightToLeft };
            var pg3 = new GoProgress { Left = 10, Top = 130, Width = 40, Height = 200, Direction = ProgressDirection.BottomToTop };
            var pg4 = new GoProgress { Left = 60, Top = 130, Width = 40, Height = 200, Direction = ProgressDirection.TopToBottom };
            var slider = new GoSlider { Left = 10, Top = 400, Width = 200, Height = 30, HandleRadius = 14 };
            Childrens.Add(pg1);
            Childrens.Add(pg2);
            Childrens.Add(pg3);
            Childrens.Add(pg4);
            Childrens.Add(slider);

            Childrens.Add(new GoInputCombo { Left = 10, Top = 500, Width = 200, Height = 40, Items = [.. Enum.GetValues<DayOfWeek>().Select(x => new GoListItem { Text = x.ToString() })] });
            Childrens.Add(new GoListBox { Left = 300, Top = 300, Width = 200, Height = 150, Items = [.. Enum.GetValues<DayOfWeek>().Select(x => new GoListItem { Text = x.ToString() })] });
        }
    }
}
