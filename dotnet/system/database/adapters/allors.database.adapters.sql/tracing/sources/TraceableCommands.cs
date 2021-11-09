// <copyright file="TraceableCommands.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Tracing;

    public sealed class TraceableCommands : Commands
    {
        private readonly ISink sink;

        public TraceableCommands(Transaction transaction, IConnection connection) : base(transaction, connection) => this.sink = transaction.Database.Sink;

        internal override void DeleteObject(Strategy strategy)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsDeleteObject) { Strategy = strategy };
            this.sink.OnBefore(@event);

            base.DeleteObject(strategy);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void GetUnitRoles(Strategy strategy)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetUnitRoles) { Strategy = strategy };
            this.sink.OnBefore(@event);

            base.GetUnitRoles(strategy);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void SetUnitRoles(Strategy strategy, List<IRoleType> sortedRoleTypes)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsSetUnitRoles) { Strategy = strategy, RoleTypes = sortedRoleTypes.ToArray() };
            this.sink.OnBefore(@event);

            base.SetUnitRoles(strategy, sortedRoleTypes);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void GetCompositeRole(Strategy strategy, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetCompositeRole) { Strategy = strategy, RoleType = roleType };
            this.sink.OnBefore(@event);

            base.GetCompositeRole(strategy, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void SetCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsSetCompositeRole) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.SetCompositeRole(relations, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void GetCompositesRole(Strategy strategy, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetCompositesRole) { Strategy = strategy, RoleType = roleType };
            this.sink.OnBefore(@event);

            base.GetCompositesRole(strategy, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void AddCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsAddCompositeRole) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.AddCompositeRole(relations, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void RemoveCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsRemoveCompositeRole) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.RemoveCompositeRole(relations, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override void ClearCompositeAndCompositesRole(IList<long> associations, IRoleType roleType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsClearCompositeAndCompositesRole) { AssociationIds = associations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.ClearCompositeAndCompositesRole(associations, roleType);

            this.sink.OnAfter(@event.Stop());
        }

        internal override Reference GetCompositeAssociation(Reference role, IAssociationType associationType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetCompositeAssociation) { Role = role, AssociationType = associationType };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetCompositeAssociation(role, associationType);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override long[] GetCompositesAssociation(Strategy role, IAssociationType associationType)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetCompositesAssociation) { Role = role.Reference, AssociationType = associationType };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetCompositesAssociation(role, associationType);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override Reference CreateObject(IClass @class)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsCreateObject) { Class = @class };
            this.sink.OnBefore(@event);

            try
            {
                return base.CreateObject(@class);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override IList<Reference> CreateObjects(IClass @class, int count)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsCreateObjects) { Class = @class, Count = count };
            this.sink.OnBefore(@event);

            try
            {
                return base.CreateObjects(@class, count);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override Reference InstantiateObject(long objectId)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsInstantiateObject) { ObjectId = objectId };
            this.sink.OnBefore(@event);

            try
            {
                return base.InstantiateObject(objectId);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override IEnumerable<Reference> InstantiateReferences(IEnumerable<long> objectIds)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsInstantiateReferences) { ObjectIds = objectIds?.ToArray() };
            this.sink.OnBefore(@event);

            try
            {
                return base.InstantiateReferences(objectIds);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override Dictionary<long, long> GetVersions(ISet<Reference> references)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsGetVersions) { References = references };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetVersions(references);
            }
            finally
            {
                this.sink.OnAfter(@event.Stop());
            }
        }

        internal override void UpdateVersion(IEnumerable<long> changed)
        {
            var @event = new Event(this.Transaction, EventKind.CommandsUpdateVersion) { ObjectIds = changed?.ToArray() };
            this.sink.OnBefore(@event);

            base.UpdateVersion(changed);

            this.sink.OnAfter(@event.Stop());
        }

        private class SortedRoleTypeComparer : IEqualityComparer<IList<IRoleType>>
        {
            public bool Equals(IList<IRoleType> x, IList<IRoleType> y)
            {
                if (x.Count == y.Count)
                {
                    for (var i = 0; i < x.Count; i++)
                    {
                        if (!x[i].Equals(y[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public int GetHashCode(IList<IRoleType> roleTypes)
            {
                var hashCode = 0;
                foreach (var roleType in roleTypes)
                {
                    hashCode ^= roleType.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}
