using System;
using Community.ContenType.DeploymentHub.Contracts.Messages;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Community.ContenType.DeploymentHub.Azure.QueueData
{
    internal class DispatchQueueMessageTableEntity : TableEntity
    {
        public string SerializedRequestInfo { get; set; }
        public string Action { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; }

        public DispatchQueueMessageTableEntity()
        {
        }

        public DispatchQueueMessageTableEntity(DispatchQueueMessage queueMessage, ActionType actionType) 
            : base(queueMessage.RequestInfo.ActionCollectionId.ToString(), "Dispatch")
        {
            SerializedRequestInfo = JsonConvert.SerializeObject(queueMessage.RequestInfo);
            Action = actionType.ToString();
            CreatedTime = queueMessage.CreationTime;
            Status = queueMessage.Status.ToString();
        }

        public DispatchQueueMessage ToDispatchQueueMessage()
        {
            var status = (DispatchQueueMessageStatus)Enum.Parse(typeof(DispatchQueueMessageStatus), Status);
            return new DispatchQueueMessage(DeserializeRequestInfo(), CreatedTime, status);
        }

        private IRequestInfo DeserializeRequestInfo()
        {
            var actionEnum = (ActionType)Enum.Parse(typeof(ActionType), Action);

            switch (actionEnum)
            {
                case ActionType.Publish:
                    return JsonConvert.DeserializeObject<PublishRequestInfo>(SerializedRequestInfo);
                case ActionType.Push:
                    return JsonConvert.DeserializeObject<PushRequestInfo>(SerializedRequestInfo);
                case ActionType.Promote:
                    return JsonConvert.DeserializeObject<PromoteRequestInfo>(SerializedRequestInfo);
                case ActionType.Pull:
                    return JsonConvert.DeserializeObject<PullRequestInfo>(SerializedRequestInfo);
                default:
                    throw new ApplicationException("Invalid actiontype while deserializing request info");
            }
        }
    }
}
