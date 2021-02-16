// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Validating
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Database.Derivations;
    using Meta;
    using Tracing;
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

            while (changeSet.Associations.Any() || changeSet.Roles.Any() || changeSet.Created.Any() || changeSet.Deleted.Any())
            {
                if (++domainCycles > maxDomainDerivationCycles)
                {
                    throw new Exception("Maximum amount of domain derivation cycles detected");
                }

                // Initialization
                if (changeSet.Created.Any())
                {
                    foreach (var newObject in changeSet.Created.Select(v => (Object)v.GetObject()))
                    {
                        newObject.OnInit();
                    }
                }

                var domainCycle = new DomainDerivationCycle { ChangeSet = changeSet, Transaction = this.Transaction, Validation = domainValidation };

                foreach (var domainDerivation in derivations)
                {
                    var matches = new HashSet<IObject>();

                    foreach (var pattern in domainDerivation.Patterns)
                    {
                        var source = pattern switch
                        {
                            // RoleDefault
                            ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleDefault roleInterface => changeSet
                                .AssociationsByRoleType
                                .Where(v => v.Key.RelationType.Equals(roleInterface.RelationType))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value)),

                            // RoleInterface
                            ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleInterface roleInterface => changeSet
                                .AssociationsByRoleType
                                .Where(v => v.Key.RelationType.Equals(roleInterface.RelationType))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value))
                                .Where(v => roleInterface.AssociationTypeComposite.IsAssignableFrom(v.Strategy.Class)),

                            // RoleClass
                            ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleClass roleClass => changeSet
                                .AssociationsByRoleType.Where(v => v.Key.Equals(roleClass))
                                .SelectMany(v => this.Transaction.Instantiate(v.Value))
                                .Where(v => v.Strategy.Class.Equals(roleClass.AssociationTypeComposite)),

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
                        using var validator = new Validator((IOnAccess)this.Transaction, domainDerivation.Patterns, matches);
                        domainDerivation.Derive(domainCycle, matches);
                    }
                }

                changeSet = this.Transaction.Checkpoint();
                domainAccumulatedChangeSet.Add(changeSet);
            }

            return domainAccumulatedChangeSet;
        }
    }
}
