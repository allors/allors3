// <copyright file="RecordBasedOriginState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Ranges;

    public abstract class RecordBasedOriginState
    {
        public abstract Strategy Strategy { get; }

        protected bool HasChanges => this.Record == null || this.ChangedRoleByRelationType?.Count > 0;

        protected abstract IEnumerable<IRoleType> RoleTypes { get; }

        protected abstract IRecord Record { get; }

        protected IRecord PreviousRecord { get; set; }

        public Dictionary<IRelationType, object> ChangedRoleByRelationType { get; private set; }

        private Dictionary<IRelationType, object> PreviousChangedRoleByRelationType { get; set; }

        public object GetUnitRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var role))
            {
                return role;
            }

            return this.Record?.GetRole(roleType);
        }

        public void SetUnitRole(IRoleType roleType, object role) { this.Session.AssertEditable(); this.SetChangedRole(roleType, role); }

        public Strategy GetCompositeRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return (Strategy)changedRole;
            }

            var role = this.Record?.GetRole(roleType);

            if (role == null)
            {
                return null;
            }

            var strategy = this.Session.GetStrategy((long)role);
            this.AssertStrategy(strategy);
            return strategy;
        }

        public void SetCompositeRole(IRoleType roleType, Strategy role)
        {
            this.Session.AssertEditable();
            if (this.SameCompositeRole(roleType, role))
            {
                return;
            }

            var associationType = roleType.AssociationType;
            if (associationType.IsOne && role != null)
            {
                var previousAssociation = this.Session.GetCompositeAssociation(role, associationType);
                this.SetChangedRole(roleType, role);
                if (associationType.IsOne && previousAssociation != null)
                {
                    // OneToOne
                    previousAssociation.SetRole(roleType, null);
                }
            }
            else
            {
                this.SetChangedRole(roleType, role);
            }
        }

        public IRange<Strategy> GetCompositesRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return (IRange<Strategy>)changedRole;
            }

            var role = (IRange<long>)this.Record?.GetRole(roleType);

            if (role == null)
            {
                return EmptyRange<Strategy>.Instance;
            }

            return this.Ranges.Load(role.Select(v =>
            {
                var strategy = this.Session.GetStrategy(v);
                this.AssertStrategy(strategy);
                return strategy;
            }));
        }

        public void AddCompositesRole(IRoleType roleType, Strategy roleToAdd)
        {
            this.Session.AssertEditable();
            var associationType = roleType.AssociationType;
            var previousAssociation = this.Session.GetCompositeAssociation(roleToAdd, associationType);

            var role = this.GetCompositesRole(roleType);

            if (role.Contains(roleToAdd))
            {
                return;
            }

            role = this.Ranges.Add(role, roleToAdd);
            this.SetChangedRole(roleType, role);

            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            previousAssociation?.SetRole(roleType, null);
        }

        public void RemoveCompositesRole(IRoleType roleType, Strategy roleToRemove)
        {
            this.Session.AssertEditable();
            var role = this.GetCompositesRole(roleType);

            if (!role.Contains(roleToRemove))
            {
                return;
            }

            role = this.Ranges.Remove(role, roleToRemove);
            this.SetChangedRole(roleType, role);
        }

        public void SetCompositesRole(IRoleType roleType, IRange<Strategy> role)
        {
            this.Session.AssertEditable();
            if (this.SameCompositesRole(roleType, role))
            {
                return;
            }

            var previousRole = this.GetCompositesRoleIfInstantiated(roleType);

            this.SetChangedRole(roleType, role);

            var associationType = roleType.AssociationType;
            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            foreach (var addedRole in role.Except(previousRole))
            {
                var previousAssociation = this.Session.GetCompositeAssociation(addedRole, associationType);
                previousAssociation?.SetRole(roleType, null);
            }
        }

        public virtual void Checkpoint(ChangeSet changeSet)
        {
            this.Session.AssertEditable();
            if (this.ExactCheckpointRecords)
            {
                this.CheckpointExact(changeSet);
                return;
            }

            // Same record
            if (this.PreviousRecord == null || this.Record == null || this.Record.Version == this.PreviousRecord.Version)
            {
                if (this.ChangedRoleByRelationType != null)
                {
                    foreach (var kvp in this.ChangedRoleByRelationType)
                    {
                        var relationType = kvp.Key;
                        var current = kvp.Value;

                        if (this.PreviousChangedRoleByRelationType != null && this.PreviousChangedRoleByRelationType.TryGetValue(relationType, out var previousChangedRole))
                        {
                            if (relationType.RoleType.ObjectType.IsUnit)
                            {
                                changeSet.DiffUnit(this.Strategy, relationType, current, previousChangedRole);
                            }
                            else if (relationType.RoleType.IsOne)
                            {
                                changeSet.DiffComposite(this.Strategy, relationType, (Strategy)current, (Strategy)previousChangedRole);
                            }
                            else
                            {
                                changeSet.DiffComposites(this.Strategy, relationType, (IRange<Strategy>)current, (IRange<Strategy>)previousChangedRole);
                            }
                        }
                        else
                        {
                            var previous = this.Record?.GetRole(relationType.RoleType);

                            if (relationType.RoleType.ObjectType.IsUnit)
                            {
                                changeSet.DiffUnit(this.Strategy, relationType, current, previous);
                            }
                            else if (relationType.RoleType.IsOne)
                            {
                                changeSet.DiffComposite(this.Strategy, relationType, (Strategy)current, (long?)previous);
                            }
                            else
                            {
                                changeSet.DiffComposites(this.Strategy, relationType, (IRange<Strategy>)current, (IRange<long>)previous);
                            }

                        }
                    }
                }
            }
            // Different record
            else
            {
                foreach (var roleType in this.RoleTypes)
                {
                    var relationType = roleType.RelationType;

                    object previous = null;
                    object current = null;

                    if (this.PreviousChangedRoleByRelationType?.TryGetValue(relationType, out previous) == true)
                    {
                        if (this.ChangedRoleByRelationType?.TryGetValue(relationType, out current) != true)
                        {
                            current = this.Record.GetRole(roleType);
                        }

                        if (relationType.RoleType.ObjectType.IsUnit)
                        {
                            changeSet.DiffUnit(this.Strategy, relationType, current, previous);
                        }
                        else if (relationType.RoleType.IsOne)
                        {
                            changeSet.DiffComposite(this.Strategy, relationType, (Strategy)current, (Strategy)previous);
                        }
                        else
                        {
                            changeSet.DiffComposites(this.Strategy, relationType, (IRange<Strategy>)current, (IRange<Strategy>)previous);
                        }
                    }
                    else
                    {
                        previous = this.PreviousRecord?.GetRole(roleType);
                        if (this.ChangedRoleByRelationType?.TryGetValue(relationType, out current) != true)
                        {
                            current = this.Record.GetRole(roleType);
                        }

                        if (relationType.RoleType.ObjectType.IsUnit)
                        {
                            changeSet.DiffUnit(this.Strategy, relationType, current, previous);
                        }
                        else if (relationType.RoleType.IsOne)
                        {
                            changeSet.DiffComposite(this.Strategy, relationType, (long?)current, (long?)previous);
                        }
                        else
                        {
                            changeSet.DiffComposites(this.Strategy, relationType, (IRange<long>)current, (IRange<long>)previous);
                        }
                    }
                }
            }

            this.PreviousRecord = this.Record;
            this.PreviousChangedRoleByRelationType = this.ChangedRoleByRelationType != null ? new Dictionary<IRelationType, object>(this.ChangedRoleByRelationType) : null;
        }

        protected virtual bool ExactCheckpointRecords => false;

        private void CheckpointExact(ChangeSet changeSet)
        {
            // Local relation changes hold Strategies; immutable baselines hold IDs. Normalize each side
            // independently so accepting a security-only record also preserves checkpoint ownership.
            foreach (var roleType in this.RoleTypes)
            {
                var relation = roleType.RelationType;
                object current = null;
                object previous = null;
                if (this.ChangedRoleByRelationType?.TryGetValue(relation, out current) != true) { current = this.Record?.GetRole(roleType); }
                if (this.PreviousChangedRoleByRelationType?.TryGetValue(relation, out previous) != true) { previous = this.PreviousRecord?.GetRole(roleType); }
                if (roleType.ObjectType.IsUnit)
                {
                    if (!(current is byte[] currentBytes && previous is byte[] previousBytes && currentBytes.SequenceEqual(previousBytes)))
                    { changeSet.DiffUnit(this.Strategy, relation, current, previous); }
                }
                else if (roleType.IsOne)
                {
                    changeSet.DiffComposite(this.Strategy, relation, current is Strategy currentStrategy ? currentStrategy.Id : (long?)current,
                        previous is Strategy previousStrategy ? previousStrategy.Id : (long?)previous);
                }
                else
                {
                    var currentIds = current is IRange<Strategy> currentStrategies ? currentStrategies.Select(v => v.Id).ToArray()
                        : ((IRange<long>)current)?.ToArray() ?? Array.Empty<long>();
                    var previousIds = previous is IRange<Strategy> previousStrategies ? previousStrategies.Select(v => v.Id).ToArray()
                        : ((IRange<long>)previous)?.ToArray() ?? Array.Empty<long>();
                    var changedIds = currentIds.Except(previousIds).Concat(previousIds.Except(currentIds)).ToArray();
                    if (changedIds.Length == 0) { continue; }
                    if (!changeSet.AssociationsByRoleType.TryGetValue(roleType, out var associations))
                    { changeSet.AssociationsByRoleType[roleType] = associations = new HashSet<IStrategy>(); }
                    associations.Add(this.Strategy);
                    if (!changeSet.RolesByAssociationType.TryGetValue(roleType.AssociationType, out var roles))
                    { changeSet.RolesByAssociationType[roleType.AssociationType] = roles = new HashSet<IStrategy>(); }
                    foreach (var id in changedIds)
                    {
                        var strategy = this.Session.GetStrategy(id);
                        if (strategy != null) { roles.Add(strategy); }
                    }
                }
            }
            this.PreviousRecord = this.Record;
            this.PreviousChangedRoleByRelationType = this.ChangedRoleByRelationType != null ? new Dictionary<IRelationType, object>(this.ChangedRoleByRelationType) : null;
        }

        public void Diff(IList<IDiff> diffs)
        {
            if (this.ChangedRoleByRelationType == null)
            {
                return;
            }

            var ranges = this.Session.Workspace.RecordRanges;

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                var relationType = kvp.Key;
                var roleType = relationType.RoleType;

                var changed = kvp.Value;
                var original = this.Record?.GetRole(roleType);

                if (roleType.ObjectType.IsUnit)
                {
                    diffs.Add(new UnitDiff(relationType, this.Strategy)
                    {
                        OriginalRole = original,
                        ChangedRole = changed,
                    });
                }
                else if (roleType.IsOne)
                {
                    diffs.Add(new CompositeDiff(relationType, this.Strategy)
                    {
                        OriginalRole = original != null ? this.Session.GetStrategy((long)original) : null,
                        ChangedRole = (Strategy)changed,
                    });
                }
                else
                {
                    diffs.Add(new CompositesDiff(relationType, this.Strategy)
                    {
                        OriginalRoles = original != null ? ranges.Ensure(original).Select(v => this.Session.GetStrategy(v)).ToArray() : Array.Empty<Strategy>(),
                        ChangedRoles = ((IRange<Strategy>)changed).Save() ?? Array.Empty<Strategy>(),
                    });
                }
            }
        }

        public bool CanMerge(IRecord newRecord)
        {
            if (this.ChangedRoleByRelationType == null)
            {
                return true;
            }

            var ranges = this.Session.Workspace.RecordRanges;

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                var relationType = kvp.Key;
                var roleType = relationType.RoleType;

                var original = this.Record?.GetRole(roleType);
                var newOriginal = newRecord?.GetRole(roleType);

                if (roleType.ObjectType.IsUnit)
                {
                    var same = original is byte[] originalBinary && newOriginal is byte[] newOriginalBinary
                        ? originalBinary.SequenceEqual(newOriginalBinary)
                        : Equals(original, newOriginal);
                    if (!same)
                    {
                        return false;
                    }
                }
                else if (roleType.IsOne)
                {
                    if (!Equals(original, newOriginal))
                    {
                        return false;
                    }
                }
                else if (!ranges.Ensure(original).Equals(ranges.Ensure(newOriginal)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HashChanges() => this.ChangedRoleByRelationType != null;

        public void Reset() { this.Session.AssertEditable(); this.ClearChanges(); }

        protected void ClearChanges() => this.ChangedRoleByRelationType = null;

        public bool IsAssociationForRole(IRoleType roleType, Strategy forRole)
        {
            if (roleType.IsOne)
            {
                var compositeRole = this.GetCompositeRoleIfInstantiated(roleType);
                return compositeRole == forRole;
            }

            var compositesRole = this.GetCompositesRoleIfInstantiated(roleType);
            return compositesRole.Contains(forRole);
        }

        public bool HasChanged(IRoleType roleType) => this.ChangedRoleByRelationType?.ContainsKey(roleType.RelationType) ?? false;

        public void RestoreRole(IRoleType roleType) { this.Session.AssertEditable(); this.ChangedRoleByRelationType?.Remove(roleType.RelationType); }

        protected abstract void OnChange();

        private void SetChangedRole(IRoleType roleType, object role)
        {
            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role;
            this.OnChange();
        }

        private Strategy GetCompositeRoleIfInstantiated(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return (Strategy)changedRole;
            }

            var role = this.Record?.GetRole(roleType);
            return role == null ? null : this.Session.GetStrategy((long)role);
        }

        private IRange<Strategy> GetCompositesRoleIfInstantiated(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return (IRange<Strategy>)changedRole;
            }

            var role = (IRange<long>)this.Record?.GetRole(roleType);
            return role == null ? EmptyRange<Strategy>.Instance : this.Ranges.Load(role.Select(v => this.Session.GetStrategy(v)).Where(v => v != null));
        }
        private bool SameCompositeRole(IRoleType roleType, Strategy role)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return role == changedRole;
            }

            var changedRoleId = this.Record?.GetRole(roleType);

            if (role == null)
            {
                return changedRoleId == null;
            }

            if (changedRoleId == null)
            {
                return false;
            }

            return role.Id == (long)changedRoleId;
        }

        private bool SameCompositesRole(IRoleType roleType, IRange<Strategy> role)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return role.Equals(changedRole);
            }

            var roleIds = this.Session.Workspace.RecordRanges.Ensure(this.Record?.GetRole(roleType));
            return role.IsEmpty ? roleIds.IsEmpty : role.Select(v => v.Id).SequenceEqual(roleIds);
        }

        protected virtual string UnloadedRelationMessage => "Strategy is not in Session.";

        private void AssertStrategy(Strategy strategy)
        {
            if (strategy == null)
            {
                throw new Exception(this.UnloadedRelationMessage);
            }
        }

        #region Proxy Properties

        protected long Id => this.Strategy.Id;

        protected IClass Class => this.Strategy.Class;

        protected Session Session => this.Strategy.Session;

        protected Workspace Workspace => this.Session.Workspace;

        private IRanges<Strategy> Ranges => this.Strategy.Session.Workspace.StrategyRanges;

        #endregion
    }
}
