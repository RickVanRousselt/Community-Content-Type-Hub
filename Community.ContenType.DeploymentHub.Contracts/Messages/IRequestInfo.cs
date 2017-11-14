using System;
using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Contracts.Messages
{
    public interface IRequestInfo
    {
        int StatusListItemId { get; }
        Uri Hub { get; }
        string InitiatingUser { get; set; }
        Guid ActionCollectionId { get; set; }
        bool EnableVerifiers { get; set; }

        ActionContext GetActionContext();
    }
}