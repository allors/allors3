// <copyright file="Context.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.Database.Push;
    using Allors.Workspace.Meta;

    public class InternalSession
    {
        private static long idCounter = 0;
        private readonly Dictionary<long, ISessionObject> sessionObjectById = new Dictionary<long, ISessionObject>();
        private readonly Dictionary<long, ISessionObject> newSessionObjectById = new Dictionary<long, ISessionObject>();

        public InternalSession(Context context, InternalWorkspace internalWorkspace)
        {
            this.Context = context;
            this.InternalWorkspace = internalWorkspace;
        }

        public bool HasChanges => this.newSessionObjectById.Count > 0 || this.sessionObjectById.Values.Any(v => v.Strategy.HasChanges);

        public Context Context { get; }

        public InternalWorkspace InternalWorkspace { get; }

        public ISessionObject Get(long id)
        {
            if (!this.sessionObjectById.TryGetValue(id, out var sessionObject))
            {
                if (!this.newSessionObjectById.TryGetValue(id, out sessionObject))
                {
                    var workspaceObject = this.InternalWorkspace.Get(id);

                    var strategy = new Strategy(this.Context, workspaceObject);
                    sessionObject = this.InternalWorkspace.ObjectFactory.Create(strategy);
                    strategy.SessionObject = sessionObject;

                    this.sessionObjectById[workspaceObject.Id] = sessionObject;
                }
            }

            return sessionObject;
        }

        public ISessionObject Create(IClass @class)
        {
            var strategy = new Strategy(this.Context, @class, --InternalSession.idCounter);
            var newSessionObject = this.InternalWorkspace.ObjectFactory.Create(strategy);
            strategy.SessionObject = newSessionObject;
            this.newSessionObjectById[newSessionObject.Id] = newSessionObject;
            return newSessionObject;
        }

        public void Reset()
        {
            foreach (var newSessionObject in this.newSessionObjectById.Values)
            {
                newSessionObject.Strategy.Reset();
            }

            foreach (var sessionObject in this.sessionObjectById.Values)
            {
                sessionObject.Strategy.Reset();
            }
        }

        public void Refresh()
        {
            foreach (var sessionObject in this.sessionObjectById.Values)
            {
                sessionObject.Strategy.Refresh();
            }
        }


        public PushRequest PushRequest() =>
            new PushRequest
            {
                NewObjects = this.newSessionObjectById.Select(v => v.Value.Strategy.SaveNew()).ToArray(),
                Objects = this.sessionObjectById.Select(v => v.Value.Strategy.Save()).Where(v => v != null).ToArray(),
            };

        public void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var newId = long.Parse(pushResponseNewObject.NI);
                    var id = long.Parse(pushResponseNewObject.I);

                    var sessionObject = this.newSessionObjectById[newId];
                    sessionObject.Strategy.PushResponse(id);

                    this.newSessionObjectById.Remove(newId);
                    this.sessionObjectById[id] = sessionObject;
                }
            }

            if (this.newSessionObjectById != null && this.newSessionObjectById.Count != 0)
            {
                throw new Exception("Not all new objects received ids");
            }
        }

        public IEnumerable<ISessionObject> GetAssociation(ISessionObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            var associations = this.InternalWorkspace.Get((IComposite)associationType.ObjectType).Select(v => this.Get(v.Id));
            foreach (var association in associations)
            {
                if (association.Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (ISessionObject)((ISessionObject)association).Strategy.GetForAssociation(roleType);
                        if (role != null && role.Id == @object.Id)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (ISessionObject[])((ISessionObject)association).Strategy.GetForAssociation(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        internal ISessionObject GetForAssociation(long id)
        {
            if (!this.sessionObjectById.TryGetValue(id, out var sessionObject))
            {
                this.newSessionObjectById.TryGetValue(id, out sessionObject);
            }

            return sessionObject;
        }
    }
}
