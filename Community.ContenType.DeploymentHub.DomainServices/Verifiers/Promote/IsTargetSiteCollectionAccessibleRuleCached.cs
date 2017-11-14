using System;
using System.Collections.Concurrent;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class IsTargetSiteCollectionAccessibleRuleCached : IsTargetSiteCollectionAccessibleRule
    {
        private static readonly ConcurrentDictionary<Hub, bool> Cache = new ConcurrentDictionary<Hub, bool>();

        public IsTargetSiteCollectionAccessibleRuleCached(Func<SiteCollection, IUserRepository> userRepositoryGenerator) 
            : base(userRepositoryGenerator) { }

        protected override bool IsCurrentUserSiteAdmin(Hub target)
        {
            if (!Cache.ContainsKey(target))
            {
                var isAdmin = base.IsCurrentUserSiteAdmin(target);
                Cache.TryAdd(target, isAdmin);
            }
            return Cache[target];
        }
    }
}