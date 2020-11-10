// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Meta;

    public class DatabaseStrategy : Strategy, IDatabaseStrategy
    {
        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private Dictionary<IRoleType, object> roleByRoleType = new Dictionary<IRoleType, object>();

        public DatabaseStrategy(Session session, IClass @class, long workspaceId) : base(session, workspaceId, @class) { }

        public DatabaseStrategy(Session session, DatabaseObject databaseObject, long workspaceId) : base(session, workspaceId, databaseObject.Class) => this.DatabaseObject = databaseObject;

        public DatabaseObject DatabaseObject { get; private set; }

        public long? DatabaseId => this.DatabaseObject?.DatabaseId;

        public long? Version => this.DatabaseObject?.Version;

        public bool HasDatabaseChanges
        {
            get
            {
                if (this.Class.HasDatabaseOrigin && !this.ExistDatabaseObject)
                {
                    return true;
                }

                return this.changedRoleByRoleType != null;
            }
        }

        ISession IStrategy.Session => this.Session;

        private bool ExistDatabaseObject => this.DatabaseObject != null;

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.WorkspaceDatabase.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.WorkspaceDatabase.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.WorkspaceDatabase.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public override bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public override object Get(IRoleType roleType)
        {
            if (roleType.Origin != Origin.Database)
            {
                return base.Get(roleType);
            }

            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseObject.GetRole(roleType);
                    if (databaseRole != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.Instantiate((long)databaseRole);
                            }
                            else
                            {
                                var ids = (long[])databaseRole;
                                var array = Array.CreateInstance(roleType.ObjectType.ClrType, ids.Length);
                                for (var i = 0; i < ids.Length; i++)
                                {
                                    array.SetValue(this.Session.Instantiate(ids[i]), i);
                                }

                                value = array;
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
                }

                this.roleByRoleType[roleType] = value;
            }

            return value;

        }

        public override void Set(IRoleType roleType, object value)
        {
            if (roleType.Origin != Origin.Database)
            {
                base.Set(roleType, value);
            }
            else
            {
                var current = this.Get(roleType);
                if (roleType.ObjectType.IsUnit || roleType.IsOne)
                {
                    if (object.Equals(current, value))
                    {
                        return;
                    }
                }
                else
                {
                    value ??= Array.Empty<IStrategy>();

                    var currentCollection = (IList<object>)current;
                    var valueCollection = (IList<object>)value;
                    if (currentCollection.Count == valueCollection.Count &&
                        !currentCollection.Except(valueCollection).Any())
                    {
                        return;
                    }
                }

                this.changedRoleByRoleType ??= new Dictionary<IRoleType, object>();

                if (roleType.ObjectType.IsComposite && roleType.IsMany)
                {
                    // TODO: Optimize
                    value = new ArrayList((Array)value).ToArray(roleType.ObjectType.ClrType);
                }

                this.roleByRoleType[roleType] = value;
                this.changedRoleByRoleType[roleType] = value;
            }
        }

        public override void Add(IRoleType roleType, IObject value)
        {
            if (roleType.Origin != Origin.Database)
            {
                base.Add(roleType, value);
            }
            else
            {
                var roles = (IObject[])this.Get(roleType);
                if (!roles.Contains(value))
                {
                    roles = new List<IObject>(roles) { value }.ToArray();
                }

                this.Set(roleType, roles);
            }
        }

        public override void Remove(IRoleType roleType, IObject value)
        {
            if (roleType.Origin != Origin.Database)
            {
                base.Remove(roleType, value);
            }
            else
            {
                var roles = (IStrategy[])this.Get(roleType);
                if (roles.Contains(value.Strategy))
                {
                    var newRoles = new List<IStrategy>(roles);
                    newRoles.Remove(value.Strategy);
                    roles = newRoles.ToArray();
                }

                this.Set(roleType, roles);
            }
        }

        public override IObject GetAssociation(IAssociationType associationType) => associationType.Origin != Origin.Database ?
            base.GetAssociation(associationType) :
            this.Session.GetAssociation((IDatabaseObject)this.Object, associationType).FirstOrDefault();

        public override IEnumerable<IObject> GetAssociations(IAssociationType associationType) => associationType.Origin != Origin.Database ?
            base.GetAssociations(associationType) :
            this.Session.GetAssociation((IDatabaseObject)this.Object, associationType);
        
        internal void Reset()
        {
            if (this.DatabaseObject != null)
            {
                this.DatabaseObject = this.Session.Workspace.WorkspaceDatabase.Get(this.DatabaseId.Value);
            }

            this.changedRoleByRoleType = null;

            this.roleByRoleType = new Dictionary<IRoleType, object>();
        }

        internal void PushResponse(DatabaseObject databaseObject) => this.DatabaseObject = databaseObject;

        internal void Refresh(bool merge = false)
        {
            if (!this.HasDatabaseChanges)
            {
                this.Reset();
            }
            else
            {
                if (merge)
                {
                    if (this.DatabaseObject != null)
                    {
                        this.DatabaseObject = this.Session.Workspace.WorkspaceDatabase.Get(this.DatabaseId.Value);
                    }
                }
            }
        }

        internal object GetDatabase(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseObject.GetRole(roleType);
                    if (databaseRole != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.GetForAssociation((long)databaseRole);
                            }
                            else
                            {
                                var ids = (long[])databaseRole;
                                value = ids.Select(v => this.Session.GetForAssociation(v))
                                    .Where(v => v != null)
                                    .ToArray();
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
                }
            }

            return value;
        }

        internal override State GetPopulation(Origin origin) =>
            origin switch
            {
                Origin.Workspace => this.Session.Workspace.State,
                Origin.Session => this.Session.State,
                _ => throw new Exception($"Unsupported origin: {origin}")
            };
    }
}
