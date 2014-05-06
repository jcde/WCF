namespace WcfDomain.Contracts
{
    public enum GeneralCommandType
    {
        /// <summary>
        /// set/get time in local timezone
        /// </summary>
        TimeRequest = 10,

        /// <summary>
        /// logs <message> to Server
        /// </summary>
        Log = 13,

        /// <summary>
        /// checks connection, keeps alive
        /// </summary>
        Ping = 10000,

        /// <summary>
        /// parameter name - CNString, Database, DBServer
        /// </summary>
        SettingGetRequest = 42,

        /// <summary>
        /// parameter name
        /// </summary>
        SettingDefaultRequest = 40,

        VersionInfoRequest = 58,

        RestartServer = 777,
    }
}