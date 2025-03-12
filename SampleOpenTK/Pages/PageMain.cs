using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Json;
using SkiaSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SampleOpenTK.Pages
{
    public class PageMain : GoPage
    {
        Thread th;
        int Value = 0;

        public PageMain()
        {
            Name = "PageMain";

            Childrens.Add(new GoStep { Left = 10, Top = 10, Width = 300, Height = 40, IsCircle = true });

            Childrens.Add(new GoProgress { Left = 10, Top = 100, Width = 500, Height = 30, Value = Value,Title = ""});
            Childrens.Add(new GoProgress { Left = 10, Top = 150, Width = 500, Height = 30, Value = Value, Title = "" });
            Childrens.Add(new GoProgress { Left = 600, Top = 10, Width = 30, Height = 300, Value = Value, Title = "" });
            Childrens.Add(new GoProgress { Left = 700, Top = 10, Width = 30, Height = 300, Value = Value, Title = "" });
    
            var pg1 = Childrens[1] as GoProgress;
            var pg2 = Childrens[2] as GoProgress;
            var pg3 = Childrens[3] as GoProgress;
            var pg4 = Childrens[4] as GoProgress;

            pg1.Direction = ProgressDirection.LeftToRight;
            pg2.Direction = ProgressDirection.RightToLeft;
            pg3.Direction = ProgressDirection.BottomToTop;
            pg4.Direction = ProgressDirection.TopToBottom;

            pg1.FillColor = "Good";
            pg2.FillColor = "warning";
            pg3.FillColor = "danger";
            pg4.FillColor = "hignlight";
            

            th = new Thread(() =>
            {
                while (true)
                {
                    Value++;

                    
                    pg1.Value = Value;
                    pg2.Value = Value;
                    pg3.Value = Value;
                    pg4.Value = Value;
                    
                    if (Value >= 100) Value = 0;
                    Thread.Sleep(50);
                }
            })
            { IsBackground = true };
            th.Start();
        }
    }
}
