using System;
using Community.ContenType.DeploymentHub.Domain.Core;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.Contracts.Repositories
{
    public interface ITermStoreRepository
    {
        Guid EnsureDeploymentGroupsTermSet();
        May<TermPath> MaybeGetTermPathToTermIdentifier(TermIdentifier identifier);
        May<TermIdentifier> MaybeGetTermIdentifierByPath(TermPath path);
        DeploymentGroup GetDeploymentGroupById(Guid deploymentGroupId);
        May<Guid> MaybeGetNewSspId(Guid sspId);
    }
}