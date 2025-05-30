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

namespace test.Pages
{
    partial class PageSetting
    {
        #region declare
        #endregion

        public void InitializeComponent()
        {
            #region base
            var json = Encoding.UTF8.GetString(Convert.FromBase64String("eyJDaGlsZHJlbnMiOltdLCJCYWNrZ3JvdW5kSW1hZ2UiOm51bGwsIklkIjoiZjVjOGM3YTAtMzA5Ny00YmFlLWIyYmUtZDAwNzllZGM3MTA5IiwiTmFtZSI6IlBhZ2VTZXR0aW5nIiwiVmlzaWJsZSI6dHJ1ZSwiRW5hYmxlZCI6dHJ1ZSwiU2VsZWN0YWJsZSI6ZmFsc2UsIkJvdW5kcyI6IjAsMCw3MCwzMCIsIkZpbGwiOmZhbHNlLCJNYXJnaW4iOnsiTGVmdCI6MywiVG9wIjozLCJSaWdodCI6MywiQm90dG9tIjozfX0="));
            var c = JsonSerializer.Deserialize<GoPage>(json, GoJsonConverter.Options);
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
