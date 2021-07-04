// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using Numbers;

    public class Push : Result, IPushResult
    {
        internal Push(Session session) : base(session)
        {
            this.Workspace = session.Workspace;
            this.Transaction = this.Workspace.DatabaseConnection.Database.CreateTransaction();

            var sessionContext = this.Transaction.Services();
            var databaseContext = this.Transaction.Database.Services();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.DatabaseConnection.Configuration.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.DatabaseConnection.Configuration.Name);
            this.M = databaseContext.M;
            this.Build = @class => (IObject)DefaultObjectBuilder.Build(this.Transaction, @class);
            this.Derive = () => this.Transaction.Derive(false);

            this.Objects = new HashSet<IObject>();
        }

        internal Dictionary<long, IObject> ObjectByNewId { get; private set; }

        internal IAccessControlLists AccessControlLists { get; }

        internal ISet<IObject> Objects { get; }

        private Workspace Workspace { get; }

        private ITransaction Transaction { get; }

        private ISet<IClass> AllowedClasses { get; }

        private MetaPopulation M { get; }

        private Func<IClass, IObject> Build { get; }

        private Func<IValidation> Derive { get; }

        private INumbers Numbers => this.Workspace.Numbers;

        internal void Execute(PushToDatabaseTracker tracker)
        {
            var metaPopulation = this.Workspace.DatabaseConnection.Database.MetaPopulation;

            if (tracker.Created != null)
            {
                this.ObjectByNewId = tracker.Created.ToDictionary(
                    k => k.Id,
                    v =>
                    {
                        var local = (Strategy)v;
                        var cls = (IClass)metaPopulation.FindByTag(v.Class.Tag);
                        if (this.AllowedClasses?.Contains(cls) == true)
                        {
                            var newObject = this.Build(cls);
                            this.Objects.Add(newObject);
                            //this.PushRequestRoles(local, newObject);
                            return newObject;
                        }

                        this.AddAccessError(local);

                        return null;
                    });


                if (this.HasErrors)
                {
                    return;
                }

                foreach (var local in tracker.Created)
                {
                    this.PushRequestRoles((Strategy)local, this.ObjectByNewId[local.Id]);
                }
            }


            if (tracker.Changed != null)
            {
                // bulk load all objects
                var objectIds = tracker.Changed.Select(v => v.Strategy.Id).ToArray();
                var objects = this.Transaction.Instantiate(objectIds);
                this.Objects.UnionWith(objects);

                if (objectIds.Length != objects.Length)
                {
                    var existingIds = objects.Select(v => v.Id);
                    var missingIds = objectIds.Where(v => !existingIds.Contains(v));
                    foreach (var missingId in missingIds)
                    {
                        this.AddMissingId(missingId);
                    }
                }

                if (!this.HasErrors)
                {
                    foreach (var state in tracker.Changed)
                    {
                        var strategy = (Strategy)state.Strategy;
                        var obj = this.Transaction.Instantiate(strategy.Id);
                        if (!strategy.DatabaseOriginState.Version.Equals(obj.Strategy.ObjectVersion))
                        {
                            this.AddVersionError(obj.Id);
                        }
                        else if (this.AllowedClasses?.Contains(obj.Strategy.Class) == true)
                        {
                            this.PushRequestRoles(strategy, obj);
                        }
                        else
                        {
                            this.AddAccessError(strategy);
                        }
                    }
                }
            }

            var validation = this.Derive();
            if (validation.HasErrors)
            {
                this.AddDerivationErrors(validation.Errors);
            }

            if (!this.HasErrors)
            {
                this.Transaction.Commit();
            }
        }

        private void PushRequestRoles(Strategy local, IObject obj)
        {
            if (local.DatabaseOriginState.ChangedRoleByRelationType == null)
            {
                return;
            }

            // TODO: Cache and filter for workspace
            var composite = (IComposite)obj.Strategy.Class;
            var roleTypes = composite.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Length > 0);
            var acl = this.AccessControlLists[obj];

            foreach (var keyValuePair in local.DatabaseOriginState.ChangedRoleByRelationType)
            {
                var relationType = keyValuePair.Key;
                var roleType = ((IRelationType)this.M.FindByTag(keyValuePair.Key.Tag)).RoleType;

                if (acl.CanWrite(roleType))
                {
                    var roleValue = keyValuePair.Value;

                    if (roleValue == null)
                    {
                        obj.Strategy.RemoveRole(roleType);
                    }
                    else if (roleType.ObjectType.IsUnit)
                    {
                        obj.Strategy.SetUnitRole(roleType, roleValue);
                    }
                    else
                    {
                        if (relationType.RoleType.IsOne)
                        {
                            var roleId = (long)roleValue;
                            IObject role = null;

                            if (roleId < 0)
                            {
                                this.ObjectByNewId.TryGetValue(roleId, out role);
                            }
                            else
                            {
                                role = this.Transaction.Instantiate(roleId);
                            }

                            obj.Strategy.SetCompositeRole(roleType, role);
                        }
                        else
                        {
                            if (this.ObjectByNewId == null)
                            {
                                var roles = this.Transaction.Instantiate(this.Numbers.Enumerate(roleValue));
                                obj.Strategy.SetCompositeRoles(roleType, this.GetRoles(roles));
                            }
                            else
                            {
                                obj.Strategy.SetCompositeRoles(roleType, this.GetRoles(roleValue));
                            }

                        }
                    }
                }
                else
                {
                    this.AddAccessError(local);
                }
            }
        }

        private IEnumerable<IObject> GetRoles(object ids)
        {
            var newIds = this.Numbers.Enumerate(ids).Where(v => v < 0);
            foreach (var v in newIds)
            {
                this.ObjectByNewId.TryGetValue(v, out var role);
                yield return role;
            };

            var existingIds = this.Numbers.Enumerate(ids).Where(v => v > 0);
            foreach (var role in this.Transaction.Instantiate(existingIds))
            {
                yield return role;
            }
        }
    }
}
