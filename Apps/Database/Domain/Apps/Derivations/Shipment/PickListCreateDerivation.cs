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

    public class PickListCreateDerivation : DomainDerivation
    {
        public PickListCreateDerivation(M m) : base(m, new Guid("64a825af-5287-4984-9264-60f150ace179")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PickList.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PickList>())
            {
                if (!@this.ExistCreationDate)
                {
                    @this.CreationDate = @this.Session().Now();
                }

                if (!@this.ExistPickListState)
                {
                    @this.PickListState = new PickListStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistStore)
                {
                    @this.Store = @this.Strategy.Session.Extent<Store>().First;
                }
            }
        }
    }
}
