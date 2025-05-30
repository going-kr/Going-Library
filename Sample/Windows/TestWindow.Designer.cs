using System.Text;
using System.Text.Json;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Json;
using Going.UI.OpenTK.Windows;
using Going.UI.Utils;
using OpenTK.Windowing.Common;

namespace test.Windows
{
    partial class TestWindow
    {
        #region declare
        #endregion

        public void InitializeComponent()
        {
            #region base
            var json = Encoding.UTF8.GetString(Convert.FromBase64String("eyJJY29uU3RyaW5nIjpudWxsLCJJY29uU2l6ZSI6MTIsIkljb25HYXAiOjUsIlRleHQiOiJXaW5kb3ciLCJGb250TmFtZSI6Ilx1QjA5OFx1QjIxNFx1QUNFMFx1QjUxNSIsIkZvbnRTdHlsZSI6MCwiRm9udFNpemUiOjEyLCJUZXh0Q29sb3IiOiJGb3JlIiwiV2luZG93Q29sb3IiOiJCYWNrIiwiQm9yZGVyQ29sb3IiOiJCYXNlMiIsIlJvdW5kIjoxLCJUaXRsZUhlaWdodCI6NDAsIkNoaWxkcmVucyI6W10sIklkIjoiY2VmZWRlOGUtMjk3YS00MDczLTg1ZTktZDNjZTVlMDIxYzg1IiwiTmFtZSI6IlRlc3RXaW5kb3ciLCJWaXNpYmxlIjp0cnVlLCJFbmFibGVkIjp0cnVlLCJTZWxlY3RhYmxlIjpmYWxzZSwiQm91bmRzIjoiMCwwLDMwMCwyMDAiLCJGaWxsIjpmYWxzZSwiTWFyZ2luIjp7IkxlZnQiOjMsIlRvcCI6MywiUmlnaHQiOjMsIkJvdHRvbSI6M319"));
            var c = JsonSerializer.Deserialize<GoWindow>(json, GoJsonConverter.Options);
            this.IconString = c.IconString;
            this.IconSize = c.IconSize;
            this.IconGap = c.IconGap;
            this.Text = c.Text;
            this.FontName = c.FontName;
            this.FontStyle = c.FontStyle;
            this.FontSize = c.FontSize;
            this.TextColor = c.TextColor;
            this.WindowColor = c.WindowColor;
            this.BorderColor = c.BorderColor;
            this.Round = c.Round;
            this.TitleHeight = c.TitleHeight;
            this.Name = c.Name;
            this.Visible = c.Visible;
            this.Enabled = c.Enabled;
            this.Bounds = c.Bounds;
            this.Fill = c.Fill;
            this.Margin = c.Margin;
            this.Childrens.AddRange(c.Childrens);

            var dic = Util.AllControls(this).ToDictionary(x => x.Id.ToString(), y => y);
            #endregion
        }
    }
}
