using System;
using System.IO;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    public class BatchLoggingConfig
    {
        public static void RegisterAppenders(string logFilePath, string baseFileName)
        {

            IAppender debugAppender = null;
            if (LogConfigAppSettings.LogLevel == Level.Debug) debugAppender = RegisterDebugAppender(logFilePath, baseFileName);
            IAppender infoAppender = null;
            if (LogConfigAppSettings.LogLevel <= Level.Info) infoAppender = RegisterInfoAppender(logFilePath, baseFileName);
            var warningsAndErrorsAppender = RegisterWarningsAndErrorsAppender(logFilePath, baseFileName);
            var consoleAppender = RegisterConsoleAppender();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var root = hierarchy.Root;
            root.Level = LogConfigAppSettings.LogLevel;
            root.RemoveAllAppenders();

            if (LogConfigAppSettings.LogLevel == Level.Debug && debugAppender != null) BasicConfigurator.Configure(debugAppender);
            if (LogConfigAppSettings.LogLevel <= Level.Info && infoAppender != null) BasicConfigurator.Configure(infoAppender);
            BasicConfigurator.Configure(warningsAndErrorsAppender);
            BasicConfigurator.Configure(consoleAppender);
        }

        private static IAppender RegisterDebugAppender(string logFilePath, string baseFileName)
        {
            var appender = RegisterFileAppender(logFilePath, baseFileName, "debug.log");

            appender.ActivateOptions();
            return appender;
        }

        private static IAppender RegisterInfoAppender(string logFilePath, string baseFileName)
        {
            var appender = RegisterFileAppender(logFilePath, baseFileName, "info.log");

            var levelRangeFilter = new LevelRangeFilter
            {
                AcceptOnMatch = false,
                LevelMax = Level.Fatal,
                LevelMin = Level.Info
            };
            levelRangeFilter.ActivateOptions();
            appender.AddFilter(levelRangeFilter);
            appender.ActivateOptions();

            return appender;
        }

        private static IAppender RegisterWarningsAndErrorsAppender(string logFilePath, string baseFileName)
        {
            var appender = RegisterFileAppender(logFilePath, baseFileName, "errors.log");

            var levelRangeFilter = new LevelRangeFilter
            {
                AcceptOnMatch = false,
                LevelMax = Level.Fatal,
                LevelMin = Level.Warn
            };
            levelRangeFilter.ActivateOptions();
            appender.AddFilter(levelRangeFilter);
            appender.ActivateOptions();

            return appender;
        }

        private static FileAppender RegisterFileAppender(string filePath, string baseFileName, string logTypeName)
        {
            var layout = new PatternLayout(LogConfigAppSettings.DefaultMessageLayout)
            {
                Header = LogConfigAppSettings.MessageHeader
            };
            layout.ActivateOptions();
            var appender = new FileAppender
            {
                AppendToFile = true,
                LockingModel = new FileAppender.MinimalLock(),
                Encoding = Encoding.UTF8,
                File = Path.Combine(filePath, $"{DateTime.Now:yyyy-MM-dd_hhmmss}_{baseFileName}.{logTypeName}.csv"),
                Layout = layout,
                Name = $"{baseFileName}.{logTypeName}"
            };
            return appender;
        }

        private static IAppender RegisterConsoleAppender()
        {
            var layout = new PatternLayout(LogConfigAppSettings.ConsoleMessageLayout);
            layout.ActivateOptions();
            var appender = new ColoredConsoleAppender()
            {

                Layout = layout,
                Threshold = Level.Info
            };

            appender.AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Debug,
                ForeColor = ColoredConsoleAppender.Colors.Cyan | ColoredConsoleAppender.Colors.HighIntensity
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Info,
                ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Warn,
                ForeColor = ColoredConsoleAppender.Colors.Yellow | ColoredConsoleAppender.Colors.HighIntensity
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Error,
                ForeColor = ColoredConsoleAppender.Colors.Red | ColoredConsoleAppender.Colors.HighIntensity
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Fatal,
                ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity,
                BackColor = ColoredConsoleAppender.Colors.Red
            });

            appender.ActivateOptions();

            return appender;
        }
    }
}
