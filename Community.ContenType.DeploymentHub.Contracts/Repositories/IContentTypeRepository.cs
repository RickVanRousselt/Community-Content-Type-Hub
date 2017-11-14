using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IContentTypeRepository
    {
        ISet<ContentTypeInfo> GetContentTypeInfos();
        ISet<ContentType> GetContentTypes(ISet<string> contentTypeIds);
        bool IsContentTypeSetToReadOnly(ContentTypeInfo contentType);
    }
}
