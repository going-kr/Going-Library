using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Utils
{
    /// <summary>
    /// 터치 및 마우스 기반 스크롤 동작을 처리하는 클래스입니다. 관성 스크롤과 스크롤바 드래그를 지원합니다.
    /// </summary>
    public class Scroll
    {
        #region Const
        const float GapSize = 10;
        const float GapTime = 1;
        const double DecelerationRate = 0.996;
        const int TaskInterval = 10;
        const bool Cut = true;
        internal const int GestureTime = 100;
        #endregion

        #region Properties
        /// <summary>
        /// 스크롤바의 너비 또는 높이를 가져오거나 설정합니다.
        /// </summary>
        public static int SC_WH { get; set; } = 12;

        /// <summary>
        /// 스크롤바 드래그 중인지 여부를 가져옵니다.
        /// </summary>
        public bool IsScrolling => scDown != null;
        /// <summary>
        /// 터치 스크롤 중인지 여부를 가져옵니다.
        /// </summary>
        public bool IsTouchScrolling => tcDown != null;
        /// <summary>
        /// 터치 관성 스크롤이 진행 중인지 여부를 가져옵니다.
        /// </summary>
        public bool IsTouchMoving => IsTouchStart;
        /// <summary>
        /// 터치 모드 활성화 여부를 가져오거나 설정합니다.
        /// </summary>
        public static bool TouchMode { get; set; } = true;

        /// <summary>
        /// 스크롤 방향을 가져오거나 설정합니다.
        /// </summary>
        public ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;

        /// <summary>
        /// 전체 스크롤 가능 크기를 반환하는 함수를 가져오거나 설정합니다.
        /// </summary>
        public Func<double>? GetScrollTotal { get; set; }
        /// <summary>
        /// 보이는 영역의 크기를 반환하는 함수를 가져오거나 설정합니다.
        /// </summary>
        public Func<double>? GetScrollView { get; set; }
        /// <summary>
        /// 마우스 휠 한 틱의 스크롤 양을 반환하는 함수를 가져오거나 설정합니다.
        /// </summary>
        public Func<double>? GetScrollTick { get; set; }
        /// <summary>
        /// 스크롤 스케일 팩터를 반환하는 함수를 가져오거나 설정합니다.
        /// </summary>
        public Func<double>? GetScrollScaleFactor { get; set; }
        /// <summary>
        /// 범위 제한 무시 여부를 반환하는 함수를 가져오거나 설정합니다.
        /// </summary>
        public Func<bool>? GetConstrainIgnore { get; set; }

        /// <summary>범위 제한 무시 여부를 가져옵니다.</summary>
        public bool ConstrainIgnore => GetConstrainIgnore != null ? GetConstrainIgnore() : false;
        /// <summary>전체 스크롤 크기를 가져옵니다.</summary>
        public double ScrollTotal { get { return GetScrollTotal != null ? GetScrollTotal() : 0; } }
        /// <summary>보이는 영역의 크기를 가져옵니다.</summary>
        public double ScrollView { get { return GetScrollView != null ? GetScrollView() : 0; } }
        /// <summary>마우스 휠 한 틱의 스크롤 양을 가져옵니다.</summary>
        public double ScrollTick { get { return GetScrollTick != null ? GetScrollTick() : 0; } }
        /// <summary>스크롤 스케일 팩터를 가져옵니다.</summary>
        public double ScrollScaleFactor { get { return GetScrollScaleFactor != null ? GetScrollScaleFactor() : 1; } }
        private double _ScrollPosition = 0;
        /// <summary>현재 스크롤 위치를 가져오거나 설정합니다.</summary>
        public double ScrollPosition
        {
            get
            {
                if (ScrollView < ScrollTotal)
                {
                    if (!ConstrainIgnore) _ScrollPosition = (MathTool.Constrain(_ScrollPosition, 0, ScrollTotal - ScrollView));
                }
                else _ScrollPosition = 0;
                return _ScrollPosition;
            }
            set
            {
                if (ScrollView < ScrollTotal) _ScrollPosition = (MathTool.Constrain(value, 0, ScrollTotal - ScrollView));
                else _ScrollPosition = 0;
            }
        }

        /// <summary>터치 드래그에 의한 스크롤 오프셋을 가져옵니다.</summary>
        public double TouchOffset
        {
            get
            {
                if (TouchMode)
                {
                    double ret;
                    if (scDown == null)
                    {
                        if (Direction == ScrollDirection.Vertical)
                            ret = tcDown != null && ScrollView < ScrollTotal ? (Convert.ToDouble(tcDown.MovePoint.Y - tcDown.DownPoint.Y) * ScrollScaleFactor) : 0;
                        else
                            ret = tcDown != null && ScrollView < ScrollTotal ? (Convert.ToDouble(tcDown.MovePoint.X - tcDown.DownPoint.X) * ScrollScaleFactor) : 0;
                    }
                    else ret = 0;
                    return ret;
                }
                else return 0;
            }
        }

        /// <summary>터치 오프셋과 애니메이션을 고려한 스크롤 위치를 가져옵니다.</summary>
        public double ScrollPositionWithOffset
        {
            get
            {
                var ao = 0.0;
                if (ani.IsPlaying)
                {
                    var sp = (ani.Variable ?? "").Split(':');
                    var avar = Convert.ToDouble(sp[1]);
                    if (sp[0] == "S") ao = ani.Value(AnimationAccel.DCL, -avar, 0F);
                    else if (sp[0] == "E") ao = ani.Value(AnimationAccel.DCL, (-avar - (ScrollTotal - ScrollView)), 0);
                }

                var ret = !Cut ?
                   -ScrollPosition + TouchOffset - ao :
                   (ScrollTotal > ScrollView ? (MathTool.Constrain((-ScrollPosition + TouchOffset), -(ScrollTotal - ScrollView), 0)) : 0);
                return ret;
            }
        }

        /// <summary>반전 방향을 위한 터치 오프셋과 애니메이션을 고려한 스크롤 위치를 가져옵니다.</summary>
        public double ScrollPositionWithOffsetR
        {
            get
            {
                var ao = 0.0;
                if (ani.IsPlaying)
                {
                    var sp = (ani.Variable ?? "").Split(':');
                    var avar = Convert.ToDouble(sp[1]);
                    if (sp[0] == "S") ao = ani.Value(AnimationAccel.DCL, -avar, 0F);
                    else if (sp[0] == "E") ao = ani.Value(AnimationAccel.DCL, (-avar - (ScrollTotal - ScrollView)), 0);
                }

                var ret = !Cut ?
                    -(-ScrollPosition - TouchOffset) - ao :
                    (ScrollTotal > ScrollView ? (MathTool.Constrain(-(-ScrollPosition - TouchOffset), 0, (ScrollTotal - ScrollView))) : 0);
                return ret;
            }
        }


        /// <summary>
        /// 스크롤이 필요한지 여부를 가져옵니다. 전체 크기가 뷰 크기보다 클 때 true입니다.
        /// </summary>
        public bool ScrollVisible => ScrollTotal > ScrollView;

        /// <summary>
        /// 화면 갱신 콜백 액션입니다.
        /// </summary>
        public Action? Refresh;
        #endregion

        #region Member Variable
        SCDI? scDown = null;
        TCDI? tcDown = null;

        double initPos;
        double initVel;
        double destPos;
        double destTime;
        double dCoeff = 1000 * Math.Log(DecelerationRate);
        double threshold = 0.1;

        bool IsTouchStart;
        Animation ani = new();

        bool barDown = false;
        #endregion

        #region Event
        /// <summary>
        /// 스크롤 위치가 변경되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action? ScrollChanged;
        /// <summary>
        /// 스크롤이 끝났을 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action? ScrollEnded;
        #endregion

        #region Constructor
        /// <summary><see cref="Scroll"/> 클래스의 새 인스턴스를 초기화합니다.</summary>
        public Scroll()
        {

        }
        #endregion

        #region Method
        #region GetScrollCursorRect(rtScroll)
        /// <summary>스크롤바 커서 영역을 계산합니다.</summary>
        /// <param name="rtScroll">스크롤바 전체 영역.</param>
        /// <returns>스크롤 커서 사각형. 스크롤이 필요 없을 경우 null을 반환합니다.</returns>
        public SKRect? GetScrollCursorRect(SKRect rtScroll)
        {
            var nsc = (SC_WH / 3F);
            rtScroll.Inflate(-nsc, -nsc);
            if (ScrollView < ScrollTotal)
            {
                var ao = 0.0;
                if (ani.IsPlaying)
                {
                    var sp = (ani.Variable ?? "").Split(':');
                    var avar = Convert.ToDouble(sp[1]);
                    if (sp[0] == "S") ao = ani.Value(AnimationAccel.DCL, -avar, 0F);
                    else if (sp[0] == "E") ao = ani.Value(AnimationAccel.DCL, (-avar - (ScrollTotal - ScrollView)), 0);
                }

                if (Direction == ScrollDirection.Vertical)
                {
                    var h = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Height));
                    h = Math.Max(h, 30);

                    var y = 0D;
                    y = !Cut ? (MathTool.Map(ScrollPosition - TouchOffset + ao, 0, ScrollTotal - ScrollView, rtScroll.Top, rtScroll.Bottom - h))
                             : (MathTool.Map(MathTool.Constrain(ScrollPosition - TouchOffset, 0, ScrollTotal - ScrollView), 0, ScrollTotal - ScrollView, rtScroll.Top, rtScroll.Bottom - h));

                    return Util.FromRect(rtScroll.Left, Convert.ToSingle(y), rtScroll.Width, Convert.ToSingle(h));
                }
                else
                {
                    var w = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Width));
                    w = Math.Max(w, 30);

                    var x = 0D;
                    x = !Cut ? (MathTool.Map(ScrollPosition - TouchOffset + ao, 0, ScrollTotal - ScrollView, rtScroll.Left, rtScroll.Right - w))
                             : (MathTool.Map(MathTool.Constrain(ScrollPosition - TouchOffset, 0, ScrollTotal - ScrollView), 0, ScrollTotal - ScrollView, rtScroll.Left, rtScroll.Right - w));

                    return Util.FromRect(Convert.ToSingle(x), rtScroll.Top, Convert.ToSingle(w), rtScroll.Height);
                }
            }

            else return null;
        }
        #endregion

        #region MouseWheel
        /// <summary>마우스 휠 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="delta">휠 델타 값.</param>
        public void MouseWheel(float x, float y, float delta)
        {
            ScrollPosition += (-delta * ScrollTick);
        }
        #endregion
        #region MouseDown
        /// <summary>마우스 다운 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        /// <param name="barClickMode">스크롤바 영역 클릭으로 위치를 이동시킬지 여부.</param>
        public void MouseDown(float x, float y, SKRect rtScroll, bool barClickMode = false)
        {
            var rtcur = GetScrollCursorRect(rtScroll);
            if (rtcur.HasValue)
            {
                var rtcur2 = rtcur.Value;
                rtcur2.Inflate(3, 3);

                if (!IsTouchStart && CollisionTool.Check(rtcur2, x, y)) scDown = new SCDI() { DownPoint = new SKPoint(x, y), CursorBounds = rtcur.Value };

                if (barClickMode && !CollisionTool.Check(rtcur2, new SKPoint(x, y)))
                {
                    barDown = true;

                    var nsc = (SC_WH / 3F);

                    if (Direction == ScrollDirection.Vertical && x >= rtScroll.Left && x <= rtScroll.Right)
                    {
                        var h = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Height));
                        h = Math.Max(h, 30);
                        var v = y - (h / 2);
                        ScrollPosition = (MathTool.Map(MathTool.Constrain(v, rtScroll.Top, rtScroll.Bottom - h), rtScroll.Top + nsc, rtScroll.Bottom - h - nsc, 0, ScrollTotal - ScrollView));
                    }
                    else if (Direction == ScrollDirection.Horizon && y >= rtScroll.Top && x <= rtScroll.Bottom)
                    {
                        var w = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Width));
                        w = Math.Max(w, 30);
                        var v = x;
                        ScrollPosition = (MathTool.Map(MathTool.Constrain(v, rtScroll.Left, rtScroll.Right - w), rtScroll.Left + nsc, rtScroll.Right - w - nsc, 0, ScrollTotal - ScrollView));
                    }
                }
            }

            if (!ScrollVisible) _ScrollPosition = 0;
        }
        #endregion
        #region MouseUp
        /// <summary>마우스 업 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="barClickMode">스크롤바 영역 클릭 모드 여부.</param>
        public void MouseUp(float x, float y, bool barClickMode = false)
        {
            if (barClickMode)
            {
                barDown = false;
            }

            if (scDown != null) scDown = null;
        }
        #endregion
        #region MouseMove
        /// <summary>마우스 이동 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        public void MouseMove(float x, float y, SKRect rtScroll)
        {
            if (scDown != null)
            {
                if (Direction == ScrollDirection.Vertical)
                {
                    var h = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Height));
                    h = Math.Max(h, 30);
                    var v = (double)y - ((double)scDown.DownPoint.Y - (double)scDown.CursorBounds.Top);
                    ScrollPosition = (MathTool.Map(MathTool.Constrain(v, rtScroll.Top, rtScroll.Bottom - h), rtScroll.Top, rtScroll.Bottom - h, 0, ScrollTotal - ScrollView));
                }
                else if (Direction == ScrollDirection.Horizon)
                {
                    var w = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Width));
                    w = Math.Max(w, 30);
                    var v = (double)x - ((double)scDown.DownPoint.X - (double)scDown.CursorBounds.Left);
                    ScrollPosition = (MathTool.Map(MathTool.Constrain(v, rtScroll.Left, rtScroll.Right - w), rtScroll.Left, rtScroll.Right - w, 0, ScrollTotal - ScrollView));
                }
            }
        }
        #endregion

        #region TouchDown
        /// <summary>터치 다운 입력을 처리합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchDown(float x, float y)
        {
            if (TouchMode && scDown == null)
            {
                tcDown = new TCDI() { DownPoint = new SKPoint(x, y), MovePoint = new SKPoint(x, y), DownTime = DateTime.Now };
                tcDown.List.Add(new TCMI() { Time = DateTime.Now, Point = new SKPoint(x, y) });
                IsTouchStart = false;
                Thread.Sleep(15);
            }
        }
        #endregion
        #region TouchMove
        /// <summary>터치 이동 입력을 처리합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchMove(float x, float y)
        {
            if (TouchMode)
            {
                if (tcDown != null && MathTool.GetDistance(tcDown.DownPoint, new SKPoint(x, y)) >= GapSize)
                {
                    tcDown.MovePoint = new SKPoint(x, y);
                    tcDown.List.Add(new TCMI() { Time = DateTime.Now, Point = new SKPoint(x, y) });
                }
            }
        }
        #endregion
        #region TouchUp
        /// <summary>터치 업 입력을 처리하고 관성 스크롤을 시작합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchUp(float x, float y)
        {
            if (TouchMode && tcDown != null)
            {
                if (ScrollView < ScrollTotal && MathTool.GetDistance(tcDown.DownPoint, new SKPoint(x, y)) >= GapSize)
                {
                    var sp = tcDown.List.Where(x => (DateTime.Now - x.Time).TotalMilliseconds < GestureTime).FirstOrDefault();
                    if (sp == null) sp = tcDown.List.Count > 0 ? tcDown.List.Last() : new TCMI() { Time = tcDown.DownTime, Point = tcDown.DownPoint };

                    if (Direction == ScrollDirection.Vertical)
                    {
                        ScrollPosition = (MathTool.Constrain(ScrollPosition - TouchOffset + ((sp.Point.Y - y) * ScrollScaleFactor), 0, (ScrollTotal - ScrollView)));
                        initPos = ScrollPosition;
                        initVel = ((sp.Point.Y - y) * ScrollScaleFactor) / ((double)(DateTime.Now - sp.Time).TotalMilliseconds / 1000.0);
                        destPos = MathTool.Constrain(initPos - initVel / dCoeff, 0, (ScrollTotal - ScrollView));
                        destTime = Math.Log(-dCoeff * threshold / Math.Abs(initVel)) / dCoeff;
                    }
                    else
                    {
                        ScrollPosition = (MathTool.Constrain(ScrollPosition - TouchOffset + ((sp.Point.X - x) * ScrollScaleFactor), 0, (ScrollTotal - ScrollView)));
                        initPos = ScrollPosition;
                        initVel = ((sp.Point.X - x) * ScrollScaleFactor) / ((double)(DateTime.Now - sp.Time).TotalMilliseconds / 1000.0);
                        destPos = MathTool.Constrain(initPos - initVel / dCoeff, 0, (ScrollTotal - ScrollView));
                        destTime = Math.Log(-dCoeff * threshold / Math.Abs(initVel)) / dCoeff;
                    }

                    if ((DateTime.Now - sp.Time).TotalSeconds <= 0.25 && (Math.Abs(initPos - destPos) > GapSize && destTime > GapTime))
                    {
                        Task.Run(async () =>
                        {
                            IsTouchStart = true;

                            try
                            {
                                var stime = DateTime.Now;
                                var time = 0.0;
                                var tot = (ScrollTotal - ScrollView);
                                while (IsTouchStart && time < destTime * 1000 && Convert.ToInt32(ScrollPosition / ScrollScaleFactor) != Convert.ToInt32(destPos / ScrollScaleFactor))
                                {
                                    time = (DateTime.Now - stime).TotalMilliseconds;
                                    var oldV = ScrollPosition;
                                    var newV = (MathTool.Constrain((initPos + (Math.Pow(DecelerationRate, time) - 1) / dCoeff * initVel), 0, tot));
                                    if (oldV != newV) { ScrollPosition = (newV); try { ScrollChanged?.Invoke(); } catch { } }

                                    await Task.Delay(TaskInterval);

                                    Refresh?.Invoke();
                                }
                            }
                            catch (OperationCanceledException ex) { }
                            finally
                            {
                                IsTouchStart = false;
                                try
                                {
                                    ScrollChanged?.Invoke();
                                    ScrollEnded?.Invoke();
                                }
                                catch { }
                            }
                        });
                    }
                    else
                    {
                        if (ScrollPosition - TouchOffset < 0)
                            ani.Start(100, $"S:{ScrollPositionWithOffset}");
                        else if (ScrollPosition - TouchOffset > ScrollTotal - ScrollView)
                            ani.Start(100, $"E:{ScrollPositionWithOffset}");
                    }

                }
                tcDown = null;
            }
        }
        #endregion
        #region TouchStop
        /// <summary>진행 중인 터치 관성 스크롤을 중지합니다.</summary>
        public void TouchStop()
        {
            IsTouchStart = false;
        }
        #endregion

        #region GetScrollCursorRectR(rtScroll)
        /// <summary>반전 방향 스크롤바 커서 영역을 계산합니다.</summary>
        /// <param name="rtScroll">스크롤바 전체 영역.</param>
        /// <returns>스크롤 커서 사각형. 스크롤이 필요 없을 경우 null을 반환합니다.</returns>
        public SKRect? GetScrollCursorRectR(SKRect rtScroll)
        {
            var nsc = (SC_WH / 3F);
            rtScroll.Inflate(-nsc, -nsc);
            if (ScrollView < ScrollTotal)
            {
                var ao = 0.0;
                if (ani.IsPlaying)
                {
                    var sp = (ani.Variable ?? "").Split(':');
                    var avar = Convert.ToDouble(sp[1]);
                    if (sp[0] == "S") ao = ani.Value(AnimationAccel.DCL, -avar, 0F);
                    else if (sp[0] == "E") ao = ani.Value(AnimationAccel.DCL, (-avar - (ScrollTotal - ScrollView)), 0);
                }

                if (Direction == ScrollDirection.Vertical)
                {
                    var h = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Height));
                    h = Math.Max(h, 30);

                    var y = 0D;
                    y = !Cut ? (MathTool.Map(ScrollPosition + TouchOffset - ao, 0, ScrollTotal - ScrollView, rtScroll.Bottom - h, rtScroll.Top))
                             : (MathTool.Map(MathTool.Constrain(ScrollPosition + TouchOffset, 0, ScrollTotal - ScrollView), 0, ScrollTotal - ScrollView, rtScroll.Bottom - h, rtScroll.Top));
                    return Util.FromRect(rtScroll.Left, Convert.ToSingle(y), rtScroll.Width, Convert.ToSingle(h));
                }
                else
                {
                    var w = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Width));
                    w = Math.Max(w, 30);

                    var x = 0D;
                    x = !Cut ? (MathTool.Map(ScrollPosition + TouchOffset - ao, 0, ScrollTotal - ScrollView, rtScroll.Right - w, rtScroll.Left))
                             : (MathTool.Map(MathTool.Constrain(ScrollPosition + TouchOffset, 0, ScrollTotal - ScrollView), 0, ScrollTotal - ScrollView, rtScroll.Right - w, rtScroll.Left));
                    return Util.FromRect(Convert.ToSingle(x), rtScroll.Top, Convert.ToSingle(w), rtScroll.Height);
                }
            }

            else return null;
        }
        #endregion

        #region MouseDownR
        /// <summary>반전 방향 스크롤에 대한 마우스 다운 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        public void MouseDownR(float x, float y, SKRect rtScroll)
        {
            var rtcur = GetScrollCursorRectR(rtScroll);
            if (!IsTouchStart && rtcur.HasValue && CollisionTool.Check(rtcur.Value, x, y)) scDown = new SCDI() { DownPoint = new SKPoint(x, y), CursorBounds = rtcur.Value };
        }
        #endregion
        #region MouseUpR
        /// <summary>반전 방향 스크롤에 대한 마우스 업 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        public void MouseUpR(float x, float y)
        {
            if (scDown != null) scDown = null;
        }
        #endregion
        #region MouseMoveR
        /// <summary>반전 방향 스크롤에 대한 마우스 이동 입력을 처리합니다.</summary>
        /// <param name="x">마우스 X 좌표.</param>
        /// <param name="y">마우스 Y 좌표.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        public void MouseMoveR(float x, float y, SKRect rtScroll)
        {
            if (scDown != null)
            {
                if (Direction == ScrollDirection.Vertical)
                {
                    var h = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Height));
                    h = Math.Max(h, 30);
                    var v = (double)y - ((double)scDown.DownPoint.Y - (double)scDown.CursorBounds.Top);
                    ScrollPosition = MathTool.Map(MathTool.Constrain(v, rtScroll.Top, rtScroll.Bottom - h), rtScroll.Bottom - h, rtScroll.Top, 0, ScrollTotal - ScrollView);
                }
                else if (Direction == ScrollDirection.Horizon)
                {
                    var w = (MathTool.Map(ScrollView, 0, ScrollTotal, 0, rtScroll.Width));
                    w = Math.Max(w, 30);
                    var v = (double)x - ((double)scDown.DownPoint.X - (double)scDown.CursorBounds.Left);
                    ScrollPosition = MathTool.Map(MathTool.Constrain(v, rtScroll.Left, rtScroll.Right - w), rtScroll.Right - w, rtScroll.Left, 0, ScrollTotal - ScrollView);
                }
            }
        }
        #endregion

        #region TouchDownR
        /// <summary>반전 방향 스크롤에 대한 터치 다운 입력을 처리합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchDownR(float x, float y)
        {
            if (TouchMode)
            {
                tcDown = new TCDI() { DownPoint = new SKPoint(x, y), MovePoint = new SKPoint(x, y), DownTime = DateTime.Now };
                tcDown.List.Add(new TCMI() { Time = DateTime.Now, Point = new SKPoint(x, y) });
                IsTouchStart = false;
                Thread.Sleep(15);
            }
        }
        #endregion
        #region TouchMoveR
        /// <summary>반전 방향 스크롤에 대한 터치 이동 입력을 처리합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchMoveR(float x, float y)
        {
            if (TouchMode)
            {
                if (tcDown != null && MathTool.GetDistance(tcDown.DownPoint, new SKPoint(x, y)) >= GapSize)
                {
                    tcDown.MovePoint = new SKPoint(x, y);
                    tcDown.List.Add(new TCMI() { Time = DateTime.Now, Point = new SKPoint(x, y) });
                }
            }
        }
        #endregion
        #region TouchUpR
        /// <summary>반전 방향 스크롤에 대한 터치 업 입력을 처리하고 관성 스크롤을 시작합니다.</summary>
        /// <param name="x">터치 X 좌표.</param>
        /// <param name="y">터치 Y 좌표.</param>
        public void TouchUpR(float x, float y)
        {
            if (TouchMode && tcDown != null)
            {
                if (ScrollView < ScrollTotal && MathTool.GetDistance(tcDown.DownPoint, new SKPoint(x, y)) >= GapSize)
                {
                    var sp = tcDown.List.Where(x => (DateTime.Now - x.Time).TotalMilliseconds < GestureTime).FirstOrDefault();
                    if (sp == null) sp = tcDown.List.Count > 0 ? tcDown.List.Last() : new TCMI() { Time = tcDown.DownTime, Point = tcDown.DownPoint };

                    if (Direction == ScrollDirection.Vertical)
                    {
                        ScrollPosition = (MathTool.Constrain(ScrollPosition + TouchOffset + ((y - sp.Point.Y) * ScrollScaleFactor), 0, (ScrollTotal - ScrollView)));
                        initPos = ScrollPosition;
                        initVel = ((y - sp.Point.Y) * ScrollScaleFactor) / ((double)(DateTime.Now - sp.Time).TotalMilliseconds / 1000.0);
                        destPos = MathTool.Constrain(initPos - initVel / dCoeff, 0, (ScrollTotal - ScrollView));
                        destTime = Math.Log(-dCoeff * threshold / Math.Abs(initVel)) / dCoeff;
                    }
                    else
                    {
                        ScrollPosition = (MathTool.Constrain(ScrollPosition + TouchOffset + ((x - sp.Point.X) * ScrollScaleFactor), 0, (ScrollTotal - ScrollView)));
                        initPos = ScrollPosition;
                        initVel = ((x - sp.Point.X) * ScrollScaleFactor) / ((double)(DateTime.Now - sp.Time).TotalMilliseconds / 1000.0);
                        destPos = MathTool.Constrain(initPos - initVel / dCoeff, 0, (ScrollTotal - ScrollView));
                        destTime = Math.Log(-dCoeff * threshold / Math.Abs(initVel)) / dCoeff;
                    }

                    if ((DateTime.Now - sp.Time).TotalSeconds <= 0.25 && (Math.Abs(initPos - destPos) > GapSize && destTime > GapTime))
                    {
                        Task.Run(async () =>
                        {
                            IsTouchStart = true;

                            try
                            {
                                var stime = DateTime.Now;
                                var time = 0.0;
                                var tot = (ScrollTotal - ScrollView);
                                while (IsTouchStart && time < destTime * 1000 && Convert.ToInt32(ScrollPosition / ScrollScaleFactor) != Convert.ToInt32(destPos / ScrollScaleFactor))
                                {
                                    time = (DateTime.Now - stime).TotalMilliseconds;
                                    var oldV = ScrollPosition;
                                    var newV = (MathTool.Constrain((initPos + (Math.Pow(DecelerationRate, time) - 1) / dCoeff * initVel), 0, tot));
                                    if (oldV != newV) { ScrollPosition = (newV); try { ScrollChanged?.Invoke(); } catch { } }

                                    await Task.Delay(TaskInterval);

                                    Refresh?.Invoke();
                                }
                            }
                            catch (OperationCanceledException ex) { }
                            finally
                            {
                                IsTouchStart = false;
                                try
                                {
                                    ScrollChanged?.Invoke();
                                    ScrollEnded?.Invoke();
                                }
                                catch { }
                            }
                        });
                    }
                    else
                    {
                        if (ScrollPosition + TouchOffset < 0)
                            ani.Start(100, $"S:{ScrollPositionWithOffset}");
                        else if (ScrollPosition + TouchOffset > ScrollTotal - ScrollView)
                            ani.Start(100, $"E:{ScrollPositionWithOffset}");
                    }
                }
                tcDown = null;
            }
        }
        #endregion

        #region Clear
        /// <summary>스크롤 상태를 초기화합니다.</summary>
        public void Clear()
        {
            scDown = null;
            tcDown = null;
            IsTouchStart = false;
        }
        #endregion

        #region Draw
        /// <summary>스크롤바를 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="thm">테마.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        /// <param name="round">모서리 둥근 처리 방식.</param>
        public void Draw(SKCanvas canvas, GoTheme thm, SKRect rtScroll, GoRoundType round = GoRoundType.All)
        {
            if (ScrollVisible)
            {
                var sp = canvas.Save();
                canvas.ClipRect(rtScroll);
                var cCur = (IsScrolling || IsTouchMoving) ? thm.ScrollCursor : Util.FromArgb(120, thm.ScrollCursor);
                var rtcur = GetScrollCursorRect(rtScroll);
                if (rtcur.HasValue) Util.DrawBox(canvas, rtcur.Value, cCur, round, thm.Corner);
                canvas.RestoreToCount(sp);
            }

            if (!ScrollVisible) _ScrollPosition = 0;
        }

        /// <summary>반전 방향 스크롤바를 그립니다.</summary>
        /// <param name="canvas">대상 캔버스.</param>
        /// <param name="thm">테마.</param>
        /// <param name="rtScroll">스크롤바 영역.</param>
        /// <param name="round">모서리 둥근 처리 방식.</param>
        public void DrawR(SKCanvas canvas, GoTheme thm, SKRect rtScroll, GoRoundType round = GoRoundType.All)
        {
            if (ScrollVisible)
            {
                var sp = canvas.Save();
                canvas.ClipRect(rtScroll);
                var cCur = (IsScrolling || IsTouchMoving) ? thm.ScrollCursor : Util.FromArgb(120, thm.ScrollCursor);
                var rtcur = GetScrollCursorRectR(rtScroll);
                if (rtcur.HasValue) Util.DrawBox(canvas, rtcur.Value, cCur, round, thm.Corner);
                canvas.RestoreToCount(sp);
            }

            if (!ScrollVisible) _ScrollPosition = 0;
        }
        #endregion
        #endregion
    }

    #region enum : ScrollDirection
    /// <summary>
    /// 스크롤 방향을 정의합니다.
    /// </summary>
    public enum ScrollDirection
    {
        /// <summary>수평 스크롤</summary>
        Horizon,
        /// <summary>수직 스크롤</summary>
        Vertical
    }
    #endregion
    #region enum : ScrollMode
    /// <summary>
    /// 스크롤 모드를 정의합니다.
    /// </summary>
    public enum ScrollMode
    {
        /// <summary>수평 스크롤</summary>
        Horizon,
        /// <summary>수직 스크롤</summary>
        Vertical,
        /// <summary>수평 및 수직 스크롤</summary>
        Both
    }
    #endregion
    #region class : SCDI
    internal class SCDI
    {
        internal SKPoint DownPoint { get; set; }
        internal SKRect CursorBounds { get; set; }
    }
    #endregion
    #region class : TCDI
    internal class TCDI
    {
        internal SKPoint DownPoint { get; set; }
        internal SKPoint MovePoint { get; set; }
        internal DateTime DownTime { get; set; }
        internal List<TCMI> List { get; } = new List<TCMI>();
    }
    #endregion
    #region class : TCMI
    class TCMI
    {
        public DateTime Time { get; set; }
        public SKPoint Point { get; set; }
    }
    #endregion
}
