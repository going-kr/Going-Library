using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Measure
{
    public enum StableMode
    {
        Absolute,
        Relative,
        Hybrid
    }

    public class Stable
    {
        #region Event
        public event EventHandler<StableEventArgs>? Measured;
        public event EventHandler<StableEventArgs>? Measuring;
        #endregion

        #region Properties
        public double Value { get; private set; }
        public double ErrorRange { get; set; } = 1.0;
        public int MeasureTime { get; set; } = 1000;
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
        public Stable()
        {
        }
        #endregion

        #region Method
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

        public void Reset()
        {
            IsMeasuringStable = false;
            InitialStableValue = 0;
            CurrentStableValue = 0;
            StableStartTime = DateTime.MinValue;
            IsFirstValue = true;
        }

        public bool IsStabilizing => IsMeasuringStable;

        public double CurrentReferenceValue => CurrentStableValue;

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

        public string GetStatus()
        {
            return $"Mode={Mode}, Value={Value:F2}, IsMeasuring={IsMeasuringStable}, " +
                   $"Initial={InitialStableValue:F2}, Current={CurrentStableValue:F2}, " +
                   $"Remaining={RemainingTime:F0}ms";
        }
        #endregion
    }

    public class StableEventArgs : EventArgs
    {
        public double Value { get; private set; }

        public StableEventArgs(double value)
        {
            Value = value;
        }
    }
}
