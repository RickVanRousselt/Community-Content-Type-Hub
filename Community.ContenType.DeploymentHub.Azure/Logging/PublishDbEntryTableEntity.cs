namespace Community.ContenType.DeploymentHub.Azure.Logging
{
    public class PublishDbEntryTableEntity : DbEntryTableEntity
    {
        public int Version { get; set; }

        public PublishDbEntryTableEntity(string actionCollectionId, string itemId, string hubUrl, string message, string initiatingUser, int version) 
            : base(actionCollectionId, itemId, hubUrl, message, initiatingUser)
        {
            Version = version;
        }
    }
}
