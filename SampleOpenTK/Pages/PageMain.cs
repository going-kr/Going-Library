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
        public PageMain()
        {
            Name = "PageMain";

            Childrens.Add(new GoStep { Left = 10, Top = 10, Width = 300, Height = 40, IsCircle = true });

        }
    }
}
