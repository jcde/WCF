using System;
using System.ServiceModel.Configuration;

namespace WcfServer.Debug
{
    /// <summary>
    /// used in app.config
    /// </summary>
    public class ConsoleOutputBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new ConsoleOutputBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ConsoleOutputBehavior);
            }
        }
    }
}