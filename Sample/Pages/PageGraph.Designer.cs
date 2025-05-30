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
    partial class PageGraph
    {
        #region declare
        #endregion

        public void InitializeComponent()
        {
            #region base
            var json = Encoding.UTF8.GetString(Convert.FromBase64String("eyJDaGlsZHJlbnMiOltdLCJCYWNrZ3JvdW5kSW1hZ2UiOm51bGwsIklkIjoiNTM5Yzg4MjEtYzZjMy00ZGNkLWE0MjgtNTcyNjMxNjI1ZmEyIiwiTmFtZSI6IlBhZ2VHcmFwaCIsIlZpc2libGUiOnRydWUsIkVuYWJsZWQiOnRydWUsIlNlbGVjdGFibGUiOmZhbHNlLCJCb3VuZHMiOiIwLDAsNzAsMzAiLCJGaWxsIjpmYWxzZSwiTWFyZ2luIjp7IkxlZnQiOjMsIlRvcCI6MywiUmlnaHQiOjMsIkJvdHRvbSI6M319"));
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
