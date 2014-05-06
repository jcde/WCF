using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class GeneralCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public GeneralCommand()
        {
        }

        public GeneralCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            if (message is GeneralCommandType)
            {
                var type = (GeneralCommandType) message;
                var cc = SourceControl as ContentControl;
                string data = null;
                if (cc != null)
                    cc.Dispatcher.Invoke(DispatcherPriority.Normal,
                                         (Action) delegate { data = cc.Name; });

                object r = GeneralMessage(type, data);

                if (r != null && cc != null)
                    cc.Dispatcher.Invoke(DispatcherPriority.Normal,
                                         (Action) delegate { cc.Content = r; });
            }
            if (message is KeyValuePair<GeneralCommandType, string>)
            {
                var pair = (KeyValuePair<GeneralCommandType, string>) message;
                return GeneralMessage(pair.Key, pair.Value);
            }
            return null;
        }

        private object GeneralMessage(GeneralCommandType type, string data)
        {
            object r;
            CheckError(MainChannel.GeneralMessage(type, data, out r));
            return r;
        }
    }
}