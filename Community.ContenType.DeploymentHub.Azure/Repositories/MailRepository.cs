using System;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Text;
using Community.ContenType.DeploymentHub.Common;
using Community.ContenType.DeploymentHub.Domain.Events;
using Community.ContenType.DeploymentHub.Contracts.Repositories;
using log4net;

namespace Community.ContenType.DeploymentHub.Azure.Repositories
{
    public class MailRepository : IMailRepository
    {
        private readonly MailAddress _user;
        private readonly SecureString _password;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MailRepository));

        public MailRepository(MailAddress user, SecureString password, string smtpServer, int smtpPort)
        {
            _user = user;
            _password = password;
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
        }

        public void SendMail(PublishActionsExecutedEvent e, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(e))
            {
                try
                {
                    var siteColumnActions = new StringBuilder();
                    foreach (var s in e.ActionCollection.SiteColumnActions)
                    {
                        siteColumnActions.Append($"<li>{s.Key.SiteColumn.Name} - {s.Value}</li>");
                    }
                    var contentTypeActions = new StringBuilder();
                    foreach (var s in e.ActionCollection.ContentTypeActions)
                    {
                        contentTypeActions.Append($"<li>{s.Key.ContentType.Title} - {s.Value}</li>");
                    }

                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Publish request finished",
                        Body = string.Format(mailTemplate,
                        e.ActionContext.InitiatingUser,
                        e.ActionContext.Hub,
                        e.ActionCollection.SiteColumnActions.Count,
                        e.ActionCollection.ContentTypeActions.Count,
                        e.ActionCollection.SuccessActionCount,
                        e.ActionCollection.GetSuccessSiteColumnActions().Count(),
                        siteColumnActions,
                        e.ActionCollection.GetSuccessContentTypeActions().Count(),
                        contentTypeActions,
                        e.ActionCollection.ActionContext.ActionCollectionId,
                        e.ActionCollection.ActionContext.StatusListItemId
                        )
                    };
                    mailMessage.To.Add(e.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {e.ActionContext.InitiatingUser}, ActionCollection: {e.ActionCollection.ActionContext.ActionCollectionId}, ID: {e.ActionCollection.ActionContext.StatusListItemId},HUB: {e.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        public void SendMail(PublishActionsFailedEvent publishActionsFailedEvent, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(publishActionsFailedEvent))
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Publish request failed",
                        Body = string.Format(mailTemplate,
                        publishActionsFailedEvent.ActionContext.InitiatingUser,
                        publishActionsFailedEvent.ActionContext.Hub,
                        publishActionsFailedEvent.Exception.Message,
                        publishActionsFailedEvent.ActionContext.ActionCollectionId,
                        publishActionsFailedEvent.ActionContext.StatusListItemId,
                        publishActionsFailedEvent.Exception
                        )
                    };
                    mailMessage.To.Add(publishActionsFailedEvent.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {publishActionsFailedEvent.ActionContext.InitiatingUser}, ActionCollection: {publishActionsFailedEvent.ActionContext.ActionCollectionId}, ID: {publishActionsFailedEvent.ActionContext.StatusListItemId},HUB: {publishActionsFailedEvent.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        public void SendMail(PushActionsExecutedEvent pushActionExecutedEvent, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(pushActionExecutedEvent))
            {
                try
                {
                    var siteColumnActions = new StringBuilder();
                    foreach (var s in pushActionExecutedEvent.ActionCollection.SiteColumnActions)
                    {
                        siteColumnActions.Append($"<li>{s.Key.SiteColumn.Title} - {s.Value}</li>");
                    }
                    var contentTypeActions = new StringBuilder();
                    foreach (var s in pushActionExecutedEvent.ActionCollection.ContentTypeActions)
                    {
                        contentTypeActions.Append($"<li>{s.Key.ContentType.Title} - {s.Value}</li>");
                    }
                    var siteCollections = new StringBuilder();
                    var totalSiteCollections = 0;
                    foreach (var s in pushActionExecutedEvent.ActionCollection.ContentTypeActions.Keys.Select(x => x.TargetSiteCollection.Url.ToString()).Distinct())
                    {
                        siteCollections.Append($"<li>{s}</li>");
                        totalSiteCollections++;
                    }

                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Push request finished",
                        Body = string.Format(mailTemplate,
                        pushActionExecutedEvent.ActionContext.InitiatingUser,
                        pushActionExecutedEvent.ActionContext.Hub,
                        pushActionExecutedEvent.ActionCollection.SiteColumnActionCount,
                        pushActionExecutedEvent.ActionCollection.ContentTypeActionCount,
                        totalSiteCollections,
                        pushActionExecutedEvent.ActionCollection.SuccessActionCount,
                        pushActionExecutedEvent.ActionCollection.GetSuccessSiteColumnActions().Count(),
                        siteColumnActions,
                        pushActionExecutedEvent.ActionCollection.GetSuccessContentTypeActions().Count(),
                        contentTypeActions,
                        siteCollections,
                        pushActionExecutedEvent.ActionCollection.ActionContext.ActionCollectionId,
                        pushActionExecutedEvent.ActionCollection.ActionContext.StatusListItemId
                        )
                    };
                    mailMessage.To.Add(pushActionExecutedEvent.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {pushActionExecutedEvent.ActionContext.InitiatingUser}, ActionCollection: {pushActionExecutedEvent.ActionContext.ActionCollectionId}, ID: {pushActionExecutedEvent.ActionContext.StatusListItemId},HUB: {pushActionExecutedEvent.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        public void SendMail(PushActionsFailedEvent pushActionsFailedEvent, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(pushActionsFailedEvent))
            {
                try
                {

                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Push request failed",
                        Body = string.Format(mailTemplate,
                        pushActionsFailedEvent.ActionContext.InitiatingUser, 
                        pushActionsFailedEvent.ActionContext.Hub, 
                        pushActionsFailedEvent.Exception.Message,
                        pushActionsFailedEvent.ActionContext.ActionCollectionId, 
                        pushActionsFailedEvent.ActionContext.StatusListItemId, 
                        pushActionsFailedEvent.Exception
                        )
                    };
                    mailMessage.To.Add(pushActionsFailedEvent.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {pushActionsFailedEvent.ActionContext.InitiatingUser}, ActionCollection: {pushActionsFailedEvent.ActionContext.ActionCollectionId}, ID: {pushActionsFailedEvent.ActionContext.StatusListItemId},HUB: {pushActionsFailedEvent.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        public void SendMail(PromoteActionsExecutedEvent promoteActionsExecutedEvent, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(promoteActionsExecutedEvent))
            {
                try
                {
                    var siteColumnActions = new StringBuilder();
                    foreach (var s in promoteActionsExecutedEvent.ActionCollection.SiteColumnActions)
                    {
                        siteColumnActions.Append($"<li>{s.Key} - {s.Value}</li>");
                    }
                    var contentTypeActions = new StringBuilder();
                    foreach (var s in promoteActionsExecutedEvent.ActionCollection.ContentTypeActions)
                    {
                        contentTypeActions.Append($"<li>{s.Key} - {s.Value}</li>");
                    }
                    var siteCollections = new StringBuilder();
                    var totalSiteCollections = 0;
                    foreach (var s in promoteActionsExecutedEvent.ActionCollection.ContentTypeActions.Keys.Select(x => x.TargetHub.Url.ToString()).Distinct())
                    {
                        siteCollections.Append($"<li>{s}</li>");
                        totalSiteCollections++;
                    }

                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Promote request finished",
                        Body = string.Format(mailTemplate,
                        promoteActionsExecutedEvent.ActionContext.InitiatingUser,
                        promoteActionsExecutedEvent.ActionContext.Hub,
                        promoteActionsExecutedEvent.ActionCollection.SiteColumnActionCount,
                        promoteActionsExecutedEvent.ActionCollection.ContentTypeActionCount,
                        totalSiteCollections,
                        promoteActionsExecutedEvent.ActionCollection.SuccessActionCount,
                        promoteActionsExecutedEvent.ActionCollection.GetSuccessSiteColumnActions().Count(),
                        siteColumnActions,
                        promoteActionsExecutedEvent.ActionCollection.GetSuccessContentTypeActions().Count(),
                        contentTypeActions,
                        siteCollections,
                        promoteActionsExecutedEvent.ActionCollection.ActionContext.ActionCollectionId,
                        promoteActionsExecutedEvent.ActionCollection.ActionContext.StatusListItemId
                        )
                    };
                    mailMessage.To.Add(promoteActionsExecutedEvent.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {promoteActionsExecutedEvent.ActionContext.InitiatingUser}, ActionCollection: {promoteActionsExecutedEvent.ActionContext.ActionCollectionId}, ID: {promoteActionsExecutedEvent.ActionContext.StatusListItemId},HUB: {promoteActionsExecutedEvent.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        public void SendMail(PromoteActionsFailedEvent promoteActionsFailedEvent, string mailTemplate)
        {
            using (Logger.MethodTraceLogger(promoteActionsFailedEvent))
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = _user,
                        Subject = "Content Type Deployment: Push request failed",
                        Body = string.Format(mailTemplate, 
                        promoteActionsFailedEvent.ActionContext.InitiatingUser,
                        promoteActionsFailedEvent.ActionContext.Hub,
                        promoteActionsFailedEvent.Exception.Message,
                        promoteActionsFailedEvent.ActionContext.ActionCollectionId,
                        promoteActionsFailedEvent.ActionContext.StatusListItemId,
                        promoteActionsFailedEvent.Exception)
                    };
                    mailMessage.To.Add(promoteActionsFailedEvent.ActionContext.InitiatingUser);

                    mailMessage.IsBodyHtml = true;

                    using (var mailClient = GetMailClient())
                    {
                        mailClient.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Error while SendMail. To: {promoteActionsFailedEvent.ActionContext.InitiatingUser}, ActionCollection: {promoteActionsFailedEvent.ActionContext.ActionCollectionId}, ID: {promoteActionsFailedEvent.ActionContext.StatusListItemId},HUB: {promoteActionsFailedEvent.ActionContext.Hub}, Exception: ", ex);
                }
            }
        }

        private SmtpClient GetMailClient()
        {
            using (Logger.MethodTraceLogger())
            {
                try
                {
                    var client = new SmtpClient(_smtpServer)
                    {
                        Port = _smtpPort,
                        EnableSsl = true,
                        Credentials = new System.Net.NetworkCredential(_user.Address, _password),
                        DeliveryMethod = SmtpDeliveryMethod.Network
                    };
                    return client;
                }
                catch (Exception e)
                {
                    Logger.Error("Cannot get SMTP client", e);
                    throw;
                }
            }
        }
    }
}
