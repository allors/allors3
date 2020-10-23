// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Data;
    using Meta;
    using Object = Domain.Object;

    public class DomainDerive 
    {
        public DomainDerive(ISession session, IValidation validation, DerivationConfig derivationConfig)
        {
            this.Session = session;
            this.Validation = validation;
            this.DerivationConfig = derivationConfig;
        }

        public ISession Session { get; }

        public IValidation Validation { get; }

        public DerivationConfig DerivationConfig { get; }

        public AccumulatedChangeSet Execute()
        {
            // Domain Derivations
            var maxDomainDerivationCycles = this.DerivationConfig.MaxDomainDerivationCycles;
            var domainCycles = 0;

            AccumulatedChangeSet domainAccumulatedChangeSet = null;

            var domainDerivationById = this.Session.Database.DomainDerivationById;
            if (domainDerivationById.Any())
            {
                domainAccumulatedChangeSet = new AccumulatedChangeSet();
                var domainValidation = new DomainValidation(this.Validation);

                var changeSet = this.Session.Checkpoint();
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
                        var newObjects = changeSet.Created.Select(v => (Object)v.GetObject());
                        foreach (var newObject in newObjects)
                        {
                            newObject.OnInit();
                        }
                    }

                    var domainCycle = new DomainDerivationCycle { ChangeSet = changeSet, Session = this.Session, Validation = domainValidation };

                    var matchesByDerivation = new Dictionary<IDomainDerivation, IEnumerable<IObject>>();
                    foreach (var kvp in domainDerivationById)
                    {
                        var domainDerivation = kvp.Value;
                        var matches = new HashSet<IObject>();

                        foreach (var pattern in domainDerivation.Patterns)
                        {
                            var source = pattern switch
                            {
                                CreatedPattern createdPattern => changeSet.Created
                                    .Where(v => createdPattern.Composite.IsAssignableFrom(v.Class))
                                    .Select(v => v.GetObject()),
                                ChangedRolePattern changedRolePattern when changedRolePattern.RoleType is RoleInterface roleInterface => changeSet
                                        .AssociationsByRoleType
                                        .Where(v => v.Key.RelationType.Equals(roleInterface.RelationType))
                                        .SelectMany(v => this.Session.Instantiate(v.Value)),
                                ChangedRolePattern changedRolePattern when changedRolePattern.RoleType is RoleClass roleClass => changeSet
                                        .AssociationsByRoleType.Where(v => v.Key.Equals(roleClass))
                                        .SelectMany(v => this.Session.Instantiate(v.Value))
                                        .Where(v => v.Strategy.Class.Equals(roleClass.AssociationTypeClass)),
                                ChangedAssociationPattern changedAssociationsPattern => changeSet
                                    .RolesByAssociationType
                                    .Where(v => v.Key.Equals(changedAssociationsPattern.AssociationType))
                                    .SelectMany(v => this.Session.Instantiate(v.Value)),
                                _ => Array.Empty<IObject>()
                            };

                            if (source != null)
                            {
                                if (pattern.Steps?.Length > 0)
                                {
                                    var step = new Step(pattern.Steps);
                                    var stepped = source.SelectMany(v => step.Get(v));
                                    matches.UnionWith(stepped);
                                }
                                else
                                {
                                    matches.UnionWith(source);
                                }
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

                    changeSet = this.Session.Checkpoint();
                    domainAccumulatedChangeSet.Add(changeSet);
                }
            }

            return domainAccumulatedChangeSet;
        }
    }
}
