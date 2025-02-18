using Going.UI.Controls;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Going.UI.Managers
{
    public class GoInputEventer
    {
        private static readonly Lazy<GoInputEventer> _instance = new Lazy<GoInputEventer>(() => new GoInputEventer());
        public static GoInputEventer Current => _instance.Value;

        public IGoControl? InputControl { get; private set; }

        public event Action<IGoControl, SKRect, Action<string>, string?>? InputString;
        public event Action<IGoControl, SKRect, Action<string>, Type, object, object?, object?>? InputNumber;

        public void GenString(IGoControl control, SKRect bounds, Action<string> callback, string? value) 
            => InputString?.Invoke(control, bounds, callback, value);
        public void GenNumber<T>(IGoControl control, SKRect bounds, Action<string> callback, T value, T? min, T? max) where T : struct
            => InputNumber?.Invoke(control, bounds, callback, typeof(T), value, min, max);

        public void SetInputControl(IGoControl control) => InputControl = control;
        public void ClearInputControl() => InputControl = null;
    }

    public enum InputType { String, Number }

}
