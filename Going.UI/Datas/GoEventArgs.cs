using Going.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    public class GoMouseEventArgs(float x, float y) : EventArgs
    {
        public float X { get; private set; } = x;
        public float Y { get; private set; } = y;
    }

    public class GoMouseClickEventArgs(float x, float y, GoMouseButton button) : GoMouseEventArgs(x, y)
    {
        public GoMouseButton Button { get; private set; } = button;
    }

    public class GoMouseWheelEventArgs(float x, float y, float delta) : GoMouseEventArgs(x, y)
    {
        public float Delta { get; private set; }
    }

    public class GoCancelableEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    public class GoDragEventArgs(float x, float y, object dragItem) : EventArgs
    {
        public float X { get; private set; } = x;
        public float Y { get; private set; } = y;
        public object DragItem { get; private set; } = dragItem;
    }
}
