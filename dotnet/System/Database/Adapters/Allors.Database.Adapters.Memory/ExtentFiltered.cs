// <copyright file="ExtentFiltered.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class ExtentFiltered : Extent
    {
        private readonly IComposite objectType;

        private And filter;

        internal ExtentFiltered(Transaction transaction, IComposite objectType)
            : base(transaction) =>
            this.objectType = objectType;

        public override ICompositePredicate Filter => this.filter ??= new And(this);

        public override IComposite ObjectType => this.objectType;

        internal void CheckForAssociationType(IAssociationType association)
        {
            if (!this.objectType.ExistAssociationType(association))
            {
                throw new ArgumentException("Extent does not have association " + association);
            }
        }

        internal void CheckForRoleType(IRoleType roleType)
        {
            if (!this.objectType.ExistRoleType(roleType))
            {
                throw new ArgumentException("Extent does not have role " + roleType.SingularName);
            }
        }

        protected override void Evaluate()
        {
            if (this.Strategies == null)
            {
                // Try multi-index evaluation first (most efficient)
                if (this.TryMultiIndexEvaluation())
                {
                    if (this.Sorter != null)
                    {
                        this.Strategies.Sort(this.Sorter);
                    }

                    return;
                }

                // Fallback to full scan
                this.Strategies = new List<Strategy>();

                foreach (var strategy in this.Transaction.GetStrategiesForExtentIncludingDeleted(this.objectType))
                {
                    if (!strategy.IsDeleted)
                    {
                        if (this.filter?.Include != true || this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                        {
                            this.Strategies.Add(strategy);
                        }
                    }
                }

                if (this.Sorter != null)
                {
                    this.Strategies.Sort(this.Sorter);
                }
            }
        }

        /// <summary>
        /// Attempts to use multiple indexes to evaluate the query.
        /// Collects all indexable predicates, retrieves their index results,
        /// intersects them using efficient O(n+m) sorted merge, then filters the result.
        /// </summary>
        private bool TryMultiIndexEvaluation()
        {
            if (this.filter == null || !this.filter.Include)
            {
                return false;
            }

            var indexStore = this.Transaction.Database.IndexStore;

            // Collect all index results from indexable predicates
            var indexResults = new List<IndexResult>();

            foreach (var predicate in this.filter.Filters)
            {
                if (!predicate.Include)
                {
                    continue;
                }

                var indexResult = this.TryGetIndexResult(predicate, indexStore);
                if (indexResult != null)
                {
                    indexResults.Add(indexResult.Value);
                }
            }

            // No indexes available - fall back to full scan
            if (indexResults.Count == 0)
            {
                return false;
            }

            // Sort by cardinality (smallest first) for efficient intersection
            indexResults.Sort((a, b) => a.ObjectIds.Count.CompareTo(b.ObjectIds.Count));

            // Compute intersection of all index results
            long[] candidateIds;
            if (indexResults.Count == 1)
            {
                // Single index - just convert to sorted array
                candidateIds = SetOperations.ToSortedArray(indexResults[0].ObjectIds);
            }
            else
            {
                // Multiple indexes - intersect them all
                candidateIds = this.IntersectIndexResults(indexResults);
            }

            // Early exit if intersection is empty
            if (candidateIds.Length == 0)
            {
                this.Strategies = new List<Strategy>();
                this.AddNewAndModifiedObjects();
                return true;
            }

            // Filter candidates by type and all predicates
            this.Strategies = new List<Strategy>(Math.Min(candidateIds.Length, 1000));
            var concreteClasses = this.GetConcreteClasses();

            foreach (var objectId in candidateIds)
            {
                var strategy = this.Transaction.InstantiateMemoryStrategy(objectId);
                if (strategy == null || strategy.IsDeleted)
                {
                    continue;
                }

                // Check type compatibility
                if (!concreteClasses.Contains(strategy.UncheckedObjectType))
                {
                    continue;
                }

                // Apply ALL filters including the indexed predicates.
                // The index gives us candidates from committed data, but the strategy
                // may have local modifications that change the role value.
                // We must always re-evaluate to get correct transaction-local results.
                if (this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                {
                    this.Strategies.Add(strategy);
                }
            }

            // Add new and modified objects that might match
            this.AddNewAndModifiedObjects();

            return true;
        }

        /// <summary>
        /// Tries to get index results for a predicate.
        /// Returns null if the predicate is not indexable or no index exists.
        /// </summary>
        private IndexResult? TryGetIndexResult(Predicate predicate, IndexStore indexStore)
        {
            if (predicate is RoleUnitEquals roleUnitEquals)
            {
                var (roleType, value) = roleUnitEquals.GetIndexKey();
                if (roleType != null && value != null)
                {
                    var objectIds = indexStore.GetObjectIdsByUnitRoleValue(roleType, value);
                    if (objectIds != null)
                    {
                        return new IndexResult(objectIds, predicate);
                    }
                }
            }
            else if (predicate is RoleCompositeEqualsValue roleCompositeEquals)
            {
                var (roleType, targetId) = roleCompositeEquals.GetIndexKey();
                if (roleType != null && targetId.HasValue)
                {
                    var objectIds = indexStore.GetObjectIdsByCompositeRole(roleType, targetId.Value);
                    if (objectIds != null)
                    {
                        return new IndexResult(objectIds, predicate);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Intersects multiple index results using efficient O(n+m) sorted merge algorithm.
        /// Results are sorted by cardinality before intersection for optimal performance.
        /// </summary>
        private long[] IntersectIndexResults(List<IndexResult> indexResults)
        {
            // Start with the smallest set
            var result = SetOperations.ToSortedArray(indexResults[0].ObjectIds);

            // Intersect with each subsequent set
            for (var i = 1; i < indexResults.Count; i++)
            {
                if (result.Length == 0)
                {
                    break; // Early exit - intersection is already empty
                }

                var nextSet = SetOperations.ToSortedArray(indexResults[i].ObjectIds);
                result = SetOperations.IntersectSorted(result, nextSet);
            }

            return result;
        }

        /// <summary>
        /// Adds new and modified objects that might match the filter.
        /// These objects are not in the committed store indexes yet.
        /// </summary>
        private void AddNewAndModifiedObjects()
        {
            var concreteClasses = this.GetConcreteClasses();
            var foundObjectIds = new HashSet<long>(this.Strategies.Select(s => s.ObjectId));

            // Check new objects in transaction that aren't in the committed store yet
            foreach (var strategy in this.Transaction.GetNewStrategiesForType(this.objectType))
            {
                if (strategy.IsDeleted)
                {
                    continue;
                }

                // Check type compatibility
                if (!concreteClasses.Contains(strategy.UncheckedObjectType))
                {
                    continue;
                }

                // Apply all filters
                if (this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                {
                    this.Strategies.Add(strategy);
                    foundObjectIds.Add(strategy.ObjectId);
                }
            }

            // Check modified objects whose role values may have changed to match the query
            // The index contains committed values, but locally modified values might now match
            foreach (var strategy in this.Transaction.GetModifiedStrategiesForType(this.objectType))
            {
                // Skip if already found via index lookup
                if (foundObjectIds.Contains(strategy.ObjectId))
                {
                    continue;
                }

                if (strategy.IsDeleted)
                {
                    continue;
                }

                // Check type compatibility
                if (!concreteClasses.Contains(strategy.UncheckedObjectType))
                {
                    continue;
                }

                // Apply all filters - this checks the current (modified) role values
                if (this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                {
                    this.Strategies.Add(strategy);
                }
            }
        }

        private HashSet<IObjectType> GetConcreteClasses()
        {
            var classes = new HashSet<IObjectType>();

            if (this.objectType is IClass @class)
            {
                classes.Add(@class);
            }

            if (this.objectType is IInterface @interface)
            {
                foreach (var subClass in @interface.DatabaseClasses)
                {
                    classes.Add(subClass);
                }
            }

            return classes;
        }

        /// <summary>
        /// Represents an index lookup result with its source predicate.
        /// </summary>
        private readonly struct IndexResult
        {
            public readonly IReadOnlySet<long> ObjectIds;
            public readonly Predicate SourcePredicate;

            public IndexResult(IReadOnlySet<long> objectIds, Predicate sourcePredicate)
            {
                this.ObjectIds = objectIds;
                this.SourcePredicate = sourcePredicate;
            }
        }
    }
}
