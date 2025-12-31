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
                // Try index-based evaluation first
                if (this.TryIndexBasedEvaluation())
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

        private bool TryIndexBasedEvaluation()
        {
            if (this.filter == null || !this.filter.Include)
            {
                return false;
            }

            // Find the first indexable predicate in the AND filter to get candidate object IDs
            var indexStore = this.Transaction.Database.IndexStore;
            HashSet<long> candidateObjectIds = null;

            foreach (var predicate in this.filter.Filters)
            {
                if (!predicate.Include)
                {
                    continue;
                }

                if (predicate is RoleUnitEquals roleUnitEquals)
                {
                    var (roleType, value) = roleUnitEquals.GetIndexKey();
                    if (roleType != null && value != null)
                    {
                        var objectIds = indexStore.GetObjectIdsByUnitRoleValue(roleType, value);
                        if (objectIds != null)
                        {
                            candidateObjectIds = objectIds;
                            break;
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
                            candidateObjectIds = objectIds;
                            break;
                        }
                    }
                }
            }

            if (candidateObjectIds == null)
            {
                return false;
            }

            // Filter candidates by type and predicates
            this.Strategies = new List<Strategy>();
            var concreteClasses = this.GetConcreteClasses();

            foreach (var objectId in candidateObjectIds)
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

                // Apply ALL filters including the indexed predicate.
                // The index gives us candidates from committed data, but the strategy
                // may have local modifications that change the role value.
                // We must always re-evaluate to get correct transaction-local results.
                if (this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                {
                    this.Strategies.Add(strategy);
                }
            }

            // Also check new objects in transaction that aren't in the committed store yet
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

                // Apply all filters including the indexed one
                if (this.filter.Evaluate(strategy) == ThreeValuedLogic.True)
                {
                    this.Strategies.Add(strategy);
                }
            }

            return true;
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
    }
}
