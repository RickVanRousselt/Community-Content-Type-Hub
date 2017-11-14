using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Publish;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class PublishRequestInitiatedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }

        public PublishRequestInitiatedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos)
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
        }

        public override string ToString() => base.ToString() + $": CT IDs={string.Join(",", ContentTypeInfos)}";
    }

    public class PublishRequestInitiateFailedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }
        public Exception Exception { get; }

        public PublishRequestInitiateFailedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos, Exception exception)
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": CT IDs={string.Join(",", ContentTypeInfos)}, Ex={Exception}";
    }

    public class PublishActionsCalculatedEvent : Event
    {
        public PublishActionCollection ActionCollection { get; }

        public PublishActionsCalculatedEvent(ActionContext actionContext, PublishActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.TotalActionCount} Publish actions calculated";
    }

    public class PublishSiteColumnVerifiedEvent : Event
    {
        public PublishSiteColumnAction Action { get; }
        public IPublishVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PublishSiteColumnVerifiedEvent(PublishSiteColumnAction action, IPublishVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Site column:{Action.SiteColumn.Name} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";
    }

    public class PublishContentTypeVerifiedEvent : Event
    {
        public PublishContentTypeAction Action { get; }
        public IPublishVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PublishContentTypeVerifiedEvent(PublishContentTypeAction action, IPublishVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Content type:{Action.ContentType.Title} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";
    }

    public class PublishActionsVerifiedEvent : Event
    {
        public PublishActionCollection ActionCollection { get; }

        public PublishActionsVerifiedEvent(ActionContext actionContext, PublishActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PublishActionsImpactUpdatedEvent : Event
    {
        public PublishActionCollection ActionCollection { get; }

        public PublishActionsImpactUpdatedEvent(ActionContext actionContext, PublishActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PublishSiteColumnActionExecutedEvent : Event
    {
        public PublishSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public PublishedSiteColumn PublishedSiteColumn { get; }

        public PublishSiteColumnActionExecutedEvent(PublishSiteColumnAction action, int index, int totalCount, PublishedSiteColumn publishedSiteColumn)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            PublishedSiteColumn = publishedSiteColumn;
        }
        public override string ToString() => base.ToString() + $": Site column action executed: {PublishedSiteColumn.Title} on {Action.Target}";

    }

    public class PublishSiteColumnActionFailedEvent : Event
    {
        public PublishSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PublishSiteColumnActionFailedEvent(PublishSiteColumnAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Site column action execution failed: {Action.SiteColumn.Name} with exception:{Exception}";

    }

    public class PublishContentTypeActionExecutedEvent : Event
    {
        public PublishContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public PublishedContentType PublishedContentType { get; }

        public PublishContentTypeActionExecutedEvent(PublishContentTypeAction action, int index, int totalCount, PublishedContentType publishedContentType)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            PublishedContentType = publishedContentType;
        }
        public override string ToString() => base.ToString() + $": Content Type action executed: {PublishedContentType.Title}";

    }

    public class PublishContentTypeActionFailedEvent : Event
    {
        public PublishContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PublishContentTypeActionFailedEvent(PublishContentTypeAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Content Type action execution failed: {Action.ContentType.Title} with exception:{Exception}";

    }

    public class PublishActionsExecutedEvent : Event
    {
        public PublishActionCollection ActionCollection { get; }

        public PublishActionsExecutedEvent(ActionContext actionContext, PublishActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.TotalActionCount} publish actions complete. {ActionCollection.GetSuccessContentTypeActions()} content types and {ActionCollection.GetSuccessSiteColumnActions()} site columns succesfull";

    }

    public class PublishActionsFailedEvent : Event
    {
        public Exception Exception { get; }

        public PublishActionsFailedEvent(ActionContext actionContext, Exception exception)
            : base(actionContext)
        {
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Publish failed with exception: {Exception}";

    }
}
