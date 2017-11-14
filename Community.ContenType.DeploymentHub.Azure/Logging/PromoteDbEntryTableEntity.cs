namespace Community.ContenType.DeploymentHub.Azure.Logging
{
    public class PromoteDbEntryTableEntity : DbEntryTableEntity
    {
        public int Version { get; set; }

        public PromoteDbEntryTableEntity(string actionCollectionId, string itemId, string hubUrl, string message,
            string initiatingUser, int version)
            : base(actionCollectionId, itemId, hubUrl, message, initiatingUser)
        {
            Version = version;
        }

        public PromoteDbEntryTableEntity()
        {
        }
    }
}
