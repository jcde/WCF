using System;
using System.Collections.Generic;
using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    /// <summary>
    /// calls commands without CommandBinding
    /// </summary>
    public class CommandsManager<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public ClientInstance<MT, T> Client;

        public MT MainChannel
        {
            get { return Client.MainChannel; }
        }

        public virtual Type CommandType(ComEnum en)
        {
            switch (en)
            {
                case ComEnum.Connect:
                    return typeof(ConnectCommand<MT, T>);
                case ComEnum.Disconnect:
                    return typeof(DisconnectCommand<MT, T>);
                case ComEnum.Kill:
                    return typeof(KillCommand<MT, T>);
                case ComEnum.General:
                    return typeof(GeneralCommand<MT, T>);
                case ComEnum.Main:
                    return typeof(MainCommand<MT, T>);
                case ComEnum.Broadcast:
                    return typeof(BroadcastCommand<MT, T>);
                case ComEnum.Settings:
                    return typeof(SettingsCommand<MT, T>);
                case ComEnum.Users:
                    return typeof(UsersCommand<MT, T>);
                case ComEnum.Time:
                    return typeof(TimeGetSetCommand<MT, T>);
            }
            return null;
        }

        public ClientCommand<MT, T> Command(ComEnum en, bool noNewBinding)
        {
            Type t = CommandType(en);
            if (t != null)
            {
                var com = Activator.CreateInstance(t, new object[] { noNewBinding }) as ClientCommand<MT, T>;
                return com;
            }
            return null;
        }

        /// <summary>
        /// synchronous execution
        /// it is recommended for massive calls from user interface 
        /// </summary>
        public object Execute(Type commandType)
        {
            return Execute(commandType, null);
        }

        public object Execute(ComEnum en)
        {
            return Execute(CommandType(en));
        }

        public object Execute(ComEnum en, object par)
        {
            return Execute(CommandType(en), par);
        }

        public object Execute(Type commandType, object par)
        {
            var com = Activator.CreateInstance(commandType, new object[] { true }) as ClientCommand<MT, T>;
            if (com != null)
                return com.ExecuteSafe(Client, par);
            return string.Format("Command {0} dismatch client {1}", commandType.Name, Client);
        }

        public object ExecuteGeneralCommand(GeneralCommandType type)
        {
            return ExecuteGeneralCommand(type, null);
        }

        public object ExecuteGeneralCommand(GeneralCommandType type, string data)
        {
            return Execute(ComEnum.General, new KeyValuePair<GeneralCommandType, string>(type, data));
        }

        public string ExecuteIfCan(Type commandType)
        {
            var com = Activator.CreateInstance(commandType, new object[] { true }) as ClientCommand<MT, T>;
            if (com != null
                && com.CanExecuteCommand(Client, null))
            {
                var r = com.ExecuteSafe(Client, null);
                return r != null ? r.ToString() : null;
            }
            return "Cannot execute Command";
        }
    }
}