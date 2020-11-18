// <copyright file="Strategy.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsStrategySql type.
// </summary>

namespace Allors.Database.Adapters.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Adapters;

    using Allors.Database.Meta;

    public class Strategy : IStrategy
    {
        private IObject allorsObject;
        private Roles roles;

        internal Strategy(Reference reference)
        {
            this.Reference = reference;
            this.ObjectId = reference.ObjectId;
        }

        ISession IStrategy.Session => this.Reference.Session;

        public Session Session => this.Reference.Session;

        public IClass Class
        {
            get
            {
                if (!this.Reference.Exists)
                {
                    throw new Exception("Object that had  " + this.Reference.Class.Name + " with id " + this.ObjectId + " does not exist");
                }

                return this.Reference.Class;
            }
        }

        public long ObjectId { get; }

        public long ObjectVersion => this.Reference.Version;

        public bool IsDeleted => !this.Reference.Exists;

        public bool IsNewInSession => this.Reference.IsNew;

        internal Roles Roles => this.roles ??= this.Reference.Session.State.GetOrCreateRoles(this.Reference);

        internal Reference Reference { get; }

        public IObject GetObject() => this.allorsObject ??= this.Reference.Session.Database.ObjectFactory.Create(this);

        public virtual void Delete()
        {
            this.AssertExist();

            foreach (var roleType in this.Class.DatabaseRoleTypes)
            {
                if (roleType.ObjectType.IsComposite)
                {
                    this.RemoveRole(roleType);
                }
            }

            foreach (var associationType in this.Class.DatabaseAssociationTypes)
            {
                var roleType = associationType.RoleType;

                if (associationType.IsMany)
                {
                    foreach (var association in this.Session.GetAssociations(this, associationType))
                    {
                        var associationStrategy = this.Session.State.GetOrCreateReferenceForExistingObject(association, this.Session).Strategy;
                        if (roleType.IsMany)
                        {
                            associationStrategy.RemoveCompositeRole(roleType, this.GetObject());
                        }
                        else
                        {
                            associationStrategy.RemoveCompositeRole(roleType);
                        }
                    }
                }
                else
                {
                    var association = this.GetCompositeAssociation(associationType);
                    if (association != null)
                    {
                        if (roleType.IsMany)
                        {
                            association.Strategy.RemoveCompositeRole(roleType, this.GetObject());
                        }
                        else
                        {
                            association.Strategy.RemoveCompositeRole(roleType);
                        }
                    }
                }
            }

            this.Session.Commands.DeleteObject(this);
            this.Reference.Exists = false;

            this.Session.State.ChangeSet.OnDeleted(this);
        }

        public virtual bool ExistRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.ExistUnitRole(roleType);
            }

            return roleType.IsMany
                ? this.ExistCompositeRoles(roleType)
                : this.ExistCompositeRole(roleType);
        }

        public virtual object GetRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.GetUnitRole(roleType);
            }

            return roleType.IsMany
                       ? (object)this.GetCompositeRoles(roleType)
                       : this.GetCompositeRole(roleType);
        }

        public virtual void SetRole(IRoleType roleType, object value)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnitRole(roleType, value);
            }
            else
            {
                if (roleType.IsMany)
                {
                    this.SetCompositeRoles(roleType, (IEnumerable<IObject>)value);
                }
                else
                {
                    this.SetCompositeRole(roleType, (IObject)value);
                }
            }
        }

        public virtual void RemoveRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.RemoveUnitRole(roleType);
            }
            else
            {
                if (roleType.IsMany)
                {
                    this.RemoveCompositeRoles(roleType);
                }
                else
                {
                    this.RemoveCompositeRole(roleType);
                }
            }
        }

        public virtual bool ExistUnitRole(IRoleType roleType) => this.GetUnitRole(roleType) != null;

        public virtual object GetUnitRole(IRoleType roleType)
        {
            this.AssertExist();
            return this.Roles.GetUnitRole(roleType);
        }

        public virtual void SetUnitRole(IRoleType roleType, object role)
        {
            this.AssertExist();
            roleType.UnitRoleChecks(this);
            role = roleType.Normalize(role);

            this.Roles.SetUnitRole(roleType, role);
        }

        public virtual void RemoveUnitRole(IRoleType roleType) => this.SetUnitRole(roleType, null);

        public virtual bool ExistCompositeRole(IRoleType roleType) => this.GetCompositeRole(roleType) != null;

        public virtual IObject GetCompositeRole(IRoleType roleType)
        {
            this.AssertExist();
            var role = this.Roles.GetCompositeRole(roleType);
            return role == null ? null : this.Session.State.GetOrCreateReferenceForExistingObject(role.Value, this.Session).Strategy.GetObject();
        }

        public virtual void SetCompositeRole(IRoleType roleType, IObject newRoleObject)
        {
            if (newRoleObject == null)
            {
                this.RemoveCompositeRole(roleType);
            }
            else
            {
                this.AssertExist();
                roleType.CompositeRoleChecks(this, newRoleObject);

                var newRoleObjectId = (Strategy)newRoleObject.Strategy;
                this.Roles.SetCompositeRole(roleType, newRoleObjectId);
            }
        }

        public virtual void RemoveCompositeRole(IRoleType roleType)
        {
            this.AssertExist();
            roleType.CompositeRoleChecks(this);

            this.Roles.RemoveCompositeRole(roleType);
        }

        public virtual bool ExistCompositeRoles(IRoleType roleType) => this.GetCompositeRoles(roleType).Count != 0;

        public virtual Allors.Database.Extent GetCompositeRoles(IRoleType roleType)
        {
            this.AssertExist();
            return new ExtentRoles(this, roleType);
        }

        public virtual void AddCompositeRole(IRoleType roleType, IObject roleObject)
        {
            this.AssertExist();

            if (roleObject != null)
            {
                roleType.CompositeRolesChecks(this, roleObject);

                var role = (Strategy)roleObject.Strategy;
                this.Roles.AddCompositeRole(roleType, role);
            }
        }

        public virtual void RemoveCompositeRole(IRoleType roleType, IObject roleObject)
        {
            this.AssertExist();

            if (roleObject != null)
            {
                roleType.CompositeRolesChecks(this, roleObject);

                var role = (Strategy)roleObject.Strategy;
                this.Roles.RemoveCompositeRole(roleType, role);
            }
        }

        public virtual void SetCompositeRoles(IRoleType roleType, IEnumerable<IObject> roleObjects)
        {
            var roleCollection = roleObjects != null ? roleObjects as ICollection<IObject> ?? roleObjects.ToArray() : null;

            if (roleCollection == null || roleCollection.Count == 0)
            {
                this.RemoveCompositeRoles(roleType);
            }
            else
            {
                this.AssertExist();

                // TODO: use CompositeRoles
                var previousRoles = new List<long>(this.Roles.GetCompositesRole(roleType));
                var newRoles = new HashSet<long>();

                foreach (var roleObject in roleCollection)
                {
                    if (roleObject != null)
                    {
                        roleType.CompositeRolesChecks(this, roleObject);
                        var role = (Strategy)roleObject.Strategy;

                        if (!previousRoles.Contains(role.ObjectId))
                        {
                            this.Roles.AddCompositeRole(roleType, role);
                        }

                        newRoles.Add(role.ObjectId);
                    }
                }

                foreach (var previousRole in previousRoles)
                {
                    if (!newRoles.Contains(previousRole))
                    {
                        this.Roles.RemoveCompositeRole(roleType, this.Session.State.GetOrCreateReferenceForExistingObject(previousRole, this.Session).Strategy);
                    }
                }
            }
        }

        public virtual void RemoveCompositeRoles(IRoleType roleType)
        {
            this.AssertExist();

            roleType.CompositeRoleChecks(this);

            var previousRoles = this.Roles.GetCompositesRole(roleType);

            foreach (var previousRole in previousRoles)
            {
                this.Roles.RemoveCompositeRole(roleType, this.Session.State.GetOrCreateReferenceForExistingObject(previousRole, this.Session).Strategy);
            }
        }

        public virtual bool ExistAssociation(IAssociationType associationType) => associationType.IsMany ? this.ExistCompositeAssociations(associationType) : this.ExistCompositeAssociation(associationType);

        public virtual object GetAssociation(IAssociationType associationType) => associationType.IsMany ? (object)this.GetCompositeAssociations(associationType) : this.GetCompositeAssociation(associationType);

        public virtual bool ExistCompositeAssociation(IAssociationType associationType) => this.GetCompositeAssociation(associationType) != null;

        public virtual IObject GetCompositeAssociation(IAssociationType associationType)
        {
            this.AssertExist();
            var association = this.Session.GetAssociation(this, associationType);
            return association?.Strategy.GetObject();
        }

        public virtual bool ExistCompositeAssociations(IAssociationType associationType) => this.GetCompositeAssociations(associationType).Count != 0;

        public virtual Allors.Database.Extent GetCompositeAssociations(IAssociationType associationType)
        {
            this.AssertExist();
            return new ExtentAssociations(this, associationType);
        }

        public override string ToString() => "[" + this.Class + ":" + this.ObjectId + "]";

        internal virtual void Release() => this.roles = null;

        internal int ExtentRolesGetCount(IRoleType roleType)
        {
            this.AssertExist();

            return this.Roles.ExtentCount(roleType);
        }

        internal IObject ExtentRolesFirst(IRoleType roleType)
        {
            this.AssertExist();

            return this.Roles.ExtentFirst(this.Session, roleType);
        }

        internal void ExtentRolesCopyTo(IRoleType roleType, Array array, int index) => this.Roles.ExtentCopyTo(this.Session, roleType, array, index);

        internal int ExtentIndexOf(IRoleType roleType, IObject value)
        {
            var i = 0;
            foreach (var oid in this.Roles.GetCompositesRole(roleType))
            {
                if (oid.Equals(value.Id))
                {
                    return i;
                }

                ++i;
            }

            return -1;
        }

        internal IObject ExtentGetItem(IRoleType roleType, int index)
        {
            var i = 0;
            foreach (var oid in this.Roles.GetCompositesRole(roleType))
            {
                if (i == index)
                {
                    return this.Session.State.GetOrCreateReferenceForExistingObject(oid, this.Session).Strategy.GetObject();
                }

                ++i;
            }

            return null;
        }

        internal bool ExtentRolesContains(IRoleType roleType, IObject value) => this.Roles.ExtentContains(roleType, value.Id);

        internal virtual long[] ExtentGetCompositeAssociations(IAssociationType associationType)
        {
            this.AssertExist();

            return this.Session.GetAssociations(this, associationType);
        }

        protected virtual void AssertExist()
        {
            if (!this.Reference.Exists)
            {
                throw new Exception("Object of class " + this.Class.Name + " with id " + this.ObjectId + " does not exist");
            }
        }
    }
}
