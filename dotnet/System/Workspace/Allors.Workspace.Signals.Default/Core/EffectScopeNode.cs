// <copyright file="EffectScopeNode.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using Allors.Workspace.Signals;

    internal sealed class EffectScopeNode : IEffectScope
    {
        private readonly EffectScopeNode previousScope;
        private EffectNode effectsHead;
        private EffectScopeNode childHead;
        private EffectScopeNode childTail;
        private EffectScopeNode nextSibling;
        private EffectScopeNode previousSibling;
        private bool disposed;

        internal EffectScopeNode()
        {
            this.previousScope = TrackingContext.ActiveScope;
            TrackingContext.ActiveScope = this;
            this.previousScope?.AddChild(this);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            // The active scope may be a descendant about to be disposed with this subtree;
            // restoring it here, before recursing, keeps it from ever pointing at a disposed scope.
            if (this.ContainsActiveScope())
            {
                TrackingContext.ActiveScope = this.previousScope;
            }

            var child = this.childHead;
            while (child != null)
            {
                var next = child.nextSibling;
                child.Dispose();
                child = next;
            }

            var effect = this.effectsHead;
            while (effect != null)
            {
                var next = effect.ScopeNext;
                effect.Dispose();
                effect = next;
            }

            if (this.previousSibling != null)
            {
                this.previousSibling.nextSibling = this.nextSibling;
            }
            else if (this.previousScope != null)
            {
                this.previousScope.childHead = this.nextSibling;
            }

            if (this.nextSibling != null)
            {
                this.nextSibling.previousSibling = this.previousSibling;
            }
            else if (this.previousScope != null)
            {
                this.previousScope.childTail = this.previousSibling;
            }

            // previousScope is intentionally kept: ContainsActiveScope walks it on disposed nodes.
            this.nextSibling = null;
            this.previousSibling = null;
            this.childHead = null;
            this.childTail = null;
            this.effectsHead = null;
        }

        private bool ContainsActiveScope()
        {
            var scope = TrackingContext.ActiveScope;
            while (scope != null)
            {
                if (scope == this)
                {
                    return true;
                }

                scope = scope.previousScope;
            }

            return false;
        }

        internal void AddEffect(EffectNode effect)
        {
            effect.Scope = this;
            effect.ScopeNext = this.effectsHead;
            if (this.effectsHead != null)
            {
                this.effectsHead.ScopePrevious = effect;
            }

            this.effectsHead = effect;
        }

        internal void RemoveEffect(EffectNode effect)
        {
            if (!ReferenceEquals(effect.Scope, this))
            {
                return;
            }

            if (effect.ScopePrevious != null)
            {
                effect.ScopePrevious.ScopeNext = effect.ScopeNext;
            }
            else
            {
                this.effectsHead = effect.ScopeNext;
            }

            if (effect.ScopeNext != null)
            {
                effect.ScopeNext.ScopePrevious = effect.ScopePrevious;
            }

            effect.Scope = null;
            effect.ScopeNext = null;
            effect.ScopePrevious = null;
        }

        private void AddChild(EffectScopeNode child)
        {
            if (this.childTail == null)
            {
                this.childHead = child;
                this.childTail = child;
                return;
            }

            child.previousSibling = this.childTail;
            this.childTail.nextSibling = child;
            this.childTail = child;
        }
    }
}
