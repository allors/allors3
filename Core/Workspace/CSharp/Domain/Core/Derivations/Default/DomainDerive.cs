// <copyright file="DomainDerivationCycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Data;
    using Meta;
    using Domain;

    public class DomainDerive
    {
        public DomainDerive(IWorkspace workspace, int maxDomainDerivationCycles)
        {
            this.Workspace = workspace;
            this.MaxDomainDerivationCycles = maxDomainDerivationCycles;

            this.Validation = new DomainValidation();
        }

        public IWorkspace Workspace { get; }

        public int MaxDomainDerivationCycles { get; }

        public IDomainValidation Validation { get; }

        public void Execute()
        {
            // Domain Derivations
            var domainCycles = 0;

            var domainDerivationById = this.Workspace.DomainDerivationById;
            if (domainDerivationById.Any())
            {
                var changeSets = this.Workspace.Checkpoint();

                while (changeSets.Any(v => v.Associations.Any() || v.Roles.Any() || v.Created.Any() || v.Deleted.Any()))
                {
                    foreach (var changeSet in changeSets)
                    {
                        var session = changeSet.Session;

                        if (++domainCycles > this.MaxDomainDerivationCycles)
                        {
                            throw new Exception("Maximum amount of domain derivation cycles detected");
                        }

                        var domainCycle = new DomainDerivationCycle { ChangeSet = changeSet, Session = session, Validation = this.Validation };

                        var matchesByDerivation = new Dictionary<IDomainDerivation, IEnumerable<IObject>>();
                        foreach (var kvp in domainDerivationById)
                        {
                            var domainDerivation = kvp.Value;
                            var matches = new HashSet<IObject>();

                            foreach (var pattern in domainDerivation.Patterns)
                            {
                                var source = pattern switch
                                {
                                    // Create
                                    CreatedPattern createdPattern => changeSet.Created
                                        .Where(v => createdPattern.Composite.IsAssignableFrom(v.Class))
                                        .Select(v => v.Object),

                                    // RoleDefault
                                    ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleDefault roleInterface => changeSet
                                            .AssociationsByRoleType
                                            .Where(v => v.Key.RelationType.Equals(roleInterface.RelationType))
                                            .SelectMany(v => session.Instantiate(v.Value)),

                                    // RoleInterface
                                    ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleInterface roleInterface => changeSet
                                            .AssociationsByRoleType
                                            .Where(v => v.Key.RelationType.Equals(roleInterface.RelationType))
                                            .SelectMany(v => session.Instantiate(v.Value))
                                            .Where(v => roleInterface.AssociationTypeComposite.IsAssignableFrom(v.Strategy.Class)),

                                    // RoleClass
                                    ChangedPattern changedRolePattern when changedRolePattern.RoleType is RoleClass roleClass => changeSet
                                            .AssociationsByRoleType.Where(v => v.Key.Equals(roleClass))
                                            .SelectMany(v => session.Instantiate(v.Value))
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
                                domainDerivation.Derive(domainCycle, matches);
                            }
                        }
                    }

                    changeSets = this.Workspace.Checkpoint();
                }
            }
        }
    }
}
