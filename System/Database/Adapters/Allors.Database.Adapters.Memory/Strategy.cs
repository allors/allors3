// <copyright file="Strategy.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Allors;
    using Adapters;
    using Meta;

    public sealed class Strategy : IStrategy
    {
        private readonly Dictionary<IRoleType, object> unitRoleByRoleType;
        private readonly Dictionary<IRoleType, Strategy> compositeRoleByRoleType;
        private readonly Dictionary<IRoleType, HashSet<Strategy>> compositesRoleByRoleType;
        private readonly Dictionary<IAssociationType, Strategy> compositeAssociationByAssociationType;
        private readonly Dictionary<IAssociationType, HashSet<Strategy>> compositesAssociationByAssociationType;

        private Dictionary<IRoleType, object> rollbackUnitRoleByRoleType;
        private Dictionary<IRoleType, Strategy> rollbackCompositeRoleByRoleType;
        private Dictionary<IRoleType, HashSet<Strategy>> rollbackCompositesRoleByRoleType;
        private Dictionary<IAssociationType, Strategy> rollbackCompositeAssociationByAssociationType;
        private Dictionary<IAssociationType, HashSet<Strategy>> rollbackCompositesAssociationByAssociationType;
        private bool isDeletedOnRollback;
        private WeakReference allorizedObjectWeakReference;

        internal Strategy(Session session, IClass objectType, long objectId, long version)
        {
            this.Session = session;
            this.UncheckedObjectType = objectType;
            this.ObjectId = objectId;

            this.IsDeleted = false;
            this.isDeletedOnRollback = true;
            this.IsNewInSession = true;

            this.ObjectVersion = version;

            this.unitRoleByRoleType = new Dictionary<IRoleType, object>();
            this.compositeRoleByRoleType = new Dictionary<IRoleType, Strategy>();
            this.compositesRoleByRoleType = new Dictionary<IRoleType, HashSet<Strategy>>();
            this.compositeAssociationByAssociationType = new Dictionary<IAssociationType, Strategy>();
            this.compositesAssociationByAssociationType = new Dictionary<IAssociationType, HashSet<Strategy>>();

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.rollbackCompositesRoleByRoleType = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.rollbackCompositesAssociationByAssociationType = null;
        }

        public bool IsDeleted { get; private set; }

        public bool IsNewInSession { get; private set; }

        public long ObjectId { get; }

        public long ObjectVersion { get; private set; }

        public IClass Class
        {
            get
            {
                this.AssertNotDeleted();
                return this.UncheckedObjectType;
            }
        }

        ISession IStrategy.Session => this.Session;

        internal IClass UncheckedObjectType { get; }

        internal Session Session { get; }

        private ChangeSet ChangeSet => this.Session.MemoryChangeSet;

        private Dictionary<IRoleType, object> RollbackUnitRoleByRoleType => this.rollbackUnitRoleByRoleType ??= new Dictionary<IRoleType, object>();

        private Dictionary<IRoleType, Strategy> RollbackCompositeRoleByRoleType => this.rollbackCompositeRoleByRoleType ??= new Dictionary<IRoleType, Strategy>();

        private Dictionary<IRoleType, HashSet<Strategy>> RollbackCompositesRoleByRoleType => this.rollbackCompositesRoleByRoleType ??= new Dictionary<IRoleType, HashSet<Strategy>>();

        private Dictionary<IAssociationType, Strategy> RollbackCompositeAssociationByAssociationType => this.rollbackCompositeAssociationByAssociationType ??= new Dictionary<IAssociationType, Strategy>();

        private Dictionary<IAssociationType, HashSet<Strategy>> RollbackCompositesAssociationByAssociationType => this.rollbackCompositesAssociationByAssociationType ??= new Dictionary<IAssociationType, HashSet<Strategy>>();

        public override string ToString() => this.UncheckedObjectType.Name + " " + this.ObjectId;

        public object GetRole(IRoleType roleType) =>
            roleType switch
            {
                { } unitRole when unitRole.ObjectType.IsUnit => this.GetUnitRole(roleType),
                { } compositeRole when compositeRole.IsOne => this.GetCompositeRole(roleType),
                _ => this.GetCompositeRoles(roleType)
            };

        public void SetRole(IRoleType roleType, object value)
        {
            switch (roleType)
            {
                case { } unitRole when unitRole.ObjectType.IsUnit:
                    this.SetUnitRole(roleType, value);
                    break;
                case { } compositeRole when compositeRole.IsOne:
                    this.SetCompositeRole(roleType, (IObject)value);
                    break;
                default:
                    this.SetCompositeRoles(roleType, (IEnumerable<IObject>)value);
                    break;
            }
        }

        public void RemoveRole(IRoleType roleType)
        {
            switch (roleType)
            {
                case { } unitRole when unitRole.ObjectType.IsUnit:
                    this.RemoveUnitRole(roleType);
                    break;
                case { } compositeRole when compositeRole.IsOne:
                    this.RemoveCompositeRole(roleType);
                    break;
                default:
                    this.RemoveCompositeRoles(roleType);
                    break;
            }
        }

        public bool ExistRole(IRoleType roleType) =>
            roleType switch
            {
                { } unitRole when unitRole.ObjectType.IsUnit => this.ExistUnitRole(roleType),
                { } compositeRole when compositeRole.IsOne => this.ExistCompositeRole(roleType),
                _ => this.ExistCompositeRoles(roleType)
            };

        public object GetUnitRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessUnitRole?.Invoke(this, roleType);
            return this.GetInternalizedUnitRole(roleType);
        }

        public void SetUnitRole(IRoleType roleType, object role)
        {
            this.AssertNotDeleted();
            this.Session.Database.UnitRoleChecks(this, roleType);

            var previousRole = this.GetInternalizedUnitRole(roleType);
            role = roleType.Normalize(role);

            if (role == previousRole)
            {
                return;
            }

            if (!this.RollbackUnitRoleByRoleType.ContainsKey(roleType))
            {
                this.RollbackUnitRoleByRoleType[roleType] = this.GetInternalizedUnitRole(roleType);
            }

            this.ChangeSet.OnChangingUnitRole(this.ObjectId, roleType);

            switch (role)
            {
                case null:
                    this.unitRoleByRoleType.Remove(roleType);
                    break;
                default:
                    this.unitRoleByRoleType[roleType] = role;
                    break;
            }
        }

        public void RemoveUnitRole(IRoleType relationType) => this.SetUnitRole(relationType, null);

        public bool ExistUnitRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessUnitRole?.Invoke(this, roleType);
            return this.unitRoleByRoleType.ContainsKey(roleType);
        }

        public IObject GetCompositeRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositeRole?.Invoke(this, roleType);
            this.compositeRoleByRoleType.TryGetValue(roleType, out var strategy);
            return strategy?.GetObject();
        }

        public void SetCompositeRole(IRoleType roleType, IObject newRole)
        {
            if (newRole == null)
            {
                this.RemoveCompositeRole(roleType);
            }
            else
            {
                if (roleType.AssociationType.IsOne)
                {
                    // 1-1
                    this.SetCompositeRoleOne2One(roleType, (Strategy)newRole.Strategy);
                }
                else
                {
                    // *-1
                    this.SetCompositeRoleMany2One(roleType, (Strategy)newRole.Strategy);
                }
            }
        }

        public void RemoveCompositeRole(IRoleType roleType)
        {
            if (roleType.AssociationType.IsOne)
            {
                // 1-1
                this.RemoveCompositeRoleOne2One(roleType);
            }
            else
            {
                // *-1
                this.RemoveCompositeRoleMany2One(roleType);
            }
        }

        public bool ExistCompositeRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositeRole?.Invoke(this, roleType);
            return this.compositeRoleByRoleType.ContainsKey(roleType);
        }

        public Allors.Database.Extent GetCompositeRoles(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositesRole?.Invoke(this, roleType);
            return new ExtentSwitch(this, roleType);
        }

        public void SetCompositeRoles(IRoleType roleType, IEnumerable<IObject> roles)
        {
            if (roles == null || (roles is ICollection<IObject> collection && collection.Count == 0))
            {
                this.RemoveCompositeRoles(roleType);
            }
            else
            {
                var strategies = roles
                    .Where(v => v != null)
                    .Select(v => this.Session.Database.CompositeRolesChecks(this, roleType, (Strategy)v.Strategy))
                    .Distinct();
                
                if (roleType.AssociationType.IsMany)
                {
                    this.SetCompositesRolesMany2Many(roleType, strategies);
                }
                else
                {
                    this.SetCompositesRolesOne2Many(roleType, strategies);
                }
            }
        }

        public void AddCompositeRole(IRoleType roleType, IObject role)
        {
            this.AssertNotDeleted();
            if (role == null)
            {
                return;
            }

            var roleStrategy = this.Session.Database.CompositeRolesChecks(this, roleType, (Strategy)role.Strategy);

            if (roleType.AssociationType.IsMany)
            {
                this.AddCompositeRoleMany2Many(roleType, roleStrategy);
            }
            else
            {
                this.AddCompositeRoleOne2Many(roleType, roleStrategy);
            }
        }

        public void RemoveCompositeRole(IRoleType roleType, IObject role)
        {
            this.AssertNotDeleted();

            if (role == null)
            {
                return;
            }

            var roleStrategy = this.Session.Database.CompositeRolesChecks(this, roleType, (Strategy)role.Strategy);

            if (roleType.AssociationType.IsMany)
            {
                this.RemoveCompositeRoleMany2Many(roleType, roleStrategy);
            }
            else
            {
                this.RemoveCompositeRoleOne2Many(roleType, roleStrategy);
            }
        }

        public void RemoveCompositeRoles(IRoleType roleType)
        {
            this.AssertNotDeleted();

            if (roleType.AssociationType.IsMany)
            {
                this.RemoveCompositeRolesMany2Many(roleType);
            }
            else
            {
                this.RemoveCompositeRolesOne2Many(roleType);
            }
        }

        public bool ExistCompositeRoles(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositesRole?.Invoke(this, roleType);
            this.compositesRoleByRoleType.TryGetValue(roleType, out var roleStrategies);
            return roleStrategies != null;
        }

        public object GetAssociation(IAssociationType associationType) => associationType.IsMany ? (object)this.GetCompositeAssociations(associationType) : this.GetCompositeAssociation(associationType);

        public bool ExistAssociation(IAssociationType associationType) => associationType.IsMany ? this.ExistCompositeAssociations(associationType) : this.ExistCompositeAssociation(associationType);

        public IObject GetCompositeAssociation(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositeAssociation?.Invoke(this, associationType);
            this.compositeAssociationByAssociationType.TryGetValue(associationType, out var strategy);
            return strategy?.GetObject();
        }

        public bool ExistCompositeAssociation(IAssociationType relationType) => this.GetCompositeAssociation(relationType) != null;

        public Allors.Database.Extent GetCompositeAssociations(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositesAssociation?.Invoke(this, associationType);
            return new ExtentSwitch(this, associationType);
        }

        public bool ExistCompositeAssociations(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            this.Session.OnAccessCompositesAssociation?.Invoke(this, associationType);
            this.compositesAssociationByAssociationType.TryGetValue(associationType, out var strategies);
            return strategies != null;
        }

        public void Delete()
        {
            this.AssertNotDeleted();

            // Roles
            foreach (var roleType in this.UncheckedObjectType.DatabaseRoleTypes)
            {
                if (this.ExistRole(roleType))
                {
                    if (roleType.ObjectType is IUnit)
                    {
                        this.RemoveUnitRole(roleType);
                    }
                    else
                    {
                        var associationType = roleType.AssociationType;
                        if (associationType.IsMany)
                        {
                            if (roleType.IsMany)
                            {
                                this.RemoveCompositeRolesMany2Many(roleType);
                            }
                            else
                            {
                                this.RemoveCompositeRoleMany2One(roleType);
                            }
                        }
                        else
                        {
                            if (roleType.IsMany)
                            {
                                this.RemoveCompositeRolesOne2Many(roleType);
                            }
                            else
                            {
                                this.RemoveCompositeRoleOne2One(roleType);
                            }
                        }
                    }
                }
            }

            // Associations
            foreach (var associationType in this.UncheckedObjectType.DatabaseAssociationTypes)
            {
                var roleType = associationType.RoleType;

                if (this.ExistAssociation(associationType))
                {
                    if (associationType.IsMany)
                    {
                        this.compositesAssociationByAssociationType.TryGetValue(associationType, out var associationStrategies);

                        // TODO: Optimize
                        if (associationStrategies != null)
                        {
                            foreach (var associationStrategy in new HashSet<Strategy>(associationStrategies))
                            {
                                if (roleType.IsMany)
                                {
                                    associationStrategy.RemoveCompositeRoleMany2Many(roleType, this);
                                }
                                else
                                {
                                    associationStrategy.RemoveCompositeRoleMany2One(roleType);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.compositeAssociationByAssociationType.TryGetValue(associationType, out var associationStrategy);

                        if (associationStrategy != null)
                        {
                            if (roleType.IsMany)
                            {
                                associationStrategy.RemoveCompositeRoleOne2Many(roleType, this);
                            }
                            else
                            {
                                associationStrategy.RemoveCompositeRoleOne2One(roleType);
                            }
                        }
                    }
                }
            }

            this.IsDeleted = true;

            this.ChangeSet.OnDeleted(this);
        }

        public IObject GetObject()
        {
            IObject allorsObject;
            if (this.allorizedObjectWeakReference == null)
            {
                allorsObject = this.Session.Database.ObjectFactory.Create(this);
                this.allorizedObjectWeakReference = new WeakReference(allorsObject);
            }
            else
            {
                allorsObject = (IObject)this.allorizedObjectWeakReference.Target;
                if (allorsObject == null)
                {
                    allorsObject = this.Session.Database.ObjectFactory.Create(this);
                    this.allorizedObjectWeakReference.Target = allorsObject;
                }
            }

            return allorsObject;
        }

        internal void Commit()
        {
            if (!this.IsDeleted && !this.Session.Database.IsLoading)
            {
                if (this.rollbackUnitRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null)
                {
                    ++this.ObjectVersion;
                }
            }

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.rollbackCompositesRoleByRoleType = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.rollbackCompositesAssociationByAssociationType = null;

            this.isDeletedOnRollback = this.IsDeleted;
            this.IsNewInSession = false;
        }

        internal void Rollback()
        {
            foreach (var dictionaryItem in this.RollbackUnitRoleByRoleType)
            {
                var roleType = dictionaryItem.Key;
                var role = dictionaryItem.Value;

                if (role != null)
                {
                    this.unitRoleByRoleType[roleType] = role;
                }
                else
                {
                    this.unitRoleByRoleType.Remove(roleType);
                }
            }

            foreach (var dictionaryItem in this.RollbackCompositeRoleByRoleType)
            {
                var roleType = dictionaryItem.Key;
                var role = dictionaryItem.Value;

                if (role != null)
                {
                    this.compositeRoleByRoleType[roleType] = role;
                }
                else
                {
                    this.compositeRoleByRoleType.Remove(roleType);
                }
            }

            foreach (var dictionaryItem in this.RollbackCompositesRoleByRoleType)
            {
                var roleType = dictionaryItem.Key;
                var role = dictionaryItem.Value;

                if (role != null)
                {
                    this.compositesRoleByRoleType[roleType] = role;
                }
                else
                {
                    this.compositesRoleByRoleType.Remove(roleType);
                }
            }

            foreach (var dictionaryItem in this.RollbackCompositeAssociationByAssociationType)
            {
                var associationType = dictionaryItem.Key;
                var association = dictionaryItem.Value;

                if (association != null)
                {
                    this.compositeAssociationByAssociationType[associationType] = association;
                }
                else
                {
                    this.compositeAssociationByAssociationType.Remove(associationType);
                }
            }

            foreach (var dictionaryItem in this.RollbackCompositesAssociationByAssociationType)
            {
                var associationType = dictionaryItem.Key;
                var association = dictionaryItem.Value;

                if (association != null)
                {
                    this.compositesAssociationByAssociationType[associationType] = association;
                }
                else
                {
                    this.compositesAssociationByAssociationType.Remove(associationType);
                }
            }

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.rollbackCompositesRoleByRoleType = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.rollbackCompositesAssociationByAssociationType = null;

            this.IsDeleted = this.isDeletedOnRollback;
            this.IsNewInSession = false;
        }

        internal object GetInternalizedUnitRole(IRoleType roleType)
        {
            this.unitRoleByRoleType.TryGetValue(roleType, out var unitRole);
            return unitRole;
        }

        internal List<Strategy> GetStrategies(IAssociationType associationType)
        {
            this.compositesAssociationByAssociationType.TryGetValue(associationType, out var strategies);
            if (strategies == null)
            {
                return new List<Strategy>();
            }

            return strategies.ToList();
        }

        internal List<Strategy> GetStrategies(IRoleType roleType)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var strategies);
            if (strategies == null)
            {
                return new List<Strategy>();
            }

            return strategies.ToList();
        }

        internal void SetCompositeRoleOne2One(IRoleType roleType, Strategy newRoleStrategy)
        {
            this.AssertNotDeleted();
            this.Session.Database.CompositeRoleChecks(this, roleType, newRoleStrategy);

            this.compositeRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategy);

            if (!newRoleStrategy.Equals(previousRoleStrategy))
            {
                this.ChangeSet.OnChangingCompositeRole(this.ObjectId, roleType, previousRoleStrategy?.ObjectId, newRoleStrategy.ObjectId);

                if (previousRoleStrategy != null)
                {
                    // previous role
                    var associationType = roleType.AssociationType;
                    previousRoleStrategy.Backup(associationType);
                    previousRoleStrategy.compositeAssociationByAssociationType.Remove(associationType);
                }

                // previous association of newRole
                newRoleStrategy.compositeAssociationByAssociationType.TryGetValue(roleType.AssociationType, out var newRolePreviousAssociationStrategy);
                if (newRolePreviousAssociationStrategy != null)
                {
                    if (!this.Equals(newRolePreviousAssociationStrategy))
                    {
                        this.ChangeSet.OnChangingCompositeRole(newRolePreviousAssociationStrategy.ObjectId, roleType, previousRoleStrategy?.ObjectId, null);

                        newRolePreviousAssociationStrategy.Backup(roleType);
                        newRolePreviousAssociationStrategy.compositeRoleByRoleType.Remove(roleType);
                    }
                }

                // Set new role
                this.Backup(roleType);
                this.compositeRoleByRoleType[roleType] = newRoleStrategy;

                // Set new role's association
                var associationType1 = roleType.AssociationType;
                newRoleStrategy.Backup(associationType1);
                newRoleStrategy.compositeAssociationByAssociationType[associationType1] = this;
            }
        }

        internal void SetCompositeRoleMany2One(IRoleType roleType, Strategy newRoleStrategy)
        {
            this.AssertNotDeleted();
            this.Session.Database.CompositeRoleChecks(this, roleType, newRoleStrategy);

            this.compositeRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategy);

            if (!newRoleStrategy.Equals(previousRoleStrategy))
            {
                this.ChangeSet.OnChangingCompositeRole(this.ObjectId, roleType, previousRoleStrategy?.ObjectId, newRoleStrategy.ObjectId);

                var associationType = roleType.AssociationType;

                // Update association of previous role
                if (previousRoleStrategy != null)
                {
                    previousRoleStrategy.compositesAssociationByAssociationType.TryGetValue(associationType, out var previousRoleStrategies);
                    previousRoleStrategy.Backup(associationType);
                    previousRoleStrategies.Remove(this);
                    if (previousRoleStrategies.Count == 0)
                    {
                        previousRoleStrategy.compositesAssociationByAssociationType.Remove(associationType);
                    }
                }

                this.Backup(roleType);
                this.compositeRoleByRoleType[roleType] = newRoleStrategy;

                newRoleStrategy.compositesAssociationByAssociationType.TryGetValue(associationType, out var strategies);

                newRoleStrategy.Backup(associationType);
                if (strategies == null)
                {
                    strategies = new HashSet<Strategy>();
                    newRoleStrategy.compositesAssociationByAssociationType[associationType] = strategies;
                }

                strategies.Add(this);
            }
        }

        internal void SetCompositesRolesOne2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            this.compositesRoleByRoleType.TryGetValue(roleType, out var originalRoles);

            if (originalRoles == null || originalRoles.Count == 0)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleOne2Many(roleType, role);
                }
            }
            else
            {
                ISet<Strategy> toRemove = new HashSet<Strategy>(originalRoles);

                foreach (var role in roles)
                {
                    if (toRemove.Contains(role))
                    {
                        toRemove.Remove(role);
                    }
                    else
                    {
                        this.AddCompositeRoleOne2Many(roleType, role);
                    }
                }

                foreach (var strategy in toRemove)
                {
                    this.RemoveCompositeRoleOne2Many(roleType, strategy);
                }
            }
        }

        internal void SetCompositesRolesMany2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            this.compositesRoleByRoleType.TryGetValue(roleType, out var originalRoles);

            if (originalRoles == null || originalRoles.Count == 0)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleMany2Many(roleType, role);
                }
            }
            else
            {
                ISet<Strategy> toRemove = new HashSet<Strategy>(originalRoles);

                foreach (var role in roles)
                {
                    if (toRemove.Contains(role))
                    {
                        toRemove.Remove(role);
                    }
                    else
                    {
                        this.AddCompositeRoleMany2Many(roleType, role);
                    }
                }

                foreach (var strategy in toRemove)
                {
                    this.RemoveCompositeRoleMany2Many(roleType, strategy);
                }
            }
        }

        internal void FillRoleForSave(Dictionary<IRoleType, List<Strategy>> strategiesByRoleType)
        {
            if (this.IsDeleted)
            {
                return;
            }

            if (this.unitRoleByRoleType != null)
            {
                foreach (var dictionaryEntry in this.unitRoleByRoleType)
                {
                    var roleType = dictionaryEntry.Key;

                    if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                    {
                        strategies = new List<Strategy>();
                        strategiesByRoleType.Add(roleType, strategies);
                    }

                    strategies.Add(this);
                }
            }

            if (this.compositeRoleByRoleType != null)
            {
                foreach (var dictionaryEntry in this.compositeRoleByRoleType)
                {
                    var roleType = dictionaryEntry.Key;

                    if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                    {
                        strategies = new List<Strategy>();
                        strategiesByRoleType.Add(roleType, strategies);
                    }

                    strategies.Add(this);
                }
            }

            if (this.compositesRoleByRoleType != null)
            {
                foreach (var dictionaryEntry in this.compositesRoleByRoleType)
                {
                    var roleType = dictionaryEntry.Key;

                    if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                    {
                        strategies = new List<Strategy>();
                        strategiesByRoleType.Add(roleType, strategies);
                    }

                    strategies.Add(this);
                }
            }
        }

        internal void SaveUnit(XmlWriter writer, IRoleType roleType)
        {
            var unitType = (IUnit)roleType.ObjectType;
            var value = Serialization.WriteString(unitType.UnitTag, this.unitRoleByRoleType[roleType]);

            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        internal void SaveComposites(XmlWriter writer, IRoleType roleType)
        {
            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());

            var roleStragies = this.compositesRoleByRoleType[roleType];
            var i = 0;
            foreach (var roleStrategy in roleStragies)
            {
                if (i > 0)
                {
                    writer.WriteString(Serialization.ObjectsSplitter);
                }

                writer.WriteString(roleStrategy.ObjectId.ToString());
                ++i;
            }

            writer.WriteEndElement();
        }

        internal void SaveComposite(XmlWriter writer, IRoleType roleType)
        {
            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());

            var roleStragy = this.compositeRoleByRoleType[roleType];
            writer.WriteString(roleStragy.ObjectId.ToString());

            writer.WriteEndElement();
        }

        private void Backup(IRoleType roleType)
        {
            if (roleType.IsMany)
            {
                if (!this.RollbackCompositesRoleByRoleType.ContainsKey(roleType))
                {
                    this.compositesRoleByRoleType.TryGetValue(roleType, out var strategies);

                    if (strategies == null)
                    {
                        this.RollbackCompositesRoleByRoleType[roleType] = null;
                    }
                    else
                    {
                        this.RollbackCompositesRoleByRoleType[roleType] = new HashSet<Strategy>(strategies);
                    }
                }
            }
            else
            {
                if (!this.RollbackCompositeRoleByRoleType.ContainsKey(roleType))
                {
                    this.compositeRoleByRoleType.TryGetValue(roleType, out var strategy);

                    if (strategy == null)
                    {
                        this.RollbackCompositeRoleByRoleType[roleType] = null;
                    }
                    else
                    {
                        this.RollbackCompositeRoleByRoleType[roleType] = strategy;
                    }
                }
            }
        }

        private void Backup(IAssociationType associationType)
        {
            if (associationType.IsMany)
            {
                if (!this.RollbackCompositesAssociationByAssociationType.ContainsKey(associationType))
                {
                    this.compositesAssociationByAssociationType.TryGetValue(associationType, out var strategies);

                    if (strategies == null)
                    {
                        this.RollbackCompositesAssociationByAssociationType[associationType] = null;
                    }
                    else
                    {
                        this.RollbackCompositesAssociationByAssociationType[associationType] = new HashSet<Strategy>(strategies);
                    }
                }
            }
            else
            {
                if (!this.RollbackCompositeAssociationByAssociationType.ContainsKey(associationType))
                {
                    this.compositeAssociationByAssociationType.TryGetValue(associationType, out var strategy);

                    if (strategy == null)
                    {
                        this.RollbackCompositeAssociationByAssociationType[associationType] = null;
                    }
                    else
                    {
                        this.RollbackCompositeAssociationByAssociationType[associationType] = strategy;
                    }
                }
            }
        }

        private void RemoveCompositeRoleOne2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.Database.CompositeRoleChecks(this, roleType);

            var previousRole = this.GetCompositeRole(roleType);
            if (previousRole != null)
            {
                this.ChangeSet.OnChangingCompositeRole(this.ObjectId, roleType, previousRole.Id, null);

                var previousRoleStrategy = this.Session.GetStrategy(previousRole);
                var associationType = roleType.AssociationType;
                previousRoleStrategy.Backup(associationType);
                previousRoleStrategy.compositeAssociationByAssociationType.Remove(associationType);

                // remove role
                this.Backup(roleType);
                this.compositeRoleByRoleType.Remove(roleType);
            }
        }

        private void RemoveCompositeRoleMany2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Session.Database.CompositeRoleChecks(this, roleType);

            var previousRole = this.GetCompositeRole(roleType);

            if (previousRole != null)
            {
                this.ChangeSet.OnChangingCompositeRole(this.ObjectId, roleType, previousRole.Id, null);

                var previousRoleStrategy = this.Session.GetStrategy(previousRole);
                var associationType = roleType.AssociationType;

                previousRoleStrategy.compositesAssociationByAssociationType.TryGetValue(associationType, out var previousRoleStrategyAssociations);

                previousRoleStrategy.Backup(associationType);
                previousRoleStrategyAssociations.Remove(this);

                if (previousRoleStrategyAssociations.Count == 0)
                {
                    previousRoleStrategy.compositesAssociationByAssociationType.Remove(associationType);
                }

                // remove role
                this.Backup(roleType);
                this.compositeRoleByRoleType.Remove(roleType);
            }
        }

        private void AddCompositeRoleMany2Many(IRoleType roleType, Strategy newRoleStrategy)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategies);
            if (previousRoleStrategies != null && previousRoleStrategies.Contains(newRoleStrategy))
            {
                return;
            }

            this.ChangeSet.OnChangingCompositesRole(this.ObjectId, roleType, newRoleStrategy);

            // Add the new role
            this.Backup(roleType);
            this.compositesRoleByRoleType.TryGetValue(roleType, out var roleStrategies);
            if (roleStrategies == null)
            {
                roleStrategies = new HashSet<Strategy>();
                this.compositesRoleByRoleType[roleType] = roleStrategies;
            }

            roleStrategies.Add(newRoleStrategy);

            // Add the new association
            newRoleStrategy.Backup(roleType.AssociationType);
            newRoleStrategy.compositesAssociationByAssociationType.TryGetValue(roleType.AssociationType, out var newRoleStrategiesAssociationStrategies);
            if (newRoleStrategiesAssociationStrategies == null)
            {
                newRoleStrategiesAssociationStrategies = new HashSet<Strategy>();
                newRoleStrategy.compositesAssociationByAssociationType[roleType.AssociationType] = newRoleStrategiesAssociationStrategies;
            }

            newRoleStrategiesAssociationStrategies.Add(this);
        }

        private void AddCompositeRoleOne2Many(IRoleType roleType, Strategy newRoleStrategy)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategies);
            if (previousRoleStrategies != null && previousRoleStrategies.Contains(newRoleStrategy))
            {
                return;
            }

            this.ChangeSet.OnChangingCompositesRole(this.ObjectId, roleType, newRoleStrategy);

            // 1-...
            newRoleStrategy.compositeAssociationByAssociationType.TryGetValue(roleType.AssociationType, out var newRolePreviousAssociationStrategy);
            if (newRolePreviousAssociationStrategy != null)
            {
                this.ChangeSet.OnChangingCompositesRole(newRolePreviousAssociationStrategy.ObjectId, roleType, null);

                // Remove obsolete role
                newRolePreviousAssociationStrategy.Backup(roleType);
                newRolePreviousAssociationStrategy.compositesRoleByRoleType.TryGetValue(roleType, out var newRolePreviousAssociationStrategyRoleStrategies);
                if (newRolePreviousAssociationStrategyRoleStrategies == null)
                {
                    newRolePreviousAssociationStrategyRoleStrategies = new HashSet<Strategy>();
                    newRolePreviousAssociationStrategy.compositesRoleByRoleType[roleType] = newRolePreviousAssociationStrategyRoleStrategies;
                }

                newRolePreviousAssociationStrategyRoleStrategies.Remove(newRoleStrategy);
                if (newRolePreviousAssociationStrategyRoleStrategies.Count == 0)
                {
                    newRolePreviousAssociationStrategy.compositesRoleByRoleType.Remove(roleType);
                }
            }

            // Add the new role
            this.Backup(roleType);
            this.compositesRoleByRoleType.TryGetValue(roleType, out var roleStrategies);
            if (roleStrategies == null)
            {
                roleStrategies = new HashSet<Strategy>();
                this.compositesRoleByRoleType[roleType] = roleStrategies;
            }

            roleStrategies.Add(newRoleStrategy);

            // Set new association
            newRoleStrategy.Backup(roleType.AssociationType);
            newRoleStrategy.compositeAssociationByAssociationType[roleType.AssociationType] = this;
        }

        private void RemoveCompositeRoleMany2Many(IRoleType roleType, Strategy roleStrategy)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var roleStrategies);
            if (roleStrategies == null || !roleStrategies.Contains(roleStrategy))
            {
                return;
            }

            this.ChangeSet.OnChangingCompositesRole(this.ObjectId, roleType, roleStrategy);

            // Remove role
            this.Backup(roleType);
            roleStrategies.Remove(roleStrategy);
            if (roleStrategies.Count == 0)
            {
                this.compositesRoleByRoleType.Remove(roleType);
            }

            // Remove association
            roleStrategy.Backup(roleType.AssociationType);
            roleStrategy.compositesAssociationByAssociationType.TryGetValue(roleType.AssociationType, out var roleStrategiesAssociationStrategies);
            roleStrategiesAssociationStrategies.Remove(this);

            if (roleStrategiesAssociationStrategies.Count == 0)
            {
                roleStrategy.compositesAssociationByAssociationType.Remove(roleType.AssociationType);
            }
        }

        private void RemoveCompositeRoleOne2Many(IRoleType roleType, Strategy roleStrategy)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var roleStrategies);
            if (roleStrategies == null || !roleStrategies.Contains(roleStrategy))
            {
                return;
            }

            this.ChangeSet.OnChangingCompositesRole(this.ObjectId, roleType, roleStrategy);

            this.Backup(roleType);

            // Remove role
            roleStrategies.Remove(roleStrategy);
            if (roleStrategies.Count == 0)
            {
                this.compositesRoleByRoleType.Remove(roleType);
            }

            // Remove association
            roleStrategy.Backup(roleType.AssociationType);
            roleStrategy.compositeAssociationByAssociationType.Remove(roleType.AssociationType);
        }

        private void RemoveCompositeRolesMany2Many(IRoleType roleType)
        {
            this.compositesRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategies);
            if (previousRoleStrategies != null)
            {
                foreach (var previousRoleStrategy in previousRoleStrategies)
                {
                    this.ChangeSet.OnChangingCompositesRole(this.ObjectId, roleType, previousRoleStrategy);
                }

                foreach (var strategy in new List<Strategy>(previousRoleStrategies))
                {
                    this.RemoveCompositeRoleMany2Many(roleType, strategy);
                }
            }
        }

        private void RemoveCompositeRolesOne2Many(IRoleType roleType)
        {
            // TODO: Optimize
            this.compositesRoleByRoleType.TryGetValue(roleType, out var previousRoleStrategies);
            if (previousRoleStrategies != null)
            {
                foreach (var strategy in new List<Strategy>(previousRoleStrategies))
                {
                    this.RemoveCompositeRoleOne2Many(roleType, strategy);
                }
            }
        }

        private void AssertNotDeleted()
        {
            if (this.IsDeleted)
            {
                throw new Exception($"Object of class {this.UncheckedObjectType.Name} with id {this.ObjectId} has been deleted");
            }
        }

        public class ObjectIdComparer : IComparer<Strategy>
        {
            public int Compare(Strategy x, Strategy y) => x.ObjectId.CompareTo(y.ObjectId);
        }
    }
}
