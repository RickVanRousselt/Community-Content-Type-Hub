using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Push;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class PushRequestInitiatedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }

        public PushRequestInitiatedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos)
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
        }
        public override string ToString() => base.ToString() + $": Push request initialized: : CT IDs={string.Join(",", ContentTypeInfos)}";
    }

    public class PushRequestInitiateFailedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }
        public Exception Exception { get; }

        public PushRequestInitiateFailedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos, Exception exception)
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": CT IDs={string.Join(",", ContentTypeInfos)}, Ex={Exception}";

    }

    public class PushActionsCalculatedEvent : Event
    {
        public PushActionCollection ActionCollection { get; }

        public PushActionsCalculatedEvent(ActionContext actionContext, PushActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.TotalActionCount} Push actions calculated";

    }

    public class PushActionsVerifiedEvent : Event
    {
        public PushActionCollection ActionCollection { get; }

        public PushActionsVerifiedEvent(ActionContext actionContext, PushActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.ValidActionCount} Push actions valid";
    }

    public class PushActionsImpactUpdatedEvent : Event
    {
        public PushActionCollection ActionCollection { get; }

        public PushActionsImpactUpdatedEvent(ActionContext actionContext, PushActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PushSiteColumnVerifiedEvent : Event
    {
        public PushSiteColumnAction Action { get; }
        public IPushVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PushSiteColumnVerifiedEvent(PushSiteColumnAction action, IPushVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Site column:{Action.SiteColumn.Title} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";

    }

    public class PushSiteColumnActionExecutedEvent : Event
    {
        public PushSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }

        public PushSiteColumnActionExecutedEvent(PushSiteColumnAction action, int index, int totalCount)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
        }
        public override string ToString() => base.ToString() + $": Site column action executed: {Action.SiteColumn.Title} on {Action.TargetSiteCollection}";

    }

    public class PushSiteColumnActionFailedEvent : Event
    {
        public PushSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PushSiteColumnActionFailedEvent(PushSiteColumnAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Site column action execution failed: {Action.SiteColumn.Title} with exception:{Exception}";

    }

    public class PushContentTypeVerifiedEvent : Event
    {
        public PushContentTypeAction Action { get; }
        public IPushVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PushContentTypeVerifiedEvent(PushContentTypeAction action, IPushVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Content type:{Action.ContentType.Title} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";

    }

    public class PushContentTypeActionExecutedEvent : Event
    {
        public PushContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public SiteCollection Target;

        public PushContentTypeActionExecutedEvent(PushContentTypeAction action, int index, int totalCount, SiteCollection target)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Target = target;
        }
        public override string ToString() => base.ToString() + $": Content Type action executed: {Action.ContentType.Title} on {Target}";

    }

    public class PushContentTypeActionFailedEvent : Event
    {
        public PushContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PushContentTypeActionFailedEvent(PushContentTypeAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Content Type action execution failed: {Action.ContentType.Title} on {Action.TargetSiteCollection} with exception:{Exception}";

    }

    public class PushActionsExecutedEvent : Event
    {
        public PushActionCollection ActionCollection { get; }

        public PushActionsExecutedEvent(ActionContext actionContext, PushActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.TotalActionCount} push actions complete. {ActionCollection.GetSuccessContentTypeActions()} content types and {ActionCollection.GetSuccessSiteColumnActions()} site columns succesfull";

    }

    public class PushActionsFailedEvent : Event
    {
        public Exception Exception { get; }
        public PushActionsFailedEvent(ActionContext actionContext, Exception exception)
            : base(actionContext)
        {
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Push failed with exception: {Exception}";

    }
}
