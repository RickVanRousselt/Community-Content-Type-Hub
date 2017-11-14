using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IPublishedContentTypesListRepository : IPublishedListRepository<PublishedContentType>
    {
        void EnsurePublishedContentTypesLibrary();
        void UpdateView();
        PublishedContentType PublishContentType(ContentType contentType);
        PublishedContentTypeList GetList();
        May<PublishedContentType> GetPublishedContentType(ContentType contentType);
    }
}