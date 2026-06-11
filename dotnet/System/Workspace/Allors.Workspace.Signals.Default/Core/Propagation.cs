// <copyright file="Propagation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System.Collections.Generic;

    internal static class Propagation
    {
        internal static void Propagate(ReactiveNode source)
        {
            PropagationFrame stack = null;
            for (var subscriber = source.SubscribersHead; subscriber != null; subscriber = subscriber.NextSubscriber)
            {
                stack = new PropagationFrame(subscriber.Subscriber, true, stack);
            }

            // Schedulers are held for the duration of the walk so that effects flush
            // only after every reachable node is marked; flushing mid-walk would let
            // an effect observe dependencies the walk has not yet marked as stale.
            EffectScheduler heldScheduler = null;
            List<EffectScheduler> heldSchedulers = null;

            try
            {
                while (stack != null)
                {
                    var frame = stack;
                    stack = frame.Previous;

                    var node = frame.Node;
                    if (node == null || node.IsDisposed)
                    {
                        continue;
                    }

                    if (node is EffectNode effectNode)
                    {
                        if (frame.IsDirect)
                        {
                            effectNode.Flags |= ReactiveFlags.Dirty;
                        }
                        else if ((effectNode.Flags & ReactiveFlags.Dirty) == 0)
                        {
                            effectNode.Flags |= ReactiveFlags.Pending;
                        }

                        var scheduler = effectNode.Scheduler;
                        if (heldScheduler == null)
                        {
                            heldScheduler = scheduler;
                            scheduler.Hold();
                        }
                        else if (!ReferenceEquals(heldScheduler, scheduler) &&
                                 (heldSchedulers == null || !heldSchedulers.Contains(scheduler)))
                        {
                            heldSchedulers ??= new List<EffectScheduler>();
                            heldSchedulers.Add(scheduler);
                            scheduler.Hold();
                        }

                        scheduler.Enqueue(effectNode);
                        continue;
                    }

                    var previous = node.Flags & (ReactiveFlags.Dirty | ReactiveFlags.Pending);
                    if (frame.IsDirect)
                    {
                        node.Flags |= ReactiveFlags.Dirty;
                    }
                    else if ((node.Flags & ReactiveFlags.Dirty) == 0)
                    {
                        node.Flags |= ReactiveFlags.Pending;
                    }

                    if (previous != ReactiveFlags.None)
                    {
                        continue;
                    }

                    for (var subscriber = node.SubscribersHead; subscriber != null; subscriber = subscriber.NextSubscriber)
                    {
                        stack = new PropagationFrame(subscriber.Subscriber, false, stack);
                    }
                }
            }
            finally
            {
                heldScheduler?.Release();
                if (heldSchedulers != null)
                {
                    foreach (var scheduler in heldSchedulers)
                    {
                        scheduler.Release();
                    }
                }
            }
        }

        internal static bool CheckDirty(ComputationNode node)
        {
            UpdateDependencies(node);

            if (!node.HasValue)
            {
                return true;
            }

            for (var dependency = node.DependenciesHead; dependency != null; dependency = dependency.NextDependency)
            {
                if (dependency.Version != dependency.Dependency.Version)
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateDependencies(ComputationNode root)
        {
            EvaluationFrame stack = null;
            for (var dependency = root.DependenciesHead; dependency != null; dependency = dependency.NextDependency)
            {
                if (dependency.Dependency is ComputedNode computed && computed.NeedsEvaluation && (computed.Flags & ReactiveFlags.Checking) == 0)
                {
                    computed.Flags |= ReactiveFlags.Checking;
                    stack = new EvaluationFrame(computed, stack);
                }
            }

            try
            {
                while (stack != null)
                {
                    var frame = stack;

                    var pushedChild = false;
                    while (frame.CurrentDependency != null)
                    {
                        var dependency = frame.CurrentDependency;
                        frame.CurrentDependency = dependency.NextDependency;

                        if (dependency.Dependency is ComputedNode child && child.NeedsEvaluation && (child.Flags & ReactiveFlags.Checking) == 0)
                        {
                            child.Flags |= ReactiveFlags.Checking;
                            stack = new EvaluationFrame(child, frame);
                            pushedChild = true;
                            break;
                        }
                    }

                    if (pushedChild)
                    {
                        continue;
                    }

                    frame.Node.Flags &= ~ReactiveFlags.Checking;

                    if (!frame.Node.HasValue || HasDependencyChanges(frame.Node))
                    {
                        frame.Node.Update();
                    }
                    else
                    {
                        frame.Node.ClearDirtyFlags();
                    }

                    stack = frame.Previous;
                }
            }
            finally
            {
                while (stack != null)
                {
                    stack.Node.Flags &= ~ReactiveFlags.Checking;
                    stack = stack.Previous;
                }
            }
        }

        private static bool HasDependencyChanges(ComputationNode node)
        {
            for (var dependency = node.DependenciesHead; dependency != null; dependency = dependency.NextDependency)
            {
                if (dependency.Version != dependency.Dependency.Version)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class PropagationFrame
        {
            internal PropagationFrame(ReactiveNode node, bool isDirect, PropagationFrame previous)
            {
                this.Node = node;
                this.IsDirect = isDirect;
                this.Previous = previous;
            }

            internal ReactiveNode Node { get; }

            internal bool IsDirect { get; }

            internal PropagationFrame Previous { get; }
        }

        private sealed class EvaluationFrame
        {
            internal EvaluationFrame(ComputedNode node, EvaluationFrame previous)
            {
                this.Node = node;
                this.CurrentDependency = node.DependenciesHead;
                this.Previous = previous;
            }

            internal ComputedNode Node { get; }

            internal EvaluationFrame Previous { get; }

            internal Link CurrentDependency { get; set; }
        }
    }
}
