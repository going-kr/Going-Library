using Going.UI.Design;
using Going.UI.Themes;
using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Utils
{
    /// <summary>
    /// UI 애니메이션을 관리하는 클래스입니다. 선형, 가속, 감속 보간을 지원하며, 다양한 타입(int, float, double, SKRect, SKColor)의 값 보간을 제공합니다.
    /// </summary>
    public class Animation
    {
        #region Const
        /// <summary>
        /// 150ms 애니메이션 기본 시간 상수입니다.
        /// </summary>
        public const int Time150 = 150;
        /// <summary>
        /// 200ms 애니메이션 기본 시간 상수입니다.
        /// </summary>
        public const int Time200 = 200;

        private const double EffectLevel = -5.0;
        #endregion

        #region Properties
        /// <summary>
        /// 애니메이션 사용 여부를 가져오거나 설정합니다.
        /// </summary>
        public static bool UseAnimation { get; set; } = true;

        /// <summary>
        /// 애니메이션의 전체 재생 시간(밀리초)을 가져옵니다.
        /// </summary>
        public double TotalMillls { get; private set; }
        /// <summary>
        /// 현재까지의 재생 시간(밀리초)을 가져옵니다.
        /// </summary>
        public double PlayMillis
        {
            get
            {
                DateTime? tm = tmStart;
                return tm.HasValue ? (DateTime.Now - tm.Value).TotalMilliseconds : 0;
            }
        }
        /// <summary>
        /// 애니메이션이 재생 중인지 여부를 가져옵니다.
        /// </summary>
        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                isPlaying = value;
                States[Id] = isPlaying;
                IsAnimationPlaying = States.Values.Any(x => x);
            }
        }
        /// <summary>
        /// 애니메이션에 연결된 변수 문자열을 가져옵니다.
        /// </summary>
        public string? Variable { get; private set; }
        /// <summary>
        /// 화면 갱신 콜백 액션입니다.
        /// </summary>
        public Action? Refresh;

        private string Id { get; } = Guid.NewGuid().ToString();
        private bool isPlaying = false;
        internal static ConcurrentDictionary<string, bool> States = new();
        /// <summary>
        /// 현재 재생 중인 애니메이션이 있는지 여부를 가져옵니다.
        /// </summary>
        public static bool IsAnimationPlaying { get; private set; }
        #endregion

        #region Member Variable
        DateTime? tmStart = null;
        Action? complete = null;
        Task? task;
        CancellationTokenSource? cancel;
        #endregion

        #region Method
        #region Start
        /// <summary>
        /// 애니메이션을 시작합니다.
        /// </summary>
        /// <param name="totalMillis">전체 재생 시간 (밀리초)</param>
        /// <param name="variable">애니메이션 변수 문자열</param>
        /// <param name="act">애니메이션 완료 시 실행할 콜백</param>
        public void Start(double totalMillis, string? variable = null, Action? act = null)
        {
            if (UseAnimation)
            {
                if (!IsPlaying)
                {
                    this.tmStart = DateTime.Now;
                    this.TotalMillls = totalMillis;
                    this.Variable = variable;

                    cancel = new CancellationTokenSource();
                    task = Task.Run(async () =>
                    {
                        var token = cancel.Token;

                        this.IsPlaying = true;

                        try
                        {
                            while (!token.IsCancellationRequested && this.IsPlaying && (tmStart.HasValue && (DateTime.Now - tmStart.Value).TotalMilliseconds < TotalMillls))
                            {
                                Refresh?.Invoke();
                                await Task.Delay(10);
                            }
                        }
                        catch (OperationCanceledException ex) { }
                        finally
                        {
                            act?.Invoke();
                            this.tmStart = null;
                            this.IsPlaying = false;
                            this.TotalMillls = 0;
                            Refresh?.Invoke();
                            
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(50);
                                Refresh?.Invoke();
                            });
                        }

                    }, cancel.Token);
                }
            }
            else
            {
                act?.Invoke();
                Refresh?.Invoke();
            }
        }
        #endregion
        #region Stop
        /// <summary>
        /// 재생 중인 애니메이션을 중지합니다.
        /// </summary>
        public void Stop()
        {
            try { cancel?.Cancel(false); }
            finally
            {
                cancel?.Dispose();
                cancel = null;
            }

            task = null;

            Refresh?.Invoke();
        }
        #endregion
        #region Linear / Accel / Decel
        double Linear(double now, double goal) => now + (goal - now) * MathTool.Constrain(PlayMillis / TotalMillls, 0, 1);
        //double Accel(double now, double goal) => goal - (goal - now) * Math.Sqrt(1.0 - (Math.Pow(MathTool.Constrain(PlayMillis / TotalMillls, 0.0, 1.0), 2.0) / Math.Pow(1.0, 2.0)));
        //double Decel(double now, double goal) => now + (goal - now) * Math.Sqrt(1.0 - (Math.Pow(1.0 - MathTool.Constrain(PlayMillis / TotalMillls, 0.0, 1.0), 2.0) / Math.Pow(1.0, 2.0)));

        double Decel(double now, double goal) => now + (goal - now) * (1.0 - Math.Exp(EffectLevel * MathTool.Constrain(PlayMillis / TotalMillls, 0.0001, 0.9999)));
        double Accel(double now, double goal) => goal - (goal - now) * (1.0 - Math.Exp(EffectLevel * (1.0 - MathTool.Constrain(PlayMillis / TotalMillls, 0.0001, 0.9999))));
        #endregion

        #region Value
        #region Value(int, int)
        /// <summary>
        /// 현재 애니메이션 진행률에 따라 정수 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 값</param>
        /// <param name="end">끝 값</param>
        /// <returns>보간된 값</returns>
        public int Value(AnimationAccel velocity, int start, int end)
        {
            int ret = end;
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear: ret = Convert.ToInt32(Linear(start, end)); break;
                    case AnimationAccel.ACL: ret = Convert.ToInt32(Accel(start, end)); break;
                    case AnimationAccel.DCL: ret = Convert.ToInt32(Decel(start, end)); break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(float, float)
        /// <summary>
        /// 현재 애니메이션 진행률에 따라 float 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 값</param>
        /// <param name="end">끝 값</param>
        /// <returns>보간된 값</returns>
        public float Value(AnimationAccel velocity, float start, float end)
        {
            float ret = end;
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear: ret = Convert.ToSingle(Linear(start, end)); break;
                    case AnimationAccel.ACL: ret = Convert.ToSingle(Accel(start, end)); break;
                    case AnimationAccel.DCL: ret = Convert.ToSingle(Decel(start, end)); break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(float, float)
        /// <summary>
        /// 현재 애니메이션 진행률에 따라 double 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 값</param>
        /// <param name="end">끝 값</param>
        /// <returns>보간된 값</returns>
        public double Value(AnimationAccel velocity, double start, double end)
        {
            double ret = end;
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear: ret = (Linear(start, end)); break;
                    case AnimationAccel.ACL: ret = (Accel(start, end)); break;
                    case AnimationAccel.DCL: ret = (Decel(start, end)); break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(Point, Rect)
        /// <summary>
        /// 점에서 사각형으로의 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 점</param>
        /// <param name="end">끝 사각형</param>
        /// <returns>보간된 사각형</returns>
        public SKRect Value(AnimationAccel velocity, SKPoint start, SKRect end)
        {
            SKRect ret = Util.FromRect(end);
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear:
                        ret.Left = Convert.ToSingle(Linear(start.X, end.Left));
                        ret.Top = Convert.ToSingle(Linear(start.Y, end.Top));
                        ret.Right = Convert.ToSingle(Linear(start.X, end.Right));
                        ret.Bottom = Convert.ToSingle(Linear(start.Y, end.Bottom));
                        break;
                    case AnimationAccel.ACL:
                        ret.Left = Convert.ToSingle(Accel(start.X, end.Left));
                        ret.Top = Convert.ToSingle(Accel(start.Y, end.Top));
                        ret.Right = Convert.ToSingle(Accel(start.X, end.Right));
                        ret.Bottom = Convert.ToSingle(Accel(start.Y, end.Bottom));
                        break;
                    case AnimationAccel.DCL:
                        ret.Left = Convert.ToSingle(Decel(start.X, end.Left));
                        ret.Top = Convert.ToSingle(Decel(start.Y, end.Top));
                        ret.Right = Convert.ToSingle(Decel(start.X, end.Right));
                        ret.Bottom = Convert.ToSingle(Decel(start.Y, end.Bottom));
                        break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(Rect, Point)
        /// <summary>
        /// 사각형에서 점으로의 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 사각형</param>
        /// <param name="end">끝 점</param>
        /// <returns>보간된 사각형</returns>
        public SKRect Value(AnimationAccel velocity, SKRect start, SKPoint end)
        {
            SKRect ret = Util.FromRect(end.X, end.Y, 0, 0);
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear:
                        ret.Left = Convert.ToSingle(Linear(start.Left, end.X));
                        ret.Top = Convert.ToSingle(Linear(start.Top, end.Y));
                        ret.Right = Convert.ToSingle(Linear(start.Right, end.X));
                        ret.Bottom = Convert.ToSingle(Linear(start.Bottom, end.Y));
                        break;
                    case AnimationAccel.ACL:
                        ret.Left = Convert.ToSingle(Accel(start.Left, end.X));
                        ret.Top = Convert.ToSingle(Accel(start.Top, end.Y));
                        ret.Right = Convert.ToSingle(Accel(start.Right, end.X));
                        ret.Bottom = Convert.ToSingle(Accel(start.Bottom, end.Y));
                        break;
                    case AnimationAccel.DCL:
                        ret.Left = Convert.ToSingle(Decel(start.Left, end.X));
                        ret.Top = Convert.ToSingle(Decel(start.Top, end.Y));
                        ret.Right = Convert.ToSingle(Decel(start.Right, end.X));
                        ret.Bottom = Convert.ToSingle(Decel(start.Bottom, end.Y));
                        break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(Rect, Rect)
        /// <summary>
        /// 두 사각형 간의 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 사각형</param>
        /// <param name="end">끝 사각형</param>
        /// <returns>보간된 사각형</returns>
        public SKRect Value(AnimationAccel velocity, SKRect start, SKRect end)
        {
            SKRect ret = Util.FromRect(end);
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear:
                        ret.Left = Convert.ToSingle(Linear(start.Left, end.Left));
                        ret.Top = Convert.ToSingle(Linear(start.Top, end.Top));
                        ret.Right = Convert.ToSingle(Linear(start.Right, end.Right));
                        ret.Bottom = Convert.ToSingle(Linear(start.Bottom, end.Bottom));
                        break;
                    case AnimationAccel.ACL:
                        ret.Left = Convert.ToSingle(Accel(start.Left, end.Left));
                        ret.Top = Convert.ToSingle(Accel(start.Top, end.Top));
                        ret.Right = Convert.ToSingle(Accel(start.Right, end.Right));
                        ret.Bottom = Convert.ToSingle(Accel(start.Bottom, end.Bottom));
                        break;
                    case AnimationAccel.DCL:
                        ret.Left = Convert.ToSingle(Decel(start.Left, end.Left));
                        ret.Top = Convert.ToSingle(Decel(start.Top, end.Top));
                        ret.Right = Convert.ToSingle(Decel(start.Right, end.Right));
                        ret.Bottom = Convert.ToSingle(Decel(start.Bottom, end.Bottom));
                        break;
                }
            }
            return ret;
        }
        #endregion
        #region Value(Color, Color)
        /// <summary>
        /// 두 색상 간의 보간 값을 계산합니다.
        /// </summary>
        /// <param name="velocity">보간 가속도 타입</param>
        /// <param name="start">시작 색상</param>
        /// <param name="end">끝 색상</param>
        /// <returns>보간된 색상</returns>
        public SKColor Value(AnimationAccel velocity, SKColor start, SKColor end)
        {
            byte a = end.Alpha, r = end.Red, g = end.Green, b = end.Blue;
            if (IsPlaying)
            {
                switch (velocity)
                {
                    case AnimationAccel.Linear:
                        a = Convert.ToByte(Linear(start.Alpha, end.Alpha));
                        r = Convert.ToByte(Linear(start.Red, end.Red));
                        g = Convert.ToByte(Linear(start.Green, end.Green));
                        b = Convert.ToByte(Linear(start.Blue, end.Blue));
                        break;
                    case AnimationAccel.ACL:
                        a = Convert.ToByte(Accel(start.Alpha, end.Alpha));
                        r = Convert.ToByte(Accel(start.Red, end.Red));
                        g = Convert.ToByte(Accel(start.Green, end.Green));
                        b = Convert.ToByte(Accel(start.Blue, end.Blue));
                        break;
                    case AnimationAccel.DCL:
                        a = Convert.ToByte(Decel(start.Alpha, end.Alpha));
                        r = Convert.ToByte(Decel(start.Red, end.Red));
                        g = Convert.ToByte(Decel(start.Green, end.Green));
                        b = Convert.ToByte(Decel(start.Blue, end.Blue));
                        break;
                }
            }
            return Util.FromArgb(a, r, g, b);
        }
        #endregion
        #endregion
        #endregion
    }

    #region enum : AnimationAccel
    /// <summary>
    /// 애니메이션 가속도 타입을 정의합니다.
    /// </summary>
    public enum AnimationAccel
    {
        /// <summary>선형 보간</summary>
        Linear,
        /// <summary>가속 보간</summary>
        ACL,
        /// <summary>감속 보간</summary>
        DCL
    }
    #endregion
    #region enum : AnimationType
    /// <summary>
    /// 애니메이션 전환 타입을 정의합니다.
    /// </summary>
    public enum AnimationType
    {
        /// <summary>수직 슬라이드</summary>
        SlideV,
        /// <summary>수평 슬라이드</summary>
        SlideH,
        /// <summary>드릴 전환</summary>
        Drill,
        /// <summary>페이드 전환</summary>
        Fade,
        /// <summary>전환 없음</summary>
        None
    }
    #endregion
}
 