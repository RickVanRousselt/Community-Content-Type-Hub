using System.Collections.Generic;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IPublishedListRepository<out T>
    {
        IEnumerable<T> GetAllPublishedItemsNoSchema();
        IEnumerable<T> GetAllPublishedItemsWithSchema();
    }
}