using System.Collections.Generic;
using AppConfiguration;
using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class SettingsCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public SettingsCommand()
        {
        }

        public SettingsCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            KeyValuePair<string, object> pair;
            if (message is KeyValuePair<string, object>)
            {
                pair = (KeyValuePair<string, object>) message;
            }
            else
                pair = new KeyValuePair<string, object>(null, null);

            object remark = null;
            List<ConfigurationSetting> list = MainChannel.ServerSettings(pair.Key, pair.Value, ref remark);
            return new KeyValuePair<List<ConfigurationSetting>, object>(list, remark);
        }
    }
}