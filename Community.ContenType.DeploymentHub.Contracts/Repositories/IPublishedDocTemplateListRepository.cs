using System;
using System.IO;
using Community.ContenType.DeploymentHub.Domain.Core;
using Strilanc.Value;
using ContentType = Community.ContenType.DeploymentHub.Domain.Core.ContentType;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IPublishedDocTemplateListRepository
    {
        void EnsureDocTemplateLib();
        void PublishDocumentTemplate(ContentType contentType);
        May<Tuple<string, Stream>> GetTemplate(ContentTypeInfo contentType);
    }
}