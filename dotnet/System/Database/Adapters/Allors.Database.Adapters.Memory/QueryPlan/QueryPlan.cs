// <copyright file="QueryPlan.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.QueryPlan
{
    using System;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Base class for query plan nodes.
    /// Query plans form a tree that describes how to execute a query efficiently.
    /// </summary>
    internal abstract class QueryPlanNode
    {
        /// <summary>
        /// Gets the estimated number of results from this node.
        /// Used by the query planner for cost-based optimization.
        /// </summary>
        internal abstract long EstimatedCardinality { get; }

        /// <summary>
        /// Gets the estimated cost of executing this node.
        /// Cost is a relative measure combining CPU and memory access.
        /// </summary>
        internal abstract double EstimatedCost { get; }

        /// <summary>
        /// Executes the plan node and returns matching object IDs.
        /// </summary>
        internal abstract IEnumerable<long> Execute(QueryContext context);
    }

    /// <summary>
    /// Context passed during query execution.
    /// Contains references to stores and caches needed for evaluation.
    /// </summary>
    internal sealed class QueryContext
    {
        internal QueryContext(
            CommittedStore committedStore,
            IndexStore indexStore,
            Transaction transaction)
        {
            this.CommittedStore = committedStore;
            this.IndexStore = indexStore;
            this.Transaction = transaction;
        }

        internal CommittedStore CommittedStore { get; }

        internal IndexStore IndexStore { get; }

        internal Transaction Transaction { get; }
    }

    /// <summary>
    /// Scan all objects of a specific type.
    /// Used as fallback when no index is available.
    /// </summary>
    internal sealed class TypeScanNode : QueryPlanNode
    {
        private readonly IComposite objectType;
        private readonly long estimatedCount;

        internal TypeScanNode(IComposite objectType, long estimatedCount)
        {
            this.objectType = objectType;
            this.estimatedCount = estimatedCount;
        }

        internal override long EstimatedCardinality => this.estimatedCount;

        // Full scan is expensive - cost proportional to total objects
        internal override double EstimatedCost => this.estimatedCount * 1.0;

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            foreach (var cls in this.objectType.Classes)
            {
                foreach (var objectId in context.CommittedStore.GetObjectIdsForType(cls))
                {
                    yield return objectId;
                }
            }
        }
    }

    /// <summary>
    /// Index lookup for a specific value.
    /// Very efficient when the value is selective.
    /// </summary>
    internal sealed class IndexLookupNode : QueryPlanNode
    {
        private readonly IRoleType roleType;
        private readonly object value;
        private readonly long estimatedMatches;

        internal IndexLookupNode(IRoleType roleType, object value, long estimatedMatches)
        {
            this.roleType = roleType;
            this.value = value;
            this.estimatedMatches = estimatedMatches;
        }

        internal override long EstimatedCardinality => this.estimatedMatches;

        // Index lookup is cheap - constant time plus result size
        internal override double EstimatedCost => 1.0 + (this.estimatedMatches * 0.1);

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            return context.IndexStore.GetObjectIdsByUnitRoleValue(this.roleType, this.value);
        }
    }

    /// <summary>
    /// Filter node that applies a predicate to results from a child node.
    /// </summary>
    internal sealed class FilterNode : QueryPlanNode
    {
        private readonly QueryPlanNode child;
        private readonly Func<long, QueryContext, bool> predicate;
        private readonly double selectivity;

        internal FilterNode(QueryPlanNode child, Func<long, QueryContext, bool> predicate, double selectivity)
        {
            this.child = child;
            this.predicate = predicate;
            this.selectivity = selectivity;
        }

        internal override long EstimatedCardinality => (long)(this.child.EstimatedCardinality * this.selectivity);

        // Filter cost = child cost + evaluation cost per result
        internal override double EstimatedCost => this.child.EstimatedCost + (this.child.EstimatedCardinality * 0.5);

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            foreach (var objectId in this.child.Execute(context))
            {
                if (this.predicate(objectId, context))
                {
                    yield return objectId;
                }
            }
        }
    }

    /// <summary>
    /// Intersection of two result sets.
    /// Uses sorted merge when both inputs are sorted.
    /// </summary>
    internal sealed class IntersectNode : QueryPlanNode
    {
        private readonly QueryPlanNode left;
        private readonly QueryPlanNode right;

        internal IntersectNode(QueryPlanNode left, QueryPlanNode right)
        {
            this.left = left;
            this.right = right;
        }

        internal override long EstimatedCardinality =>
            Math.Min(this.left.EstimatedCardinality, this.right.EstimatedCardinality);

        // Intersection cost = both child costs + merge cost
        internal override double EstimatedCost =>
            this.left.EstimatedCost + this.right.EstimatedCost +
            Math.Min(this.left.EstimatedCardinality, this.right.EstimatedCardinality) * 0.2;

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            // Materialize the smaller set first for efficient lookup
            var leftResults = new HashSet<long>(this.left.Execute(context));
            foreach (var objectId in this.right.Execute(context))
            {
                if (leftResults.Contains(objectId))
                {
                    yield return objectId;
                }
            }
        }
    }

    /// <summary>
    /// Union of two result sets.
    /// </summary>
    internal sealed class UnionNode : QueryPlanNode
    {
        private readonly QueryPlanNode left;
        private readonly QueryPlanNode right;

        internal UnionNode(QueryPlanNode left, QueryPlanNode right)
        {
            this.left = left;
            this.right = right;
        }

        internal override long EstimatedCardinality =>
            this.left.EstimatedCardinality + this.right.EstimatedCardinality;

        internal override double EstimatedCost =>
            this.left.EstimatedCost + this.right.EstimatedCost +
            (this.left.EstimatedCardinality + this.right.EstimatedCardinality) * 0.1;

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            var seen = new HashSet<long>();
            foreach (var objectId in this.left.Execute(context))
            {
                if (seen.Add(objectId))
                {
                    yield return objectId;
                }
            }

            foreach (var objectId in this.right.Execute(context))
            {
                if (seen.Add(objectId))
                {
                    yield return objectId;
                }
            }
        }
    }

    /// <summary>
    /// Difference of two result sets (left - right).
    /// </summary>
    internal sealed class ExceptNode : QueryPlanNode
    {
        private readonly QueryPlanNode left;
        private readonly QueryPlanNode right;

        internal ExceptNode(QueryPlanNode left, QueryPlanNode right)
        {
            this.left = left;
            this.right = right;
        }

        internal override long EstimatedCardinality =>
            Math.Max(0, this.left.EstimatedCardinality - this.right.EstimatedCardinality);

        internal override double EstimatedCost =>
            this.left.EstimatedCost + this.right.EstimatedCost +
            this.right.EstimatedCardinality * 0.2;

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            // Materialize the right set for efficient exclusion check
            var rightResults = new HashSet<long>(this.right.Execute(context));
            foreach (var objectId in this.left.Execute(context))
            {
                if (!rightResults.Contains(objectId))
                {
                    yield return objectId;
                }
            }
        }
    }

    /// <summary>
    /// Navigates an association from target objects to source objects.
    /// </summary>
    internal sealed class AssociationNavigateNode : QueryPlanNode
    {
        private readonly QueryPlanNode targetNode;
        private readonly IAssociationType associationType;
        private readonly double fanOut;

        internal AssociationNavigateNode(QueryPlanNode targetNode, IAssociationType associationType, double fanOut)
        {
            this.targetNode = targetNode;
            this.associationType = associationType;
            this.fanOut = fanOut;
        }

        internal override long EstimatedCardinality =>
            (long)(this.targetNode.EstimatedCardinality * this.fanOut);

        internal override double EstimatedCost =>
            this.targetNode.EstimatedCost + this.targetNode.EstimatedCardinality * 1.0;

        internal override IEnumerable<long> Execute(QueryContext context)
        {
            var roleType = this.associationType.RoleType;
            var seen = new HashSet<long>();

            foreach (var targetId in this.targetNode.Execute(context))
            {
                // Get objects that have this target in their role
                var sourceIds = context.IndexStore.GetObjectIdsByCompositeRole(roleType, targetId);
                if (sourceIds != null)
                {
                    foreach (var sourceId in sourceIds)
                    {
                        if (seen.Add(sourceId))
                        {
                            yield return sourceId;
                        }
                    }
                }
            }
        }
    }
}
