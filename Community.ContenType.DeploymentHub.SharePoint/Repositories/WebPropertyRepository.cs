using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class WebPropertyRepository : IWebPropertyRepository
    {
        private readonly IClientContextProvider _clientContextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WebPropertyRepository));

        public WebPropertyRepository(IClientContextProvider clientContextProvider)
        {
            _clientContextProvider = clientContextProvider;
        }

        public Dictionary<string, object> GetProperties()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var properties = clientContext.Site.RootWeb.AllProperties;
                clientContext.Load(properties);
                clientContext.ExecuteQueryWithIncrementalRetry();
                return properties.FieldValues;
            }
        }

        public May<T> MaybeGetProperty<T>(string key)
        {
            using (Logger.MethodTraceLogger(key))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                    var properties = clientContext.Site.RootWeb.AllProperties;
                    clientContext.Load(properties);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    return properties.FieldValues.ContainsKey(key)
                        ? ((T)properties[key]).Maybe()
                        : May.NoValue;
            }
        }

        public void SetProperty(string key, object value)
        {
            using (Logger.MethodTraceLogger(key,value))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                web.AllProperties[key] = value;
                web.Update();
                clientContext.ExecuteQueryWithIncrementalRetry();
            }
        }
    }
}
