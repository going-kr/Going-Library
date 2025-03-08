using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Json;
using SkiaSharp;
using System;
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
        GoScrollablePanel spnl;
        public PageMain()
        {
            Name = "PageMain";

            spnl = new GoScrollablePanel { Fill = true };
            Childrens.Add(spnl);

            for (int i = 0; i < 50; i++)
                spnl.Childrens.Add(new GoButton { Left = 10, Top = 10 + (i * 50), Width = 150, Height = 40, Text = $"테스트 {i + 1}" });
        }
    }
}
