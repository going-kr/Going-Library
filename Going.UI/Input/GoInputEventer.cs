using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Going.UI.Input
{
    public class GoInputEventer
    {
        private static readonly Lazy<GoInputEventer> _instance = new Lazy<GoInputEventer>(() => new GoInputEventer());
        public static GoInputEventer Current => _instance.Value;
        
        public IGoControl? InputControl { get; private set; }

        public event Action<IGoControl, SKRect, Action<string>, string?>? InputString;

        public void GenString(IGoControl control, SKRect bounds, Action<string> callback, string? value) => InputString?.Invoke(control, bounds, callback, value);
        public void SetInputControl(IGoControl control) => InputControl = control;
        public void ClearInputControl() => InputControl = null;


    }

    public enum InputType { String, Integer, Floating }

}
