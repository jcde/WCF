using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WcfServer.Performance
{
    public class PerfData : IDisposable
    {
        /// <summary>
        /// list of samples sorted by chronology. there is always first element
        /// </summary>
        public readonly List<CounterSample> Samples = new List<CounterSample>();

        /// <summary>
        /// may be null, if performance system is OFF
        /// </summary>
        public PerformanceCounter Counter;

        public long? Max;
        public long? Min;

        public PerfData(PerformanceCounter counter)
        {
            Counter = counter;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Counter != null)
                Counter.Close();
        }

        #endregion

        public void Clear()
        {
            CounterSample? sLast = null;
            if (Samples.Count > 0)
                sLast = Samples[0];
            Samples.Clear();
            if (Counter != null)
            {
                Counter.RawValue = 0;
                try
                {
                    Samples.Add(sLast ?? Counter.NextSample());
                }
                catch (Exception)
                {
                    Counter = null;
                    // performance system is OFF
                }
            }
            Max = null;
            Min = null;
        }
    }
}