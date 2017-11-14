using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Community.ContenType.DeploymentHub.Common.Contracts;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Strilanc.Value;

namespace Community.ContenType.DeploymentHub.SharePoint.Repositories
{
    public class TermStoreRepository : ITermStoreRepository
    {
        private readonly IClientContextProvider _clientContextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TermStoreRepository));

        public TermStoreRepository(IClientContextProvider clientContextProvider)
        {
            _clientContextProvider = clientContextProvider;
        }

        public Guid EnsureDeploymentGroupsTermSet() =>
            MaybeGetTermSetInSiteCollectionGroup(TermSets.DeploymentGroups)
                .Select(ts => ts.Id)
                .Else(CreateDeploymentGroupTermSet);

        private Guid CreateDeploymentGroupTermSet()
        {
            using (Logger.MethodTraceLogger())
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                termStore.UpdateCache();

                var termSetIdInCaseOfCreate = Guid.NewGuid();
                var localeId = CultureInfo.GetCultureInfo("en-us").LCID;
                var siteCollectionGroup = termStore.GetSiteCollectionGroup(clientContext.Site, true);
                var termSet = siteCollectionGroup.CreateTermSet(TermSets.DeploymentGroups, termSetIdInCaseOfCreate, localeId);
                termStore.CommitAll();

                CreateExampleTerms(termSet, localeId);
                termStore.CommitAll();
                clientContext.ExecuteQueryWithIncrementalRetry();
                return termSetIdInCaseOfCreate;
            }
        }

        private static void CreateExampleTerms(TermSet termGroup, int localeId)
        {
            termGroup.CreateTerm("Deployment Group 1", localeId, Guid.NewGuid());
            termGroup.CreateTerm("Deployment Group 2", localeId, Guid.NewGuid());
            termGroup.CreateTerm("Deployment Group 3", localeId, Guid.NewGuid());
        }

        public bool IsTermSetProvisionedInSiteCollectionGroup(string name) => MaybeGetTermSetInSiteCollectionGroup(name).HasValue;

        public May<TermSet> MaybeGetTermSetInSiteCollectionGroup(string name)
        {
            using (Logger.MethodTraceLogger(name))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                var siteCollectionGroup = termStore.GetSiteCollectionGroup(clientContext.Site, true);

                var termSets = clientContext.LoadQuery(siteCollectionGroup.TermSets.Where(s => s.Name == name).Include(ts => ts.Name, ts => ts.Id));
                clientContext.ExecuteQueryWithIncrementalRetry();

                return termSets.MaySingle();
            }
        }

        internal May<TermPath> MaybeGetTermPathToTermSet(Guid termsetId)
        {
            using (Logger.MethodTraceLogger(termsetId))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();

                var termSet = termStore.GetTermSet(termsetId);
                clientContext.Load(termSet, ts => ts.Group.Name, ts => ts.Name);
                clientContext.ExecuteQueryWithIncrementalRetry();

                return termSet.ServerObjectIsNull ?? true
                    ? May.NoValue
                    : new TermPath($"{termSet.Group.Name};{termSet.Name}").Maybe();
            }
        }

        public May<TermPath> MaybeGetTermPathToTerm(Guid termId)
        {
            using (Logger.MethodTraceLogger(termId))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();

                var term = termStore.GetTerm(termId);
                clientContext.Load(term, t => t.TermSet.Group.Name, t => t.TermSet.Name, t => t.Name, t => t.PathOfTerm);
                clientContext.ExecuteQueryWithIncrementalRetry();

                return term.ServerObjectIsNull ?? true
                    ? May.NoValue
                    : new TermPath($"{term.TermSet.Group.Name};{term.TermSet.Name};{term.PathOfTerm}").Maybe();
            }
        }

        public May<TermIdentifier> MaybeGetTermIdentifierByPath(TermPath path)
        {
            using (Logger.MethodTraceLogger())
            {
                Logger.Info($"Searching for {path}.");
                var ids = MaybeConvertPathToIds(path.Queue).ToList();
                if (ids.Count < path.Length)
                {
                    Logger.Warn($"Path '{path}' could not be resolved completely.");
                    return May.NoValue;
                }
                Debug.Assert(ids.Count >= 2 && ids.Count == path.Length);
                Debug.Assert(!path.IsTermSet || ids.Count == 2);
                return path.IsTermSet
                    ? TermIdentifier.FromTermSet(ids.Last())
                    : TermIdentifier.FromTerm(ids[1], ids.Last());
            }
        }

        public May<Guid> MaybeGetNewSspId(Guid sspId)
        {
            using (Logger.MethodTraceLogger(sspId))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                clientContext.Load(termStore, t => t.Id);
                clientContext.ExecuteQueryWithIncrementalRetry();
                return sspId != termStore.Id ? termStore.Id : May<Guid>.NoValue;
            }
        }


        private IEnumerable<Guid> MaybeConvertPathToIds(Queue<string> queue)
        {
            using (Logger.MethodTraceLogger(queue))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();

                var groupName = queue.Dequeue();
                Logger.Debug($"Trying to get termgroup with name:{groupName}");
                var termGroup = termStore.GetTermGroupByName(groupName);
                if (termGroup == null) yield break;
                yield return termGroup.Id;

                var termSetName = queue.Dequeue();
                Logger.Debug($"Trying to get termset with name:{termSetName}");
                clientContext.Load(termGroup.TermSets, tsc => tsc.Include(ts => ts.Name, ts => ts.Id));
                clientContext.ExecuteQueryWithIncrementalRetry();
                var termSet = termGroup.TermSets.SingleOrDefault(ts => ts.Name == termSetName);
                if (termSet == null) yield break;
                yield return termSet.Id;

                var terms = termSet.Terms;
                while (queue.Any())
                {
                    var term = GetTermByNameSafe(queue, terms, clientContext);
                    if (!term.HasValue) yield break;
                    yield return term.ForceGetValue().Id;
                    terms = term.ForceGetValue().Terms;
                }
            }
        }

        private static May<Term> GetTermByNameSafe(Queue<string> queue, TermCollection terms, ClientContext clientContext)
        {
            using (Logger.MethodTraceLogger())
            {
                try
                {
                    var termName = queue.Dequeue();
                    Logger.Debug($"Trying to get term with name:{termName}");
                    var term = terms.GetByName(termName);
                    clientContext.Load(term, t => t.Id);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    return term;
                }
                catch (ServerException e)
                {
                    if (e.ServerErrorTypeName == "System.ArgumentOutOfRangeException")
                    {
                        return May<Term>.NoValue;
                    }
                    throw;
                }
            }
        }

        public May<TermPath> MaybeGetTermPathToTermIdentifier(TermIdentifier identifier)
        {
            using (Logger.MethodTraceLogger(identifier.AnchorId,identifier.TermSetId))
            {
                return identifier.IsTermSetIdentifier
                    ? MaybeGetTermPathToTermSet(identifier.TermSetId)
                    : MaybeGetTermPathToTerm(identifier.AnchorId);
            }
        }

        public DeploymentGroup GetDeploymentGroupById(Guid deploymentGroupId)
        {
            using (Logger.MethodTraceLogger(deploymentGroupId))
            using (var clientContext = _clientContextProvider.CreateClientContext())
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(clientContext).GetDefaultSiteCollectionTermStore();
                Logger.Debug($"Trying to get deployment group with id:{deploymentGroupId}");
                var term = taxonomySession.GetTerm(deploymentGroupId);

                if (term == null)
                {
                    throw new ApplicationException($"Deploymentgroup with id {deploymentGroupId} cannot be found");
                }
                var termLabel = term.GetDefaultLabel(1033);
                clientContext.ExecuteQueryWithIncrementalRetry();
                return new DeploymentGroup(termLabel.Value, deploymentGroupId);
            }
        }
    }
}
