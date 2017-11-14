using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    /// <summary>
    /// A WebJob basically is a console application run from Azure, therefore we only register a basic console appender
    /// The result of the console logging can be downloaded through the Azure Portal
    /// </summary>
    public class WebJobLoggingConfig
    {
        public static void RegisterAppender()
        {

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var root = hierarchy.Root;
            root.Level = LogConfigAppSettings.LogLevel;

            RegisterConsoleAppender();
        }

        private static void RegisterConsoleAppender()
        {
            //We use the DefaultMessageLayout, cause this will render a console logging that can be downloaded and easily read as a csv file
            //If no DefaultMessageLayout is specified in Application Settings, the layout will default to a layout that is ; seperated
            var layout = new PatternLayout(LogConfigAppSettings.DefaultMessageLayout);
            layout.ActivateOptions();

            var consoleAppender = new ConsoleAppender
            {
                Layout = layout
            };
            consoleAppender.ActivateOptions();

            BasicConfigurator.Configure(consoleAppender);
        }
    }
}
