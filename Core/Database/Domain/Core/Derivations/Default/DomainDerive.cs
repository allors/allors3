// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Database.Derivations;
    using Object = Domain.Object;

    public class DomainDerive
    {
        public DomainDerive(ITransaction transaction, IValidation validation, DerivationConfig derivationConfig)
        {
            this.Transaction = transaction;
            this.Validation = validation;
            this.DerivationConfig = derivationConfig;
        }

        public ITransaction Transaction { get; }

        public IValidation Validation { get; }

        public DerivationConfig DerivationConfig { get; }

        public AccumulatedChangeSet Execute()
        {
            var derivations = this.Transaction.Database.Derivations;

            var maxDomainDerivationCycles = this.DerivationConfig.MaxDomainDerivationCycles;
            var domainCycles = 0;

            AccumulatedChangeSet domainAccumulatedChangeSet = null;

            domainAccumulatedChangeSet = new AccumulatedChangeSet();
            var domainValidation = new DomainValidation(this.Validation);

            var changeSet = this.Transaction.Checkpoint();
            domainAccumulatedChangeSet.Add(changeSet);

            while (changeSet.Associations.Any() || changeSet.Roles.Any() || changeSet.Created.Any() ||
                   changeSet.Deleted.Any())
            {
                if (++domainCycles > maxDomainDerivationCycles)
                {
                    throw new Exception("Maximum amount of domain derivation cycles detected");
                }

                // Initialization
                if (changeSet.Created.Any())
                {
                    var newObjects = changeSet.Created.Select(v => (Object)v.GetObject());
                    foreach (var newObject in newObjects)
                    {
                        newObject.OnInit();
                    }
                }

                var domainCycle = new DomainDerivationCycle
                {
                    ChangeSet = changeSet,
                    Transaction = this.Transaction,
                    Validation = domainValidation
                };

                var matchesByDerivation = new Dictionary<IRule, IEnumerable<IObject>>();
                foreach (var domainDerivation in derivations)
                {
                    var matches = new HashSet<IObject>();

                    foreach (var pattern in domainDerivation.Patterns)
                    {
                        var source = pattern switch
                        {

                            RolePattern { ObjectType: null } rolePattern => changeSet
                                .AssociationsByRoleType
                                .Where(v => v.Key.Equals(rolePattern.RoleType))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value)),

                            RolePattern { ObjectType: { } } rolePattern => changeSet
                                .AssociationsByRoleType
                                .Where(v => v.Key.RelationType.Equals(rolePattern.RoleType.RelationType))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value))
                                .Where(v => rolePattern.ObjectType.IsAssignableFrom(v.Strategy.Class)),

                            AssociationPattern associationPattern => changeSet
                                .RolesByAssociationType
                                .Where(v => v.Key.Equals(associationPattern.AssociationType))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value)),

                            _ => Array.Empty<IObject>()
                        };

                        if (source != null)
                        {
                            if (pattern.Steps?.Length > 0)
                            {
                                var step = new Step(pattern.Steps);
                                source = source.SelectMany(v => step.Get(v));
                            }

                            if (pattern.OfType != null)
                            {
                                source = source.Where(v => pattern.OfType.IsAssignableFrom(v.Strategy.Class));
                            }

                            matches.UnionWith(source);
                        }
                    }

                    if (matches.Count > 0)
                    {
                        matchesByDerivation[domainDerivation] = matches;
                    }
                }

                // TODO: Prefetching

                foreach (var kvp in matchesByDerivation)
                {
                    var domainDerivation = kvp.Key;
                    var matches = kvp.Value;
                    domainDerivation.Derive(domainCycle, matches);
                }

                changeSet = this.Transaction.Checkpoint();
                domainAccumulatedChangeSet.Add(changeSet);
            }

            return domainAccumulatedChangeSet;
        }
    }
}
