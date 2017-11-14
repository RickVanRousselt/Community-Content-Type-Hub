using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security;

namespace Community.ContenType.DeploymentHub.Factories.Settings
{
    public interface ISettingsProvider
    {
        int LogLevel { get; }
        string AzureStorageConnectionString { get; }
        MailAddress User { get; }
        SecureString Password { get; }
        MailAddress PromoteUser { get; }
        SecureString PromotePassword { get; }
        IEnumerable<string> PublishActiveRuleNames { get; }
        IEnumerable<string> PushActiveRuleNames { get; }
        IEnumerable<string> PromoteActiveRuleNames { get; }
        string SmtpServer { get; }
        int SmtpPort { get; }
        MailAddress MailUser { get; }
        SecureString MailPassword { get; }
        bool UseXmlUpdater { get; }
        int NumberOfJobThreads { get; }
        int NumberOfJobRetries { get; }
        TimeSpan MaxPollingInterval { get; }
        bool UseDispatchQueue { get; }
        int NumberOfThreads { get; }
        bool ThrowOnExceptions { get; }
    }
}