// <copyright file="QueryPlanner.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.QueryPlan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    /// <summary>
    /// Cost-based query optimizer that selects the best execution plan.
    /// Analyzes predicates and indexes to determine the most efficient query execution strategy.
    /// </summary>
    internal sealed class QueryPlanner
    {
        private readonly Statistics statistics;
        private readonly IndexStore indexStore;

        internal QueryPlanner(Statistics statistics, IndexStore indexStore)
        {
            this.statistics = statistics;
            this.indexStore = indexStore;
        }

        /// <summary>
        /// Plans a query that returns objects of a specific type matching predicates.
        /// </summary>
        internal QueryPlanNode Plan(
            IComposite objectType,
            IReadOnlyList<PredicateInfo> predicates)
        {
            if (predicates == null || predicates.Count == 0)
            {
                // No predicates - just scan the type
                var typeCount = this.statistics.GetTypeCount(objectType);
                return new TypeScanNode(objectType, typeCount);
            }

            // Find the best starting point (lowest cardinality indexable predicate)
            QueryPlanNode bestStart = null;
            PredicateInfo bestPredicate = null;
            var bestCardinality = long.MaxValue;

            foreach (var predicate in predicates)
            {
                if (predicate.IsIndexable && this.indexStore.HasIndex(predicate.RoleType))
                {
                    var cardinality = this.EstimatePredicateCardinality(predicate);
                    if (cardinality < bestCardinality)
                    {
                        bestCardinality = cardinality;
                        bestPredicate = predicate;
                        bestStart = this.CreateIndexNode(predicate);
                    }
                }
            }

            // If no indexable predicate found, start with type scan
            if (bestStart == null)
            {
                var typeCount = this.statistics.GetTypeCount(objectType);
                bestStart = new TypeScanNode(objectType, typeCount);
            }

            // Apply remaining predicates as filters, ordered by selectivity
            var remainingPredicates = predicates
                .Where(p => p != bestPredicate)
                .OrderBy(p => this.EstimateSelectivity(p))
                .ToList();

            var result = bestStart;
            foreach (var predicate in remainingPredicates)
            {
                var selectivity = this.EstimateSelectivity(predicate);
                result = new FilterNode(result, predicate.Evaluate, selectivity);
            }

            return result;
        }

        /// <summary>
        /// Plans an intersection query (AND of multiple extents).
        /// </summary>
        internal QueryPlanNode PlanIntersect(IReadOnlyList<QueryPlanNode> children)
        {
            if (children.Count == 0)
            {
                return null;
            }

            if (children.Count == 1)
            {
                return children[0];
            }

            // Order by cardinality (smallest first) for efficient intersection
            var ordered = children.OrderBy(c => c.EstimatedCardinality).ToList();
            var result = ordered[0];
            for (var i = 1; i < ordered.Count; i++)
            {
                result = new IntersectNode(result, ordered[i]);
            }

            return result;
        }

        /// <summary>
        /// Plans a union query (OR of multiple extents).
        /// </summary>
        internal QueryPlanNode PlanUnion(IReadOnlyList<QueryPlanNode> children)
        {
            if (children.Count == 0)
            {
                return null;
            }

            if (children.Count == 1)
            {
                return children[0];
            }

            var result = children[0];
            for (var i = 1; i < children.Count; i++)
            {
                result = new UnionNode(result, children[i]);
            }

            return result;
        }

        /// <summary>
        /// Plans a difference query (first extent minus others).
        /// </summary>
        internal QueryPlanNode PlanExcept(QueryPlanNode left, QueryPlanNode right)
        {
            return new ExceptNode(left, right);
        }

        private QueryPlanNode CreateIndexNode(PredicateInfo predicate)
        {
            var cardinality = this.EstimatePredicateCardinality(predicate);
            return new IndexLookupNode(predicate.RoleType, predicate.Value, cardinality);
        }

        private long EstimatePredicateCardinality(PredicateInfo predicate)
        {
            if (predicate.RoleType == null)
            {
                return long.MaxValue;
            }

            // Use statistics if available
            var valueCount = this.statistics.GetValueCount(predicate.RoleType, predicate.Value);
            if (valueCount >= 0)
            {
                return valueCount;
            }

            // Fall back to role cardinality with selectivity estimate
            var roleCount = this.statistics.GetRoleCount(predicate.RoleType);
            var distinctValues = this.statistics.GetDistinctValueCount(predicate.RoleType);
            if (distinctValues > 0)
            {
                return roleCount / distinctValues;
            }

            // Default estimate
            return roleCount / 10;
        }

        private double EstimateSelectivity(PredicateInfo predicate)
        {
            if (predicate.RoleType == null)
            {
                return 0.5; // Unknown - assume 50%
            }

            var distinctValues = this.statistics.GetDistinctValueCount(predicate.RoleType);
            if (distinctValues > 0)
            {
                return 1.0 / distinctValues;
            }

            return predicate.PredicateType switch
            {
                PredicateType.Equals => 0.1,     // Equality is usually selective
                PredicateType.Like => 0.2,       // Like is less selective
                PredicateType.LessThan => 0.3,   // Range queries vary
                PredicateType.GreaterThan => 0.3,
                PredicateType.Between => 0.2,
                PredicateType.Exists => 0.5,     // Existence checks are 50/50
                PredicateType.Contains => 0.1,   // Contains in collection
                _ => 0.5
            };
        }
    }

    /// <summary>
    /// Information about a predicate for query planning.
    /// </summary>
    internal sealed class PredicateInfo
    {
        internal PredicateInfo(
            PredicateType predicateType,
            IRoleType roleType,
            object value,
            bool isIndexable,
            Func<long, QueryContext, bool> evaluate)
        {
            this.PredicateType = predicateType;
            this.RoleType = roleType;
            this.Value = value;
            this.IsIndexable = isIndexable;
            this.Evaluate = evaluate;
        }

        internal PredicateType PredicateType { get; }

        internal IRoleType RoleType { get; }

        internal object Value { get; }

        internal bool IsIndexable { get; }

        internal Func<long, QueryContext, bool> Evaluate { get; }
    }

    /// <summary>
    /// Types of predicates for selectivity estimation.
    /// </summary>
    internal enum PredicateType
    {
        Equals,
        NotEquals,
        LessThan,
        LessThanOrEquals,
        GreaterThan,
        GreaterThanOrEquals,
        Between,
        Like,
        Exists,
        Contains,
        InstanceOf,
        And,
        Or,
        Not
    }
}
