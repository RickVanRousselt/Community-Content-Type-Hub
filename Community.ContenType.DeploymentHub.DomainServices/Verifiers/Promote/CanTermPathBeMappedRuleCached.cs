using System;
using System.Collections.Concurrent;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class CanTermPathBeMappedRuleCached : CanTermPathBeMappedRule
    {
        private static readonly ConcurrentDictionary<Tuple<Hub, TermPath>, bool> TermPathCache = new ConcurrentDictionary<Tuple<Hub, TermPath>, bool>();

        public CanTermPathBeMappedRuleCached(Func<SiteCollection, ITermStoreRepository> termStoreRepositoryGenerator) 
            : base(termStoreRepositoryGenerator) { }

        protected override bool IsValidTermPath(Hub targetHub, TermPath termPath)
        {
            var key = Tuple.Create(targetHub, termPath);
            if (!TermPathCache.ContainsKey(key))
            {
                var isValid = base.IsValidTermPath(targetHub, termPath);
                TermPathCache.TryAdd(key, isValid);
            }
            return TermPathCache[key];
        }
    }
}