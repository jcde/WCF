using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace WcfServer.Debug
{
    public class ConsoleOutputMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            request = buffer.CreateMessage();
            Console.WriteLine("Received:\n{0}", buffer.CreateMessage());
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
            reply = buffer.CreateMessage();
            Console.WriteLine("Sending:\n{0}", buffer.CreateMessage());
        }
    }
}
