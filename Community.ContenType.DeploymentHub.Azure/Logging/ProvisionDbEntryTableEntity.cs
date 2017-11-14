using System;

namespace Community.ContenType.DeploymentHub.Azure.Logging
{
    public class ProvisionDbEntryTableEntity : DbEntryTableEntity
    {
        public ProvisionDbEntryTableEntity(string hubUrl, string message, string initiatingUser, Version version) 
            : base(Guid.NewGuid().ToString(), $"Provision {version}", hubUrl, message, initiatingUser) { }
    }
}
