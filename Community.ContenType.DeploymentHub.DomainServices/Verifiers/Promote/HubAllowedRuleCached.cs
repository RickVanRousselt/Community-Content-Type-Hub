using System;
using System.Collections.Concurrent;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.DomainServices.Verifiers.Promote
{
    public class HubAllowedRuleCached : HubAllowedRule
    {
        private static readonly ConcurrentDictionary<SiteCollection, Hub> CachedPushSources = new ConcurrentDictionary<SiteCollection, Hub>();

        public HubAllowedRuleCached(Func<SiteCollection, IWebPropertyRepository> webPropertyRepositoryGenerator) 
            : base(webPropertyRepositoryGenerator) { }

        protected override May<Hub> MaybeGetPushSource(SiteCollection target)
        {
            if (CachedPushSources.ContainsKey(target)) return CachedPushSources[target];

            var maybePushSource = base.MaybeGetPushSource(target);

            maybePushSource.IfHasValueThenDo(s => CachedPushSources.TryAdd(target, s));
            maybePushSource.ElseDo(() => { /* do not cache result if no value, will probably be set soon anyway */ });
            return maybePushSource;
        }
    }
}