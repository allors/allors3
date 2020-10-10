// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.Database.Push;
    using Allors.Workspace.Meta;
    using Protocol.Data;

    public class Strategy : IStrategy
    {
        private static readonly IStrategy[] EmptySessionObjects = new IStrategy[0];

        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private Dictionary<IRoleType, object> roleByRoleType = new Dictionary<IRoleType, object>();

        private IObject @object;

        public Strategy(Session session, DatabaseObject databaseObject)
        {
            this.Session = session;
            this.DatabaseObject = databaseObject;
            this.ObjectType = databaseObject.Class;
        }

        public Strategy(Session session, IClass @class, long newId)
        {
            this.Session = session;
            this.ObjectType = @class;
            this.NewId = newId;
        }

        public void PushResponse(long id)
        {
            this.NewId = null;
            this.DatabaseObject = this.SessionOrigin.Database.New(id, this.ObjectType);
        }

        public IObject Object
        {
            get
            {
                this.@object ??= this.Session.Workspace.Database.ObjectFactory.Create(this);
                return this.@object;
            }
        }

        public Session SessionOrigin => this.Session;

        public DatabaseObject DatabaseObject { get; private set; }

        public IClass ObjectType { get; set; }

        public long? NewId { get; set; }

        public long Id => this.DatabaseObject?.Id ?? this.NewId ?? 0;

        public long? Version => this.DatabaseObject?.Version;

        public bool HasChanges
        {
            get
            {
                if (this.NewId != null)
                {
                    return true;
                }

                return this.changedRoleByRoleType != null;
            }
        }

        ISession IStrategy.Session => this.Session;

        public Session Session { get; }

        public bool HasChangedRoles(params IRoleType[] roleTypes)
        {
            if (roleTypes.Length == 0)
            {
                return this.HasChanges;
            }

            if (this.NewId != null)
            {
                // I am new in the session, and i have at least one of the requested roleTypes
                if (roleTypes.Any(v => this.roleByRoleType.ContainsKey(v)))
                {
                    return true;
                }

                return false;
            }

            if (this.changedRoleByRoleType != null)
            {
                if (roleTypes.Any(v => this.changedRoleByRoleType.ContainsKey(v)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanRead(IRoleType roleType)
        {
            if (this.NewId != null)
            {
                return true;
            }

            var permission = this.SessionOrigin.Database.GetPermission(this.ObjectType, roleType, Operations.Read);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (this.NewId != null)
            {
                return true;
            }

            var permission = this.SessionOrigin.Database.GetPermission(this.ObjectType, roleType, Operations.Write);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (this.NewId != null)
            {
                return true;
            }

            var permission = this.SessionOrigin.Database.GetPermission(this.ObjectType, methodType, Operations.Execute);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);
            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IStrategy>)value).Any();
            }

            return value != null;
        }

        public object Get(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.NewId == null)
                {
                    var workspaceRole = this.DatabaseObject.Roles?.FirstOrDefault(v => Equals(v.RoleType, roleType));
                    if (workspaceRole?.Value != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = workspaceRole.Value;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.SessionOrigin.Instantiate((long)workspaceRole.Value);
                            }
                            else
                            {
                                var ids = (long[])workspaceRole.Value;
                                var array = Array.CreateInstance(roleType.ObjectType.ClrType, ids.Length);
                                for (var i = 0; i < ids.Length; i++)
                                {
                                    array.SetValue(this.SessionOrigin.Instantiate(ids[i]), i);
                                }

                                value = array;
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.SessionOrigin.Database.ObjectFactory.EmptyArray(roleType.ObjectType);
                }

                this.roleByRoleType[roleType] = value;
            }

            return value;
        }

        public void Set(IRoleType roleType, object value)
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
                if (value == null)
                {
                    value = EmptySessionObjects;
                }

                var currentCollection = (IList<object>)current;
                var valueCollection = (IList<object>)value;
                if (currentCollection.Count == valueCollection.Count && !currentCollection.Except(valueCollection).Any())
                {
                    return;
                }
            }

            if (this.changedRoleByRoleType == null)
            {
                this.changedRoleByRoleType = new Dictionary<IRoleType, object>();
            }

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                // TODO: Optimize
                value = new ArrayList((Array)value).ToArray(roleType.ObjectType.ClrType);
            }

            this.roleByRoleType[roleType] = value;
            this.changedRoleByRoleType[roleType] = value;
        }

        public void Add(IRoleType roleType, IObject value)
        {
            var roles = (IObject[])this.Get(roleType);
            if (!roles.Contains(value))
            {
                roles = new List<IObject>(roles) { value }.ToArray();
            }

            this.Set(roleType, roles);
        }

        public void Remove(IRoleType roleType, IObject value)
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

        public object GetAssociation(IAssociationType associationType) => this.SessionOrigin.GetAssociation(this.Object, associationType).FirstOrDefault();

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType) => this.SessionOrigin.GetAssociation(this.Object, associationType);

        public PushRequestObject Save()
        {
            if (this.changedRoleByRoleType != null)
            {
                var data = new PushRequestObject
                {
                    I = this.Id.ToString(),
                    V = this.Version.ToString(),
                    Roles = this.SaveRoles(),
                };

                return data;
            }

            return null;
        }

        public PushRequestNewObject SaveNew()
        {
            var data = new PushRequestNewObject
            {
                NI = this.NewId.ToString(),
                T = this.ObjectType.IdAsString,
            };

            if (this.changedRoleByRoleType != null)
            {
                data.Roles = this.SaveRoles();
            }

            return data;
        }

        public void Reset()
        {
            if (this.DatabaseObject != null)
            {
                this.DatabaseObject = this.SessionOrigin.Database.Get(this.Id);
            }

            this.changedRoleByRoleType = null;

            this.roleByRoleType = new Dictionary<IRoleType, object>();
        }

        public void Refresh(bool merge = false)
        {
            if (!this.HasChanges)
            {
                this.Reset();
            }
            else
            {
                if (merge)
                {
                    if (this.DatabaseObject != null)
                    {
                        this.DatabaseObject = this.SessionOrigin.Database.Get(this.Id);
                    }
                }
            }
        }

        public object GetForAssociation(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.NewId == null)
                {
                    var workspaceRole = this.DatabaseObject.Roles?.FirstOrDefault(v => Equals(v.RoleType, roleType));
                    if (workspaceRole?.Value != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = workspaceRole.Value;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.SessionOrigin.GetForAssociation((long)workspaceRole.Value);
                            }
                            else
                            {
                                var ids = (long[])workspaceRole.Value;
                                var array = ids.Select(v => this.SessionOrigin.GetForAssociation(v))
                                    .Where(v => v != null)
                                    .ToArray();
                                value = array;
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.SessionOrigin.Database.ObjectFactory.EmptyArray(roleType.ObjectType);
                }
            }

            return value;
        }

        private PushRequestRole[] SaveRoles()
        {
            var saveRoles = new List<PushRequestRole>();

            foreach (var keyValuePair in this.changedRoleByRoleType)
            {
                var roleType = keyValuePair.Key;
                var roleValue = keyValuePair.Value;

                var pushRequestRole = new PushRequestRole { T = roleType.RelationType.IdAsString };

                if (roleType.ObjectType.IsUnit)
                {
                    pushRequestRole.S = UnitConvert.ToString(roleValue);
                }
                else
                {
                    if (roleType.IsOne)
                    {
                        var sessionRole = (IObject)roleValue;
                        pushRequestRole.S = sessionRole?.Id.ToString();
                    }
                    else
                    {
                        var sessionRoles = (IObject[])roleValue;
                        var roleIds = sessionRoles.Select(item => item.Id.ToString()).ToArray();
                        if (this.NewId != null)
                        {
                            pushRequestRole.A = roleIds;
                        }
                        else
                        {
                            var workspaceRole = this.DatabaseObject.Roles.FirstOrDefault(v => Equals(v.RoleType, roleType));
                            if (workspaceRole?.Value == null)
                            {
                                pushRequestRole.A = roleIds;
                            }
                            else
                            {
                                if (workspaceRole.Value != null)
                                {
                                    var originalRoleIds = ((IEnumerable<long>)workspaceRole.Value)
                                        .Select(v => v.ToString())
                                        .ToArray();
                                    pushRequestRole.A = roleIds.Except(originalRoleIds).ToArray();
                                    pushRequestRole.R = originalRoleIds.Except(roleIds).ToArray();
                                }
                                else
                                {
                                    pushRequestRole.A = roleIds.ToArray();
                                }
                            }
                        }
                    }
                }

                saveRoles.Add(pushRequestRole);
            }

            return saveRoles.ToArray();
        }
    }
}
