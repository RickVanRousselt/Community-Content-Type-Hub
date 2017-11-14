using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Core;

namespace Community.ContenType.DeploymentHub.DomainServices
{
    public interface IStatusListRepository : IPublishRequestRetriever, IPushRequestRetriever, IPullRequestRetriever, IPromoteRequestRetriever
    {
        void EnsureStatusList();
        void UpdateView();

        int AddPublishEntry(JobState state, string substate, DateTime startDate, string message, ISet<ContentTypeInfo> contentTypeInfos);
        int AddPushEntry(JobState state, string substate, DateTime startDate, string message, ISet<ContentTypeInfo> contentTypeInfos);
        int AddPullEntry(JobState state, string substate, DateTime startDate, string message, Uri siteCollectionUrl, string deploymentGroup);
        int AddPromoteEntry(JobState state, string substate, DateTime eTimestamp, string message, ISet<ContentTypeInfo> eContentTypeInfos);

        void UpdateEntry(int listItemId, JobState state, string substate);
        void UpdateFinalEntry(int listItemId, JobState state, string substate, DateTime endDate, string summary);
        void UpdateListV1();
    }

    public enum JobState
    {
        Pending = 0,
        Running = 1,
        Success = 2,
        Failure = 3
    }
}