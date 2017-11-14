using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Community.ContenType.DeploymentHub.Common.Configuration
{
    public class AzureQueueLoggingConfig
    {
        public static IAppender RegisterAppender(TextWriter writer, Guid messageId)
        {
            ThreadContext.Properties[LogConfigAppSettings.AzureQueueMessageIdKey] = messageId.ToString();

            var appender = RegisterTextWriterAppender(writer, messageId);

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var root = hierarchy.Root;
            root.Level = LogConfigAppSettings.LogLevel;

            BasicConfigurator.Configure(appender);


            return appender;
        }

        private static IAppender RegisterTextWriterAppender(TextWriter writer, Guid messageId)
        {
            var messageLayout = new PatternLayout(LogConfigAppSettings.DefaultMessageLayout);
            messageLayout.ActivateOptions();

            var threadIdFilter = new PropertyFilter
            {
                AcceptOnMatch = true,
                Key = LogConfigAppSettings.AzureQueueMessageIdKey,
                StringToMatch = messageId.ToString()
            };
            threadIdFilter.ActivateOptions();

            var denyFilter = new DenyAllFilter();
            denyFilter.ActivateOptions();

            var textWriterAppender = new TextWriterAppender
            {
                Writer = writer,
                ImmediateFlush = true,
                Layout = messageLayout
            };
            textWriterAppender.AddFilter(threadIdFilter);
            textWriterAppender.AddFilter(denyFilter);
            textWriterAppender.ActivateOptions();

            return textWriterAppender;
        }

        public static void UnregisterAppender(IAppender appender)
        {
            var hierarchy = LogManager.GetRepository() as Hierarchy;
            if (hierarchy == null) return;

            IAppenderAttachable attachable = hierarchy.Root;
            attachable.RemoveAppender(appender);
        }
    }
}
