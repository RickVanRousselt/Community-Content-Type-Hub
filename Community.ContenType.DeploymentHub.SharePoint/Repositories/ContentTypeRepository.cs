using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using ContentType = Community.ContenType.DeploymentHub.Domain.Core.ContentType;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class ContentTypeRepository : IContentTypeRepository
    {
        private readonly IClientContextProvider _clientContextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentTypeRepository));

        public ContentTypeRepository(IClientContextProvider clientContextProvider)
        {
            _clientContextProvider = clientContextProvider;
        }

        public ISet<ContentTypeInfo> GetContentTypeInfos()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var contentTypes = clientContext.Site.RootWeb.ContentTypes;
                clientContext.Load(contentTypes, cts => cts.Include(ct => ct.StringId, ct => ct.Name));
                clientContext.ExecuteQueryWithIncrementalRetry();

                return new HashSet<ContentTypeInfo>(contentTypes.Select(ct => new ContentTypeInfo(ct.Name, ct.StringId)));
            }
        }

        public ISet<ContentType> GetContentTypes(ISet<string> contentTypeIds)
        {
            using (Logger.MethodTraceLogger(contentTypeIds))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var contentTypes = contentTypeIds.Select(x => clientContext.Site.RootWeb.ContentTypes.GetById(x)).ToList();
                foreach (var contentType in contentTypes)
                {
                    clientContext.Load(contentType, ct => ct.Id, ct => ct.Name, ct => ct.DocumentTemplate, ct => ct.Group, ct => ct.Hidden, ct => ct.ReadOnly, ct => ct.SchemaXmlWithResourceTokens, ct => ct.DocumentTemplateUrl);
                }
                clientContext.ExecuteQueryWithIncrementalRetry();
                return new HashSet<ContentType>(contentTypes.Select(x => new ContentType(x.Name, x.Id.StringValue, x.DocumentTemplate, x.Group, x.Hidden, x.ReadOnly, XDocument.Parse(x.SchemaXmlWithResourceTokens), x.DocumentTemplateUrl)));
            }
        }

        public bool IsContentTypeSetToReadOnly(ContentTypeInfo contentType)
        {
            using (Logger.MethodTraceLogger(contentType))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                try
                {
                    var cts = clientContext.Site.RootWeb.ContentTypes;
                    var ct = cts.GetById(contentType.Id);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    if(ct.ServerObjectIsNull()) return true;
                    clientContext.Load(ct, w => w.ReadOnly);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    return ct.ReadOnly;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error checking if content type is readonly", ex);
                    return false;
                }
            }
        }
    }
}