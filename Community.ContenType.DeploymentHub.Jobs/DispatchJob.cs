using System;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain;
using Community.ContenType.DeploymentHub.Contracts.Repositories;

namespace Community.ContenType.DeploymentHub.Jobs
{
    public class DispatchJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DispatchJob));

        private readonly IDispatchedQueueRepository _dispatchedQueueRepository;
        private readonly IQueueRepository _queueRepository;

        public DispatchJob(IDispatchedQueueRepository dispatchedQueueRepository, IQueueRepository queueRepository)
        {
            using (Logger.MethodTraceLogger())
            {
                _dispatchedQueueRepository = dispatchedQueueRepository;
                _queueRepository = queueRepository;
            }
        }

        public int Run()
        {
            using (Logger.MethodTraceLogger())
            {
                try
                {
                    var allRequests = _dispatchedQueueRepository.GetAllQueueMessages().ToList();
                    var inProgressHubs = CalculateInProgressHubs(allRequests);

                    Logger.Info($"{allRequests.Count} requests found in dispatch queue");
                    Logger.Info("Following HUBS are processing a message:");
                    foreach (var inProgressHub in inProgressHubs)
                    {
                        Logger.Info($"- {inProgressHub}");
                    }

                    var requestsToStart = GetFirstRequestPerHubThatIsNotInProgress(allRequests, inProgressHubs);

                    foreach (var request in requestsToStart)
                    {
                        Logger.Info($"Adding request {request.RequestInfo.ActionCollectionId} to processing queue");
                        AddMessageToQueue(request.RequestInfo);
                        _dispatchedQueueRepository.UpdateQueueMessageStatus(request.RequestInfo.ActionCollectionId, DispatchQueueMessageStatus.InProgress);
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    Logger.Error($"A unexpected error occured. Exception: {ex}");
                    throw;
                }
            }
        }

        private static HashSet<Uri> CalculateInProgressHubs(IEnumerable<DispatchQueueMessage> requests)
        {
            using (Logger.MethodTraceLogger(requests))
            {
                var hubsWihInProgressMessages = requests
                    .Where(r => r.Status == DispatchQueueMessageStatus.InProgress)
                    .Select(r => r.RequestInfo.Hub);
                return new HashSet<Uri>(hubsWihInProgressMessages, UriIgnoreCaseEqualityComparer.Instance);
            }
        }

        private static IEnumerable<DispatchQueueMessage> GetFirstRequestPerHubThatIsNotInProgress(IEnumerable<DispatchQueueMessage> requests, ISet<Uri> inProgresshubs)
        {
            using (Logger.MethodTraceLogger(requests, inProgresshubs))
            {
                return requests
                    .Where(r => !inProgresshubs.Contains(r.RequestInfo.Hub))
                    .GroupBy(r => r.RequestInfo.Hub, r => r, UriIgnoreCaseEqualityComparer.Instance)
                    .Select(g => g.OrderBy(r => r.CreationTime).First());
            }
        }

        private void AddMessageToQueue(IRequestInfo request)
        {
            if (request is PushRequestInfo)
            {
                _queueRepository.AddToQueue((PushRequestInfo)request);
            }
            else if (request is PullRequestInfo)
            {
                _queueRepository.AddToQueue((PullRequestInfo)request);
            }
            else if (request is PublishRequestInfo)
            {
                _queueRepository.AddToQueue((PublishRequestInfo)request);
            }
            else if (request is PromoteRequestInfo)
            {
                _queueRepository.AddToQueue((PromoteRequestInfo)request);
            }
            else
            {
                throw new ApplicationException("Request has invalid type while trying to add it to the queue");
            }
        }
    }
}
