// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class WorkTaskCreateDerivation : DomainDerivation
    {
        public WorkTaskCreateDerivation(M m) : base(m, new Guid("df06972e-e629-4fcd-bbce-56d965e804e7")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.WorkTask.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent()
                    .Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistTakenBy && internalOrganisations.Count() == 1)
                {
                    @this.TakenBy = internalOrganisations.First();
                }
            }
        }
    }
}
