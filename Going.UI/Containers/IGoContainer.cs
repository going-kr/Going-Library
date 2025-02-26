﻿using Going.UI.Controls;
using Going.UI.Datas;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public interface IGoContainer 
    {
        IEnumerable<IGoControl> Childrens { get; }
        SKRect Bounds { get; set; }

        [JsonIgnore] float ViewX { get; }
        [JsonIgnore] float ViewY { get; }
        [JsonIgnore] float ViewWidth { get; }
        [JsonIgnore] float ViewHeight { get; }
    }

}
