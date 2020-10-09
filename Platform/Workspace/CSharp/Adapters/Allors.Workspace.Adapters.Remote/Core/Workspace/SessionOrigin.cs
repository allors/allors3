// <copyright file="Session.cs" company="Allors bvba">
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

    public class SessionOrigin
    {
        private static long idCounter = 0;
        private readonly Dictionary<long, Strategy> strategyById = new Dictionary<long, Strategy>();
        private readonly Dictionary<long, Strategy> newStrategyById = new Dictionary<long, Strategy>();

        public SessionOrigin(Session session, DatabaseOrigin databaseOrigin)
        {
            this.Session = session;
            this.DatabaseOrigin = databaseOrigin;
        }

        public bool HasChanges => this.newStrategyById.Count > 0 || this.strategyById.Values.Any(v => v.HasChanges);

        public Session Session { get; }

        public DatabaseOrigin DatabaseOrigin { get; }

        public IObject Get(long id)
        {
            if (!this.strategyById.TryGetValue(id, out var strategy))
            {
                if (!this.newStrategyById.TryGetValue(id, out strategy))
                {
                    var workspaceObject = this.DatabaseOrigin.Get(id);
                    strategy = new Strategy(this.Session, workspaceObject);
                    this.strategyById[workspaceObject.Id] = strategy;
                }
            }

            return strategy.Object;
        }

        public IObject Create(IClass @class)
        {
            var strategy = new Strategy(this.Session, @class, --SessionOrigin.idCounter);
            this.newStrategyById[strategy.Id] = strategy;
            return strategy.Object;
        }

        public void Reset()
        {
            foreach (var newSessionObject in this.newStrategyById.Values)
            {
                newSessionObject.Reset();
            }

            foreach (var sessionObject in this.strategyById.Values)
            {
                sessionObject.Reset();
            }
        }

        public void Refresh()
        {
            foreach (var sessionObject in this.strategyById.Values)
            {
                sessionObject.Refresh();
            }
        }


        public PushRequest PushRequest() =>
            new PushRequest
            {
                NewObjects = this.newStrategyById.Select(v => v.Value.SaveNew()).ToArray(),
                Objects = this.strategyById.Select(v => v.Value.Save()).Where(v => v != null).ToArray(),
            };

        public void PushResponse(PushResponse pushResponse)
        {
            if (pushResponse.NewObjects != null && pushResponse.NewObjects.Length > 0)
            {
                foreach (var pushResponseNewObject in pushResponse.NewObjects)
                {
                    var newId = long.Parse(pushResponseNewObject.NI);
                    var id = long.Parse(pushResponseNewObject.I);

                    var sessionObject = this.newStrategyById[newId];
                    sessionObject.PushResponse(id);

                    this.newStrategyById.Remove(newId);
                    this.strategyById[id] = sessionObject;
                }
            }

            if (this.newStrategyById != null && this.newStrategyById.Count != 0)
            {
                throw new Exception("Not all new objects received ids");
            }
        }

        public IEnumerable<IObject> GetAssociation(IObject @object, IAssociationType associationType)
        {
            var roleType = associationType.RoleType;

            var associations = this.DatabaseOrigin.Get((IComposite)associationType.ObjectType).Select(v => this.Get(v.Id));
            foreach (var association in associations)
            {
                if (association.Strategy.CanRead(roleType))
                {
                    if (roleType.IsOne)
                    {
                        var role = (IObject)((Strategy)association.Strategy).GetForAssociation(roleType);
                        if (role != null && role.Id == @object.Id)
                        {
                            yield return association;
                        }
                    }
                    else
                    {
                        var roles = (IObject[])((Strategy)association.Strategy).GetForAssociation(roleType);
                        if (roles != null && roles.Contains(@object))
                        {
                            yield return association;
                        }
                    }
                }
            }
        }

        internal IObject GetForAssociation(long id)
        {
            if (!this.strategyById.TryGetValue(id, out var sessionObject))
            {
                this.newStrategyById.TryGetValue(id, out sessionObject);
            }

            return sessionObject?.Object;
        }
    }
}
