using System;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface IContentTypeSyncLogListRepository
    {
        void AddSyncLogFailEntry(string title, string message, string failureStage, string failureMessage, DateTime? eventDate, SiteCollection siteCollection);
        void AddAddSyncLogEntry(string title, string message, SiteCollection siteCollection);

    }
}