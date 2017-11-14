using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class ConfigurationListRepository : ListRepository, IConfigurationListRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigurationListRepository));


        public ConfigurationListRepository(IClientContextProvider clientContextProvider) 
            : base(clientContextProvider) { }

        public void EnsureConfigurationList()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var web = clientContext.Site.RootWeb;
                if (!ListExists(web, Lists.Configuration))
                {
                    var list = CreateGenericList(web, Lists.Configuration, "Configuration List of Community Content Type Hub");
                    RenameField(list, "Title", "Key");
                    AddField(list, "Value", "Value", "Text", true,false);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
            }
        }

        public Dictionary<string, string> GetConfigs()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = ClientContextProvider.CreateClientContext())
            {
                var configList = clientContext.Site.RootWeb.Lists.GetByTitle(Lists.Configuration);
                var listItems = configList.GetItems(CamlQuery.CreateAllItemsQuery());
                clientContext.Load(listItems, items => items.Include(li => li["Title"], li => li["Value"]));
                clientContext.ExecuteQueryWithIncrementalRetry();

                return listItems.ToDictionary(li => li["Title"].ToString(), li => li["Value"].ToString());
            }
        }
    }
}