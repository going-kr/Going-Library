﻿using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoListItem
    {
        [GoProperty(PCategory.Basic, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Basic, 1)] public string? Text { get; set; }
        public object? Tag { get; set; }

        internal SKRect Bounds { get; set; }

        public override string ToString() => Text ?? string.Empty;
    }

    public class ListItemEventArgs(GoListItem itm) : EventArgs
    {
        public GoListItem Item { get; } = itm;
    }
}
