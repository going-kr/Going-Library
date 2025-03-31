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
        #region Variables
        private int Value = 0;
        private bool increasing = true;
        private DateTime updatetime = DateTime.Now;
        #endregion
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

            slider.ValueChanged += (o, s) =>
            {
                foreach (var child in Childrens)
                {
                    if (child is GoProgress pg)
                        pg.Value = slider.Value;
                }
            };
        }

        #region Override
        protected override void OnUpdate()
        {
            /*
            if ((DateTime.Now - updatetime).TotalMilliseconds >= 100)
            {
                updatetime = DateTime.Now;
                if (increasing)
                {
                    Value += 1;
                    if (Value >= 100)
                    {
                        Value = 100;
                        increasing = false;
                    }
                }
                else
                {
                    Value -= 1;
                    if (Value <= 0)
                    {
                        Value = 0;
                        increasing = true;
                    }
                }
                
                foreach (var child in Childrens)
                {
                    if (child is GoProgress pg)
                        pg.Value = Value;
                }
            }
            */
            base.OnUpdate();
        }
        #endregion
    }
}
