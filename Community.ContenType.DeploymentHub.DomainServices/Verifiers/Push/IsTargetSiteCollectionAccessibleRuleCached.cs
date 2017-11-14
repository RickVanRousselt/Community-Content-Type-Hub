using System;
using System.Collections.Concurrent;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Push
{
    public class IsTargetSiteCollectionAccessibleRuleCached : IsTargetSiteCollectionAccessibleRule
    {
        private static readonly ConcurrentDictionary<SiteCollection, bool> Cache = new ConcurrentDictionary<SiteCollection, bool>();

        public IsTargetSiteCollectionAccessibleRuleCached(Func<SiteCollection, IUserRepository> userRepositoryGenerator)
            : base(userRepositoryGenerator) { }

        protected override bool IsCurrentUserSiteAdmin(SiteCollection target)
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