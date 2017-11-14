using System;
using System.Collections.Generic;
using System.Linq;
using Community.ContenType.DeploymentHub.Azure.QueueData;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Community.ContenType.DeploymentHub.Azure.Repositories
{
    public class DispatchedQueueRepository : IQueueRepository, IDispatchedQueueRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DispatchedQueueRepository));
        private static CloudTableClient _tableClient;
        private const string TableName = "DispatchQueue";

        public DispatchedQueueRepository(string azureStorageConnectionString)
        {
            using (Logger.MethodTraceLogger(azureStorageConnectionString))
            {
                _tableClient = CloudStorageAccount.Parse(azureStorageConnectionString).CreateCloudTableClient();
            }
        }

        public void AddToQueue(PromoteRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                AddMessageToStorageTable(info, ActionType.Promote);
            }
        }

        public void AddToQueue(PullRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                AddMessageToStorageTable(info, ActionType.Pull);
            }
        }

        public void AddToQueue(PushRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                AddMessageToStorageTable(info, ActionType.Push);
            }
        }

        public void AddToQueue(PublishRequestInfo info)
        {
            using (Logger.MethodTraceLogger(info))
            {
                AddMessageToStorageTable(info, ActionType.Publish);
            }
        }

        private static void AddMessageToStorageTable(IRequestInfo info, ActionType actionType)
        {
            using (Logger.MethodTraceLogger(info, actionType))
            {
                var table = _tableClient.GetTableReference(TableName);
                table.CreateIfNotExists();

                var dispatchQueueMessage = new DispatchQueueMessage(info);
                var dispatchQueueMessageTableEntity = new DispatchQueueMessageTableEntity(dispatchQueueMessage, actionType);
                var insertOperation = TableOperation.Insert(dispatchQueueMessageTableEntity);
                table.Execute(insertOperation);
            }
        }

        public IEnumerable<DispatchQueueMessage> GetAllQueueMessages()
        {
            using (Logger.MethodTraceLogger())
            {
                var table = _tableClient.GetTableReference(TableName);
                table.CreateIfNotExists();

                var query = new TableQuery<DispatchQueueMessageTableEntity>();
                TableContinuationToken continuationToken = null;

                var dispatchQueueMessageList = new List<DispatchQueueMessage>();

                do
                {
                    var tableQueryResult = table.ExecuteQuerySegmented(query, continuationToken);
                    continuationToken = tableQueryResult.ContinuationToken;

                    dispatchQueueMessageList.AddRange(tableQueryResult.Select(tr => tr.ToDispatchQueueMessage()));

                } while (continuationToken != null);

                return dispatchQueueMessageList;
            }
        }

        public void UpdateQueueMessageStatus(Guid actionCollectionId, DispatchQueueMessageStatus status)
        {
            using (Logger.MethodTraceLogger(actionCollectionId, status))
            {
                var table = _tableClient.GetTableReference(TableName);
                table.CreateIfNotExists();

                var retrieveOperation = TableOperation.Retrieve<DispatchQueueMessageTableEntity>(actionCollectionId.ToString(), "Dispatch");
                var retrievedResult = table.Execute(retrieveOperation);
                var dispatchQueueMessage = (DispatchQueueMessageTableEntity)retrievedResult.Result;

                if (dispatchQueueMessage != null)
                {
                    dispatchQueueMessage.Status = status.ToString();

                    var updateOperation = TableOperation.Replace(dispatchQueueMessage);
                    table.Execute(updateOperation);
                }
                else
                {
                    throw new ApplicationException("Unable to retrieve queuemessage with id:" + actionCollectionId);
                }
            }
        }

        public void DeleteDispatchQueueMessage(Guid actionCollectionId)
        {
            using (Logger.MethodTraceLogger(actionCollectionId))
            {
                var table = _tableClient.GetTableReference(TableName);
                table.CreateIfNotExists();

                var retrieveOperation = TableOperation.Retrieve<DispatchQueueMessageTableEntity>(actionCollectionId.ToString(), "Dispatch");
                var retrievedResult = table.Execute(retrieveOperation);
                var dispatchQueueMessage = (DispatchQueueMessageTableEntity)retrievedResult.Result;

                if (dispatchQueueMessage != null)
                {
                    var deleteOperation = TableOperation.Delete(dispatchQueueMessage);
                    table.Execute(deleteOperation);
                }
                else
                {
                    throw new ApplicationException("Unable to retrieve queuemessage with id:" + actionCollectionId);
                }
            }
        }
    }
}