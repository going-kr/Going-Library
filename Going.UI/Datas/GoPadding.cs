using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public struct GoPadding
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public GoPadding(float all) => this.Left = this.Top = this.Right = this.Bottom = all;
        public GoPadding(float Left, float Top, float Right, float Bottom)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
        }

        //주석
    }
}
