// <copyright file="TraceableCommands.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Tracing;
    using Meta;
    using Tracing;

    public sealed class TraceableCommands : Commands
    {
        private readonly ISink sink;

        public TraceableCommands(Transaction transaction, IConnection connection) : base(transaction, connection) => this.sink = transaction.Database.Sink;

        internal override void DeleteObject(Strategy strategy)
        {
            var @event = new SqlDeleteObjectEvent(this.Transaction) { Strategy = strategy };
            this.sink.OnBefore(@event);

            base.DeleteObject(strategy);

            this.sink.OnAfter(@event);
        }

        internal override void GetUnitRoles(Strategy strategy)
        {
            var @event = new SqlGetUnitRolesEvent(this.Transaction) { Strategy = strategy };
            this.sink.OnBefore(@event);

            base.GetUnitRoles(strategy);

            this.sink.OnAfter(@event);
        }

        internal override void SetUnitRoles(Strategy strategy, List<IRoleType> sortedRoleTypes)
        {
            var @event = new SqlSetUnitRolesEvent(this.Transaction) { Strategy = strategy, RoleTypes = sortedRoleTypes.ToArray() };
            this.sink.OnBefore(@event);

            base.SetUnitRoles(strategy, sortedRoleTypes);

            this.sink.OnAfter(@event);
        }

        internal override void GetCompositeRole(Strategy strategy, IRoleType roleType)
        {
            var @event = new SqlGetCompositeRoleEvent(this.Transaction) { Strategy = strategy, RoleType = roleType };
            this.sink.OnBefore(@event);

            base.GetCompositeRole(strategy, roleType);

            this.sink.OnAfter(@event);
        }

        internal override void SetCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new SqlSetCompositeRoleEvent(this.Transaction) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.SetCompositeRole(relations, roleType);

            this.sink.OnAfter(@event);
        }

        internal override void GetCompositesRole(Strategy strategy, IRoleType roleType)
        {
            var @event = new SqlGetCompositesRoleEvent(this.Transaction) { Strategy = strategy, RoleType = roleType };
            this.sink.OnBefore(@event);

            base.GetCompositesRole(strategy, roleType);

            this.sink.OnAfter(@event);
        }

        internal override void AddCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new SqlAddCompositeRoleEvent(this.Transaction) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.AddCompositeRole(relations, roleType);

            this.sink.OnAfter(@event);
        }

        internal override void RemoveCompositeRole(List<CompositeRelation> relations, IRoleType roleType)
        {
            var @event = new SqlRemoveCompositeRoleEvent(this.Transaction) { Relations = relations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.RemoveCompositeRole(relations, roleType);

            this.sink.OnAfter(@event);
        }

        internal override void ClearCompositeAndCompositesRole(IList<long> associations, IRoleType roleType)
        {
            var @event = new SqlClearCompositeAndCompositesRole(this.Transaction) { AssociationIds = associations?.ToArray(), RoleType = roleType };
            this.sink.OnBefore(@event);

            base.ClearCompositeAndCompositesRole(associations, roleType);

            this.sink.OnAfter(@event);
        }

        internal override Reference GetCompositeAssociation(Reference role, IAssociationType associationType)
        {
            var @event = new SqlGetCompositeAssociationEvent(this.Transaction) { Role = role, AssociationType = associationType };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetCompositeAssociation(role, associationType);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override long[] GetCompositesAssociation(Strategy role, IAssociationType associationType)
        {
            var @event = new SqlGetCompositesAssociationEvent(this.Transaction) { Role = role.Reference, AssociationType = associationType };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetCompositesAssociation(role, associationType);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override Reference CreateObject(IClass @class)
        {
            var @event = new SqlCreateObjectEvent(this.Transaction) { Class = @class };
            this.sink.OnBefore(@event);

            try
            {
                return base.CreateObject(@class);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override IList<Reference> CreateObjects(IClass @class, int count)
        {
            var @event = new SqlCreatesObjectEvent(this.Transaction) { Class = @class, Count = count };
            this.sink.OnBefore(@event);

            try
            {
                return base.CreateObjects(@class, count);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override Reference InstantiateObject(long objectId)
        {
            var @event = new SqlInstantiateObjectEvent(this.Transaction) { ObjectId = objectId };
            this.sink.OnBefore(@event);

            try
            {
                return base.InstantiateObject(objectId);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override IEnumerable<Reference> InstantiateReferences(IEnumerable<long> objectIds)
        {
            var @event = new SqlInstantiateReferencesEvent(this.Transaction) { ObjectIds = objectIds?.ToArray() };
            this.sink.OnBefore(@event);

            try
            {
                return base.InstantiateReferences(objectIds);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override Dictionary<long, long> GetVersions(ISet<Reference> references)
        {
            var @event = new SqlGetVersionsEvent(this.Transaction) { References = references };
            this.sink.OnBefore(@event);

            try
            {
                return base.GetVersions(references);
            }
            finally
            {
                this.sink.OnAfter(@event);
            }
        }

        internal override void UpdateVersion(IEnumerable<long> changed)
        {
            var @event = new SqlUpdateVersionEvent(this.Transaction) { ObjectIds = changed?.ToArray() };
            this.sink.OnBefore(@event);

            base.UpdateVersion(changed);

            this.sink.OnAfter(@event);
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
