using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace WcfServer.Requests
{
    public class HttpUserAgentMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        public const string USER_AGENT_HTTP_HEADER = "user-agent";

        private readonly string m_userAgent;

        public HttpUserAgentMessageInspector(string userAgent)
        {
            m_userAgent = userAgent;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                var userAgent = httpRequestMessage.Headers[USER_AGENT_HTTP_HEADER];
                if (!string.IsNullOrEmpty(userAgent))
                {
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[USER_AGENT_HTTP_HEADER]))
                {
                    httpRequestMessage.Headers[USER_AGENT_HTTP_HEADER] = m_userAgent;
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(USER_AGENT_HTTP_HEADER, m_userAgent);
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
    }
}