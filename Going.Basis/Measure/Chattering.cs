using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Measure
{
    using System;

    namespace Going.Basis.Measure
    {
        public enum ChatteringMode { Both, On, Off }

        public class Chattering
        {
            #region Event
            public event EventHandler<ChatteringStateChangedEventArgs>? StateChanged;
            #endregion

            #region Properties
            public bool State { get; private set; }
            public int ChatteringTime { get; set; } = 300;
            public ChatteringMode Mode { get; set; } = ChatteringMode.Both;
            #endregion

            #region Member Variable
            private bool InputValue = false;
            private DateTime InputStartTime = DateTime.Now;
            private bool IsFirstInput = true;
            #endregion

            #region Constructor
            public Chattering()
            {
            }

            public Chattering(int chatteringTime)
            {
                ChatteringTime = chatteringTime;
            }

            public Chattering(int chatteringTime, ChatteringMode mode)
            {
                ChatteringTime = chatteringTime;
                Mode = mode;
            }
            #endregion

            #region Method
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

            public bool CurrentInput => InputValue;
            public string GetStatus()
            {
                return $"Mode={Mode}, State={State}, Input={InputValue}, " +
                       $"ChatteringTime={ChatteringTime}ms, Remaining={RemainingTime:F0}ms";
            }
            #endregion
        }

        public class ChatteringStateChangedEventArgs : EventArgs
        {
            public bool Value { get; private set; }
            public bool OldValue { get; private set; }

            public ChatteringStateChangedEventArgs(bool value, bool oldValue)
            {
                Value = value;
                OldValue = oldValue;
            }
        }
    }
}
