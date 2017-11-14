using Community.ContenType.DeploymentHub.Domain.Actions;

namespace Community.ContenType.DeploymentHub.Domain.Requests
{
    public interface IRequest
    {
        ActionContext ActionContext { get; }
    }
}