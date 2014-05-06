using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using WcfServer.Properties;

namespace WcfServer.Performance
{
    /// <summary>
    /// per one Wcf service.
    /// since computer may hold many such services, then _instanceName is used
    /// 
    /// new PerformanceCounter's should be added into _countersDescriptor 
    ///     and it will be created at startup: by service restarting in practice
    /// </summary>
    public class PerfCounters : IDisposable
    {
        public const string ChatCommands2ServerPerSecond = "ChatCommands2ServerPerSecond";
        public const string Connections = "Connections";
        public const string ICBCategory = "ICB";
        public const string Messages2ServerPerSecond = "Messages2ServerPerSecond";
        public const string MessagesFromServerPerSecond = "MessagesFromServerPerSecond";
        private static bool _needRegisteringCounters = Settings.Default.EnablePerfCounters;

        internal readonly Dictionary<string, PerfData> _counters =
            new Dictionary<string, PerfData>();

        private readonly List<CounterCreationData> _countersDescriptor =
            new List<CounterCreationData>
                {
                    new CounterCreationData(Connections,
                                            "Number of current connections from client application to server",
                                            PerformanceCounterType.NumberOfItems32),
                    new CounterCreationData(Messages2ServerPerSecond,
                                            "Number of messages received by server per second",
                                            PerformanceCounterType.RateOfCountsPerSecond32),
                    new CounterCreationData(MessagesFromServerPerSecond,
                                            "Number of messages broadcasted from server per second",
                                            PerformanceCounterType.RateOfCountsPerSecond32),
                    new CounterCreationData(ChatCommands2ServerPerSecond,
                                            "Number of chat commands received by from server per second",
                                            PerformanceCounterType.RateOfCountsPerSecond32),
                    //... new PerformanceCounter's should be added here
                };

        private readonly string _instanceName;

        private readonly Thread _registeringThread;

        public PerfCounters(string instanceName)
        {
            _instanceName = instanceName;
            if (_needRegisteringCounters)
            {
                // sometimes too slow thread
                _registeringThread = new Thread(
                    delegate()
                        {
                            _needRegisteringCounters = false;
                            try
                            {
                                bool recreate;
                                try
                                {
                                    recreate = !PerformanceCounterCategory.Exists(ICBCategory)
                                               ||
                                               !PerformanceCounterCategory.InstanceExists(
                                                   instanceName, ICBCategory)
                                               || (from d in _countersDescriptor
                                                   join c in
                                                       new PerformanceCounterCategory(
                                                       ICBCategory).GetCounters(_instanceName)
                                                       on d.CounterName equals c.CounterName
                                                   where d.CounterType == c.CounterType
                                                   select c).Count() != _countersDescriptor.Count;
                                }
                                catch (InvalidOperationException)
                                {
                                    // strange exception 
                                    //      from PerformanceCounterCategory.InstanceExists(instanceName, ICBCategory)  
                                    // when PerformanceCounterCategory.Exists(ICBCategory)==true
                                    recreate = true;
                                }
                                catch (FormatException)
                                {
                                    // performance system is OFF
                                    recreate = false;
                                }
                                if (recreate)
                                {
                                    if (PerformanceCounterCategory.Exists(ICBCategory))
                                        try
                                        {
                                            PerformanceCounterCategory.Delete(ICBCategory);
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                    var ccdc = new CounterCreationDataCollection();
                                    foreach (CounterCreationData d in _countersDescriptor)
                                        ccdc.Add(d);
                                    try
                                    {
                                        if (!PerformanceCounterCategory.Exists(ICBCategory))
                                            PerformanceCounterCategory.Create(ICBCategory,
                                                                              ICBCategory,
                                                                              PerformanceCounterCategoryType
                                                                                  .MultiInstance,
                                                                              ccdc);
                                    }
                                    catch (SecurityException ex)
                                    {
                                        Logger.Write(new LogEntry
                                                         {
                                                             Message = ex.Message,
                                                             Severity = TraceEventType.Verbose,
                                                         });
                                        return;
                                    }
                                }

                                // Performance counters may be created not immediately.
                                // There is a latency time to enable the counters.
                                int i = 0;
                                do
                                {
                                    Thread.Sleep(200);
                                    if (i++ > 10)
                                        throw new ApplicationException(
                                            string.Format(
                                                "PerformanceCounterCategory {0} was not created",
                                                ICBCategory));
                                } while (!PerformanceCounterCategory.Exists(ICBCategory));

                                foreach (CounterCreationData d in _countersDescriptor)
                                {
                                    _counters.Add(d.CounterName,
                                                  new PerfData(new PerformanceCounter
                                                                   {
                                                                       CategoryName =
                                                                           ICBCategory,
                                                                       CounterName = d.CounterName,
                                                                       ReadOnly = false,
                                                                       InstanceName = _instanceName,
                                                                       InstanceLifetime =
                                                                           PerformanceCounterInstanceLifetime
                                                                           .Global,
                                                                   }));
                                }
                                Clear();
                            }
                            catch (ThreadAbortException)
                            {
                                return;
                            }
                            finally
                            {
                                _needRegisteringCounters = true;
                            }
                        })
                                         {
                                             Name = "Registering Perf Counters",
                                             Priority = ThreadPriority.Lowest,
                                         };
                _registeringThread.Start();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (PerfData c in _counters.Values)
                c.Dispose();
        }

        #endregion

        /// <summary>
        /// start counting from the beginning
        /// </summary>
        public void Clear()
        {
            foreach (PerfData c in _counters.Values)
            {
                c.Clear();
            }
        }

        public long Count(string counterName)
        {
            return Count(counterName, false);
        }

        /// <param name="getNextSample">is needed for all history storing
        /// buy may occupy too many resources</param>
        /// <returns></returns>
        private long Count(string counterName, bool getNextSample)
        {
            try
            {
                if (_counters.ContainsKey(counterName))
                {
                    PerfData perfData = _counters[counterName];
                    if (perfData.Counter != null)
                    {
                        long? v = perfData.Max = perfData.Counter.Increment();
                        if (getNextSample)
                            perfData.Samples.Add(perfData.Counter.NextSample());
                        return (long) v;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogEntry
                                 {
                                     Message = ex.Message,
                                     Severity = TraceEventType.Verbose,
                                 });
            }
            return 0;
        }

        public void Count(string counterName, long rawValue)
        {
            try
            {
                if (_counters.ContainsKey(counterName))
                {
                    PerfData perfData = _counters[counterName];
                    if (perfData.Counter != null)
                    {
                        perfData.Counter.RawValue = rawValue;
                        if (perfData.Max == null || rawValue > perfData.Max)
                            perfData.Max = rawValue;
                        if (perfData.Min == null || rawValue < perfData.Min)
                            perfData.Min = rawValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogEntry
                                 {
                                     Message = ex.Message,
                                     Severity = TraceEventType.Verbose,
                                 });
            }
        }

        internal float Sample(string counterName)
        {
            PerfData perfData = _counters[counterName];
            return perfData != null
                       ? CounterSample.Calculate(perfData.Samples[0], perfData.Counter.NextSample())
                       : 0;
        }
    }
}