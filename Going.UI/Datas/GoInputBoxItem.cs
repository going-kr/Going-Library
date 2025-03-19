using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    #region attr : InputBoxIgnore
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InputBoxIgnoreAttribute : Attribute { }
    #endregion
    #region class : InputBoxTag
    internal class InputBoxTag
    {
        public PropertyInfo? prop { get; set; }
        public InputBoxInfo? attr { get; set; }
    }
    #endregion
    #region class : InputBoxInfo
    public class InputBoxInfo
    {
        public decimal? Minimum { get; set; } = null;
        public decimal? Maximum { get; set; } = null;

        public string? Title { get; set; }
        public string? FormatString { get; set; }
        public string? OnText { get; set; }
        public string? OffText { get; set; }
        public List<GoListItem>? Items { get; set; }
    }
    #endregion
}
