// <copyright file="Preparation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Domain.Derivations.Legacy;

    public class LegacyPreparation : ILegacyPreparation
    {
        public LegacyPreparation(LegacyIteration legacyIteration, IEnumerable<Object> marked, AccumulatedChangeSet domainAccumulatedChangeSet = null)
        {
            this.LegacyIteration = legacyIteration;
            var cycle = this.LegacyIteration.LegacyCycle;
            var derivation = cycle.Derivation;
            var transaction = derivation.Transaction;

            var changeSet = domainAccumulatedChangeSet ?? transaction.Checkpoint();

            legacyIteration.ChangeSet.Add(changeSet);
            cycle.ChangeSet.Add(changeSet);
            derivation.ChangeSet.Add(changeSet);

            // Initialization
            if (domainAccumulatedChangeSet == null && changeSet.Created.Any())
            {
                var newObjects = changeSet.Created;
                foreach (Object newObject in newObjects)
                {
                    newObject.OnInit();
                }
            }

            // ChangedObjects
            var changedObjectIds = new HashSet<IObject>(changeSet.Associations);
            changedObjectIds.UnionWith(changeSet.Roles);
            changedObjectIds.UnionWith(changeSet.Created);

            this.Objects = new HashSet<Object>(derivation.Transaction.Instantiate(changedObjectIds).Cast<Object>());
            this.Objects.ExceptWith(this.LegacyIteration.LegacyCycle.Derivation.DerivedObjects);

            if (marked != null)
            {
                this.Objects.UnionWith(marked);
            }

            this.PreDerived = new HashSet<Object>();
        }

        public LegacyIteration LegacyIteration { get; }

        public ISet<Object> Objects { get; set; }

        public ISet<Object> PreDerived { get; set; }

        public void Execute()
        {
            foreach (var @object in this.Objects)
            {
                if (!@object.Strategy.IsDeleted)
                {
                    @object.OnPreDerive(x => x.WithIteration(this.LegacyIteration));
                    this.PreDerived.Add(@object);
                }
            }
        }
    }
}
