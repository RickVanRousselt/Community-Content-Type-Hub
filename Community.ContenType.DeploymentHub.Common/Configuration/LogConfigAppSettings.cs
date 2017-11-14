using System.Configuration;
using log4net.Core;

namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    public static class LogConfigAppSettings
    {
        internal static bool EnableStopWatch
        {
            get
            {
                var enableValue = ConfigurationManager.AppSettings[AppSettingsKeys.EnableStopwatch];
                bool enable;
                bool.TryParse(enableValue, out enable);
                return !string.IsNullOrWhiteSpace(enableValue) && enable;
            }
        }
        internal static Level LogLevel
        {
            get
            {
                var logLevelValue = ConfigurationManager.AppSettings[AppSettingsKeys.LogLevel];
                return string.IsNullOrWhiteSpace(logLevelValue)
                    ? Level.Info
                    : logLevelValue.ToLevel();
            }
        }

        internal static string DefaultMessageLayout
        {
            get
            {
                var layoutValue = ConfigurationManager.AppSettings[AppSettingsKeys.DefaultMessageLayout];
                return string.IsNullOrWhiteSpace(layoutValue)
                    ? "%5level;%date;[%thread];[%identity];%logger;%message%newline"
                    : layoutValue;
            }
        }

        internal static string ConsoleMessageLayout
        {
            get
            {
                var layoutValue = ConfigurationManager.AppSettings[AppSettingsKeys.ConsoleMessageLayout];
                return string.IsNullOrWhiteSpace(layoutValue)
                    ? DefaultMessageLayout
                    : layoutValue;
            }
        }

        internal static string MessageHeader
        {
            get
            {
                var layoutValue = ConfigurationManager.AppSettings[AppSettingsKeys.MessageHeader];
                return string.IsNullOrWhiteSpace(layoutValue)
                    ? "level;timestamp;thread;identity;logger;message&#13;&#10;"
                    : layoutValue;
            }
        }

        internal static int CleanupLogFilesAfterDays
        {
            get
            {
                var value = ConfigurationManager.AppSettings[AppSettingsKeys.CleanupLogFilesAfterDays];
                int cleanupValue;
                
                //fallback to 30 if no value or invalid value specified
                if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out cleanupValue)) return 30;

                return cleanupValue;
            }
        }

        public static string AzureQueueMessageIdKey
        {
            get
            {
                var value = ConfigurationManager.AppSettings[AppSettingsKeys.AzureQueueMessageIdKey];
                return string.IsNullOrWhiteSpace(value)
                    ? "MessageId"
                    : value;
            }
        }
    }
}
