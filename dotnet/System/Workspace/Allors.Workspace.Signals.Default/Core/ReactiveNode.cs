// <copyright file="ReactiveNode.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System;

    internal abstract class ReactiveNode
    {
        internal ReactiveFlags Flags;
        internal Link DependenciesHead;
        internal Link DependenciesTail;
        internal Link SubscribersHead;
        internal Link SubscribersTail;
        internal ulong Version;

        internal bool IsDisposed => (this.Flags & ReactiveFlags.Disposed) != 0;

        internal void TrackRead()
        {
            var subscriber = TrackingContext.ActiveSubscriber;
            if (this.IsDisposed || subscriber == null || ReferenceEquals(subscriber, this) || subscriber.IsDisposed)
            {
                return;
            }

            Link link = null;
            for (var current = subscriber.DependenciesHead; current != null; current = current.NextDependency)
            {
                if (ReferenceEquals(current.Dependency, this))
                {
                    link = current;
                    break;
                }
            }

            if (link == null)
            {
                link = new Link
                {
                    Dependency = this,
                    Subscriber = subscriber,
                };

                this.AppendSubscriberLink(link);
                subscriber.AppendDependencyLink(link);
            }

            link.Retained = true;
            link.Version = this.Version;
        }

        internal void DetachDependencies()
        {
            var dependency = this.DependenciesHead;
            while (dependency != null)
            {
                var next = dependency.NextDependency;
                DetachLink(dependency);
                dependency = next;
            }
        }

        internal void DetachSubscribers()
        {
            var subscriber = this.SubscribersHead;
            while (subscriber != null)
            {
                var next = subscriber.NextSubscriber;
                DetachLink(subscriber);
                subscriber = next;
            }
        }

        protected static void DetachLink(Link link)
        {
            var dependency = link.Dependency;
            if (dependency != null)
            {
                if (link.PreviousSubscriber != null)
                {
                    link.PreviousSubscriber.NextSubscriber = link.NextSubscriber;
                }
                else
                {
                    dependency.SubscribersHead = link.NextSubscriber;
                }

                if (link.NextSubscriber != null)
                {
                    link.NextSubscriber.PreviousSubscriber = link.PreviousSubscriber;
                }
                else
                {
                    dependency.SubscribersTail = link.PreviousSubscriber;
                }
            }

            var subscriber = link.Subscriber;
            if (subscriber != null)
            {
                if (link.PreviousDependency != null)
                {
                    link.PreviousDependency.NextDependency = link.NextDependency;
                }
                else
                {
                    subscriber.DependenciesHead = link.NextDependency;
                }

                if (link.NextDependency != null)
                {
                    link.NextDependency.PreviousDependency = link.PreviousDependency;
                }
                else
                {
                    subscriber.DependenciesTail = link.PreviousDependency;
                }
            }

            link.Dependency = null;
            link.Subscriber = null;
            link.NextDependency = null;
            link.PreviousDependency = null;
            link.NextSubscriber = null;
            link.PreviousSubscriber = null;
            link.Retained = false;
            link.Version = 0;
        }

        private void AppendSubscriberLink(Link link)
        {
            if (this.SubscribersTail == null)
            {
                this.SubscribersHead = link;
                this.SubscribersTail = link;
                return;
            }

            link.PreviousSubscriber = this.SubscribersTail;
            this.SubscribersTail.NextSubscriber = link;
            this.SubscribersTail = link;
        }

        private void AppendDependencyLink(Link link)
        {
            if (this.DependenciesTail == null)
            {
                this.DependenciesHead = link;
                this.DependenciesTail = link;
                return;
            }

            link.PreviousDependency = this.DependenciesTail;
            this.DependenciesTail.NextDependency = link;
            this.DependenciesTail = link;
        }
    }

    internal abstract class ComputationNode : ReactiveNode
    {
        internal abstract bool HasValue { get; }

        internal bool NeedsEvaluation => !this.HasValue || (this.Flags & (ReactiveFlags.Dirty | ReactiveFlags.Pending)) != 0;

        internal abstract bool Update();

        internal void ClearDirtyFlags() => this.Flags &= ~(ReactiveFlags.Dirty | ReactiveFlags.Pending);

        internal IDisposable BeginTracking()
        {
            for (var dependency = this.DependenciesHead; dependency != null; dependency = dependency.NextDependency)
            {
                dependency.Retained = false;
            }

            return new TrackingLease(this);
        }

        private void EndTracking()
        {
            var dependency = this.DependenciesHead;
            while (dependency != null)
            {
                var next = dependency.NextDependency;
                if (!dependency.Retained)
                {
                    DetachLink(dependency);
                }

                dependency = next;
            }
        }

        private sealed class TrackingLease : IDisposable
        {
            private readonly ComputationNode node;
            private readonly ComputationNode previousSubscriber;

            internal TrackingLease(ComputationNode node)
            {
                this.node = node;
                this.previousSubscriber = TrackingContext.ActiveSubscriber;
                TrackingContext.ActiveSubscriber = node;
            }

            public void Dispose()
            {
                TrackingContext.ActiveSubscriber = this.previousSubscriber;
                this.node.EndTracking();
            }
        }
    }

    internal abstract class ComputedNode : ComputationNode
    {
    }
}
