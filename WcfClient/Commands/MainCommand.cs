using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class MainCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public MainCommand()
        {
        }

        public MainCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            string method;
            object[] pars;
            if (message is string)
            {
                // example ListEntities=int:12;string:a
                var s = ((string) message).Split('=');
                method = s[0];
                var l = new List<object>();
                foreach (var p in s[1].Split(';'))
                {
                    var ss = p.Split(':');
                    object o;
                    switch (ss[0])
                    {
                        case "int":
                            o = int.Parse(ss[1]);
                            break;
                        default:
                            o = ss[1];
                            break;
                    }
                    l.Add(o);
                }
                pars = l.ToArray();
            }
            else
            {
                var pair = (KeyValuePair<string, object[]>) message;
                method = pair.Key;
                pars = pair.Value;
            }
            var r = MainChannel.GetType().GetMethod(method).Invoke(MainChannel, pars);
            if (SourceControl is ContentControl)
            {
                var cc = (ContentControl) SourceControl;
                cc.Dispatcher.Invoke(DispatcherPriority.Normal,
                                     (Action) delegate { cc.Content = r; });
            }
            return r;
        }
    }
}