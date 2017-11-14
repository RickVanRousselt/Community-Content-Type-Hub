using Microsoft.WindowsAzure.Storage.Table;

namespace Community.ContenType.DeploymentHub.Azure.Logging
{
    public abstract class DbEntryTableEntity : TableEntity
    {
        public string Hub { get; set; }
        public string Message { get; set; }
        public string InitiatingUser { get; set; }

        protected DbEntryTableEntity(string partitionKey, string rowKey, string hubUrl, string message, string initiatingUser)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Hub = hubUrl;
            Message = message;
            InitiatingUser = initiatingUser;
        }

        protected DbEntryTableEntity()
        {
            
        }
    }
}
