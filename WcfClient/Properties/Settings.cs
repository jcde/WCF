using System.Collections.Generic;
using System.Configuration;

namespace WcfClient.Properties
{
    [SettingsProvider(typeof (ClientSettingsProvider))]
    public partial class Settings
    {
        private static bool _clientSpecificMode;

        internal readonly Dictionary<SettingsProperty, object> _values
            = new Dictionary<SettingsProperty, object>();

        /// <summary>
        /// indicates that there are many clients in the same process with different settings
        /// </summary>
        public static bool ClientSpecificMode
        {
            get { return _clientSpecificMode; }
            internal set
            {
                // we can only set this mode
                if (!_clientSpecificMode && value)
                {
                    _clientSpecificMode = true;
                    Default.InitValues();
                }
            }
        }

        internal void InitValues()
        {
            Context.Add(ClientSettingsProvider.ValuesName, _values);
        }
    }

    public class ClientSettingsProvider : LocalFileSettingsProvider
    {
        internal const string ValuesName = "ValuesName";

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
                                                                          SettingsPropertyCollection collection)
        {
            if (Settings.ClientSpecificMode)
            {
                var values = (Dictionary<SettingsProperty, object>) context[ValuesName];
                var result = new SettingsPropertyValueCollection();

                foreach (SettingsProperty setting in collection)
                {
                    object propertyValue = values.ContainsKey(setting)
                                               ? values[setting]
                                               : base.GetPropertyValues(context,
                                                                        new SettingsPropertyCollection {setting})[
                                                     setting.Name].PropertyValue;
                    var value = new SettingsPropertyValue(setting)
                                    {
                                        IsDirty = false,
                                        PropertyValue = propertyValue
                                    };
                    result.Add(value);
                }
                return result;
            }
            try
            {
                return base.GetPropertyValues(context, collection);
            }
            catch (ConfigurationErrorsException)
            {
                //A user-scoped setting was encountered but the current configuration only supports application-scoped settings.
                //example - Asp.Net web-site

                foreach (SettingsProperty setting in collection)
                {
                    setting.Attributes.Remove(typeof (UserScopedSettingAttribute));
                    setting.Attributes.Add(typeof (ApplicationScopedSettingAttribute),
                                           new ApplicationScopedSettingAttribute());
                }
                return base.GetPropertyValues(context, collection);
            }
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            if (Settings.ClientSpecificMode)
            {
                var values = (Dictionary<SettingsProperty, object>) context[ValuesName];
                foreach (SettingsPropertyValue propval in collection)
                    if (propval.IsDirty)
                        values[propval.Property] = propval.PropertyValue;
            }
            else
            {
                try
                {
                    base.SetPropertyValues(context, collection);
                }
                catch(ConfigurationErrorsException)
                {
                }
            }
        }
    }
}