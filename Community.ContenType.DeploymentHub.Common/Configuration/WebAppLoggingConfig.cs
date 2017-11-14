using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    public class WebAppLoggingConfig
    {
        public static void RegisterAppenders()
        {
            var traceAppender = RegisterTraceAppender();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var root = hierarchy.Root;
            root.Level = LogConfigAppSettings.LogLevel;

            BasicConfigurator.Configure(traceAppender);
        }

        private static IAppender RegisterTraceAppender()
        {
            var layout = new PatternLayout(LogConfigAppSettings.ConsoleMessageLayout);
            layout.ActivateOptions();

            var traceAppender = new AspNetTraceAppender { Layout = layout };
            traceAppender.ActivateOptions();

            return traceAppender;
        }

    }
}
