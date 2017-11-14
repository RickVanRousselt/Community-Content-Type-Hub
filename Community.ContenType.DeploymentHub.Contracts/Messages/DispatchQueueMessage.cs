using System;

namespace Community.ContenType.DeploymentHub.Contracts.Messages
{
    public class DispatchQueueMessage
    {
        public IRequestInfo RequestInfo { get; }
        public DateTime CreationTime { get; }
        public DispatchQueueMessageStatus Status { get; set; }

        public DispatchQueueMessage(IRequestInfo requestInfo, DateTime creationTime, DispatchQueueMessageStatus status)
        {
            RequestInfo = requestInfo;
            CreationTime = creationTime;
            Status = status;
        }

        public DispatchQueueMessage(IRequestInfo requestInfo)
        {
            RequestInfo = requestInfo;
            CreationTime = DateTime.UtcNow;
            Status = DispatchQueueMessageStatus.Requested;
        }
    }
}