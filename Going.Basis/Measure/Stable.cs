using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Measure
{
    /// <summary>안정화 판정 모드</summary>
    public enum StableMode
    {
        /// <summary>초기 기준값과의 절대 오차로 안정 여부를 판정</summary>
        Absolute,
        /// <summary>직전 값과의 상대 오차로 안정 여부를 판정</summary>
        Relative,
        /// <summary>절대 오차와 상대 오차를 모두 적용하여 판정</summary>
        Hybrid
    }

    /// <summary>
    /// 아날로그 값의 안정화(수렴) 여부를 감지하는 클래스.
    /// 설정된 오차 범위 내에서 지정 시간 동안 값이 유지되면 안정화로 판정한다.
    /// </summary>
    public class Stable
    {
        #region Event
        /// <summary>값이 안정화되었을 때 발생하는 이벤트</summary>
        public event EventHandler<StableEventArgs>? Measured;
        /// <summary>값이 변경될 때마다 발생하는 이벤트 (안정화 판정 전)</summary>
        public event EventHandler<StableEventArgs>? Measuring;
        #endregion

        #region Properties
        /// <summary>현재 입력 값</summary>
        public double Value { get; private set; }
        /// <summary>안정 판정 오차 범위. 기본값: 1.0</summary>
        public double ErrorRange { get; set; } = 1.0;
        /// <summary>안정 판정에 필요한 유지 시간 (밀리초). 기본값: 1000ms</summary>
        public int MeasureTime { get; set; } = 1000;
        /// <summary>안정화 판정 모드. 기본값: Relative</summary>
        public StableMode Mode { get; set; } = StableMode.Relative;
        #endregion

        #region Member Variable
        private bool IsMeasuringStable = false;
        private double InitialStableValue = 0;
        private double CurrentStableValue = 0;
        private DateTime StableStartTime = DateTime.MinValue;
        private bool IsFirstValue = true;
        #endregion

        #region Constructor
        /// <summary>기본 설정(오차 1.0, 1000ms, Relative 모드)으로 안정화 감지기를 생성한다.</summary>
        public Stable()
        {
        }
        #endregion

        #region Method
        /// <summary>측정 값을 입력한다. 오차 범위 내에서 MeasureTime 동안 유지되면 Measured 이벤트가 발생한다.</summary>
        /// <param name="value">측정 값</param>
        public void Set(double value)
        {
            var oldValue = this.Value;
            this.Value = value;

            if (oldValue != value)
            {
                Measuring?.Invoke(this, new StableEventArgs(value));
            }

            if (IsFirstValue)
            {
                IsFirstValue = false;
                InitialStableValue = value;
                CurrentStableValue = value;
                return;
            }

            if (!IsMeasuringStable)
            {
                HandleNotMeasuring(value);
            }
            else
            {
                HandleMeasuring(value);
            }
        }

        private void HandleNotMeasuring(double value)
        {
            bool shouldStartMeasuring = false;

            switch (Mode)
            {
                case StableMode.Absolute:
                    shouldStartMeasuring = Math.Abs(value - InitialStableValue) > ErrorRange;
                    break;

                case StableMode.Relative:
                    shouldStartMeasuring = Math.Abs(value - CurrentStableValue) > ErrorRange;
                    if (!shouldStartMeasuring)
                    {
                        CurrentStableValue = value;
                    }
                    break;

                case StableMode.Hybrid:
                    shouldStartMeasuring = Math.Abs(value - InitialStableValue) > ErrorRange ||
                                          Math.Abs(value - CurrentStableValue) > ErrorRange;
                    if (!shouldStartMeasuring)
                    {
                        CurrentStableValue = value;
                    }
                    break;
            }

            if (shouldStartMeasuring)
            {
                InitialStableValue = value;
                CurrentStableValue = value;
                IsMeasuringStable = true;
                StableStartTime = DateTime.Now;
            }
        }

        private void HandleMeasuring(double value)
        {
            bool isStable = false;

            switch (Mode)
            {
                case StableMode.Absolute:
                    isStable = Math.Abs(value - InitialStableValue) <= ErrorRange;
                    break;

                case StableMode.Relative:
                    isStable = Math.Abs(value - CurrentStableValue) <= ErrorRange;
                    if (isStable)
                    {
                        CurrentStableValue = value;
                    }
                    break;

                case StableMode.Hybrid:
                    bool absoluteStable = Math.Abs(value - InitialStableValue) <= ErrorRange;
                    bool relativeStable = Math.Abs(value - CurrentStableValue) <= ErrorRange;
                    isStable = absoluteStable && relativeStable;

                    if (isStable)
                    {
                        CurrentStableValue = value;
                    }
                    break;
            }

            if (!isStable)
            {
                InitialStableValue = value;
                CurrentStableValue = value;
                StableStartTime = DateTime.Now;
            }
            else
            {
                var elapsed = (DateTime.Now - StableStartTime).TotalMilliseconds;

                if (elapsed >= MeasureTime)
                {
                    IsMeasuringStable = false;
                    Measured?.Invoke(this, new StableEventArgs(value));
                }
            }
        }

        /// <summary>안정화 감지기를 초기 상태로 리셋한다.</summary>
        public void Reset()
        {
            IsMeasuringStable = false;
            InitialStableValue = 0;
            CurrentStableValue = 0;
            StableStartTime = DateTime.MinValue;
            IsFirstValue = true;
        }

        /// <summary>현재 안정화 측정 진행 중 여부</summary>
        public bool IsStabilizing => IsMeasuringStable;

        /// <summary>현재 안정화 판정에 사용 중인 기준 값</summary>
        public double CurrentReferenceValue => CurrentStableValue;

        /// <summary>안정화 판정까지 남은 시간 (밀리초). 측정 중이 아니면 0을 반환한다.</summary>
        public double RemainingTime
        {
            get
            {
                if (!IsMeasuringStable)
                    return 0;

                var elapsed = (DateTime.Now - StableStartTime).TotalMilliseconds;
                return Math.Max(0, MeasureTime - elapsed);
            }
        }

        /// <summary>현재 안정화 감지기 상태를 문자열로 반환한다.</summary>
        /// <returns>모드, 값, 측정 여부, 기준값, 남은 시간을 포함한 상태 문자열</returns>
        public string GetStatus()
        {
            return $"Mode={Mode}, Value={Value:F2}, IsMeasuring={IsMeasuringStable}, " +
                   $"Initial={InitialStableValue:F2}, Current={CurrentStableValue:F2}, " +
                   $"Remaining={RemainingTime:F0}ms";
        }
        #endregion
    }

    /// <summary>안정화 감지 이벤트 인자</summary>
    public class StableEventArgs : EventArgs
    {
        /// <summary>이벤트 발생 시점의 측정 값</summary>
        public double Value { get; private set; }

        /// <summary>안정화 이벤트 인자를 생성한다.</summary>
        /// <param name="value">측정 값</param>
        public StableEventArgs(double value)
        {
            Value = value;
        }
    }
}
