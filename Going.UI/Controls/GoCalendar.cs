using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoCalendar : GoControl
    {
        #region Properties
        [GoFontNameProperty(PCategory.Control, 0)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 1)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 2)] public float FontSize { get; set; } = 12;

        [GoProperty(PCategory.Control, 3)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 4)] public string BoxColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 5)] public string SelectColor { get; set; } = "Select";
        [GoProperty(PCategory.Control, 6)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Control, 7)] public bool BackgroundDraw { get; set; } = true;
        [GoProperty(PCategory.Control, 8)] public bool MultiSelect { get; set; } = false;
        [GoProperty(PCategory.Control, 9)] public bool NoneSelect { get; set; } = false;

        [JsonIgnore] public int CurrentYear { get; private set; } = DateTime.Now.Year;
        [JsonIgnore] public int CurrentMonth { get; private set; } = DateTime.Now.Month;
        [JsonIgnore] public List<DateTime> SelectedDays { get; } = new List<DateTime>();
        #endregion

        #region Member Variable
        bool bMonthPrev = false, bMonthNext = false;
        #endregion

        #region Event
        public event EventHandler? SelectedDaysChanged;
        #endregion

        #region Constructor
        public GoCalendar()
        {
            Selectable = true;
        }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var cText = thm.ToColor(TextColor);
            var cBox = thm.ToColor(BoxColor);
            var cSel = thm.ToColor(SelectColor);
            var cSun = SKColors.Red;
            var cSat = SKColors.DeepSkyBlue;
            var cP = bMonthPrev ? cText.BrightnessTransmit(thm.DownBrightness) : cText;
            var cN = bMonthNext ? cText.BrightnessTransmit(thm.DownBrightness) : cText;

            if (BackgroundDraw) Util.DrawBox(canvas, rtContent, cBox, Round, thm.Corner);

            #region Month
            Util.DrawIcon(canvas, "fa-chevron-left", FontSize, rts["Prev"], cP);
            Util.DrawIcon(canvas, "fa-chevron-right", FontSize, rts["Next"], cN);
            Util.DrawText(canvas, $"{CurrentYear}.{CurrentMonth}", FontName, FontStyle, FontSize, rts["Month"], cText);
            #endregion
            #region Week
            string[] dow = ["SUN", "MON", "TUE", "WED", "THR", "FRI", "SAT"];
            for (int ic = 0; ic < 7; ic++)
            {
                var c = ic == 0 ? SKColors.Red : (ic == 6 ? SKColors.DeepSkyBlue : cText);
                Util.DrawText(canvas, dow[ic], FontName, FontStyle, FontSize, rts[$"Week_{ic}"], c);
            }
            #endregion
            #region Days
            using (var path = PathTool.Box(rtContent, BackgroundDraw ? Round : GoRoundType.Rect, thm.Corner))
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipPath(path, SKClipOperation.Intersect, true);

                    daysLoop(rts, (tm, rt, monthIn) =>
                    {
                        if (SelectedDays.Contains(tm)) Util.DrawBox(canvas, rt, cSel, GoRoundType.Rect, thm.Corner);

                        var c = monthIn ? tm.DayOfWeek == DayOfWeek.Sunday ? cSun : (tm.DayOfWeek == DayOfWeek.Saturday ? cSat : cText) : Util.FromArgb(90, cText);
                        Util.DrawText(canvas, $"{tm.Day}", FontName, FontStyle, FontSize, rt, c);
                    });
                }
            }
            #endregion
             
            base.OnDraw(canvas, thm);
        }
        #endregion
        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            #region Prev / Next
            if (CollisionTool.Check(rts["Prev"], x, y)) bMonthPrev = true;
            if (CollisionTool.Check(rts["Next"], x, y)) bMonthNext = true;
            #endregion

            daysLoop(rts, (tm, rt, monthIn) =>
            {
                if (CollisionTool.Check(rt, x, y))
                {
                    if(monthIn)
                    {
                        if (MultiSelect)
                        {
                            if (!SelectedDays.Remove(tm)) SelectedDays.Add(tm);
                            SelectedDaysChanged?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            SelectedDays.Clear();
                            SelectedDays.Add(tm);
                            SelectedDaysChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        CurrentYear = tm.Year;
                        CurrentMonth = tm.Month;
                    }
                }
            });

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            #region Prev / Next
            if (bMonthPrev)
            {
                bMonthPrev = false;
                if (CollisionTool.Check(rts["Prev"], x, y))
                {
                    CurrentMonth--;
                    if (CurrentMonth < 1)
                    {
                        CurrentYear--;
                        CurrentMonth = 12;
                    }
                }
            }

            if (bMonthNext)
            {
                bMonthNext = false;
                if (CollisionTool.Check(rts["Next"], x, y))
                {
                    CurrentMonth++;
                    if (CurrentMonth > 12)
                    {
                        CurrentYear++;
                        CurrentMonth = 1;
                    }
                }
            }
            #endregion
            base.OnMouseUp(x, y, button);
        }
        #endregion
        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rtContent = dic["Content"];

            var rts = Util.Grid(rtContent, ["14.2857%", "14.2857%", "14.2857%", "14.2857%", "14.2857%", "14.2857%", "14.2857%"],
                                           ["12.5%", "12.5%", "12.5%", "12.5%", "12.5%", "12.5%", "12.5%", "12.5%"]);

            var rtMonth = Util.Merge(rts, 0, 0, 7, 1);
            var rtWeek = Util.Merge(rts, 0, 1, 7, 1); 
            dic["Month"] = rtMonth;
            dic["Week"] = rtWeek;
            dic["Prev"] = Util.FromRect(rtMonth.Left, rtMonth.Top, rtMonth.Height, rtMonth.Height);
            dic["Next"] = Util.FromRect(rtMonth.Right - rtMonth.Height, rtMonth.Top, rtMonth.Height, rtMonth.Height);

           for (int ic = 0; ic < 7; ic++)
                dic[$"Week_{ic}"] = rts[1, ic];

            for (int ir = 2; ir < 8; ir++)
                for (int ic = 0; ic < 7; ic++)
                    dic[$"Day_{ic}_{ir - 2}"] = rts[ir, ic];

            return dic;
        }
        #endregion
        #endregion

        #region Method
        void daysLoop(Dictionary<string, SKRect> rts, Action<DateTime, SKRect, bool> loop)
        {
            #region DayList
            int Days = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
            DateTime dt = new DateTime(CurrentYear, CurrentMonth, 1);
            int ndw = (int)dt.DayOfWeek;
            DateTime[] d = new DateTime[42];
            int startidx = ndw == 0 ? 7 : ndw;
            int endidx = startidx + Days;
            if (dt.Date.Year == 1 && dt.Date.Month == 1 && dt.Date.Day == 1) { }
            else dt -= new TimeSpan(startidx, 0, 0, 0);

            for (int i = 0; i < 42; i++)
            {
                d[i] = dt;
                dt += new TimeSpan(1, 0, 0, 0);
            }
            #endregion
            for (int ir = 0; ir < 6; ir++)
            {
                for (int ic = 0; ic < 7; ic++)
                {
                    var rt = rts[$"Day_{ic}_{ir}"];
                    var idx = ir * 7 + ic;
                    var tm = d[idx];
                    var monthIn = idx >= startidx && idx < endidx;

                    loop(tm, rt, monthIn);
                }
            }
        }
        #endregion
    }
}
