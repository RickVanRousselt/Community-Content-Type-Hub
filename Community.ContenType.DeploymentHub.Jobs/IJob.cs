using Community.ContenType.DeploymentHub.Contracts.Messages;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public interface IJob<in TRequestInfo> where TRequestInfo : IRequestInfo
    {
        void Run(TRequestInfo info);
    }
}