// <copyright file="Propagation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System;
    using System.Collections.Generic;

    internal static class Propagation
    {
        // Reused across calls to avoid allocating one frame per subscriber per change.
        // Safe to cache per thread: the marking walk runs no user code (flushing is
        // deferred until after the walk), so no nested Propagate can run while the
        // buffer is in use.
        [ThreadStatic]
        private static PropagationEntry[] stackBuffer;

        internal static void Propagate(ReactiveNode source)
        {
            var stack = stackBuffer ??= new PropagationEntry[64];
            var count = 0;

            for (var subscriber = source.SubscribersHead; subscriber != null; subscriber = subscriber.NextSubscriber)
            {
                if (count == stack.Length)
                {
                    stack = Grow(stack);
                }

                stack[count].Node = subscriber.Subscriber;
                stack[count].IsDirect = true;
                count++;
            }

            // Schedulers are held for the duration of the walk so that effects flush
            // only after every reachable node is marked; flushing mid-walk would let
            // an effect observe dependencies the walk has not yet marked as stale.
            EffectScheduler heldScheduler = null;
            List<EffectScheduler> heldSchedulers = null;

            try
            {
                while (count > 0)
                {
                    count--;
                    var node = stack[count].Node;
                    var isDirect = stack[count].IsDirect;
                    stack[count].Node = null;

                    if (node == null || node.IsDisposed)
                    {
                        continue;
                    }

                    if (node is EffectNode effectNode)
                    {
                        if (isDirect)
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
                    if (isDirect)
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
                        if (count == stack.Length)
                        {
                            stack = Grow(stack);
                        }

                        stack[count].Node = subscriber.Subscriber;
                        stack[count].IsDirect = false;
                        count++;
                    }
                }
            }
            finally
            {
                // On an exception mid-walk, clear remaining entries so the cached
                // buffer does not root nodes.
                while (count > 0)
                {
                    count--;
                    stack[count].Node = null;
                }

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

        private static PropagationEntry[] Grow(PropagationEntry[] stack)
        {
            var grown = new PropagationEntry[stack.Length * 2];
            Array.Copy(stack, grown, stack.Length);
            stackBuffer = grown;
            return grown;
        }

        private struct PropagationEntry
        {
            internal ReactiveNode Node;
            internal bool IsDirect;
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
