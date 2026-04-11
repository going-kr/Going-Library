using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Measure
{
    /// <summary>채터링 필터 동작 모드</summary>
    public enum ChatteringMode
    {
        /// <summary>On/Off 양방향 모두 채터링 필터 적용</summary>
        Both,
        /// <summary>On 상태에 채터링 필터 적용 (On으로 전환될 때 지연)</summary>
        On,
        /// <summary>Off 상태에 채터링 필터 적용 (Off로 전환될 때 지연)</summary>
        Off
    }

    /// <summary>
    /// 디지털 입력 신호의 채터링(바운싱)을 필터링하는 클래스.
    /// 설정된 시간 동안 입력 값이 유지되어야 상태가 변경된다.
    /// </summary>
    public class Chattering
    {
        #region Event
        /// <summary>채터링 필터를 통과하여 상태가 변경되었을 때 발생하는 이벤트</summary>
        public event EventHandler<ChatteringStateChangedEventArgs>? StateChanged;
        #endregion

        #region Properties
        /// <summary>현재 필터링된 출력 상태</summary>
        public bool State { get; private set; }
        /// <summary>채터링 필터 시간 (밀리초). 이 시간 동안 입력이 유지되어야 상태가 변경된다. 기본값: 300ms</summary>
        public int ChatteringTime { get; set; } = 300;
        /// <summary>채터링 필터 동작 모드. 기본값: Both (양방향)</summary>
        public ChatteringMode Mode { get; set; } = ChatteringMode.Both;
        #endregion

        #region Member Variable
        private bool InputValue = false;
        private DateTime InputStartTime = DateTime.Now;
        private bool IsFirstInput = true;
        #endregion

        #region Constructor
        /// <summary>기본 설정(300ms, Both 모드)으로 채터링 필터를 생성한다.</summary>
        public Chattering()
        {
        }

        /// <summary>지정된 채터링 시간으로 필터를 생성한다.</summary>
        /// <param name="chatteringTime">채터링 필터 시간 (밀리초)</param>
        public Chattering(int chatteringTime)
        {
            ChatteringTime = chatteringTime;
        }

        /// <summary>지정된 채터링 시간과 동작 모드로 필터를 생성한다.</summary>
        /// <param name="chatteringTime">채터링 필터 시간 (밀리초)</param>
        /// <param name="mode">채터링 필터 동작 모드</param>
        public Chattering(int chatteringTime, ChatteringMode mode)
        {
            ChatteringTime = chatteringTime;
            Mode = mode;
        }
        #endregion

        #region Method
        /// <summary>입력 값을 설정한다. 채터링 시간 동안 값이 유지되면 State가 변경된다.</summary>
        /// <param name="value">디지털 입력 값</param>
        public void Set(bool value)
        {
            if (IsFirstInput)
            {
                IsFirstInput = false;
                State = value;
                InputValue = value;
                InputStartTime = DateTime.Now;
                return;
            }

            if (InputValue != value)
            {
                InputValue = value;
                InputStartTime = DateTime.Now;

                if (State != value)
                    if (!NeedsDelayForChange(value))
                        ChangeState(value);
            }
            else
            {
                if (State != InputValue && NeedsDelayForChange(InputValue))
                {
                    var elapsed = (DateTime.Now - InputStartTime).TotalMilliseconds;

                    if (elapsed >= ChatteringTime)
                        ChangeState(InputValue);
                }
            }
        }

        private bool NeedsDelayForChange(bool targetState)
        {
            switch (Mode)
            {
                case ChatteringMode.Both:
                    return true;

                case ChatteringMode.On:
                    return targetState;

                case ChatteringMode.Off:
                    return !targetState;

                default:
                    return true;
            }
        }

        private void ChangeState(bool newState)
        {
            if (State != newState)
            {
                var oldState = State;
                State = newState;

                StateChanged?.Invoke(this, new ChatteringStateChangedEventArgs(newState, oldState));

                InputValue = newState;
            }
        }

        /// <summary>채터링 필터를 초기화한다.</summary>
        /// <param name="resetState">true이면 State도 false로 초기화, false이면 현재 State를 유지</param>
        public void Reset(bool resetState = false)
        {
            InputStartTime = DateTime.Now;
            IsFirstInput = true;

            if (resetState)
            {
                State = false;
                InputValue = false;
            }
            else
            {
                InputValue = State;
            }
        }

        /// <summary>상태 변경까지 남은 시간 (밀리초). 대기 중이 아니면 0을 반환한다.</summary>
        public double RemainingTime
        {
            get
            {
                if (State == InputValue || !NeedsDelayForChange(InputValue))
                    return 0;

                var elapsed = (DateTime.Now - InputStartTime).TotalMilliseconds;
                return Math.Max(0, ChatteringTime - elapsed);
            }
        }

        /// <summary>현재 필터링 전 원시 입력 값</summary>
        public bool CurrentInput => InputValue;
        /// <summary>현재 채터링 필터 상태를 문자열로 반환한다.</summary>
        /// <returns>모드, 상태, 입력, 채터링 시간, 남은 시간을 포함한 상태 문자열</returns>
        public string GetStatus()
        {
            return $"Mode={Mode}, State={State}, Input={InputValue}, " +
                   $"ChatteringTime={ChatteringTime}ms, Remaining={RemainingTime:F0}ms";
        }
        #endregion
    }

    /// <summary>채터링 필터의 상태 변경 이벤트 인자</summary>
    public class ChatteringStateChangedEventArgs : EventArgs
    {
        /// <summary>변경 후 상태 값</summary>
        public bool Value { get; private set; }
        /// <summary>변경 전 상태 값</summary>
        public bool OldValue { get; private set; }

        /// <summary>상태 변경 이벤트 인자를 생성한다.</summary>
        /// <param name="value">변경 후 상태 값</param>
        /// <param name="oldValue">변경 전 상태 값</param>
        public ChatteringStateChangedEventArgs(bool value, bool oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }
    }
}
