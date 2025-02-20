using Going.UI.Controls;
using Going.UI.Datas;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public interface IGoContainer 
    {
        IEnumerable<IGoControl> Childrens { get; }
        SKRect Bounds { get; set; }
    }

}
