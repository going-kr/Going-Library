using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    /// <summary>
    /// Stopwatch 기반 고해상도 타이머.
    /// System.Timers.Timer보다 정밀한 간격으로 이벤트를 발생시키며, 밀리초 이하 단위의 타이밍이 가능하다.
    /// </summary>
    public class HiResTimer
    {
        #region Variable
        private static readonly float tickFrequency = 1000f / Stopwatch.Frequency;

        private volatile float interval;
        private volatile float ignoreElapsedThreshold = Single.PositiveInfinity;
        private volatile bool isRunning;
        #endregion

        #region Events
        /// <summary>설정된 간격마다 발생하는 이벤트</summary>
        public event EventHandler<HiResTimerElapsedEventArgs>? Elapsed;
        #endregion

        #region Properties
        /// <summary>타이머 간격 (밀리초 단위). 0 이상이어야 한다.</summary>
        public float Interval
        {
            get => interval;
            set
            {
                if (value < 0f || Single.IsNaN(value)) throw new ArgumentOutOfRangeException();
                interval = value;
            }
        }

        /// <summary>
        /// 이 임계값(밀리초)을 초과하는 지연이 발생하면 해당 Elapsed 이벤트를 건너뛴다.
        /// 기본값은 무한대(건너뛰지 않음).
        /// </summary>
        public float IgnoreElapsedThreshold
        {
            get => ignoreElapsedThreshold;
            set
            {
                if (value <= 0f || Single.IsNaN(value)) throw new ArgumentOutOfRangeException();
                ignoreElapsedThreshold = value;
            }
        }

        /// <summary>타이머 활성화 상태. true로 설정하면 Start(), false로 설정하면 Stop()이 호출된다.</summary>
        public bool Enabled
        {
            get => isRunning;
            set
            {
                if (value) Start();
                else Stop();
            }
        }
        #endregion

        #region Constructor
        /// <summary>기본 간격 1ms로 고해상도 타이머를 생성한다.</summary>
        public HiResTimer() : this(1f)
        {
        }

        /// <summary>지정된 간격으로 고해상도 타이머를 생성한다.</summary>
        /// <param name="interval">타이머 간격 (밀리초)</param>
        public HiResTimer(float interval)
        {
            if (interval < 0f || Single.IsNaN(interval))
                throw new ArgumentOutOfRangeException();
            this.interval = interval;
        }
        #endregion

        #region Methods
        private static float ElapsedHiRes(Stopwatch stopwatch) => stopwatch.ElapsedTicks * tickFrequency;

        /// <summary>타이머를 시작한다. 최고 우선순위 스레드에서 실행된다.</summary>
        public void Start()
        {
            if (isRunning)
                return;

            isRunning = true;
            Thread thread = new Thread(ExecuteTimer) { Priority = ThreadPriority.Highest };
            thread.Start();
        }

        /// <summary>타이머를 정지한다.</summary>
        public void Stop() => isRunning = false;
        #endregion

        #region Private Methods
        private void ExecuteTimer()
        {
            int fallouts = 0;
            float nextTrigger = 0f;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (isRunning)
            {
                float intervalLocal = interval;
                nextTrigger += intervalLocal;
                float elapsed;

                while (true)
                {
                    elapsed = ElapsedHiRes(stopwatch);
                    float diff = nextTrigger - elapsed;
                    if (diff <= 0f)
                        break;

                    if (diff < 1f)
                        Thread.SpinWait(10);
                    else if (diff < 10f)
                        Thread.SpinWait(100);
                    else
                    {
                        if (diff >= 16f)
                            Thread.Sleep(diff >= 100f ? 50 : 1);
                        else
                        {
                            Thread.SpinWait(1000);
                            Thread.Sleep(0);
                        }

                        float newInterval = interval;

                        if (intervalLocal != newInterval)
                        {
                            nextTrigger += newInterval - intervalLocal;
                            intervalLocal = newInterval;
                        }
                    }

                    if (!isRunning)
                        return;
                }


                float delay = elapsed - nextTrigger;
                if (delay >= ignoreElapsedThreshold)
                {
                    fallouts += 1;
                    continue;
                }

                Elapsed?.Invoke(this, new HiResTimerElapsedEventArgs(delay, fallouts));
                fallouts = 0;

                if (stopwatch.Elapsed.TotalHours >= 1d)
                {
                    float overshoot = elapsed - nextTrigger;
#if NET35
                    stopwatch.Reset();
                    stopwatch.Start();
#else
                    stopwatch.Restart();
#endif
                    nextTrigger = intervalLocal - overshoot;
                }
            }

            stopwatch.Stop();
        }
        #endregion
    }

    /// <summary>고해상도 타이머의 Elapsed 이벤트 인자</summary>
    public class HiResTimerElapsedEventArgs : EventArgs
    {
        /// <summary>예정 시각 대비 실제 발생 지연 시간 (밀리초)</summary>
        public float Delay { get; }
        /// <summary>IgnoreElapsedThreshold 초과로 건너뛴 이벤트 횟수</summary>
        public int Fallouts { get; }

        internal HiResTimerElapsedEventArgs(float delay, int fallouts)
        {
            Delay = delay;
            Fallouts = fallouts;
        }
    }
}
