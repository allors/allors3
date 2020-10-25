// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class AuditableDerivation : DomainDerivation
    {
        public AuditableDerivation(M m) : base(m, new Guid("a310ed3b-2129-4bee-8457-ae3c8441597f")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(m.Auditable.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var auditable in matches.Cast<Auditable>())
            {
                var user = auditable.Strategy.Session.State().User;
                if (user != null)
                {
                    auditable.CreationDate = auditable.Strategy.Session.Now();
                    auditable.CreatedBy = user;
                }
            }
        }
    }
}
