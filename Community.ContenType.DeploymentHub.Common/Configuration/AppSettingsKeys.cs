namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    internal static class AppSettingsKeys
    {
        public const string LogLevel = "log4net.Level";
        public const string EnableStopwatch = "log4net.EnableStopwatch";
        public const string DefaultMessageLayout = "log4net.DefaultMessageLayout";
        public const string ConsoleMessageLayout = "log4net.ConsoleMessageLayout";
        public const string MessageHeader = "log4net.MessageHeader";
        public const string AzureQueueMessageIdKey = "log4net.AzureQueueMessageIdKey";
        public const string CleanupLogFilesAfterDays = "log4net.CleanupLogFilesAfterDays";
    }
}
