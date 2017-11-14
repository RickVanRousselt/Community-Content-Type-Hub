using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Domain
{
    public class PublishedContentTypeList
    {
        private readonly ISet<PublishedContentType> _publishedContentTypes;

        public PublishedContentTypeList(ISet<PublishedContentType> publishedContentTypes)
        {
            _publishedContentTypes = publishedContentTypes;
        }

        public ISet<PublishedContentType> GetContentTypes(ISet<string> contentTypeIds)
        {
            return new HashSet<PublishedContentType>(_publishedContentTypes.Where(publishedContentType => contentTypeIds.Contains(publishedContentType.Id)));
        }
    }
}