// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class PassportDerivation : DomainDerivation
    {
        public PassportDerivation(M m) : base(m, new Guid("BB960F7C-2B67-4B4D-967A-84B50F55BE6E")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.Passport.Number),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Passport>())
            {
                cycle.Validation.AssertIsUnique(@this, this.M.Passport.Number, cycle.ChangeSet);
            }
        }
    }

}
