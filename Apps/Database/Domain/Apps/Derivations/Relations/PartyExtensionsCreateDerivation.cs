// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PartyExtensionsCreateDerivation : DomainDerivation
    {
        public PartyExtensionsCreateDerivation(M m) : base(m, new Guid("9756beaa-96d8-4fb4-a4e0-1ce631538d49")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Party.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Party>())
            {
                if (!@this.ExistPreferredCurrency)
                {
                    var singleton = session.GetSingleton();
                    @this.PreferredCurrency = singleton.Settings.PreferredCurrency;
                }
            }
        }
    }
}
