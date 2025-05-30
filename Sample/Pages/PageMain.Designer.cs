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
    partial class PageMain
    {
        #region declare
        GoButton btnTestWindow;
        #endregion

        public void InitializeComponent()
        {
            #region base
            var json = Encoding.UTF8.GetString(Convert.FromBase64String("eyJDaGlsZHJlbnMiOlt7IlR5cGUiOiJHb2luZy5VSS5Db250cm9scy5Hb1BpY3R1cmUsIEdvaW5nLlVJLCBWZXJzaW9uPTEuMC4xLjIsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbCIsIlZhbHVlIjp7IkltYWdlIjoibG9nb2kiLCJTY2FsZU1vZGUiOjMsIlJvdW5kIjowLCJJZCI6IjBkNjA0OThiLTRjZmMtNDVhYy05ZWZjLWM5ZTIzODUzNjBhNSIsIk5hbWUiOm51bGwsIlZpc2libGUiOnRydWUsIkVuYWJsZWQiOnRydWUsIlNlbGVjdGFibGUiOmZhbHNlLCJCb3VuZHMiOiIxMCwyMCwxNTYsMTE0IiwiRmlsbCI6ZmFsc2UsIk1hcmdpbiI6eyJMZWZ0IjozLCJUb3AiOjMsIlJpZ2h0IjozLCJCb3R0b20iOjN9fX0seyJUeXBlIjoiR29pbmcuVUkuQ29udHJvbHMuR29MYWJlbCwgR29pbmcuVUksIFZlcnNpb249MS4wLjEuMiwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsIiwiVmFsdWUiOnsiSWNvblN0cmluZyI6bnVsbCwiSWNvblNpemUiOjEyLCJJY29uRGlyZWN0aW9uIjowLCJJY29uR2FwIjo1LCJUZXh0IjoiR29pbmcgTGlicmFyeSBTYW1wbGUgUHJvZ3JhbSIsIkZvbnROYW1lIjoiXHVCMDk4XHVCMjE0XHVBQ0UwXHVCNTE1IiwiRm9udFN0eWxlIjoyLCJGb250U2l6ZSI6MTgsIlRleHRQYWRkaW5nIjp7IkxlZnQiOjAsIlRvcCI6MCwiUmlnaHQiOjAsIkJvdHRvbSI6MH0sIkNvbnRlbnRBbGlnbm1lbnQiOjMsIlRleHRDb2xvciI6IkZvcmUiLCJMYWJlbENvbG9yIjoiQmFzZTIiLCJSb3VuZCI6MSwiQmFja2dyb3VuZERyYXciOmZhbHNlLCJCb3JkZXJPbmx5IjpmYWxzZSwiSWQiOiI3MzY4OTRkMi03NDczLTQ1NWMtYjIxMC0zNWU4NTI5YmJlYzkiLCJOYW1lIjpudWxsLCJWaXNpYmxlIjp0cnVlLCJFbmFibGVkIjp0cnVlLCJTZWxlY3RhYmxlIjpmYWxzZSwiQm91bmRzIjoiMTY2LDIwLDQ1Niw2MCIsIkZpbGwiOmZhbHNlLCJNYXJnaW4iOnsiTGVmdCI6MywiVG9wIjozLCJSaWdodCI6MywiQm90dG9tIjozfX19LHsiVHlwZSI6IkdvaW5nLlVJLkNvbnRyb2xzLkdvTGFiZWwsIEdvaW5nLlVJLCBWZXJzaW9uPTEuMC4xLjIsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbCIsIlZhbHVlIjp7Ikljb25TdHJpbmciOm51bGwsIkljb25TaXplIjoxMiwiSWNvbkRpcmVjdGlvbiI6MCwiSWNvbkdhcCI6NSwiVGV4dCI6IkNvbnRyb2wgQ291bnQgOiAyMFxyXG5Db250YWluZXIgQ291bnQgOiAxMFxyXG5cclxuSW1hZ2VDYW52YXMgU3VwcG9ydGVkIiwiRm9udE5hbWUiOiJcdUIwOThcdUIyMTRcdUFDRTBcdUI1MTUiLCJGb250U3R5bGUiOjAsIkZvbnRTaXplIjoxMiwiVGV4dFBhZGRpbmciOnsiTGVmdCI6MCwiVG9wIjowLCJSaWdodCI6MCwiQm90dG9tIjowfSwiQ29udGVudEFsaWdubWVudCI6MCwiVGV4dENvbG9yIjoiQmFzZTUiLCJMYWJlbENvbG9yIjoiQmFzZTIiLCJSb3VuZCI6MSwiQmFja2dyb3VuZERyYXciOmZhbHNlLCJCb3JkZXJPbmx5IjpmYWxzZSwiSWQiOiI0NGYxYzYwOS1iMjkxLTQ3OGEtYTIyYS1lNzRjMTRhZDRmNDgiLCJOYW1lIjpudWxsLCJWaXNpYmxlIjp0cnVlLCJFbmFibGVkIjp0cnVlLCJTZWxlY3RhYmxlIjpmYWxzZSwiQm91bmRzIjoiMTY2LDYwLDQzNywxMTQiLCJGaWxsIjpmYWxzZSwiTWFyZ2luIjp7IkxlZnQiOjMsIlRvcCI6MywiUmlnaHQiOjMsIkJvdHRvbSI6M319fSx7IlR5cGUiOiJHb2luZy5VSS5Db250cm9scy5Hb0J1dHRvbiwgR29pbmcuVUksIFZlcnNpb249MS4wLjEuMiwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsIiwiVmFsdWUiOnsiSWNvblN0cmluZyI6bnVsbCwiSWNvblNpemUiOjEyLCJJY29uRGlyZWN0aW9uIjowLCJJY29uR2FwIjo1LCJUZXh0IjoiVGVzdCBXaW5kb3ciLCJGb250TmFtZSI6Ilx1QjA5OFx1QjIxNFx1QUNFMFx1QjUxNSIsIkZvbnRTdHlsZSI6MCwiRm9udFNpemUiOjEyLCJUZXh0Q29sb3IiOiJGb3JlIiwiQnV0dG9uQ29sb3IiOiJCYXNlMyIsIlJvdW5kIjoxLCJCYWNrZ3JvdW5kRHJhdyI6dHJ1ZSwiQm9yZGVyT25seSI6ZmFsc2UsIklkIjoiNTRlOGM2NGUtOGNmZS00YmE0LTg3YzUtNGViNGY5Y2U5Yjc2IiwiTmFtZSI6ImJ0blRlc3RXaW5kb3ciLCJWaXNpYmxlIjp0cnVlLCJFbmFibGVkIjp0cnVlLCJTZWxlY3RhYmxlIjp0cnVlLCJCb3VuZHMiOiI1NzEsMjkxLDY4NiwzMzAiLCJGaWxsIjpmYWxzZSwiTWFyZ2luIjp7IkxlZnQiOjMsIlRvcCI6MywiUmlnaHQiOjMsIkJvdHRvbSI6M319fSx7IlR5cGUiOiJHb2luZy5VSS5Db250YWluZXJzLkdvQm94UGFuZWwsIEdvaW5nLlVJLCBWZXJzaW9uPTEuMC4xLjIsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbCIsIlZhbHVlIjp7IkNoaWxkcmVucyI6W10sIkJvcmRlckNvbG9yIjoiQmFzZTMiLCJCb3hDb2xvciI6IkJhc2UyIiwiUm91bmQiOjEsIkJhY2tncm91bmREcmF3Ijp0cnVlLCJJZCI6IjI3YTgxNDc4LWRkMmItNDgxNy05MDMwLTEyNjhlNjIxNGY4ZSIsIk5hbWUiOm51bGwsIlZpc2libGUiOnRydWUsIkVuYWJsZWQiOnRydWUsIlNlbGVjdGFibGUiOmZhbHNlLCJCb3VuZHMiOiIxMCwxMjQsNDkwLDIwMiIsIkZpbGwiOmZhbHNlLCJNYXJnaW4iOnsiTGVmdCI6MywiVG9wIjozLCJSaWdodCI6MywiQm90dG9tIjozfX19XSwiQmFja2dyb3VuZEltYWdlIjpudWxsLCJJZCI6IjhmN2NhODVkLWYyZGMtNDQ5NC04Nzk2LTJiMWMyNjMwYmQxNyIsIk5hbWUiOiJQYWdlTWFpbiIsIlZpc2libGUiOnRydWUsIkVuYWJsZWQiOnRydWUsIlNlbGVjdGFibGUiOmZhbHNlLCJCb3VuZHMiOiIxNTAsNTAsNjUwLDQ0MCIsIkZpbGwiOmZhbHNlLCJNYXJnaW4iOnsiTGVmdCI6MywiVG9wIjozLCJSaWdodCI6MywiQm90dG9tIjozfX0="));
            var c = JsonSerializer.Deserialize<GoPage>(json, GoJsonConverter.Options);
            this.Name = c.Name;
            this.Visible = c.Visible;
            this.Enabled = c.Enabled;
            this.Bounds = c.Bounds;
            this.Fill = c.Fill;
            this.Margin = c.Margin;
            this.Childrens.AddRange(c.Childrens);

            var dic = Util.AllControls(this).ToDictionary(x => x.Id.ToString(), y => y);
            btnTestWindow = dic["54e8c64e-8cfe-4ba4-87c5-4eb4f9ce9b76"] as GoButton;
            #endregion
        }
    }
}
