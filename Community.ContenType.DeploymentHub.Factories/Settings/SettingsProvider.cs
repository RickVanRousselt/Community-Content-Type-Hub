using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Security;


namespace Community.ContenType.DeploymentHub.Factories.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
       
     
        public int LogLevel => Convert.ToInt32(ConfigurationManager.AppSettings["LogLevel"]);
        public string AzureStorageConnectionString => ConfigurationManager.AppSettings["AzureStorageConnectionString"];
        public MailAddress User => new MailAddress(ConfigurationManager.AppSettings["FunctionalUserIdCurrent"]);
        public SecureString Password => ToSecureString(ConfigurationManager.AppSettings["FunctionalPasswordCurrent"]);
        public MailAddress PromoteUser => new MailAddress(ConfigurationManager.AppSettings["FunctionalUserIdPromote"]);
        public SecureString PromotePassword => ToSecureString(ConfigurationManager.AppSettings["FunctionalPasswordPromote"]);
        public IEnumerable<string> PublishActiveRuleNames => ConfigurationManager.AppSettings.Get("ActivePublishRules").Split(';');
        public IEnumerable<string> PushActiveRuleNames => ConfigurationManager.AppSettings.Get("ActivePushRules").Split(';');
        public IEnumerable<string> PromoteActiveRuleNames => ConfigurationManager.AppSettings.Get("ActivePromoteRules").Split(';');
        public string SmtpServer => ConfigurationManager.AppSettings["SmtpServer"];
        public int SmtpPort => Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
        public MailAddress MailUser => new MailAddress(ConfigurationManager.AppSettings["MailUser"]);
        public SecureString MailPassword => ToSecureString(ConfigurationManager.AppSettings["MailUserEncryptedPassword"]);
        public bool UseXmlUpdater => bool.Parse(ConfigurationManager.AppSettings["UseXmlUpdater"]);
        public bool UseDispatchQueue => bool.Parse(ConfigurationManager.AppSettings["UseDispatchQueue"]);
        public int NumberOfJobThreads => Convert.ToInt32(ConfigurationManager.AppSettings.Get("NumberOfJobThreads"));
        public int NumberOfJobRetries => Convert.ToInt32(ConfigurationManager.AppSettings.Get("NumberOfJobRetries"));
        public TimeSpan MaxPollingInterval => TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings.Get("MaxPollingInterval")));
        public int NumberOfThreads => Convert.ToInt32(ConfigurationManager.AppSettings.Get("NumberOfThreads"));

        public bool ThrowOnExceptions => bool.Parse(ConfigurationManager.AppSettings["ThrowOnExceptions"]);


        private SecureString ToSecureString(string password)
        {
            var secure = new SecureString();
            foreach (char c in password)
            {
                secure.AppendChar(c);
            }
            return secure;
        }
    }
}