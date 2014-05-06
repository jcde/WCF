using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace WcfClient.ErrorsHandling
{
    /// <summary>
    /// converts server fault into real exception thrown on client
    /// may not be able to deserialize service exception if its type is not available on the client side. 
    ///     In this case, it will allow WCF to perform standard fault handling.
    /// uses ErrorsHandling.ErrorHandler 
    /// </summary>
    public class ExceptionMarshallingMessageInspector : IClientMessageInspector
    {
        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                // Create a copy of the original reply to allow default WCF processing
                MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                Message copy = buffer.CreateMessage(); // Create a copy to work with
                reply = buffer.CreateMessage(); // Restore the original message 

                object faultDetail = ReadFaultDetail(copy);
                var exception = faultDetail as Exception;
                if (exception != null)
                    throw exception;
            }
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }

        #endregion

        private static object ReadFaultDetail(Message reply)
        {
            const string detailElementName = "Detail";
            const string reasonElementName = "Reason";
            string reason = null;
            
            using (XmlDictionaryReader reader = reply.GetReaderAtBodyContents())
            {
                // Find <soap:Reason>
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == reasonElementName)
                    {
                        // Find <soap:Text>
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Text")
                            {
                                reader.Read();
                                reason = reader.Value;
                                break;
                            }
                        }
                        break;
                    }
                }

                // Find <soap:Detail>
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == detailElementName)
                    {
                        break;
                    }
                }

                // Did we find it?
                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != detailElementName)
                {
                    return null;
                }

                // Move to the contents of <soap:Detail>
                if (!reader.Read())
                {
                    return null;
                }

                // Deserialize the fault
                var serializer = new NetDataContractSerializer();
                try
                {
                    return serializer.ReadObject(reader, false);
                }
                catch (SerializationException ex)
                {
                    throw reason != null
                              ? new SerializationException(reason)
                              : ex;
                }
                catch (FileNotFoundException)
                {
                    // Serializer was unable to find assembly where exception is defined 
                    return null;
                }
            }
        }
    }
}

/*
{<s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope" xmlns:a="http://www.w3.org/2005/08/addressing">
  <s:Header>
    <a:Action s:mustUnderstand="1">http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault</a:Action>
    <a:RelatesTo>urn:uuid:ab8da88f-f88a-4d05-8c1b-6b120bc45528</a:RelatesTo>
    <a:To s:mustUnderstand="1">http://www.w3.org/2005/08/addressing/anonymous</a:To>
  </s:Header>
  <s:Body>
    <s:Fault>
      <s:Code>
        <s:Value>s:Receiver</s:Value>
        <s:Subcode>
          <s:Value xmlns:a="http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher">a:InternalServiceFault</s:Value>
        </s:Subcode>
      </s:Code>
      <s:Reason>
        <s:Text xml:lang="en-AU">Could not find file 'D:\NumSite\Sources\Projects\foreign\Odesk\ICB\trunk\Server VS Solution\ICB.ServerHost\bin\Debug\ICB.EmailModule.dll.config'.</s:Text>
      </s:Reason>
      <s:Detail>
        <ExceptionDetail xmlns="http://schemas.datacontract.org/2004/07/System.ServiceModel" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
          <HelpLink i:nil="true">
          </HelpLink>
          <InnerException i:nil="true">
          </InnerException>
          <Message>Could not find file 'D:\NumSite\Sources\Projects\foreign\Odesk\ICB\trunk\Server VS Solution\ICB.ServerHost\bin\Debug\ICB.EmailModule.dll.config'.</Message>
          <StackTrace>   at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
   at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy)
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize)
   at System.Xml.XmlDownloadManager.GetStream(Uri uri, ICredentials credentials)
   at System.Xml.XmlUrlResolver.GetEntity(Uri absoluteUri, String role, Type ofObjectToReturn)
   at System.Xml.XmlTextReaderImpl.OpenUrlDelegate(Object xmlResolver)
   at System.Threading.CompressedStack.runTryCode(Object userData)
   at System.Runtime.CompilerServices.RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(TryCode code, CleanupCode backoutCode, Object userData)
   at System.Threading.CompressedStack.Run(CompressedStack compressedStack, ContextCallback callback, Object state)
   at System.Xml.XmlTextReaderImpl.OpenUrl()
   at System.Xml.XmlTextReaderImpl.Read()
   at System.Xml.XmlLoader.Load(XmlDocument doc, XmlReader reader, Boolean preserveWhitespace)
   at System.Xml.XmlDocument.Load(XmlReader reader)
   at System.Xml.XmlDocument.Load(String filename)
   at AppConfiguration.ConfigurationWriter.WriteApplicationSetting(String appConfigPath, String settingName, Object settingValue, Boolean isConnection) in D:\NumSite\Sources\Core\AppConfiguration\ConfigurationWriter.cs:line 60
   at ICB.WcfServer.Services.DuplexService`1.SaveSetting(String configFile, String settingName, Object value, Boolean isConnection) in D:\NumSite\Sources\Projects\foreign\Odesk\ICB\trunk\Libraries\ICB.WcfServer\Services\DuplexService.cs:line 539
   at SyncInvokeSaveSetting(Object , Object[] , Object[] )
   at System.ServiceModel.Dispatcher.SyncMethodInvoker.Invoke(Object instance, Object[] inputs, Object[]&amp; outputs)
   at System.ServiceModel.Dispatcher.DispatchOperationRuntime.InvokeBegin(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage5(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage4(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage3(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage2(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage1(MessageRpc&amp; rpc)
   at System.ServiceModel.Dispatcher.MessageRpc.Process(Boolean isOperationContextSet)</StackTrace>
          <Type>System.IO.FileNotFoundException</Type>
        </ExceptionDetail>
      </s:Detail>
    </s:Fault>
  </s:Body>
</s:Envelope>*/
