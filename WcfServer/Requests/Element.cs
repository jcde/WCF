using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfServer.Requests
{
    public class HttpUserAgentEndpointBehavior : IEndpointBehavior
    {
        private readonly string m_userAgent;

        public HttpUserAgentEndpointBehavior(string userAgent)
        {
            m_userAgent = userAgent;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (!string.IsNullOrEmpty(m_userAgent))
            {
                var inspector = new HttpUserAgentMessageInspector(m_userAgent);
                clientRuntime.MessageInspectors.Add(inspector);
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            var inspector = new HttpUserAgentMessageInspector(null);
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }

    public class HttpUserAgentBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof (HttpUserAgentEndpointBehavior); }
        }

        [ConfigurationProperty("userAgent", IsRequired = false)]
        public string UserAgent
        {
            get { return (string) base["userAgent"]; }
            set { base["userAgent"] = value; }
        }

        protected override object CreateBehavior()
        {
            return new HttpUserAgentEndpointBehavior(UserAgent);
        }
    }
}