using System;

namespace WcfClient
{
    public class FinishWaitTimeoutException : ApplicationException
    {
        public FinishWaitTimeoutException() { }
        public FinishWaitTimeoutException(string m) : base(m) { }
    }
}