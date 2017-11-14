using System;
using System.Collections.Generic;
using Community.ContenType.DeploymentHub.Domain.Actions;
using Community.ContenType.DeploymentHub.Domain.Actions.Promote;
using Community.ContenType.DeploymentHub.Domain.Core;
using Community.ContenType.DeploymentHub.Domain.Verifiers;

namespace Community.ContenType.DeploymentHub.Domain.Events
{
    public class PromoteRequestInitiatedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }

        public PromoteRequestInitiatedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos) 
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
        }
    }

    public class PromoteRequestInitiateFailedEvent : Event
    {
        public ISet<ContentTypeInfo> ContentTypeInfos { get; }
        public Exception Exception { get; }

        public PromoteRequestInitiateFailedEvent(ActionContext actionContext, ISet<ContentTypeInfo> contentTypeInfos, Exception exception)
            : base(actionContext)
        {
            ContentTypeInfos = contentTypeInfos;
            Exception = exception;
        }
    }

    public class PromoteActionsCalculatedEvent : Event
    {
        public PromoteActionCollection ActionCollection { get; }

        public PromoteActionsCalculatedEvent(ActionContext actionContext, PromoteActionCollection actionCollection) 
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PromoteSiteColumnVerifiedEvent : Event
    {
        public PromoteSiteColumnAction Action { get; }
        public IPromoteVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PromoteSiteColumnVerifiedEvent(PromoteSiteColumnAction action, IPromoteVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Site column:{Action.SiteColumn.Title} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";

    }

    public class PromoteContentTypeVerifiedEvent : Event
    {
        public PromoteContentTypeAction Action { get; }
        public IPromoteVerificationRule Rule { get; }
        public VerificationRuleResult Result { get; }

        public PromoteContentTypeVerifiedEvent(PromoteContentTypeAction action, IPromoteVerificationRule rule, VerificationRuleResult result)
            : base(action.ActionContext)
        {
            Action = action;
            Rule = rule;
            Result = result;
        }
        public override string ToString() => base.ToString() + $": Content type:{Action.ContentType.Title} verified with rule:{Rule.Name} and level:{Rule.Level} has result:{Result}";

    }

    public class PromoteActionsVerifiedEvent : Event
    {
        public PromoteActionCollection ActionCollection { get; }

        public PromoteActionsVerifiedEvent(ActionContext actionContext, PromoteActionCollection actionCollection) 
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PromoteActionsImpactUpdatedEvent : Event
    {
        public PromoteActionCollection ActionCollection { get; }

        public PromoteActionsImpactUpdatedEvent(ActionContext actionContext, PromoteActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
    }

    public class PromoteSiteColumnActionExecutedEvent : Event
    {
        public PromoteSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }

        public PromoteSiteColumnActionExecutedEvent(PromoteSiteColumnAction action, int index, int totalCount) 
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
        }
        public override string ToString() => base.ToString() + $": Site column action executed: {Action.SiteColumn.Title} on {Action.TargetHub}";

    }

    public class PromoteSiteColumnActionFailedEvent : Event
    {
        public PromoteSiteColumnAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PromoteSiteColumnActionFailedEvent(PromoteSiteColumnAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Site column action execution failed: {Action.SiteColumn.Title} with exception:{Exception}";

    }

    public class PromoteContentTypeActionExecutedEvent : Event
    {
        public PromoteContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }

        public PromoteContentTypeActionExecutedEvent(PromoteContentTypeAction action, int index, int totalCount)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
        }
        public override string ToString() => base.ToString() + $": Content Type action executed: {Action.ContentType.Title}";

    }

    public class PromoteContentTypeActionFailedEvent : Event
    {
        public PromoteContentTypeAction Action { get; }
        public int Index { get; }
        public int TotalCount { get; }
        public Exception Exception { get; }

        public PromoteContentTypeActionFailedEvent(PromoteContentTypeAction action, int index, int totalCount, Exception exception)
            : base(action.ActionContext)
        {
            Action = action;
            Index = index;
            TotalCount = totalCount;
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Content Type action execution failed: {Action.ContentType.Title} with exception:{Exception}";

    }

    public class PromoteActionsExecutedEvent : Event
    {
        public PromoteActionCollection ActionCollection { get; }

        public PromoteActionsExecutedEvent(ActionContext actionContext, PromoteActionCollection actionCollection)
            : base(actionContext)
        {
            ActionCollection = actionCollection;
        }
        public override string ToString() => base.ToString() + $": {ActionCollection.TotalActionCount} promote actions complete. {ActionCollection.GetSuccessContentTypeActions()} content types and {ActionCollection.GetSuccessSiteColumnActions()} site columns succesfull";

    }

    public class PromoteActionsFailedEvent : Event
    {
        public Exception Exception { get; }

        public PromoteActionsFailedEvent(ActionContext actionContext, Exception exception)
            : base(actionContext)
        {
            Exception = exception;
        }
        public override string ToString() => base.ToString() + $": Promote failed with exception: {Exception}";

    }
}
