using System;
using System.Linq;
using System.Xml.Linq;
using Community.ContenType.DeploymentHub.Azure.Logging;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Community.ContenType.DeploymentHub.Azure.Repositories
{
    public class DbEntryRepository : IDbEntryRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DbEntryRepository));
        private static Lazy<CloudTableClient> _tableClient;
        private static Lazy<CloudBlobClient> _blobContainerClient;


        public DbEntryRepository(string azureStorageConnectionString)
        {
            _tableClient =
                new Lazy<CloudTableClient>(
                    () => CloudStorageAccount.Parse(azureStorageConnectionString).CreateCloudTableClient());
            _blobContainerClient =
                new Lazy<CloudBlobClient>(
                    () => CloudStorageAccount.Parse(azureStorageConnectionString).CreateCloudBlobClient());
        }

        private static void LogEntity(string tableName, ITableEntity tableEntity)
        {
            try
            {
                var table = _tableClient.Value.GetTableReference(tableName);
                table.CreateIfNotExists();
                var insertOperation = TableOperation.Insert(tableEntity);
                table.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in log LogEntity", ex);
            }
        }

        private static void LogSchema(string blobName, XDocument schema)
        {
            try
            {
                var container = _blobContainerClient.Value.GetContainerReference("ctdmasterdb");
                container.CreateIfNotExists();
                var appendBlob = container.GetBlockBlobReference(blobName);
                appendBlob.UploadTextAsync(schema.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error("Error in log LogEntity", ex);
            }
        }

        #region Provision

        public void Log(ProvisionActionExecutedEvent e)
        {
            var table = _tableClient.Value.GetTableReference(e.ActionContext.Hub.SafeAlphaNumericName);
            table.CreateIfNotExists();
            string message = $"Converted Site Collection {e.ActionContext.Hub.Url} to Content Type Deployment Hub with version {e.Version}";
            var insertOperation = TableOperation.Insert(new ProvisionDbEntryTableEntity(
                e.ActionContext.Hub.Url.AbsolutePath,
                message,
                e.ActionContext.InitiatingUser.Address, e.Version));
            table.Execute(insertOperation);
        }

        public void Log(ProvisionActionFailedEvent e)
        {
            var table = _tableClient.Value.GetTableReference(e.ActionContext.Hub.SafeAlphaNumericName);
            table.CreateIfNotExists();
            string message =
                $"Failed to convert Site Collection {e.ActionContext.Hub.Url} with version:{e.Version} to Content Type Deployment Hub: {e.Exception}";
            var insertOperation = TableOperation.Insert(new ProvisionDbEntryTableEntity(
                e.ActionContext.Hub.Url.AbsolutePath,
                message,
                e.ActionContext.InitiatingUser.Address, e.Version));
            table.Execute(insertOperation);
        }

        #endregion

        #region Publish

        public void Log(PublishRequestInitiatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"Publish Request for {e.ContentTypeInfos.Count} Content Types";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.ActionContext.StatusListItemId.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PublishSiteColumnActionExecutedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.PublishedSiteColumn.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PublishSiteColumnActionFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PublishContentTypeActionExecutedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.PublishedContentType.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PublishContentTypeActionFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PublishRequestInitiateFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish Request for {e.ContentTypeInfos.Count} StatusListID:{e.ActionContext.StatusListItemId} - Content Types failed: {e.Exception}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Publish Request Failed", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PublishActionsCalculatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions calculated for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Publish Actions Calculated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PublishActionsVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions verified for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Publish Actions Verified", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }
        public void Log(PublishActionsImpactUpdatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions impact updated for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Publish Actions impact updated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }
        public void Log(PublishSiteColumnVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions verified for {e.Action.SiteColumn.Name} -{e.ActionContext.Hub.SafeAlphaNumericName}. Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
               $"{e.Action.SiteColumn.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }
        public void Log(PublishContentTypeVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                 $"Publish actions verified for {e.Action.ContentType.Title} -{e.ActionContext.Hub.SafeAlphaNumericName}. Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                 $"{e.Action.ContentType.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PublishActionsExecutedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions Executed for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Publish Actions Executed", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PublishActionsFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions execution failed for {e.ActionContext.ActionCollectionId}.StatusListID:{e.ActionContext.StatusListItemId} - {e.Exception.Message} - {e.Exception.StackTrace}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Publish Failed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        #endregion

        #region Push

        public void Log(PushRequestInitiatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"Push Request for {e.ContentTypeInfos.Count} Content Types";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.ActionContext.StatusListItemId.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushSiteColumnActionExecutedEvent e)
        {
            var tableName = e.Action.TargetSiteCollection.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.SiteColumn.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PushSiteColumnActionFailedEvent e)
        {
            var tableName = e.Action.TargetSiteCollection.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.SiteColumn.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PushContentTypeActionExecutedEvent e)
        {
            var tableName = e.Action.TargetSiteCollection.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.ContentType.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PushContentTypeActionFailedEvent e)
        {
            var tableName = e.Action.TargetSiteCollection.SafeAlphaNumericName;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.ContentType.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PushRequestInitiateFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Push Request for {e.ContentTypeInfos.Count} Content Types failed. List ID:{e.ActionContext.StatusListItemId} - {e.Exception.Message} - {e.Exception}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Push Request Failed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushActionsCalculatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Actions Calculated for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} Actions are valid";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Push Action Calculated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushActionsVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Push Actions Verified for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} Actions are verified";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Push Action Verified", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }
        public void Log(PushActionsImpactUpdatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Push Actions impact updated for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} Actions are updated";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Push Action impact updated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushActionsExecutedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Push Actions executed: Success: {e.ActionCollection.AllActionsSucceeded} for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} Actions are executed";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Push Executed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushActionsFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Push Actions failed: for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - Ex: {e.Exception}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Push Failed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushSiteColumnVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions verified for {e.Action.SiteColumn.Title}.-{e.ActionContext.Hub.SafeAlphaNumericName} Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
               $"{e.Action.SiteColumn.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PushContentTypeVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Publish actions verified for {e.Action.ContentType.Title}.-{e.ActionContext.Hub.SafeAlphaNumericName} Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PublishDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
               $"{e.Action.ContentType.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        #endregion

        #region Pull

        public void Log(PullRequestInitiatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"Pull Request for {e.SiteCollectionUrl},  Deploymentgroup: {e.DeploymentGroup}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.ActionContext.StatusListItemId.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PullActionsFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Pull Actions failed: for {e.ActionContext.ActionCollectionId}. List ID:{e.ActionContext.StatusListItemId} - Ex: {e.Exception}";
            var entity = new PushDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Pull Failed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        #endregion

        #region Promote

        public void Log(PromoteRequestInitiatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message = $"Promote Request for {e.ContentTypeInfos.Count} Content Types";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.ActionContext.StatusListItemId.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteRequestInitiateFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote Request for {e.ContentTypeInfos.Count} StatusListID:{e.ActionContext.StatusListItemId} - Content Types failed: {e.Exception}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Promote Request Failed", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteActionsCalculatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote actions calculated for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Promote Actions Calculated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteActionsVerifiedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote actions verified for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Promote Actions Verified", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }
        public void Log(PromoteActionsImpactUpdatedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote actions impact updated for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Promote Actions impact updated", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteContentTypeVerifiedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message =
                 $"Promote actions verified for {e.Action.ContentType.Title} -{e.Action.TargetHub.SafeAlphaNumericNameFullUri}. Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                 $"{e.Action.ContentType.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteSiteColumnVerifiedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message =
                $"Promote actions verified for {e.Action.SiteColumn.Title} -{e.Action.TargetHub.SafeAlphaNumericNameFullUri}. Compliant: {e.Result.IsCompliant} - {e.Result.Reason}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
               $"{e.Action.SiteColumn.Id}-{e.ActionContext.Hub.SafeAlphaNumericName}-Compliant:{e.Result.IsCompliant}-{e.Rule.Name}", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteSiteColumnActionExecutedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.SiteColumn.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PromoteSiteColumnActionFailedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.SiteColumn.Id.ToString(), e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.SiteColumn.Id}.txt";
            LogSchema(blobName, e.Action.SiteColumn.Schema);
        }

        public void Log(PromoteContentTypeActionExecutedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} completed";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, e.Action.ContentType.Version);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PromoteContentTypeActionFailedEvent e)
        {
            var tableName = e.Action.TargetHub.SafeAlphaNumericNameFullUri;
            var message = $"[{e.Index}/{e.TotalCount}] {e.Action} failed: {e.Exception}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                e.Action.ContentType.Id, e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);

            var blobName =
                $"{e.ActionContext.Hub.NoStartSlashName}/{e.ActionContext.ActionCollectionId}/{e.Action.ContentType.Id}.txt";
            LogSchema(blobName, e.Action.ContentType.Schema);
        }

        public void Log(PromoteActionsExecutedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote actions Executed for {e.ActionCollection}. {e.ActionCollection.ValidActionCount}/{e.ActionCollection.TotalActionCount} are valid. StatusListID:{e.ActionContext.StatusListItemId} Valid Content Types:{e.ActionCollection.GetValidContentTypeActions()} - Valid Site Columns: {e.ActionCollection.GetValidSiteColumnActions()}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(),
                "Promote Actions Executed", e.ActionContext.Hub.Url.AbsolutePath, message,
                e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        public void Log(PromoteActionsFailedEvent e)
        {
            var tableName = e.ActionContext.Hub.SafeAlphaNumericName;
            var message =
                $"Promote actions execution failed for {e.ActionContext.ActionCollectionId}.StatusListID:{e.ActionContext.StatusListItemId} - {e.Exception.Message} - {e.Exception.StackTrace}";
            var entity = new PromoteDbEntryTableEntity(e.ActionContext.ActionCollectionId.ToString(), "Promote Failed",
                e.ActionContext.Hub.Url.AbsolutePath, message, e.ActionContext.InitiatingUser.Address, -1);
            LogEntity(tableName, entity);
        }

        #endregion

        public bool IsVersionDeployed(PushSiteColumnAction action)
        {
            try
            {
                var tableName = action.TargetSiteCollection.SafeAlphaNumericName;
                var table = _tableClient.Value.GetTableReference(tableName);
                table.CreateIfNotExists();
                var rangeQuery = new TableQuery<PushDbEntryTableEntity>().Where(
                        TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, action.SiteColumn.Id.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForInt("Version", QueryComparisons.GreaterThanOrEqual, action.SiteColumn.Version)));
                return table.ExecuteQuery(rangeQuery).Any();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetVersion site column from table storage", ex);
                return false;
            }
        }

        public bool IsVersionDeployed(PushContentTypeAction action)
        {
            try
            {
                var tableName = action.TargetSiteCollection.SafeAlphaNumericName;
                var table = _tableClient.Value.GetTableReference(tableName);
                table.CreateIfNotExists();
                var rangeQuery = new TableQuery<PushDbEntryTableEntity>().Where(
                        TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, action.ContentType.Id),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForInt("Version", QueryComparisons.GreaterThanOrEqual, action.ContentType.Version)));
               return table.ExecuteQuery(rangeQuery).Any();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetVersion content type from table storage", ex);
                return false;
            }
        }
    }
}
